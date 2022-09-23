using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using BlazorApp.Shared;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace BlazorApp.Api
{
    public static class WeatherForecastFunction
    {
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
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log, ClaimsPrincipal principal)
        {

            log.LogInformation("B2C_CLIENT_ID: " + Environment.GetEnvironmentVariable("B2C_CLIENT_ID"));

            StringBuilder strInformazioni= new StringBuilder();

            strInformazioni.Append("Controllo tramite ClaimsPrincipal");
            foreach(var claim in principal.Claims)
                strInformazioni.Append($"{claim.Type}->{claim.Value}");
            log.LogInformation(strInformazioni.ToString());

            strInformazioni.Clear();

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
            string clientPrincipalDecoded = System.Text.Encoding.Default.GetString(decodedBytes);
            strInformazioni.Append(clientPrincipalDecoded);
            log.LogInformation(strInformazioni.ToString());

            strInformazioni.Clear();



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
