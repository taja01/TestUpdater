# TestUpdater
Updating test case steps, titles, etc., involves a lot of manual work. Let's automate it!

#How to Azure:

Prerequisites:
Azure DevOps Account: Make sure you have access to an Azure DevOps organization and a project.
Personal Access Token (PAT): For authentication, you can use a PAT. Follow this guide to create one.
Install Postman on your system.

##Know your fields
First let's fetch all fields using Postman:
https://learn.microsoft.com/en-us/rest/api/azure/devops/wit/fields/list?view=azure-devops-rest-7.1&tabs=HTTP

Shortly:
GET https://dev.azure.com/{organization}/{project}/_apis/wit/fields?api-version=7.1
Auth type: Bearer Token
Token: use your PAT

After call, you can find all your fields
like: "referenceName": "Microsoft.VSTS.TCM.AutomationStatus", (in xml I use it as /fields/Microsoft.VSTS.TCM.AutomationStatus)

##TypeScript - PlayWright
TODO: finish it
TODO: add example
TODO: add tests
TODO: move project variables into appsettings.json
