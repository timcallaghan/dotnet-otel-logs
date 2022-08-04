using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace WebHost.Telemetry.Logging;

public class ActivityEventWriterSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        Activity.Current?.AddEvent(
            new ActivityEvent(name: logEvent.RenderMessage(), tags: GetTagsFromProperties(logEvent)));
    }

    private ActivityTagsCollection GetTagsFromProperties(LogEvent logEvent)
    {
        const string attributeExceptionType = "exception.type";
        const string attributeExceptionMessage = "exception.message";
        const string attributeExceptionStacktrace = "exception.stacktrace";

        ActivityTagsCollection? exceptionTags = null;
        if (logEvent.Exception != null)
        {
            exceptionTags = new ActivityTagsCollection
            {
                {attributeExceptionType, logEvent.Exception.GetType().FullName},
                {attributeExceptionStacktrace, logEvent.Exception.ToString()},
            };

            if (!string.IsNullOrWhiteSpace(logEvent.Exception.Message))
            {
                exceptionTags.Add(attributeExceptionMessage, logEvent.Exception.Message);
            }
        }

        var items = new Dictionary<string, object> {
            {nameof(logEvent.Level), logEvent.Level},
            {nameof(logEvent.MessageTemplate), logEvent.MessageTemplate},
        }.Concat(logEvent.Properties
            .Select(lep =>
                new KeyValuePair<string, object>(lep.Key, GetValueFromLogEventProperty(lep.Value))));

        if (exceptionTags != null && exceptionTags.Count > 0)
        {
            items = items.Concat(exceptionTags!);
        }

        return new ActivityTagsCollection(items!);
    }

    private object GetValueFromLogEventProperty(LogEventPropertyValue value)
    {
        switch (value)
        {
            case ScalarValue scalarValue:
                return scalarValue.Value;
            default:
                return value.ToString();
        }
    }
}