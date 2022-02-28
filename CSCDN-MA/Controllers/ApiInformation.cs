using System;

namespace CSCDNMA.Controllers
{
    internal class ApiInformation
    {
        public ApiInformation(DateTime startTime)
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
            StartTime = DateTime.Parse(appProcess.StartTime.ToString("yyyy'.'MM'.'dd'T'HH':'mm':'ss"));
            Id = Globals.Config.AppGuid;
            CompileTime = Builtin.CompileTime;
            var ver = new Version(CzomPack.Settings.Application.Version);
            Version = $"{ver.ToString(3)}-build{ver.Revision:00}";
        }

        public Guid Id { get; internal set; }
        public string Type { get; internal set; }
        public string Version { get; internal set; }
        public DateTime CompileTime { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public Guid Build { get; internal set; }
    }
}