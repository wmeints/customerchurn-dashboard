
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CustomerDashboard.Functions.Messages;

namespace CustomerDashboard.Functions
{
    /// <summary>
    /// Function that can be used to predict churn probability for a customer.
    /// </summary>
    public static class PredictCustomerChurn
    {
        /// <summary>
        /// Predicts the churn probability of a customer
        /// </summary>
        /// <param name="req">Incoming request data</param>
        /// <param name="log">Logger for the function</param>
        /// <returns>Returns the outcome of the prediction</returns>
        [FunctionName("PredictCustomerChurn")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Predicting churn probability");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CustomerChurnPredictionData>(requestBody);

            return new OkObjectResult(new CustomerChurnPredictionResult());
        }
    }
}
