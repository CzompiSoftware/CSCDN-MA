namespace CzompiSoftwareCDN.Model
{
    internal class ApiInformation
    {
        public Guid Id { get; internal set; }
        public string Type { get; internal set; }
        public string Version { get; internal set; }
        public DateTime CompileTime { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public Guid Build { get; internal set; }
    }
}