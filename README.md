# WCCG.PAS.Referrals.API
WCCG PAS Referrals API service as a part of discovery work. Acting as translation layer for WPAS Instance.

:)
# WCCG.PAS.Referrals.UI

## Description
The UI for checking and editing existing referrals in Cosmos DB.

## Local development

1. Set up local configuration by editing `appsettings.Development.json` file
2. Set `AuthKey' value for Cosmos DB in you user secrects file:
    ```json
    {
        "Cosmos": {
            "AuthKey": "YOUR_AUTH_KEY"
        }
    }
    ```
