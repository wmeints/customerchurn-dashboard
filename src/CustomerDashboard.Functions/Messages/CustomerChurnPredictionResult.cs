using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerDashboard.Functions.Messages
{
    public class CustomerChurnPredictionResult
    {
        public float Probability { get; set; }
        public bool PredictedLabel { get; set; }
        public float Score { get; set; }
    }
}
