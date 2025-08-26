# Invoicing System

## Setup
- 1) Install .NET SDK 8.0 from the following site: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0).
- 2) Navigate to the `InvoiceSystem/InvoiceSystem` directory and run `dotnet restore` to restore dependencies.
- 3) Navigate to the next folder `InvoiceSystem` (`InvoiceSystem/InvoiceSystem/InvoiceSystem`), run `dotnet run` to start the application.
- 4) The endpoints are visible via swagger at the url `http://localhost:5235/swagger`.

## Endpoints
- POST /invoice: Create a new invoice (Authorization: Bearer demo-token-compA).
- GET /invoice/sent?counter_party_company=compB&date_issued=2025-08-21T00:00:00Z&invoice_id=inv1
- GET /invoice/received?counter_party_company=compA&date_issued=2025-08-21T00:00:00Z&invoice_id=inv1

## Testing
**Testing POST /invoice:**
- Hardcoded auth token (`Bearer demo-token-compA`) for demo purposes.
- Test the API using Swagger with the following steps:
  1. Open the Swagger UI at `http://localhost:5235/swagger` after running the application (follow Setup steps 1-3).
  2. Click the lock button (Authorize) to set the authentication token.
  3. Enter `Bearer demo-token-compA` in the value field and click "Authorize".
  4. Navigate to the `POST /invoice` endpoint.
  5. Click "Try it out" to enable the request body input.
  6. Provide the following JSON body:
     ```json
     {
       "invoiceId": "inv5",
       "dateIssued": "2025-08-25T11:18:00Z",
       "netAmount": 150.0,
       "vatAmount": 30.0,
       "totalAmount": 180.0,
       "description": "Consulting Services",
       "companyId": "compA",
       "counterPartyCompanyId": "compB"
     }
 7. Click "Execute" to send the request. Expected Result: If successful, you should receive a 201 Created response with the new invoice location.
 8. Duplicate Check: Execute the same request again with the same invoiceId ("inv5"). You should receive a 400 Bad Request response with the message "Invoice with ID 'inv5' already exists" to confirm duplicate handling.
 9. Unauthorized Test: Log out (click "Authorize" and log out), then retry. Expected Result: 401 Unauthorized.

**Testing GET /invoice/sent**
 1. Open the Swagger UI at http://localhost:5235/swagger after running the application.
 2. Navigate to the GET /invoice/sent endpoint.
 3. Click "Try it out" to enable the query parameters.
 4. Enter optional filters (e.g., counter_party_company = "compB", date_issued = "2025-08-21T00:00:00Z", invoice_id = "inv1").
 5. Click "Execute" to send the request. Expected Result: 200 OK response with an array of matching sent invoices (e.g., ["inv1"] if filtered).
 6. Test without filters: Leave all fields blank. Expected Result: 200 OK with all sent invoices.

**Testing GET /invoice/received:**
 1. Open the Swagger UI at http://localhost:5235/swagger after running the application.
 2. Navigate to the GET /invoice/received endpoint.
 3. Click "Try it out" to enable the query parameters.
 4. Enter optional filters (e.g., counter_party_company = "compB", date_issued = "2025-08-22T00:00:00Z", invoice_id = "inv2").
 5. Click "Execute" to send the request. Expected Result: 200 OK response with an array of matching received invoices (e.g., ["inv2"] if filtered).
 6. Test without filters: Leave all fields blank. Expected Result: 200 OK with all received invoices.

## Unit Tests
- 1) Navigate to the `InvoiceSystem\InvoicingSystem.Test` directory
- 2) Run `dotnet test` to execute unit tests.

## Storage
- Data is stored in memory.

## Docker
- Build: `docker build -t invoicing-system .`
- Run: `docker run -d -p 8080:80 invoicing-system`

