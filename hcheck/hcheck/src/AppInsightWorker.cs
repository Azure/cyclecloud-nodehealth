using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace hcheck
{
    class AppInsightWorker
    {
        IServiceCollection services;
        IServiceProvider serviceProvider;
        ILogger<AppInsightWorker> logger;
        TelemetryClient telemetryClient;
        HttpClient httpClient;

        public AppInsightWorker(string instrumentationKey)
        {
            services = new ServiceCollection();

            // Being a regular console app, there is no appsettings.json or configuration providers enabled by default.
            // Hence instrumentation key/ connection string and any changes to default logging level must be specified here.
            services.AddLogging(loggingBuilder => loggingBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("Category", LogLevel.Information));
            services.AddApplicationInsightsTelemetryWorkerService(instrumentationKey);

            // To pass a connection string
            // - aiserviceoptions must be created
            // - set connectionstring on it
            // - pass it to AddApplicationInsightsTelemetryWorkerService()

            // Build ServiceProvider.
            serviceProvider = services.BuildServiceProvider();

            // Obtain logger instance from DI.
            logger = serviceProvider.GetRequiredService<ILogger<AppInsightWorker>>();

            // Obtain TelemetryClient instance from DI, for additional manual tracking or to flush.
            telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
           
            httpClient = new HttpClient();

        }

        public async Task Send(string message)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            // Replace with a name which makes sense for this operation.
            using (telemetryClient.StartOperation<RequestTelemetry>("operation"))
            {
                
                logger.LogError(message);
                telemetryClient.TrackEvent(message);
            }

            await Task.Delay(1000);

            // Explicitly call Flush() followed by sleep is required in Console Apps.
            // This is to ensure that even if application terminates, telemetry is sent to the back-end.
            telemetryClient.Flush();
            Task.Delay(5000).Wait();
        }
    }
}