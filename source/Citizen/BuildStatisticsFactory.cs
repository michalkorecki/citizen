using System;
using System.Collections.Generic;
using System.Linq;

namespace Citizen
{
    public class BuildStatisticsFactory
    {
        public IEnumerable<Tuple<string, int, double, double>> CreateStatistics(IEnumerable<Build> builds, string status)
        {
            var buildsByBuildType = builds.Where(b => b.Status == status).GroupBy(b => b.BuildTypeName);
            foreach (var group in buildsByBuildType)
            {
                var averageRunTime = group.Select(b => b.Finished - b.Started).Average(diff => diff.TotalSeconds);
                var averageLagTime = group.Select(b => b.Started - b.Queued).Average(diff => diff.TotalSeconds);
                yield return Tuple.Create(group.Key, group.Count(), averageLagTime, averageRunTime);
            }
        }
    }
}