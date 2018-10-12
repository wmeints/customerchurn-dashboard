
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
using Microsoft.ML.Runtime.Data;

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
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, 
            [Blob("models/customer-churn.zip", FileAccess.Read, Connection = "AzureWebJobsStorage")]Stream modelStream,
            ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CustomerChurnPredictionData>(requestBody);

            log.LogInformation("Loading model from blob storage");

            var env = new LocalEnvironment();
            var model = TransformerChain.LoadFrom(env, modelStream);

            var predictor = model.MakePredictionFunction<CustomerChurnPredictionData, CustomerChurnPredictionResult>(env);

            log.LogInformation("Scoring sample for customer churn");

            var result = predictor.Predict(data);

            return new OkObjectResult(result);
        }
    }
}
