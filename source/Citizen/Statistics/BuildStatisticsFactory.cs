using System.Collections.Generic;
using System.Linq;

namespace Citizen.Statistics
{
	public class BuildStatisticsFactory
	{
		public IEnumerable<BuildStatistics> CreateStatistics(IEnumerable<Build> builds, string status)
		{
			var buildsByBuildType = builds.Where(b => b.Status == status).GroupBy(b => b.BuildTypeId);
			foreach (var group in buildsByBuildType)
			{
				var buildsMostRecentLast = group.OrderBy(b => b.Queued).ToArray();
				var lastBuild = buildsMostRecentLast.Last();
				var averageLagTime = MovingAverage.Compute(buildsMostRecentLast.Select(b => b.Started - b.Queued).ToArray()).Last();
				var averageRunTime = MovingAverage.Compute(buildsMostRecentLast.Select(b => b.Finished - b.Started).ToArray()).Last();
				yield return new BuildStatistics
				{
                    BuildTypeId = group.Key,
					AverageLagTime = averageLagTime,
					AverageRunTime = averageRunTime,
					BuildCount = group.Count(),
					BuildTypeName = group.First().BuildTypeName,
					LastBuildId = lastBuild.Id,
					LastBuildQueuedAt = lastBuild.Queued,
					LastBuildLagTime = lastBuild.Started - lastBuild.Queued,
					LastBuildRunTime = lastBuild.Finished - lastBuild.Started,
				};
			}
		}
	}
}