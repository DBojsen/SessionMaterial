using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchestrationPipelinesAlert.Microsoft.ActionableMessages.Utilities
{
    // Borrowed from https://github.com/OfficeDev/o365-actionable-messages-utilities-for-dotnet/blob/master/src/Microsoft.O365.ActionableMessages.Utilities/O365OpenIdConfiguration.cs
    internal static class O365OpenIdConfiguration
    {
        /// <summary>
        /// The URL of the O365 OpenID Connect metadata endpoint.
        /// </summary>
        public const string MetadataUrl = "https://substrate.office.com/sts/common/.well-known/openid-configuration";

        /// <summary>
        /// The value of the "iss" claim in the token.
        /// </summary>
        public const string TokenIssuer = "https://substrate.office.com/sts/";

        /// <summary>
        /// The value of the "appid" claim in the token.
        /// </summary>
        public const string AppId = "48af08dc-f6d2-435f-b2a7-069abd99c086";

        /// <summary>
        /// The value of the "ver" claim in the token.
        /// </summary>
        public const string Version = "STI.ExternalAccessToken.V1";

        /// <summary>
        /// The type of token issued by O365.
        /// </summary>
        public const string TokenType = "JWT";

        /// <summary>
        /// The signing algorithm for JWT tokens.
        /// </summary>
        public const string JwtSigningAlgorithm = "RS256";

        /// <summary>
        /// The hash algorithm used to sign the JWT token.
        /// </summary>
        public const string HashAlgorithm = "SHA256";

        /// <summary>
        /// The value of the "appidacr" claim in the token.
        /// </summary>
        public const string AppAuthContextClassReference = "2";

        /// <summary>
        /// The value of the "acr" claim in the token.
        /// </summary>
        public const string AuthContextClassReference = "0";
    }
}
