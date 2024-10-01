using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchestrationPipelinesAlert.Microsoft.ActionableMessages.Utilities
{
    // Borrowed from https://github.com/OfficeDev/o365-actionable-messages-utilities-for-dotnet/blob/master/src/Microsoft.O365.ActionableMessages.Utilities/ActionableMessageTokenValidationResult.cs
    internal class ActionableMessageTokenValidationResult
    {
        /// <summary>
        /// Gets or sets the flag to indicate if a token validation operation succeeded.
        /// </summary>
        public bool ValidationSucceeded { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person who performed the action. In some cases,
        /// it will be the hash of the email address.
        /// </summary>
        public string ActionPerformer { get; set; }

        /// <summary>
        /// Gets or sets the email address of the sender of the actionable message.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the exception happened during the token validation.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Constructor of the <see cref="ActionableMessageTokenValidationResult"/> class.
        /// </summary>
        public ActionableMessageTokenValidationResult()
        {
            this.ValidationSucceeded = false;
            this.Exception = null;
        }
    }
}
