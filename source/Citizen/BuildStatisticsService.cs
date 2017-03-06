using Citizen.Statistics;
using System.Linq;
using System.Threading.Tasks;

namespace Citizen
{
	public class BuildStatisticsService
	{
		private readonly BuildSource buildSource;
		private readonly BuildStatisticsGenerator generator;

		public BuildStatisticsService(BuildSource buildSource, BuildStatisticsGenerator generator)
		{
			this.buildSource = buildSource;
		}

		public async Task<BuildStatistics[]> GetOverviewStatistics()
		{
			var builds = await this.buildSource.GetBuilds();
			return this.generator
				.CreateOverviewStatistics(builds)
				.ToArray();
		}

		public async Task<DurationAtDate[]> GetRunTimeStatistics(string buildTypeId)
		{
			var builds = await this.buildSource.GetBuildsByType(buildTypeId);
			return this.generator
				.CreateBuildRunTimeStatistics(builds)
				.ToArray();
		}

		public async Task<DurationAtDate[]> GetLagTimeStatistics(string buildTypeId)
		{
			var builds = await this.buildSource.GetBuildsByType(buildTypeId);
			return this.generator
				.CreateBuildLagTimeStatistics(builds)
				.ToArray();
		}
	}
}