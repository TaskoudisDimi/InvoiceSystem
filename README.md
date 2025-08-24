# Invoicing System

## Setup
- Install .NET SDK 8.0.
- Run `dotnet restore` to restore dependencies.
- Run `dotnet run` to start the application.

## Endpoints
- POST /invoice: Create a new invoice (Authorization: Bearer demo-token-compA).
  - Body: JSON `Invoice` object.
- GET /invoice/sent?counter_party_company=compB&date_issued=2025-08-21T00:00:00Z&invoice_id=inv1
- GET /invoice/received?counter_party_company=compA&date_issued=2025-08-21T00:00:00Z&invoice_id=inv1

## Storage
- Data is stored in memory.

## Tests
- Run `dotnet test` to execute unit tests.

## Docker
- Build: `docker build -t invoicing-system .`
- Run: `docker run -d -p 8080:80 invoicing-system`

## Notes
- Hardcoded auth token (`demo-token-compA`) for demo purposes.