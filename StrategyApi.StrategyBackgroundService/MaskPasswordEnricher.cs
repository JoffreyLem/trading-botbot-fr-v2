using System.Text;
using System.Text.Json;
using Serilog.Core;
using Serilog.Events;
using JsonException = Newtonsoft.Json.JsonException;

namespace StrategyApi.StrategyBackgroundService;

public class XtbMaskPasswordEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("Tcp", out var tcp) && tcp is StructureValue tcpStructure)
        {
            var propertyValues = tcpStructure.Properties.ToDictionary(p => p.Name, p => p.Value);

            if (propertyValues.TryGetValue("RequestMessage", out var requestMessageValue) &&
                requestMessageValue is ScalarValue scalarValue)
            {
                var requestMessage = scalarValue.Value.ToString();
                var maskedRequestMessage = MaskPasswordInRequestMessage(requestMessage);

                propertyValues["RequestMessage"] = new ScalarValue(maskedRequestMessage);
            }

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Tcp",
                new StructureValue(propertyValues.Select(kvp => new LogEventProperty(kvp.Key, kvp.Value)))));
        }
    }


    private string MaskPasswordInRequestMessage(string requestMessage)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(requestMessage);
            var rootElement = jsonDoc.RootElement;

            if (rootElement.TryGetProperty("arguments", out var argumentsElement) &&
                argumentsElement.TryGetProperty("password", out var _))
            {
                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream))
                {
                    MaskPassword(writer, rootElement);
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        catch (JsonException)
        {
            // Return the original message if it's not valid JSON
        }

        return requestMessage;
    }

    private void MaskPassword(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);
                    if (property.Name == "password")
                        writer.WriteStringValue("****");
                    else
                        MaskPassword(writer, property.Value);
                }

                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray()) MaskPassword(writer, item);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }
}