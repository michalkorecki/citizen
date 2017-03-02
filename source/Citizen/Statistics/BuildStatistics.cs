using System;

namespace Citizen.Statistics
{
	public class BuildStatistics
	{
		public string BuildTypeName { get; set; }
		public int BuildCounts { get; set; }
		public TimeSpan AverageRunTime { get; set; }
		public TimeSpan AverageLagTime { get; set; }
	}
}
