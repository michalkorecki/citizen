using System;

namespace Citizen.Statistics
{
	public class BuildStatistics
	{
        public string BuildTypeId { get; set; }
		public string BuildTypeName { get; set; }
		public int BuildCount { get; set; }
		public TimeSpan AverageRunTime { get; set; }
		public TimeSpan AverageLagTime { get; set; }
		public DateTime LastBuildQueuedAt { get; set; }
		public TimeSpan LastBuildRunTime { get; set; }
		public TimeSpan LastBuildLagTime { get; set; }
		public string LastBuildId { get; set; }
	}
}
