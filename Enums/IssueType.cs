using System.Text.Json.Serialization;

namespace Task_Management_System.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IssueType
    {
        Task,
        Bug,
        Documentation,
        Feature,
        Improvement,
        Incident,
        Research
    }
}
