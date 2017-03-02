using Nancy;

namespace Citizen.Endpoint.Modules
{
	public class IndexModule : NancyModule
	{
		public IndexModule()
		{
			Get["/"] = context => GetIndex();
		}

		private Response GetIndex()
		{
			var assembly = typeof(IndexModule).Assembly;
			var stream = assembly.GetManifestResourceStream("Citizen.Endpoint.Static.index.html");

			return Response.FromStream(stream, "text/html");
		}
	}
}
