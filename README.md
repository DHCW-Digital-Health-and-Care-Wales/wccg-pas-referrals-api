# PAS Referrals API

## Description
This project is an ASP.NET Core API that provides endpoints for handling Referrals.

## Prerequisites
Make sure you have the following installed and set up:
- [.NET SDK](https://dotnet.microsoft.com/download) version 8.0
- `az login --tenant <YOUR_TENNANT>`
- Cosmos database:
    - Deployed CosmosDB in Azure with Read/Write RBAC role for your account **OR** local emulator
    - Database and container with `/id` partition key.

## Required configuration for local development
To configure the project, follow these steps:
1. Open `appsettings.Development.json` and configure target CosmosDB database and container names. Change log levels if needed.
2. Open user secrets file `secrets.json` (not included in project) and add following values:
```
{
  "Cosmos": {
    "DatabaseEndpoint": "<YOUR_COSMOS_DB__ENDPOINT>"
  },

  "ApplicationInsights": {
    "ConnectionString": "<YOUR_APPINSIGHTS_CONNECTION_STRING>"
  }
}
```

## Project Structure
The core project structure is organized as follows:
```
WCCG.PAS.Referrals.API/
│
├── Configuration
│   └── Controller files
│
├── Controllers
│   └── v1
|       └── Controllers for API of version 1
├── DbModels
│   └── Database models
|
├── Mappers
│   └── Mappers for database models
|
├── Middleware
│   └── Exception handling center
|
├── Repositories
│   └── CosmosDB repositories
|
├── Services
│   └── Service classes
|
├── Swagger
│   └── Helper classes for Swagger
│
├── Validators
│   └── Database model validators
|
├── appsettings.json
|   └── appsettings.Development.json
|
├── launchSettings.json
|
└── Program.cs
```

## Running the Project
To run the project locally, follow these steps:
1. Clone the repository.
2. Don't forget `az login --tenant <YOUR_TENNANT>`
3. Setup local configuration according to `Required configuration for local development` section
2. Rebuild and run the project.
6. Open your web browser and navigate to `https://localhost:xxxxx/swagger/index.html` to access the SwaggerUI with API endpoints.

## API Endpoints
Example payloads for POST endpoints can be found in the `Examples` folder. 

### POST /api/v1/Referrals/createReferral
- Description: Creates a referral in CosmosDB and returns enriched response 
- Request body should be a valid FHIR Bundle JSON object. [Example Payload](./src/WCCG.PAS.Referrals.API/Examples/createReferral-example-payload.json)
- Response is also a FHIR Bundle but enriched with new values generated while the creation process:
