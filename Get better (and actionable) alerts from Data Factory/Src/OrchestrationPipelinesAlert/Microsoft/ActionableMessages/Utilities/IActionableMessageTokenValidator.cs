using System.Threading.Tasks;

namespace OrchestrationPipelinesAlert.Microsoft.ActionableMessages.Utilities
{
    // Borrowed from https://github.com/OfficeDev/o365-actionable-messages-utilities-for-dotnet/blob/master/src/Microsoft.O365.ActionableMessages.Utilities/IActionableMessageTokenValidator.cs
    internal interface IActionableMessageTokenValidator
    {
        /// <summary>
        /// Validates the token with the given target service base URL.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <param name="targetServiceBaseUrl">The expected target service base URL.</param>
        /// <returns>The result of the validation.</returns>
        Task<ActionableMessageTokenValidationResult> ValidateTokenAsync(string token, string targetServiceBaseUrl);
    }
}