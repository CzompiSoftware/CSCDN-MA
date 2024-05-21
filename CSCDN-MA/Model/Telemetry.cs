namespace CSCDNMA.Model;
public class Telemetry
{
    public TelemetryProvider Provider { get; set; }
    public string Host { get; set; }
    public string Token { get; set; } = null;
}
