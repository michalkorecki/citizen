using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;

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

            var projectsDocument = await FetchAsync(teamCityHost, "httpAuth/app/rest/projects", client);
            var projects = GetProjectsAsync(projectsDocument);

            foreach (var project in projects)
            {
                var projectDetailsDocument = await FetchAsync(teamCityHost, $"httpAuth/app/rest/projects/{project.Item1}", client);
                var buildTypes = GetBuildTypes(projectDetailsDocument);
                foreach (var buildType in buildTypes)
                {
                    var buildHistoryDocument = await FetchAsync(teamCityHost, $"httpAuth/app/rest/buildTypes/{buildType.Item1}/builds", client);
                    var builds = GetBuilds(buildHistoryDocument);

                    foreach (var build in builds)
                    {
                        var buildDetails = await FetchAsync(teamCityHost, $"httpAuth/app/rest/builds/{build}", client);
                        var buildInfo = $"{buildDetails.Root.Attribute("id").Value}:{buildDetails.Root.Attribute("status").Value}";
                        Console.WriteLine(buildInfo);
                    }
                }
            }
        }


        private static async Task<XDocument> FetchAsync(string host, string resource, HttpClient client)
        {
            var root = new Uri(host);
            var url = new Uri(root, resource);

            Console.WriteLine($"Fetching {url}");

            var responseStream = await client.GetStreamAsync(url);

            return XDocument.Load(responseStream);
        }

        private static IEnumerable<Tuple<string, string>> GetProjectsAsync(XDocument document) => document
            .Root
            .Elements("project")
            .Select(p => Tuple.Create(p.Attribute("id").Value, p.Attribute("name").Value));

        private static IEnumerable<Tuple<string, string>> GetBuildTypes(XDocument document) => document
            .Root
            .Element("buildTypes")
            .Elements("buildType")
            .Select(buildType => Tuple.Create(buildType.Attribute("id").Value, buildType.Attribute("name").Value));

        private static IEnumerable<string> GetBuilds(XDocument document) => document
            .Root
            .Elements("build")
            .Select(b => b.Attribute("id").Value);
    }
}