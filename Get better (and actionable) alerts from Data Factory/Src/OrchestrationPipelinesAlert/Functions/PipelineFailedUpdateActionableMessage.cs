using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchestrationPipelinesAlert.Entities;
using OrchestrationPipelinesAlert.Microsoft.ActionableMessages.Utilities;
using OrchestrationPipelinesAlert.Templates;
using System.Net;
using System.Net.Http.Headers;
using OrchestrationPipelinesAlert.Microsoft.DataFactory;
using OrchestrationPipelinesAlert.Microsoft.Storage;

namespace OrchestrationPipelinesAlert.Functions
{
    public class PipelineFailedUpdateActionableMessage(
        ILogger<PipelineFailedUpdateActionableMessage> logger,
        IPipelineRuns pipelineRuns,
        IStorageConnector storageConnector)
    {
        private readonly TemplateCompiler _templateCompiler = new TemplateCompiler();
        private readonly IPipelineRuns _pipelineRuns = pipelineRuns;

        [Function("PipelineFailedUpdateActionableMessage")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            #region Validate request
            // Request has to be anonymous to be able to be called from an adaptive card, so we must verify it here (see https://github.com/OfficeDev/outlook-actionable-messages-csharp-token-validation)
            // Validate that we have a bearer token.
            if (!req.Headers.Contains("Authorization")) return req.CreateResponse(HttpStatusCode.Forbidden);

            var auth = req.Headers.GetValues("Authorization");
            if (auth.Count() != 1) return req.CreateResponse(HttpStatusCode.Forbidden);

            var authHeader = AuthenticationHeaderValue.Parse(auth.First());
            if (!string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)) return req.CreateResponse(HttpStatusCode.Forbidden);

            var bearerToken = authHeader.Parameter;
            if (string.IsNullOrWhiteSpace(bearerToken)) return req.CreateResponse(HttpStatusCode.Forbidden);

            var validator = new ActionableMessageTokenValidator();

            // This will validate that the token has been issued by Microsoft for the
            // specified target URL i.e. the target matches the intended audience (“aud” claim in token)
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
            var refreshRequest = JsonConvert.DeserializeObject<AdaptiveCardRefreshRequest>(requestBody);

            var actionPipeline = pipelineRuns.GetPipelineRun(refreshRequest.DataFactoryInstanceId, refreshRequest.PipelineRunId);

            var alertStatusResponse = await storageConnector.TableClient.GetEntityAsync<PipelinesAlertTableData>(
                Environment.GetEnvironmentVariable("AzureStorageTables_TableName"),
                actionPipeline.PipelineRun.RunId.ToString());
            
            if (!alertStatusResponse.HasValue) throw new InvalidOperationException("Alert status not found in table storage");
            var alertStatus = alertStatusResponse.Value;

            if (string.IsNullOrEmpty(alertStatus.ChosenAction))
            {
                actionPipeline.ShowButtons = true;
            }
            else
            {
                actionPipeline.ShowButtons = false;
                actionPipeline.ActionUser = alertStatus.ActionUser;
                actionPipeline.DecidedAction = alertStatus.ChosenAction;
            }

            var cardPayload = _templateCompiler.CompileAdaptiveCardPayload(actionPipeline);
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.Headers.Add("CARD-UPDATE-IN-BODY", "true");
            await response.WriteStringAsync(cardPayload);

            return response;
        }
    }
}
