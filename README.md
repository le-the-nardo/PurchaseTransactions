# Purchase Transactions API

A RESTful API for storing and retrieving purchase transactions. Built with ASP.NET Core, Entity Framework Core, and SQLite — no external database or web server required.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Running the application

```bash
cd src/PurchaseTransactions.Api
dotnet run
```

The API will start at `http://localhost:5039`. The SQLite database is created automatically on first run.

The OpenAPI spec is available at `http://localhost:5039/openapi/v1.json` when running in Development mode.

## Running the tests

```bash
dotnet test
```

## Testing with Postman

A Postman collection with all endpoints pre-configured is available here:
[Purchase Transactions — Postman Collection](https://.postman.co/workspace/My-Workspace~84160585-e85c-4a33-8541-016c4088e73c/collection/10584594-0d7ef5db-5fed-4b26-805e-9f37d0aa9bfd?action=share&creator=10584594)

## API Endpoints

### Create a transaction

```
POST /transactions
Content-Type: application/json

{
  "description": "Grocery shopping",
  "transactionDate": "2024-06-15",
  "purchaseAmount": 85.50
}
```

**Response: 201 Created**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Grocery shopping",
  "transactionDate": "2024-06-15",
  "purchaseAmount": 85.50
}
```

### Get a transaction by ID

```
GET /transactions/{id}
```

**Response: 200 OK** or **404 Not Found**

### Get all transactions

```
GET /transactions
```

**Response: 200 OK**

## Field rules

| Field | Rule |
|---|---|
| `description` | Required. Max 50 characters. |
| `transactionDate` | Required. Valid date in `yyyy-MM-dd` format. |
| `purchaseAmount` | Required. Positive value. Stored rounded to the nearest cent. |
| `id` | Auto-assigned GUID. Unique per transaction. |

## Validation errors

Invalid requests return `422 Unprocessable Entity` with a problem details body:

```json
{
  "title": "Validation Error",
  "status": 422,
  "detail": "Description must not exceed 50 characters.",
  "errorCode": "INVALID_DESCRIPTION"
}
```

## Project structure

```
src/
  PurchaseTransactions.Api/
    Domain/         # Transaction entity and DomainException
    Data/           # EF Core DbContext and migrations
    Repositories/   # ITransactionRepository and implementation
    DTOs/           # Request and response records
    Endpoints/      # Minimal API route handlers
    Middleware/      # Global exception handler
tests/
  PurchaseTransactions.Tests/
    Unit/           # Domain invariant and rounding tests
    Integration/    # End-to-end API tests via WebApplicationFactory
```
