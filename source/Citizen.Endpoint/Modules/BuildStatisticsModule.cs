using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Nancy;
using Citizen.Statistics;

namespace Citizen.Endpoint.Modules
{
	public class BuildStatisticsModule : NancyModule
	{
		public BuildStatisticsModule()
		{
			Get["/builds", true] = async (parameters, context) => await GetBuilds();
			Get["/builds/{build_type_id}", true] = async (parameters, context) => await GetBuilds(parameters.build_type_id);
		}

		private async Task<Response> GetBuilds()
		{
			var teamCityHost = ConfigurationManager.AppSettings["TeamCityHost"];
			var buildStatisticsService = ComposeStatisticsService(teamCityHost);
			var statistics = await buildStatisticsService.GetOverviewStatistics();
			var results = statistics
				.OrderByDescending(b => b.BuildCount)
				.Select(s => new
				{
					s.BuildTypeId,
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

		private async Task<Response> GetBuilds(string buildTypeId)
		{
			var teamCityHost = ConfigurationManager.AppSettings["TeamCityHost"];
			var buildStatisticsService = ComposeStatisticsService(teamCityHost);
			var statistics = await buildStatisticsService.GetRunTimeStatistics(buildTypeId);
			var results = statistics
				.Select(b => new
				{
					b.Date,
					DurationInSeconds = (int) b.Duration.TotalSeconds,
				})
				.ToArray();

			return Response.AsJson(results);
		}

		private static BuildStatisticsService ComposeStatisticsService(string teamCityHost)
		{
			//todo[mk]: composition root + di
			var authenticationHeader = ConfigurationManager.AppSettings["TeamCityAuthenticationHeader"];
			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authenticationHeader);
			var source = new BuildSource(client, teamCityHost);
			var statisticsService = new BuildStatisticsService(source, new BuildStatisticsGenerator());

			return statisticsService;
		}

		private string FormatTimeSpan(TimeSpan timeSpan) => timeSpan.ToString(@"hh\:mm\:ss");
	}
}