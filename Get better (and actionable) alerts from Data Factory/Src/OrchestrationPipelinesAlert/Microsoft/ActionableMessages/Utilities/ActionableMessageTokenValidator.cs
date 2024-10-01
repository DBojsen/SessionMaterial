using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OrchestrationPipelinesAlert.Microsoft.ActionableMessages.Utilities
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
        private readonly IConfigurationManager<OpenIdConnectConfiguration> configurationManager;

        /// <summary>
        /// Constructor of the <see cref="ActionableMessageTokenValidator"/> class.
        /// </summary>
        public ActionableMessageTokenValidator()
        {
            this.configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
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
            this.configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
        }

        /// <inheritdoc />
        public async Task<ActionableMessageTokenValidationResult> ValidateTokenAsync(
            string token,
            string targetServiceBaseUrl)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("token is null or empty.", "token");
            }

            if (string.IsNullOrEmpty(targetServiceBaseUrl))
            {
                throw new ArgumentException("url is null or empty.", "targetServiceBaseUrl");
            }

            CancellationToken cancellationToken = default;
            var o365OpenIdConfig = await configurationManager.GetConfigurationAsync(cancellationToken);
            ClaimsPrincipal claimsPrincipal;
            var result = new ActionableMessageTokenValidationResult();

            var parameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { O365OpenIdConfiguration.TokenIssuer },
                ValidateAudience = true,
                ValidAudiences = new[] { targetServiceBaseUrl },
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(TokenTimeValidationClockSkewBufferInMinutes),
                RequireSignedTokens = true,
                IssuerSigningKeys = o365OpenIdConfig.SigningKeys,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null;

            try
            {
                // This will validate the token's lifetime and the following claims:
                // 
                // iss
                // aud
                //
                claimsPrincipal = tokenHandler.ValidateToken(token, parameters, out validatedToken);
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

            var identity = claimsPrincipal.Identities.OfType<ClaimsIdentity>().FirstOrDefault();
            if (identity == null)
            {
                Trace.TraceError("Claims not found in the token.");
                result.Exception = new InvalidOperationException("Claims not found in the token.");
                return null;
            }

            if (!string.Equals(GetClaimValue(identity, "appid"), O365OpenIdConfiguration.AppId, StringComparison.OrdinalIgnoreCase))
            {
                Trace.TraceError(
                    "App ID does not match. Expected: {0} Actual: {1}",
                    O365OpenIdConfiguration.AppId,
                    GetClaimValue(identity, "appid"));
                return null;
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
            return claim?.Value;
        }
    }
}
