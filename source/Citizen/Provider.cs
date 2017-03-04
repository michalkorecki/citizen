using Citizen.Statistics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Citizen
{
	public static class Provider
	{
		public static async Task<BuildStatistics[]> GetStatistics(string teamCityHost, string authenticationHeader)
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authenticationHeader);
			var buildSource = new BuildSource(client, teamCityHost);
			var builds = await buildSource.GetBuilds();
			var statisticsFactory = new BuildStatisticsFactory();
			return statisticsFactory
				.CreateStatistics(builds, "SUCCESS")
				.ToArray();
		}
	}
}