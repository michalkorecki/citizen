using Topshelf;

namespace Citizen.Endpoint
{
	class Program
	{
		static void Main(string[] args)
		{
			HostFactory.Run(configuration =>
			{
				configuration.Service<Host>(s =>
				{
					s.ConstructUsing(name => new Host());
					s.WhenStarted(host => host.Start());
					s.WhenStopped(host => host.Stop());
				});

				configuration.RunAsLocalSystem();
				configuration.SetDescription("Provides TeamCity build statistics endpoints and dashboard");
				configuration.SetDisplayName("Citizen.Endpoint");
				configuration.SetServiceName("Citizen.Endpoint");
			});
		}
	}
}
