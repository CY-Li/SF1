using Serilog.Core;
using Serilog.Events;

namespace DotNetBackEndApi.Event
{
    public class LogEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue sourceContext))
            {
                if (sourceContext.ToString().Contains("Controllers"))
                {
                    var controllerName = sourceContext.ToString().Replace("\"", "").Split('.').Last().Replace("Controllers", "");
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("ControllerName", new ScalarValue(controllerName)));
                }
            }
        }
    }
}
