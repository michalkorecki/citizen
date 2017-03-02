using System;

namespace Citizen
{
    public class Build
    {
        public string Id { get; set; }
        public string BuildTypeId { get; set; }
        public string BuildTypeName { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Status { get; set; }
        public string State { get; set; }
        public string StatusText { get; set; }
        public DateTime Queued { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
    }
}
