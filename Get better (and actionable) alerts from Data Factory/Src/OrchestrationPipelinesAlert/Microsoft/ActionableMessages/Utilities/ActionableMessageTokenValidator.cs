using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DBojsen.OrchestrationPipelinesAlert.Microsoft.ActionableMessages.Utilities
{
    // Borrowed from https://github.com/OfficeDev/o365-actionable-messages-utilities-for-dotnet/blob/master/src/Microsoft.O365.ActionableMessages.Utilities/ActionableMessageTokenValidator.cs
    internal class ActionableMessageTokenValidator
    {
        /// <summary>
        /// The clock skew to apply when validating times in a token.
        /// </summary>
        private const int TokenTimeValidationClockSkewBufferInMinutes = 5;

        /// <summary>
        /// The OpenID configuration data retriever.
        /// </summary>
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        /// <summary>
        /// Constructor of the <see cref="ActionableMessageTokenValidator"/> class.
        /// </summary>
        public ActionableMessageTokenValidator()
        {
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                O365OpenIdConfiguration.MetadataUrl,
                new OpenIdConnectConfigurationRetriever());
        }

        /// <summary>
        /// Constructor of the <see cref="ActionableMessageTokenValidator"/> class. 
        /// DO NOT use this constructor. This constructor is used for unit testing.
        /// </summary>
        /// <param name="configurationManager">The configuration manager to read the OpenID configuration from.</param>
        public ActionableMessageTokenValidator(IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
        {
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
        }

        
        public async Task<ActionableMessageTokenValidationResult> ValidateTokenAsync(
            string token,
            string targetServiceBaseUrl)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("token is null or empty.", nameof(token));
            }

            if (string.IsNullOrEmpty(targetServiceBaseUrl))
            {
                throw new ArgumentException("url is null or empty.", nameof(targetServiceBaseUrl));
            }

            CancellationToken cancellationToken = default;
            var o365OpenIdConfig = await _configurationManager.GetConfigurationAsync(cancellationToken);
            ClaimsPrincipal claimsPrincipal;
            var result = new ActionableMessageTokenValidationResult();

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = [O365OpenIdConfiguration.TokenIssuer],
                ValidateAudience = true,
                ValidAudiences = [targetServiceBaseUrl],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(TokenTimeValidationClockSkewBufferInMinutes),
                RequireSignedTokens = true,
                IssuerSigningKeys = o365OpenIdConfig.SigningKeys
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // This will validate the token's lifetime and the following claims:
                // 
                // iss
                // aud
                //
                claimsPrincipal = tokenHandler.ValidateToken(token, parameters, out _);
            }
            catch (SecurityTokenSignatureKeyNotFoundException ex)
            {
                Trace.TraceError("Token signature key not found.");
                result.Exception = ex;
                return result;
            }
            catch (SecurityTokenExpiredException ex)
            {
                Trace.TraceError("Token expired.");
                result.Exception = ex;
                return result;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                Trace.TraceError("Invalid signature.");
                result.Exception = ex;
                return result;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                result.Exception = ex;
                return result;
            }

            if (claimsPrincipal == null)
            {
                Trace.TraceError("Identity not found in the token.");
                result.Exception = new InvalidOperationException("Identity not found in the token");
                return result;
            }

            var identity = claimsPrincipal.Identities.FirstOrDefault();
            if (identity == null)
            {
                Trace.TraceError("Claims not found in the token.");
                result.Exception = new InvalidOperationException("Claims not found in the token.");
                return result;
            }

            if (!string.Equals(GetClaimValue(identity, "appid"), O365OpenIdConfiguration.AppId, StringComparison.OrdinalIgnoreCase))
            {
                Trace.TraceError(
                    "App ID does not match. Expected: {0} Actual: {1}",
                    O365OpenIdConfiguration.AppId,
                    GetClaimValue(identity, "appid"));
                result.Exception = new InvalidOperationException("App ID does not match.");
                return result;
            }

            result.ValidationSucceeded = true;
            result.Sender = GetClaimValue(identity, "sender");

            // Get the value of the "sub" claim. Passing in "sub" will not return a value because the TokenHandler
            // maps "sub" to ClaimTypes.NameIdentifier. More info here
            // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/415.
            result.ActionPerformer = GetClaimValue(identity, ClaimTypes.NameIdentifier);

            return result;
        }

        /// <summary>
        /// Gets the value of a claim type from the identity.
        /// </summary>
        /// <param name="identity">The identity to read the claim from.</param>
        /// <param name="claimType">The claim type.</param>
        /// <returns>The value of the claim if it exists; else is null.</returns>
        private static string GetClaimValue(ClaimsIdentity identity, string claimType)
        {
            var claim = identity.Claims.FirstOrDefault(c => c.Type == claimType);
            return claim?.Value ?? string.Empty;
        }
    }
}
