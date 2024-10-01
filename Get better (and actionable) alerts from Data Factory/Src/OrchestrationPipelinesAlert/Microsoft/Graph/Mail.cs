using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace OrchestrationPipelinesAlert.Microsoft.Graph
{
    internal static class Mail
    {
        public static async Task SendMail(string mailSubject, string mailBody, string receiverEmail, bool saveInSentMail, ILogger log)
        {
            try
            {
                // Authenticate
                var cred = new ClientSecretCredential(
                    Environment.GetEnvironmentVariable("MicrosoftGraph_SendMailTenantId"),
                    Environment.GetEnvironmentVariable("MicrosoftGraph_SendMailClientId"),
                    Environment.GetEnvironmentVariable("MicrosoftGraph_SendMailClientSecret"),
                    new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud }
                );

                // Send thru Microsoft Graph
                var graphServiceClient = new GraphServiceClient(cred);

                // Get variables

                var senderOid = Environment.GetEnvironmentVariable("MicrosoftGraph_SendMailSenderObjectId");

                // Define e-mail message.
                var message = new Message
                {
                    Subject = mailSubject,
                    Body = new ItemBody()
                    {
                        ContentType = BodyType.Html,
                        Content = mailBody
                    },
                    ToRecipients = [new Recipient { EmailAddress = new EmailAddress { Address = receiverEmail } }]
                };

                // Send mail as the given user.
                var mailMessage = new SendMailPostRequestBody() { Message = message, SaveToSentItems = saveInSentMail };
                await graphServiceClient.Users[senderOid!].SendMail.PostAsync(mailMessage);
            }
            catch (Exception ex)
            {
                log.LogError($"Error happened while attempting to send mail: {ex.Message}");
                log.LogInformation(ex.StackTrace);
            }
        }
    }
}
