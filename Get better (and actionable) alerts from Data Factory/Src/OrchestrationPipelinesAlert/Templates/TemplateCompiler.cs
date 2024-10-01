using HandlebarsDotNet;
using OrchestrationPipelinesAlert.Entities;

namespace OrchestrationPipelinesAlert.Templates
{
    internal class TemplateCompiler
    {
        internal readonly HandlebarsTemplate<object, object> MailBodyTemplate;
        internal readonly HandlebarsTemplate<object, object> MailSubjectTemplate;
        internal readonly HandlebarsTemplate<object, object> AdaptiveCardBodyTemplate;
        internal readonly HandlebarsTemplate<object, object> AdaptiveCardSubjectTemplate;
        internal readonly string AdaptiveCardBodyHtmlWrapper;

        internal readonly string ActionableMessageOriginator = Environment.GetEnvironmentVariable("ActionableMessageOriginator") ?? throw new InvalidOperationException();
        internal readonly string AzureFunctionsRootUrl = $"https://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}/api" ?? throw new InvalidOperationException();
        public TemplateCompiler()
        {
            var rawMailBodyTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Templates", "Mail", "ErrorMailBody.html"));
            var rawMailSubjectTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Templates", "Mail", "ErrorMailSubject.txt"));
            
            var rawAdaptiveCardBodyTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Templates", "Adaptive Cards", "PipelineFailedAdaptiveCard.json"));
            var rawAdaptiveCardSubjectTemplate = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Templates", "Adaptive Cards", "ErrorMailSubject.txt"));
            AdaptiveCardBodyHtmlWrapper = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Templates", "Adaptive Cards", "ErrorMailBody.html"));

            Handlebars.RegisterHelper("IsOdd", (output, options, context, arguments) =>
            {
                if (arguments.Length != 1)
                {
                    throw new HandlebarsException("{{#IsOdd}} helper must have exactly one arguments");
                }

                var index = arguments.At<int>(0);
                if (index % 2 == 0)
                    options.Template(output, context);
                else
                    options.Inverse(output, context);
            });

            Handlebars.RegisterHelper("IsEvenAdaptiveCard", (output, options, context, arguments) =>
            {
                if (arguments.Length != 1)
                {
                    throw new HandlebarsException("{{#IsEvenAdaptiveCard}} helper must have exactly one arguments");
                }

                var index = arguments.At<int>(0);
                if (index % 2 == 0)
                    output.WriteSafeString("emphasis");
                else
                    output.WriteSafeString("default");
            });

            Handlebars.RegisterHelper("NewGuid", (output, context, arguments) =>
            {
                output.WriteSafeString(Guid.NewGuid().ToString());
            });

            Handlebars.RegisterHelper("ActionableMessageOriginator", (output, context, arguments) =>
            {
                output.WriteSafeString(ActionableMessageOriginator);
            });

            Handlebars.RegisterHelper("AzureFunctionsRootUrl", (output, context, arguments) =>
            {
                output.WriteSafeString(AzureFunctionsRootUrl);
            });

            MailBodyTemplate = Handlebars.Compile(rawMailBodyTemplate);
            MailSubjectTemplate = Handlebars.Compile(rawMailSubjectTemplate);
            AdaptiveCardBodyTemplate = Handlebars.Compile(rawAdaptiveCardBodyTemplate);
            AdaptiveCardSubjectTemplate = Handlebars.Compile(rawAdaptiveCardSubjectTemplate);
        }
        internal string CompileMailBody(PipelineRunFailed pipelineRun)
        {
            return MailBodyTemplate(pipelineRun);
        }

        internal string CompileMailSubject(PipelineRunFailed pipelineRun)
        {
            return MailSubjectTemplate(pipelineRun);
        }

        internal string CompileAdaptiveCardPayload(PipelineRunFailed pipelineRun)
        {
            return AdaptiveCardBodyTemplate(pipelineRun);
        }
        internal string CompileAdaptiveCardBody(PipelineRunFailed pipelineRun)
        {
            var cardDefinition = CompileAdaptiveCardPayload(pipelineRun);
            return AdaptiveCardBodyHtmlWrapper.Replace("{{AdaptiveCardContent}}", cardDefinition);
        }

        internal string CompileAdaptiveCardSubject(PipelineRunFailed pipelineRun)
        {
            return AdaptiveCardSubjectTemplate(pipelineRun);
        }
    }
}
