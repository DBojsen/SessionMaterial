namespace DBojsen.OrchestrationPipelinesAlert.Microsoft.ActionableMessages.Utilities
{
    // Borrowed from https://github.com/OfficeDev/o365-actionable-messages-utilities-for-dotnet/blob/master/src/Microsoft.O365.ActionableMessages.Utilities/ActionableMessageTokenValidationResult.cs
    internal class ActionableMessageTokenValidationResult
    {
        /// <summary>
        /// Gets or sets the flag to indicate if a token validation operation succeeded.
        /// </summary>
        public bool ValidationSucceeded { get; set; } = false;

        /// <summary>
        /// Gets or sets the email address of the person who performed the action. In some cases,
        /// it will be the hash of the email address.
        /// </summary>
        public string ActionPerformer { get; set; } = null!;

        /// <summary>
        /// Gets or sets the email address of the sender of the actionable message.
        /// </summary>
        public string Sender { get; set; } = null!;

        /// <summary>
        /// Gets or sets the exception happened during the token validation.
        /// </summary>
        public Exception Exception { get; set; } = null!;

        /// <summary>
        /// Constructor of the <see cref="ActionableMessageTokenValidationResult"/> class.
        /// </summary>
        public ActionableMessageTokenValidationResult() {}
    }
}
