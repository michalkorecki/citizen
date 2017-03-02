using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Citizen
{
    class Program
    {
        static void Main(string[] args) => ListProjectConfigurations().Wait();

        private static async Task ListProjectConfigurations()
        {
            var teamCityHost = Parameters.GetTeamCityHost();
            var authenticationHeader = Parameters.GetAuthenticationHeader();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authenticationHeader);

            var builds = await BuildSource.GetBuilds(client, teamCityHost);
            
            foreach (var build in builds)
            {
                Console.WriteLine($"Build {build.Id} started {build.Started}, status {build.Status}");
            }
        }
    }
}