using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Citizen
{
    class Program
    {
        static void Main(string[] args) => ListBuilds().Wait();

        private static async Task ListBuilds()
        {
            var teamCityHost = Parameters.GetTeamCityHost();
            var authenticationHeader = Parameters.GetAuthenticationHeader();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authenticationHeader);
            var buildSource = new BuildSource(client, teamCityHost);
            var builds = await buildSource.GetBuilds();
            
            foreach (var build in builds)
            {
                Console.WriteLine($"Build {build.Id} started {build.Started}, status {build.Status}");
            }

            var statisticsFactory = new BuildStatisticsFactory();
            var successStatistics = statisticsFactory.CreateStatistics(builds, "SUCCESS");
            Console.WriteLine("SUCCESS");
            PrintBuildStatistics(successStatistics);
            var failureStatistics = statisticsFactory.CreateStatistics(builds, "FAILURE");
            Console.WriteLine("FAILURE");
            PrintBuildStatistics(failureStatistics);
        }

        private static void PrintBuildStatistics(IEnumerable<Tuple<string, int, double, double>> statistics)
        {
            foreach (var stat in statistics.OrderByDescending(s => s.Item2))
            {
                Console.Write($"#{stat.Item2:D3}");
                Console.Write($" lag {TimeSpan.FromSeconds(stat.Item3):hh\\:mm\\:ss}");
                Console.Write($" run {TimeSpan.FromSeconds(stat.Item4):hh\\:mm\\:ss}");
                Console.WriteLine($" ({stat.Item1})");
            }
        }
    }
}