using System.Text.Json.Serialization;

namespace Core.Models
{
    public class Step
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string StepName { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string StepError { get; set; } = string.Empty;
    }
}
