using Citizen.Statistics;
using System.Linq;
using System.Threading.Tasks;

namespace Citizen
{
    public class BuildStatisticsService
	{
	    private readonly BuildSource buildSource;

	    public BuildStatisticsService(BuildSource buildSource)
	    {
	        this.buildSource = buildSource;
	    }

		public async Task<BuildStatistics[]> GetStatistics()
		{
			var builds = await this.buildSource.GetBuilds();
			var statisticsFactory = new BuildStatisticsFactory();
			return statisticsFactory
				.CreateStatistics(builds, "SUCCESS")
				.ToArray();
		}
	}
}