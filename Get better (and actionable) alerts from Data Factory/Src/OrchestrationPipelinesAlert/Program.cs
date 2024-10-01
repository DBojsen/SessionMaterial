using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchestrationPipelinesAlert.Microsoft.DataFactory;
using OrchestrationPipelinesAlert.Microsoft.Storage;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<IPipelineRuns, PipelineRuns>();
        services.AddSingleton<IStorageConnector, StorageConnector>();
    })
    .Build();

host.Run();
