using Nancy;
using Nancy.Hosting.Self;
using System;

namespace Citizen.Endpoint
{
	public class Host
	{
		private readonly Uri uri;
		private readonly DefaultNancyBootstrapper bootstrapper;

		private NancyHost host;

		public Host()
		{
			this.uri = new Uri("http://localhost:9021");
			this.bootstrapper = new Bootstrapper();
		}

		public void Start()
		{
			this.host = new NancyHost(uri, bootstrapper);
			this.host.Start();
		}

		public void Stop()
		{
			this.host.Stop();
			this.host.Dispose();
			this.host = null;
		}
	}
}
