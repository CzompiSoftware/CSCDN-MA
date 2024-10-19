using CzomPack.Cryptography;
using System.Xml.Linq;

namespace CSCDNMA.Model;

internal class ApiInformation
{
#nullable enable
	public ApiInformation(DateTime startTime, string? node = null)
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
		Id = node is not null ? SHA1.Encode(node) : SHA1.Encode(Guid.NewGuid().ToString());
		Node = node ?? "Standalone";
		CompileTime = Builtin.CompileTime;
		var ver = CzomPack.Settings.Application.Assembly.GetName().Version;
		Version = $"{ver.ToString(3)}-build{ver.Revision:00}";
	}
	public string Id { get; internal set; }
	public string Node { get; internal set; }
	public string Type { get; internal set; }
	public string Version { get; internal set; }
	public DateTime CompileTime { get; internal set; }
	public DateTime StartTime { get; internal set; }
	public Guid Build { get; internal set; }
}