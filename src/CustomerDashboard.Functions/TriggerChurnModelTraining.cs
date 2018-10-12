
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

namespace CustomerDashboard.Functions
{
    /// <summary>
    /// This function triggers the training job by posting a job message
    /// in the right storage queue. By using an intermediate queue we decouple the 
    /// training process as it is too slow to handle in a single HTTP call.
    /// </summary>
    public static class StartCustomerChurnModelTraining
    {
        /// <summary>
        /// Posts a trigger message to the training jobs storage queue.
        /// 
        /// This message is later picked up by the TrainCustomerChurnModel function
        /// in order to train the customer churn model.
        /// </summary>
        /// <param name="req">Incoming HTTP request</param>
        /// <param name="log">Logger to use for the function</param>
        /// <returns>Returns the training job message to post to the training jobs queue</returns>
        [FunctionName("TriggerChurnModelTraining")]
        [return: Queue("training-jobs", Connection = "AzureWebJobsStorage")]
        public static async Task<TrainingJobTriggerData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get","post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Trigger training job for customer churn model");

            return new TrainingJobTriggerData();
        }
    }
}
