namespace CSCDNMA.Model;

internal class ApiInformation
{
#nullable enable
	public ApiInformation(DateTime startTime, string? id = null)
#nullable disable
	{
#if STAGING
        Type = "STAGING";
#elif BETA
		Type = "BETA";
#elif RELEASE
        Type = "RELEASE";
#else
        Type = "INVLAID";
#endif
		Build = Builtin.BuildId;
		StartTime = DateTime.Parse(startTime.ToString("yyyy'.'MM'.'dd'T'HH':'mm':'ss"));
		Id = id is not null ? Guid.Parse(id) : Guid.NewGuid(); // Use app param as guid or create a new one when not set. 
		CompileTime = Builtin.CompileTime;
		var ver = CzomPack.Settings.Application.Assembly.GetName().Version;
		Version = $"{ver.ToString(3)}-build{ver.Revision:00}";
	}
	public Guid Id { get; internal set; }
	public string Type { get; internal set; }
	public string Version { get; internal set; }
	public DateTime CompileTime { get; internal set; }
	public DateTime StartTime { get; internal set; }
	public Guid Build { get; internal set; }
}