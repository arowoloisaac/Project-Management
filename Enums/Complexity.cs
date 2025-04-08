using System.Text.Json.Serialization;

namespace Task_Management_System.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Complexity
    {
        Easy,
        Medium,
        Complex
    }
}
