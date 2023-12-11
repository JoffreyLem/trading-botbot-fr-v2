using System.Text.Json;
using Serilog.Core;
using Serilog.Events;

namespace StrategyApi.StrategyBackgroundService;

public class JsonPrettyPrintEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var property in logEvent.Properties)
            if (property.Value is ScalarValue scalarValue && scalarValue.Value is string stringValue)
                try
                {
                    // Tenter de parser la valeur en tant que JSON
                    using var jsonDoc = JsonDocument.Parse(stringValue);
                    var formattedJson = Environment.NewLine +
                                        JsonSerializer.Serialize(jsonDoc,
                                            new JsonSerializerOptions { WriteIndented = true });

                    // Si le parsing réussit, mettre à jour la propriété avec la version formatée
                    logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(property.Key,
                        new ScalarValue(formattedJson)));
                }
                catch (JsonException)
                {
                    // Ignorer si la valeur n'est pas un JSON valide
                }
    }
}