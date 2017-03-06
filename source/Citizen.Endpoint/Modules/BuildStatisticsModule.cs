using System;
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
					AverageLagTime = FormatTimeSpan(s.AverageLagTime),
					AverageRunTime = FormatTimeSpan(s.AverageRunTime),
					s.BuildTypeName,
					s.BuildCount,
					s.LastBuildQueuedAt,
					LastBuildLagTime = FormatTimeSpan(s.LastBuildLagTime),
					LastBuildRunTime = FormatTimeSpan(s.LastBuildRunTime),
					LastBuildUrl = $"{teamCityHost}/viewLog.html?buildId={s.LastBuildId}",
				})
				.ToArray();

			return Response.AsJson(results);
		}

		private string FormatTimeSpan(TimeSpan timeSpan) => timeSpan.ToString(@"hh\:mm\:ss");
	}
}