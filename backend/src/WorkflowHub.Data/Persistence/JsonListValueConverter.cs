using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WorkflowHub.Data.Persistence;

internal sealed class JsonListValueConverter<T> : ValueConverter<List<T>, string>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public JsonListValueConverter()
        : base(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<List<T>>(v, JsonOptions) ?? new List<T>())
    {
    }
}
