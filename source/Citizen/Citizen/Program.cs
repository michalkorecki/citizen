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

            var projectsDocument = FetchAsync(teamCityHost, "projects", client);
            var projects = await GetProjectsAsync(projectsDocument);

            foreach (var project in projects)
            {
                var buildsDocument = FetchAsync(teamCityHost, $"projects/{project.Item1}", client);
                var builds = await GetBuildsAsync(buildsDocument);
                foreach (var build in builds)
                {
                    var buildTypeDocument = FetchAsync(teamCityHost, $"buildTypes/{build.Item1}", client);
                    var buildsUrl = await GetBuildUrl(buildTypeDocument);
                    var url = buildsUrl.Replace("/httpAuth/app/rest/", "");
                    var b = await FetchAsync(teamCityHost, url, client);
                    var count = b.Root.Attribute("count").Value;
                    Console.WriteLine(count);
                    Console.WriteLine(b);
                }
            }
        }


        private static async Task<XDocument> FetchAsync(string host, string urlPath, HttpClient client)
        {
            var root = new Uri(host);
            var path = "httpAuth/app/rest/" + urlPath;
            var url = new Uri(root, path);

            var responseStream = await client.GetStreamAsync(url);

            return XDocument.Load(responseStream);
        }

        private static async Task<IEnumerable<Tuple<string, string>>> GetProjectsAsync(Task<XDocument> projects)
        {
            var document = await projects;

            return document.Root.Elements("project").Select(p => Tuple.Create(p.Attribute("id").Value, p.Attribute("name").Value));
        }

        private static async Task<IEnumerable<Tuple<string, string>>> GetBuildsAsync(Task<XDocument> builds)
        {
            var document = await builds;

            return document.Root
                .Element("buildTypes")
                .Elements("buildType")
                .Select(buildType => Tuple.Create(buildType.Attribute("id").Value, buildType.Attribute("name").Value));
        }

        private static async Task<string> GetBuildUrl(Task<XDocument> buildType)
        {
            var document = await buildType;

            return document
                .Root
                .Element("builds")
                .Attribute("href")
                .Value;
        }
    }
}