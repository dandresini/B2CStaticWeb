using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using BlazorApp.Shared;
using System.Text;
using System.Text.Json;
using Microsoft.Graph;
using System.Threading.Tasks;


namespace BlazorApp.Api
{
    public class WeatherForecastFunction
    {
        private readonly GraphServiceClient graphClient;

        

        public WeatherForecastFunction(GraphServiceClient GraphClient)
        {
            graphClient= GraphClient;
        }

        private static string GetSummary(int temp)
        {
            var summary = "Mild";

            if (temp >= 32)
            {
                summary = "Hot";
            }
            else if (temp <= 16 && temp > 0)
            {
                summary = "Cold";
            }
            else if (temp <= 0)
            {
                summary = "Freezing!";
            }

            return summary;
        }

        [FunctionName("WeatherForecast")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var principal = new ClientPrincipal();
            log.LogInformation("verifica x-ms-client-principal");
            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {

                var data = header[0];
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json);
            }
            
            principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);
            log.LogInformation($"fine verifica x-ms-client-principal {principal.UserId} - {principal.UserRoles.Count()}");
            
            if (!principal.UserRoles?.Any() ?? true)
            {

                log.LogInformation("Call graphClient");

                var user = await graphClient.Users[$"{principal.UserId}"]
                           .Request()
                           .GetAsync();

                log.LogInformation("Risultato" + JsonSerializer.Serialize(user));

                var randomNumber = new Random();
                var temp = 0;

                var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = temp = randomNumber.Next(-20, 55),
                    Summary = GetSummary(temp)
                }).ToArray();

                return new OkObjectResult(result);
            }

            return new UnauthorizedResult();    
        }
    }
}
