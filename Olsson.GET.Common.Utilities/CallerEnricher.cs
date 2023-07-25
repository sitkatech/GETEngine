using System.Diagnostics;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Olsson.GET.Common.Utilities;

class CallerEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var skip = 3;
        while (true)
        {
            var stack = new StackFrame(skip, true);

            // add FileName
            var fileName = stack.GetFileName();
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty("FileName", new ScalarValue(fileName)));
            }
            
            // attempt to add fileLine
            logEvent.AddPropertyIfAbsent(new LogEventProperty("LineNumber", new ScalarValue(stack.GetFileLineNumber())));
            
            // add
            if (!stack.HasMethod())
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty("MethodName", new ScalarValue("<unknown method>")));
                return;
            }
                
            var method = stack.GetMethod();
            if (method.DeclaringType.Assembly != typeof(Log).Assembly)
            {
                var caller = $"{method.DeclaringType.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.FullName))})";
                logEvent.AddPropertyIfAbsent(new LogEventProperty("MethodName", new ScalarValue(caller)));
                return;
            }

            skip++;
        }
    }
}