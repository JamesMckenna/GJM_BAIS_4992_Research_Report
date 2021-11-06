.NET 5 SDK for latest MS Nuget packages

luanch setting
Dev: Set start up projects

https://localhost:5001/.well-known/openid-configuration
namespace Client
{
    public class Program
    {
        private static async Task Main()
        {
            // discover endpoints from metadata
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }
            Console.WriteLine("\n\n");
            Console.WriteLine("The Discovery Document for an OpenID Connect Secure Token Service can always be found (without a Token) @ https://URL/.well-known/openid-configuration");
            Console.WriteLine("For this project the STS URL is https://localhost:5001/.well-known/openid-configuration");
            Console.WriteLine("\n");
            var getDiscoDoc = new HttpClient();
            var discoDocument = await getDiscoDoc.GetAsync("https://localhost:5001/.well-known/openid-configuration");
            if (!discoDocument.IsSuccessStatusCode)
            {
                Console.WriteLine(discoDocument.StatusCode);
            }
            else
            {
                var content = await discoDocument.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            Console.WriteLine("\n\n");

            Console.WriteLine("The Token");
            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "ANativeClientOrService",
                ClientSecret = "aSuperSecret",

                Scope = "IdentityAPI"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");


            Console.WriteLine("Token Contents parse by IdentityAPI Get() IdentityController Action and returned");
            // call api
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await apiClient.GetAsync("https://localhost:6001/identity");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }

            Console.ReadLine();
        }
    }
}


is4inmem template from dotnet cli
might have to run: To search for the templates on NuGet.org, run 'dotnet new is4inmem --search'.

Changed some of the names of APIs and clients

"Most of the code lives in the “Quickstart” folder using a “feature folder” style. If this style doesn’t suit you, feel free to organize the code in any way you want."

                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";


MvcClient: change launch settings and port number and start up projects
Startup                     options.ClientId = "mvcClient"; options.ClientSecret = "aDifferentSecret"; must match IdentityServer Config.Client
add Logout to a controller and a link to the nav menu _Layout.cshtmml