using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Citizen
{
    class Program
    {
        static void Main(string[] args) => ListProjectConfigurations().Wait();

        private static async Task ListProjectConfigurations()
        {
            var teamCityHost = ConfigurationManager.AppSettings["TeamCityHost"];
            if (string.IsNullOrEmpty(teamCityHost))
            {
                teamCityHost = GetHost();
                RewriteAppConfig("TeamCityHost", teamCityHost);
            }

            var authenticationHeader = ConfigurationManager.AppSettings["TeamCityAuthenticationHeader"];
            if (string.IsNullOrEmpty(authenticationHeader))
            {
                var userName = GetUserName();
                var password = GetPassword();
                var basicHttpAuthHeader = CreateBasicHttpAuthHeader(userName, password);
                authenticationHeader = basicHttpAuthHeader;
                RewriteAppConfig("TeamCityAuthenticationHeader", basicHttpAuthHeader);
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authenticationHeader);

            var projectsDocument = FetchAsync(teamCityHost, "projects", client);
            var projects = await GetProjectsAsync(projectsDocument);

            foreach (var project in projects)
            {
                //Console.WriteLine($"{project.Item2}");
                var buildsDocument = FetchAsync(teamCityHost, $"projects/{project.Item1}", client);
                var builds = await GetBuildsAsync(buildsDocument);
                foreach (var build in builds)
                {
                    //Console.WriteLine($"  {build.Item2}");
                    var buildTypeDocument = FetchAsync(teamCityHost, $"buildTypes/{build.Item1}", client);
                    var commands = await GetBuildTypesCommandsAsync(buildTypeDocument);
                    foreach (var command in commands)
                    {
                        var buildCommand = new BuildCommand
                        {
                            Project = project.Item2,
                            BuildType = build.Item2,
                            Command = command
                        };
                        Console.WriteLine($"    {command}");
                    }
                }
            }
        }



        private static void RewriteAppConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Minimal);
        }

        private static string CreateBasicHttpAuthHeader(string userName, string password)
        {
            var input = $"{userName}:{password}";
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes);
        }

        private static string GetHost()
        {
            Console.Write("TeamCity host: ");
            return Console.ReadLine();
        }

        private static string GetUserName()
        {
            Console.Write("TeamCity username: ");
            return Console.ReadLine();
        }

        private static string GetPassword()
        {
            Console.Write("TeamCity password: ");
            return Console.ReadLine();
        }

        private static void Restart()
        {

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

        private static async Task<IEnumerable<string>> GetBuildTypesCommandsAsync(Task<XDocument> build)
        {
            var document = await build;

            return document
                .XPathSelectElements("//property[@name='command.parameters']")
                .Select(p => p.Attribute("value").Value);
        }

        public class BuildCommand
        {
            public string Project { get; set; }
            public string BuildType { get; set; }
            public string Command { get; set; }
        }
    }
}
