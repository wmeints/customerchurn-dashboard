# Customer churn dashboard
Welcome to the Customer churn dashboard sample. 
This is a demo application to show how you can build a customer churn dashboard with ML.NET in Azure Functions.

## Getting started
### Install the tools
You can run the sample on your local computer by installing the azure functions CLI:

```
npm i -g azure-functions-core-tools@2
```

Please make sure you use version 2 of the tools as you need x64 support for the sample to work.
Also the sample uses .NET core which is only supported by this version of the tooling.

### Run the sample locally
Execute the following steps to run the application locally:

* `dotnet build -c Debug`
* `cd bin/debug/netstandard2.0`
* `func host start`

You can now access the functions through your browser or a REST client.

### Deploy the sample to Azure
To deploy the sample, create a new azure functions app, download the publish profile and push the code using the Visual Studio publication wizard.
Alternatively you can use these steps to deploy the app:

``` powershell
Compress-Archive -Path bin/debug/netstandard2.0/* -DestinationPath CustomerChurnDashboard.zip
func settings add AzureWebJobsStorage [connection string]
func azure functionapp deploy --zip CustomerChurnDashboard.zip
```

Replace the `[connection string]` placeholder with the connection string for the storage account to use.

### Train the model
In order to train the model you need two files, `train.csv` and `validate.csv` which you can find in the `data` folder.
Upload them to the `data` container in the storage account attached to your function application. When debugging you 
can use the local storage emulator. 

**Note** You can use the Azure Storage Explorer to upload the files to the storage account. 

Now invoke the `TriggerChurnModelTraining` function from a browser window. This will queue the message to train the model.
After a short delay, the training function gets triggered and the model is trained.

### Predicting churn
Once you have the model trained, invoke the `PredictCustomerChurn` function with data similar to this:

``` json
{
	"seniorCitizen": "0",
	"partner": "No",
	"dependents": "No",
	"internetService": "Fiber optic",
	"onlineSecurity": "No",
	"onlineBackup": "No",
	"deviceProtection": "No",
	"techSupport": "No internet service",
	"contract": "Month-to-month",
	"paperlessBilling": "Yes",
	"paymentMethod": "Electronic check",
	"tenure": 4.0,
	"monthlyCharges": 85.65
}
```

The function will use the trained model to make a prediction of the probability a customer is going to churn.
The output looks similar to this:

``` json
{
    "probability": 0.5451466,
    "predictedLabel": true,
    "score": 0.181079566
}
```
