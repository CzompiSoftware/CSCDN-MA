using System.Text.Json.Serialization;

namespace CSCDNMA.Model
{
    public struct ErrorResult
    {
        public string Error { get; internal set; }
        public string ErrorMessage { get; internal set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Cause { get; internal set; }
    }
}