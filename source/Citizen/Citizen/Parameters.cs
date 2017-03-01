using System;
using System.Configuration;
using System.Text;

namespace Citizen
{
    public static class Parameters
    {
        public static string GetTeamCityHost()
        {
            var teamCityHost = ConfigurationManager.AppSettings["TeamCityHost"];
            if (string.IsNullOrEmpty(teamCityHost))
            {
                teamCityHost = GetHost();
                RewriteAppConfig("TeamCityHost", teamCityHost);
            }

            return teamCityHost;
        }

        public static string GetAuthenticationHeader()
        {
            var authenticationHeader = ConfigurationManager.AppSettings["TeamCityAuthenticationHeader"];
            if (string.IsNullOrEmpty(authenticationHeader))
            {
                var userName = GetUserName();
                var password = GetPassword();
                authenticationHeader = CreateBasicHttpAuthHeader(userName, password);
                RewriteAppConfig("TeamCityAuthenticationHeader", authenticationHeader);
            }

            return authenticationHeader;
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
    }
}