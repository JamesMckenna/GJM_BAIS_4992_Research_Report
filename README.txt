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