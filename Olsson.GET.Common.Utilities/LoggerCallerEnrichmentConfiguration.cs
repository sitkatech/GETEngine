using Serilog;
using Serilog.Configuration;

namespace Olsson.GET.Common.Utilities;

static class LoggerCallerEnrichmentConfiguration
{
    public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<CallerEnricher>();
    }
}