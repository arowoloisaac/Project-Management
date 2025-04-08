using System.Text.Json.Serialization;

namespace Task_Management_System.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkComponent
    {
        Planning,      // Initial stage for outlining and preparing work
        Analysis,      // Understanding requirements or impact
        Design,        // System or UI design activities
        Development,   // Coding or implementation phase
        Testing,       // Creating or running tests
        Documentation, // Writing or updating documentation
        Deployment,    // Releasing changes to production
        Monitoring,
        Coding
    }
}
