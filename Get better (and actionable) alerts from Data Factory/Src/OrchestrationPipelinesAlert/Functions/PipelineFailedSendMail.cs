using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DBojsen.OrchestrationPipelinesAlert.Microsoft.DataFactory;
using DBojsen.OrchestrationPipelinesAlert.Microsoft.Graph;
using DBojsen.OrchestrationPipelinesAlert.Templates;

namespace DBojsen.OrchestrationPipelinesAlert.Functions
{
    public class PipelineFailedSendMail(ILogger<PipelineFailedSendMail> logger, IPipelineRuns pipelineRuns)
    {
        private readonly string _mailReceivers = Environment.GetEnvironmentVariable("MicrosoftGraph_SendMailReceiverEmails") ?? throw new InvalidOperationException();
        private readonly TemplateCompiler _templateCompiler = new();

        [Function("PipelineFailedSendMail")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            try
            {
                logger.LogInformation("C# HTTP trigger function processed a request.");
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic body = JsonConvert.DeserializeObject(requestBody)!;

                var alertTargetId = body.data.essentials.alertTargetIDs[0].Value as string ?? throw new InvalidOperationException();
                var windowStartTime = (DateTime)body.data.alertContext.condition.windowStartTime.Value;
                var windowEndTime = (DateTime)body.data.alertContext.condition.windowEndTime.Value;

                var failedPipelineRuns =
                    pipelineRuns.GetFailedPipelineRuns(alertTargetId, windowStartTime, windowEndTime);

                foreach (var pipelineRunFailed in failedPipelineRuns)
                {
                    var mailBody = _templateCompiler.CompileMailBody(pipelineRunFailed);
                    var mailSubject = _templateCompiler.CompileMailSubject(pipelineRunFailed);

                    await Mail.SendMail(mailSubject, mailBody, _mailReceivers, true, logger);
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
