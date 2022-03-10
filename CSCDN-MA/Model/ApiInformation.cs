using System;

namespace CSCDNMA.Model
{
    internal class ApiInformation
    {
        public ApiInformation(Guid appGuid, DateTime startTime)
        {
            Id = appGuid;
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
}