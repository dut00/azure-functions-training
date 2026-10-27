# Azure Functions training (C#, .NET 10 isolated worker)

| Function | Trigger | How to fire it |
|---|---|---|
| `HttpHello` | HTTP `GET/POST /api/hello` | Browser / curl |
| `BlobUploadProcessor` | Blob in `uploads/` (Event Grid source) | Upload a file to `uploads` |
| `EventGridLogger` | Event Grid event | Delete a blob (BlobDeleted subscription) |
| `TimerHeartbeat` | Timer, every 5 minutes | Just wait, or run it manually from the portal |

## Prerequisites

```powershell
winget install Microsoft.Azure.FunctionsCoreTools   # "func" CLI
npm install -g azurite                              # local storage emulator
```

## Run locally

```powershell
azurite --silent --location .azurite      # separate terminal
func start
```

- HTTP: `curl "http://localhost:7071/api/hello?name=Kuba"`
  or `curl -X POST http://localhost:7071/api/hello -H "Content-Type: application/json" -d '{"name":"Kuba"}'`
- Timer: watch the console, it logs every 5 minutes.
- Event Grid (simulated):
  ```powershell
  curl -X POST "http://localhost:7071/runtime/webhooks/EventGrid?functionName=EventGridLogger" `
       -H "Content-Type: application/json" -H "aeg-event-type: Notification" `
       -d "@samples/eventgrid-blobdeleted.json"
  ```
- Blob trigger: the Event Grid source cannot be fired by Azurite; test it in Azure.

## Deploy to Azure (Flex Consumption)

1. Function App runtime must be **.NET isolated 10** (if your app is on .NET 8, change `TargetFramework` to `net8.0`).
2. Add app setting **`TrainingStorage`** = connection string of the storage account (e.g. `unikat2blob`).
3. Create containers **`uploads`** and **`processed`** in that storage account.
4. Publish:
   ```powershell
   az login
   func azure functionapp publish <function-app-name>
   ```

## Event Grid subscriptions (storage account → Events → + Event Subscription)

**For `BlobUploadProcessor`:**
- Event types: `Blob Created`
- Endpoint type: **Web Hook**
- URL: `https://<function-app-host>/runtime/webhooks/blobs?functionName=Host.Functions.BlobUploadProcessor&code=<blobs_extension key>`
  (key: Function App → Functions → App keys → System keys → `blobs_extension`)
- Filters → Subject begins with: `/blobServices/default/containers/uploads/`

**For `EventGridLogger`:**
- Event types: `Blob Deleted`
- Endpoint type: **Azure Function** → select `EventGridLogger`

## Test in Azure

- HTTP: `https://<function-app-host>/api/hello?name=Kuba&code=<function key>`
- Upload a file to `uploads` → `processed/<file>.info.txt` appears.
- Delete any blob → `EventGridLogger` logs it.
- Logs: Function App → Log stream, or Application Insights → Logs (`traces` table).
