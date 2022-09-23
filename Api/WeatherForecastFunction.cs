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
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            StringBuilder strInformazioni= new StringBuilder();

            strInformazioni.Append("Controllo tramite X-MS Header");
            var principal_name = req.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].FirstOrDefault();
            var principal_Id = req.Headers["X-MS-CLIENT-PRINCIPAL-ID"].FirstOrDefault();
            string easyAuthProvider = req.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].FirstOrDefault();
            string clientPrincipalEncoded = req.Headers["X-MS-CLIENT-PRINCIPAL"].FirstOrDefault();

            strInformazioni.Append("User ID: " + principal_name);
            strInformazioni.Append("User Principal ID: " + principal_Id);
            strInformazioni.Append("EasyAuth Provider: " + easyAuthProvider);
            strInformazioni.Append("Encoded Client Principal: " + clientPrincipalEncoded);

            //Decode the Client Principal
            byte[] decodedBytes = Convert.FromBase64String(clientPrincipalEncoded);
            string clientPrincipalDecoded = Encoding.Default.GetString(decodedBytes);
            strInformazioni.Append(clientPrincipalDecoded);
            log.LogInformation(strInformazioni.ToString());

            log.LogInformation("Call graphClient");
            log.LogInformation($"tenant:{Environment.GetEnvironmentVariable("B2C_TENANTID")},clientid:{Environment.GetEnvironmentVariable("B2C_CLIENT_ID")}, secret:{Environment.GetEnvironmentVariable("B2C_CLIENT_SECRET")}");

            var user = await graphClient.Users[$"{principal_Id}"]
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
    }
}
