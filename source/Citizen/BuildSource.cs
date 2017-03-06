using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Citizen
{
	public class BuildSource
	{
		private readonly HttpClient client;
		private readonly string host;

		public BuildSource(HttpClient client, string host)
		{
			this.client = client;
			this.host = host;
		}

		public async Task<Build[]> GetBuilds()
		{
			var projects = await GetProjectsAsync();
			var buildTypes = await Task.WhenAll(projects.Select(GetBuildTypesAsync));
			var buildInstancesIds = await Task.WhenAll(buildTypes.SelectMany(b => b).Select(GetBuildInstancesIdsAsync));
			var builds = await Task.WhenAll(buildInstancesIds.SelectMany(b => b).Select(GetBuildDetailsAsync));

			return builds;
		}

        //todo[mk]: for chart use case this could be optimized to use 1 request:
        //httpAuth/app/rest/builds?locator=buildType:(id:<build_type_id>)&fields=build(status,queuedDate,startDate,finishDate)
        public async Task<Build[]> GetBuildsByType(string buildTypeId)
        {
            var buildInstancesIds = await GetBuildInstancesIdsAsync(buildTypeId);
            var builds = await Task.WhenAll(buildInstancesIds.Select(GetBuildDetailsAsync));

            return builds;
        }

		private async Task<XDocument> FetchAsync(string resource)
		{
			var root = new Uri(this.host);
			var url = new Uri(root, resource);

			var responseStream = await this.client.GetStreamAsync(url);

			return XDocument.Load(responseStream);
		}

		private async Task<string[]> GetProjectsAsync()
		{
			var projectsDocument = await FetchAsync("httpAuth/app/rest/projects");
			return projectsDocument
				.Root
				.Elements("project")
				.Select(project => project.Attribute("id").Value)
				.ToArray();
		}

		private async Task<string[]> GetBuildTypesAsync(string projectId)
		{
			var projectDetailsDocument = await FetchAsync($"httpAuth/app/rest/projects/{projectId}");
			return projectDetailsDocument
				.Root
				.Element("buildTypes")
				.Elements("buildType")
				.Select(buildType => buildType.Attribute("id").Value)
				.ToArray();
		}

		private async Task<string[]> GetBuildInstancesIdsAsync(string buildTypeId)
		{
			var buildHistoryDocument = await FetchAsync($"httpAuth/app/rest/buildTypes/{buildTypeId}/builds");
			return buildHistoryDocument
				.Root
				.Elements("build")
				.Select(b => b.Attribute("id").Value)
				.ToArray();
		}

		private async Task<Build> GetBuildDetailsAsync(string buildId)
		{
			var buildDetails = await FetchAsync($"httpAuth/app/rest/builds/{buildId}");
			return new Build
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
		}

		private static DateTime ParseDate(string value) => DateTime.ParseExact(value, "yyyyMMddTHHmmsszzz", CultureInfo.InvariantCulture);
	}
}