using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchestrationPipelinesAlert.Microsoft.DataFactory;
using OrchestrationPipelinesAlert.Microsoft.Graph;
using OrchestrationPipelinesAlert.Templates;
using Azure;
using Azure.Data.Tables;
using OrchestrationPipelinesAlert.Entities;
using OrchestrationPipelinesAlert.Microsoft.Storage;

namespace OrchestrationPipelinesAlert.Functions
{
    public class PipelineFailedSendActionableMessage(ILogger<PipelineFailedSendActionableMessage> logger, IPipelineRuns pipelineRuns, IStorageConnector storageConnector)
    {
        private readonly string _mailReciever = Environment.GetEnvironmentVariable("MicrosoftGraph_SendMailRecieverEmail") ?? throw new InvalidOperationException();
        private readonly TemplateCompiler _templateCompiler = new TemplateCompiler();

        [Function("PipelineFailedSendActionableMessage")]
        [TableOutput("PipelineFailedAlerts", Connection = "AzureWebJobsStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            // TODO: List of recievers - reciever name + email included in action (so that it can be saved in the state who answered)

            try
            {
                
                logger.LogInformation("C# HTTP trigger function processed a request.");
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic body = JsonConvert.DeserializeObject(requestBody)!;

                var alertTargetId = body.data.essentials.alertTargetIDs[0].Value as string;
                var windowStartTime = (DateTime)body.data.alertContext.condition.windowStartTime.Value;
                var windowEndTime = (DateTime)body.data.alertContext.condition.windowEndTime.Value;

                var failedPipelineRuns = pipelineRuns.GetFailedPipelineRuns(alertTargetId ?? throw new InvalidOperationException(), windowStartTime, windowEndTime);
                
                foreach (var pipelineRunFailed in failedPipelineRuns)
                {
                    // Write the entity to the table
                    try
                    {
                        var tableEntryStatus = await storageConnector.TableClient.AddEntityAsync(
                            new PipelinesAlertTableData(pipelineRunFailed.PipelineRun.RunId.ToString() ?? throw new InvalidOperationException()));
                    }
                    catch (Exception e)
                    {
                        if (e is RequestFailedException { ErrorCode: "EntityAlreadyExists" })
                        {
                            // This should only happen during testing
                        }
                        else
                        {
                            throw;
                        }
                        
                    }

                    var mailBody = _templateCompiler.CompileAdaptiveCardBody(pipelineRunFailed);
                    var mailSubject = _templateCompiler.CompileAdaptiveCardSubject(pipelineRunFailed);
    
                    await Mail.SendMail(mailSubject, mailBody, _mailReciever, true, logger);
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception while processing Data Factory alert to send mail: {ex.Message}");
                logger.LogError(ex.StackTrace);
                return new BadRequestResult();
            }
        }
    }
}
