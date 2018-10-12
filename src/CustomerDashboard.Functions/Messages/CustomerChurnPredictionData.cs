using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerDashboard.Functions.Messages
{
    public class CustomerChurnPredictionData
    {
        public bool Churn { get; set; }
        public string SeniorCitizen { get; set; }
        public string Partner { get; set; }
        public string Dependents { get; set; }
        public string InternetService { get; set; }
        public string OnlineSecurity { get; set; }
        public string OnlineBackup { get; set; }
        public string DeviceProtection { get; set; }
        public string TechSupport { get; set; }
        public string Contract { get; set; }
        public string PaperlessBilling { get; set; }
        public string PaymentMethod { get; set; }
        public float Tenure { get; set; }
        public float MonthlyCharges { get; set; }
    }
}
