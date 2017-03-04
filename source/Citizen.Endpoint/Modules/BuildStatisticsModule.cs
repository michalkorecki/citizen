using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using Nancy;

namespace Citizen.Endpoint.Modules
{
	public class BuildStatisticsModule : NancyModule
	{
		public BuildStatisticsModule()
		{
			Get["/builds", true] = async (parameters, context) => await GetBuilds();
		}

		private async Task<Response> GetBuilds()
		{
			var teamCityHost = ConfigurationManager.AppSettings["TeamCityHost"];
			var authenticationHeader = ConfigurationManager.AppSettings["TeamCityAuthenticationHeader"];
			var statistics = await Provider.GetStatistics(teamCityHost, authenticationHeader);
			var results = statistics
				.Select(s => new
				{
					s.AverageLagTime,
					s.AverageRunTime,
					s.BuildTypeName,
					s.BuildCount,
					s.LastBuildQueuedAt,
					s.LastBuildLagTime,
					s.LastBuildRunTime,
					LastBuildUrl = $"{teamCityHost}/viewLog.html?buildId={s.LastBuildId}",
				})
				.ToArray();

			return Response.AsJson(results);
		}
	}
}