using System.Text.Json;
using System.Text.Json.Serialization;

namespace Synthesis.Registry.MutagenScraper;

public class JsonSerializerOptionsProvider
{
    public readonly JsonSerializerOptions Options  = new()
    {
        WriteIndented = true
    };

    public JsonSerializerOptionsProvider()
    {
        Options.Converters.Add(new JsonStringEnumConverter());
        Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
}