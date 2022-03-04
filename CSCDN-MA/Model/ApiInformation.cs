using System;

namespace CSCDNMA.Model
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
            StartTime = DateTime.Parse(startTime.ToString("yyyy'.'MM'.'dd'T'HH':'mm':'ss"));
            Id = Guid.NewGuid(); // Use app param as guid or create a new one when not set. 
            CompileTime = Builtin.CompileTime;
            //var ver = CzomPack.Settings.Application.Assembly.GetName().Version;
            //Version = $"{ver.ToString(3)}-build{ver.Revision:00}";
        }

        public Guid Id { get; internal set; }
        public string Type { get; internal set; }
        public string Version { get; internal set; }
        public DateTime CompileTime { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public Guid Build { get; internal set; }
    }
}