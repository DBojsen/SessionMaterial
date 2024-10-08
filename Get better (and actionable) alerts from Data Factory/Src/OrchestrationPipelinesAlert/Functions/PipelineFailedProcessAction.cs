using System.Net;
using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DBojsen.OrchestrationPipelinesAlert.Entities;
using DBojsen.OrchestrationPipelinesAlert.Templates;
using DBojsen.OrchestrationPipelinesAlert.Microsoft.DataFactory;
using Azure;
using DBojsen.OrchestrationPipelinesAlert.Microsoft.ActionableMessages.Utilities;
using DBojsen.OrchestrationPipelinesAlert.Microsoft.Storage;

namespace DBojsen.OrchestrationPipelinesAlert.Functions
{
    public class PipelineFailedProcessAction(ILogger<PipelineFailedProcessAction> logger, IPipelineRuns pipelineRuns, IStorageConnector storageConnector)
    {
        private readonly TemplateCompiler _templateCompiler = new();

        [Function("PipelineFailedProcessAction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            #region Validate request
            // Request has to be anonymous to be able to be called from an adaptive card, so we must verify it here (see https://github.com/OfficeDev/outlook-actionable-messages-csharp-token-validation)
            // Validate that we have a bearer token.
            if (!req.Headers.Contains("Authorization")) return req.CreateResponse(HttpStatusCode.Forbidden);

            var auth = req.Headers.GetValues("Authorization").ToList();
            if (auth.Count != 1) return req.CreateResponse(HttpStatusCode.Forbidden);

            var authHeader = AuthenticationHeaderValue.Parse(auth.First());
            if (!string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)) return req.CreateResponse(HttpStatusCode.Forbidden);

            var bearerToken = authHeader.Parameter;
            if (string.IsNullOrWhiteSpace(bearerToken)) return req.CreateResponse(HttpStatusCode.Forbidden);

            var validator = new ActionableMessageTokenValidator();

            // This will validate that the token has been issued by Microsoft for the
            // specified target URL i.e. the target matches the intended audience (�aud� claim in token)
            var validationResult = await validator.ValidateTokenAsync(bearerToken, $"https://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}");

            if (!validationResult.ValidationSucceeded)
            {
                // Log the validation result.
                logger.LogError($"Token validation failed: {validationResult.Exception.Message}");
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            // We have a valid token. We will now verify that the sender is who we expect. 
            if (validationResult.Sender != Environment.GetEnvironmentVariable("MicrosoftGraph_SendMailSenderEmail")) return req.CreateResponse(HttpStatusCode.Forbidden);
            #endregion

            // Now that we've checked the authenticity of the request, we can proceed with the actual logic
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var pipelineAction = JsonConvert.DeserializeObject<PipelineAction>(requestBody) ?? throw new ArgumentNullException();

            var actionPipeline = pipelineRuns.GetPipelineRun(pipelineAction.DataFactoryInstanceId, pipelineAction.PipelineRunId);

            var alertStatusResponse = await storageConnector.TableClient.GetEntityAsync<PipelinesAlertTableData>(
                Environment.GetEnvironmentVariable("AzureStorageTables_TableName"),
                actionPipeline.PipelineRun.RunId.ToString());

            if (!alertStatusResponse.HasValue) throw new InvalidOperationException("Alert status not found in table storage");
            var alertStatus = alertStatusResponse.Value;

            actionPipeline.ShowButtons = false;
            actionPipeline.ActionUser = validationResult.ActionPerformer;
            alertStatus.ActionUser = validationResult.ActionPerformer;

            string cardActionStatus;

            switch (pipelineAction.Action)
            {
                case "Rerun":
                    actionPipeline.DecidedAction = "Rerun failed pipeline";
                    var rerunId = pipelineRuns.RerunFailedPipelineRun(actionPipeline);
                    cardActionStatus = $"Rerun started with id: {rerunId}";
                    break;
                case "Escalate":
                    actionPipeline.DecidedAction = "Escalate to BI Ops";
                    cardActionStatus = "This has yet to be implemented, sorry!";
                    break;
                default:
                    actionPipeline.DecidedAction = "Create task in Azure DevOps";
                    cardActionStatus = "This has yet to be implemented, sorry!";
                    break;
            }

            alertStatus.ChosenAction = actionPipeline.DecidedAction;
            var updateResponse = await storageConnector.TableClient.UpdateEntityAsync(alertStatus, ETag.All);
            if (updateResponse.Status != 204) throw new InvalidOperationException("Failed to update alert status in table storage");

            var cardPayload = _templateCompiler.CompileAdaptiveCardPayload(actionPipeline);
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.Headers.Add("CARD-ACTION-STATUS", cardActionStatus);
            response.Headers.Add("CARD-UPDATE-IN-BODY", "true");
            await response.WriteStringAsync(cardPayload);

            return response;
        }
    }
}
