using System;
using System.Collections.Generic;
using System.Linq;

namespace Citizen.Statistics
{
	public class BuildStatisticsGenerator
	{
		public IEnumerable<BuildStatistics> CreateOverviewStatistics(IEnumerable<Build> builds)
		{
			var buildsByBuildType = builds.Where(b => b.Status == "SUCCESS").GroupBy(b => b.BuildTypeId);
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

		public IEnumerable<DurationAtDate> CreateBuildRunTimeStatistics(IEnumerable<Build> builds) =>
			CreateDurationStatistics(builds, b => b.Finished - b.Started);

		public IEnumerable<DurationAtDate> CreateBuildLagTimeStatistics(IEnumerable<Build> builds) =>
			CreateDurationStatistics(builds, b => b.Started - b.Queued);

		public IEnumerable<DurationAtDate> CreateDurationStatistics(IEnumerable<Build> builds, Func<Build, TimeSpan> selector)
		{
			var buildsFiltered = builds
				.Where(b => b.Status == "SUCCESS")
				.OrderBy(b => b.Queued)
				.ToArray();
			var averageLagTime = MovingAverage.Compute(buildsFiltered.Select(selector).ToArray());
			for (var i = 0; i < averageLagTime.Length; i++)
			{
				yield return new DurationAtDate
				{
					Date = buildsFiltered[i].Queued,
					Duration = averageLagTime[i]
				};
			}
		}
	}
}