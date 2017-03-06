using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Nancy;

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
            var buildSource = CreateBuildSource(teamCityHost);
            var buildStatisticsService = new BuildStatisticsService(buildSource); 
            var statistics = await buildStatisticsService.GetStatistics();
			var results = statistics
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
            var buildSource = CreateBuildSource(teamCityHost);
            var builds = await buildSource.GetBuildsByType(buildTypeId);
            var results = builds
                .Select(b => new
                {
                    Lag = (int) (b.Started - b.Queued).TotalSeconds,
                    Run = (int) (b.Finished - b.Started).TotalSeconds,
                })
                .ToArray();

            return Response.AsJson(results);
        }

	    private static BuildSource CreateBuildSource(string teamCityHost)
	    {
	        //todo[mk]: composition root + di
	        var authenticationHeader = ConfigurationManager.AppSettings["TeamCityAuthenticationHeader"];
	        var client = new HttpClient();
	        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authenticationHeader);
	        return new BuildSource(client, teamCityHost);
	    }

	    private string FormatTimeSpan(TimeSpan timeSpan) => timeSpan.ToString(@"hh\:mm\:ss");
	}
}