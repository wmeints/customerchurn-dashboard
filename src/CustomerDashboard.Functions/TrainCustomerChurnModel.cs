using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomerDashboard.Functions.Data;
using CustomerDashboard.Functions.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers;

namespace CustomerDashboard.Functions
{
    public static class TrainCustomerChurnModel
    {
        private static readonly List<string> FieldNames = new List<string>
        {
            "RowIndex",
            "CustomerID",
            "Gender",
            "SeniorCitizen",
            "Partner",
            "Dependents",
            "Tenure",
            "PhoneService",
            "MultipleLines",
            "InternetService",
            "OnlineSecurity",
            "OnlineBackup",
            "DeviceProtection",
            "TechSupport",
            "StreamingTV",
            "StreamingMovies",
            "Contract",
            "PaperlessBilling",
            "PaymentMethod",
            "MonthlyCharges",
            "TotalCharges",
            "Churn"
        };

        [FunctionName("TrainCustomerChurnModel")]
        public static void Run(
            [QueueTrigger("training-jobs", Connection = "AzureWebJobsStorage")]TrainingJobTriggerData myQueueItem,
            [Blob("models/customer-churn.zip", FileAccess.Write, Connection = "AzureWebJobsStorage")]Stream modelStream,
            [Blob("data/train.csv", FileAccess.Read, Connection = "AzureWebJobsStorage")]Stream trainingData,
            [Blob("data/validate.csv", FileAccess.Read, Connection = "AzureWebJobsStorage")]Stream validationData,
            ILogger log)
        {
            if (typeof(Microsoft.ML.Runtime.Data.LoadTransform) == null ||
                typeof(Microsoft.ML.Runtime.Learners.LinearClassificationTrainer) == null ||
                typeof(Microsoft.ML.Runtime.Internal.CpuMath.SseUtils) == null ||
                typeof(Microsoft.ML.Runtime.FastTree.FastTree) == null)
            {
                log.LogError("Error loading ML.NET");
            }

            log.LogInformation("Training customer churn model.");

            var env = new LocalEnvironment();
            var classificationContext = new BinaryClassificationContext(env);

            var loader = TextLoader.CreateReader(env, ctx => (
               SeniorCitizen: ctx.LoadText(FieldNames.IndexOf("SeniorCitizen")),
               Partner: ctx.LoadText(FieldNames.IndexOf("Partner")),
               Dependents: ctx.LoadText(FieldNames.IndexOf("Dependents")),
               InternetService: ctx.LoadText(FieldNames.IndexOf("InternetService")),
               OnlineSecurity: ctx.LoadText(FieldNames.IndexOf("OnlineSecurity")),
               OnlineBackup: ctx.LoadText(FieldNames.IndexOf("OnlineBackup")),
               DeviceProtection: ctx.LoadText(FieldNames.IndexOf("DeviceProtection")),
               TechSupport: ctx.LoadText(FieldNames.IndexOf("TechSupport")),
               Contract: ctx.LoadText(FieldNames.IndexOf("Contract")),
               PaperlessBilling: ctx.LoadText(FieldNames.IndexOf("PaperlessBilling")),
               PaymentMethod: ctx.LoadText(FieldNames.IndexOf("PaymentMethod")),
               Tenure: ctx.LoadFloat(FieldNames.IndexOf("Tenure")),
               MonthlyCharges: ctx.LoadFloat(FieldNames.IndexOf("MonthlyCharges")),
               Churn: ctx.LoadBool(FieldNames.IndexOf("Churn"))
           ), new StreamDataSource(trainingData), hasHeader: true, separator: ',');

            var estimator = loader.MakeNewEstimator()
                .Append(row => (
                    SeniorCitizen: row.SeniorCitizen.OneHotEncoding(),
                    Partner: row.Partner.OneHotEncoding(),
                    Dependents: row.Dependents.OneHotEncoding(),
                    InternetService: row.InternetService.OneHotEncoding(),
                    OnlineSecurity: row.OnlineSecurity.OneHotEncoding(),
                    OnlineBackup: row.OnlineBackup.OneHotEncoding(),
                    DeviceProtection: row.DeviceProtection.OneHotEncoding(),
                    TechSupport: row.TechSupport.OneHotEncoding(),
                    Contract: row.Contract.OneHotEncoding(),
                    PaperlessBilling: row.PaperlessBilling.OneHotEncoding(),
                    PaymentMethod: row.PaymentMethod.OneHotEncoding(),
                    Tenure: row.Tenure,
                    MonthlyCharges: row.MonthlyCharges,
                    Churn: row.Churn
                ))
                .Append(row => (
                    Churn: row.Churn,
                    Features: row.SeniorCitizen.ConcatWith(
                        row.Partner, row.Dependents, row.InternetService,
                        row.OnlineSecurity, row.OnlineBackup, row.DeviceProtection,
                        row.TechSupport, row.Contract, row.PaperlessBilling,
                        row.Tenure, row.MonthlyCharges
                )))
                .Append(row =>
                {
                    var prediction = classificationContext.Trainers.Sdca(row.Churn, row.Features);
                    return (
                        PredictedLabel: prediction.predictedLabel,
                        Score: prediction.score,
                        Label: row.Churn,
                        Probability: prediction.probability);
                });

            var trainingSet = loader.Read(new StreamDataSource(trainingData));
            var validationSet = loader.Read(new StreamDataSource(validationData));

            var trainingSampleCount = trainingSet
                .AsDynamic
                .AsEnumerable<CustomerChurnPredictionData>(env, false)
                .Count();

            var validationSampleCount = validationSet
                .AsDynamic
                .AsEnumerable<CustomerChurnPredictionData>(env, false)
                .Count();

            log.LogInformation("Training on {Rows} samples", trainingSampleCount);

            var model = estimator.Fit(trainingSet);

            log.LogInformation("Validating on {Rows} samples", trainingSampleCount);

            var predictions = model.Transform(validationSet).AsDynamic;
            var score = classificationContext.Evaluate(predictions, "Label");

            log.LogInformation("Model accuracy: {Accuracy}, F1 score: {F1Score}", score.Accuracy, score.F1Score);

            model.AsDynamic.SaveTo(env, modelStream);
        }

        private static (string Id, string SeniorCitizen,
            string Partner, string Dependents, string InternetService,
            string OnlineSecurity, string OnlineBackup, string DeviceProtection,
            string TechSupport, string Contract, string PaperlessBilling,
            string PaymentMethod, string tenureBins,
            float MonthlyCharges, string Churn) LoadCustomerChurnPredictionData(TextLoader.Context arg)
        {
            throw new NotImplementedException();
        }
    }
}
