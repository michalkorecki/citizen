using Nancy;
using System.IO;

namespace Citizen.Endpoint
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private byte[] favicon;

        protected override byte[] FavIcon => LoadFavicon();

        private byte[] LoadFavicon()
        {
            if (this.favicon == null)
            {
                using (var resourceStream = GetType().Assembly.GetManifestResourceStream("Citizen.Endpoint.Static.favicon.ico"))
                {
                    var memoryStream = new MemoryStream();
                    resourceStream.CopyTo(memoryStream);
                    this.favicon = memoryStream.GetBuffer();
                }
            }

            return this.favicon;
 ;       }
    }
}