using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Citizen
{
    public static class BuildSource
    {
        public static async Task<Build[]> GetBuilds(HttpClient teamCityClient, string teamCityHost)
        {
            var projectsDocument = await FetchAsync(teamCityHost, "httpAuth/app/rest/projects", teamCityClient);
            var projects = GetProjectsAsync(projectsDocument);
            var builds = new List<Build>();
            foreach (var project in projects)
            {
                var projectDetailsDocument = await FetchAsync(teamCityHost, $"httpAuth/app/rest/projects/{project.Item1}", teamCityClient);
                var buildTypes = GetBuildTypes(projectDetailsDocument);
                foreach (var buildType in buildTypes)
                {
                    var buildHistoryDocument = await FetchAsync(teamCityHost, $"httpAuth/app/rest/buildTypes/{buildType.Item1}/builds", teamCityClient);
                    var buildIds = GetBuildIds(buildHistoryDocument);

                    foreach (var buildId in buildIds)
                    {
                        var buildDetails = await FetchAsync(teamCityHost, $"httpAuth/app/rest/builds/{buildId}", teamCityClient);
                        var build = GetBuildDetails(buildDetails);

                        builds.Add(build);
                    }
                }
            }

            return builds.ToArray();
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

        private static IEnumerable<string> GetBuildIds(XDocument document) => document
            .Root
            .Elements("build")
            .Select(b => b.Attribute("id").Value);

        private static Build GetBuildDetails(XDocument buildDetails) => new Build
        {
            Id = buildDetails.Root.Attribute("id").Value,
            BuildTypeId = buildDetails.Root.Element("buildType").Attribute("id").Value,
            BuildTypeName = buildDetails.Root.Element("buildType").Attribute("name").Value,
            ProjectId = buildDetails.Root.Element("buildType").Attribute("projectId").Value,
            ProjectName = buildDetails.Root.Element("buildType").Attribute("projectName").Value,
            Status = buildDetails.Root.Attribute("status").Value,
            StatusText = buildDetails.Root.Element("statusText").Value,
            State = buildDetails.Root.Attribute("state").Value,
            Queued = ParseDate(buildDetails.Root.Element("queuedDate").Value),
            Started = ParseDate(buildDetails.Root.Element("startDate").Value),
            Finished = ParseDate(buildDetails.Root.Element("finishDate").Value)
        };

        private static DateTime ParseDate(string value) => DateTime.ParseExact(value, "yyyyMMddTHHmmsszzz", CultureInfo.InvariantCulture);
    }
}