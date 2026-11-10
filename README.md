# Notes Application

A notes REST API built with ASP.NET Core, structured in layers (Domain, Application, Infrastructure, API) and backed by PostgreSQL.

## Tech stack

- .NET 10 / ASP.NET Core (controllers)
- Entity Framework Core with Npgsql (PostgreSQL)
- FluentValidation
- OpenAPI with Swagger UI

## Project structure

```
NotesApplication/
├── NotesApplication.Domain/          # Entities, value objects, domain events, rules
├── NotesApplication.Application/     # Services, queries, DTOs, validators
├── NotesApplication.Infrastructure/  # EF Core DbContext, configurations, migrations, repositories
└── NotesApplication.Api/             # Controllers, middleware, API models, startup
```

Dependencies point inward: `Api` and `Infrastructure` depend on `Application`, and `Application` depends on `Domain`. The Domain has no dependency on any other project.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) running locally
- EF Core CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- Optional: [pgAdmin 4](https://www.pgadmin.org/) to inspect the database

## Getting started

### 1. Create the database

Make sure the PostgreSQL service is running, then create an empty database named `notesdb`. In pgAdmin 4, right-click **Databases** → **Create** → **Database**. Or from the command line:

```bash
psql -U postgres -c "CREATE DATABASE notesdb;"
```

### 2. Configure the connection string

In `NotesApplication.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "NotesDb": "Host=localhost;Port=5432;Database=notesdb;Username=postgres;Password=<your-password>"
  }
}
```

Use the username, password and port from your own PostgreSQL installation. Don't commit real secrets. Use user secrets or environment variables instead.

### 3. Apply the database migrations

```bash
dotnet ef database update -p NotesApplication.Infrastructure
```

### 4. Run the API

```bash
dotnet run --project NotesApplication.Api
```

Or use hot reload while developing:

```bash
dotnet watch --project NotesApplication.Api
```

### 5. Open Swagger

```
https://localhost:<port>/swagger
```

The port is printed in the console when the app starts. The raw OpenAPI document is at `/openapi/v1.json`.

## API endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/notes` | List notes (search and paging) |
| `GET` | `/api/notes/{id}` | Get a note by id |
| `POST` | `/api/notes` | Create a note |
| `PUT` | `/api/notes/{id}` | Update a note's title and content |

### List notes

```
GET /api/notes?q=milk&page=1&size=20
```

| Query parameter | Default | Rules |
|---|---|---|
| `q` | empty (no filter) | Up to 100 characters. Matches titles containing the text |
| `page` | `1` | Whole number from 1 to 10,000 |
| `size` | `20` | Whole number from 1 to 100 |

Archived notes are excluded. Results are ordered by title, and the response includes a `hasMore` flag that says whether another page exists.

```bash
curl "https://localhost:<port>/api/notes?q=milk&page=1&size=10"
```

### Get a note

```bash
curl https://localhost:<port>/api/notes/<id>
```

Returns `404` if the note doesn't exist.

### Create a note

```bash
curl -X POST https://localhost:<port>/api/notes \
  -H "Content-Type: application/json" \
  -d '{"title":"First note","content":"Hello Postgres"}'
```

Returns `201 Created` with a `Location` header pointing to the new note.

### Update a note

```bash
curl -X PUT https://localhost:<port>/api/notes/<id> \
  -H "Content-Type: application/json" \
  -d '{"title":"Updated title","content":"Updated content"}'
```

Returns `404` if the note doesn't exist, and `409` if it was changed by someone else after it was loaded.

### Response format

Every response uses the same envelope:

```json
{
  "success": true,
  "message": null,
  "data": { },
  "errors": null,
  "timestamp": "2026-11-10T08:30:00Z"
}
```

Errors are returned in the same shape. Validation failures include a field-level `errors` dictionary, for example `{ "Title": ["Title is required."] }`.

### Error handling

A global exception handler converts exceptions into API responses:

| Exception | Status | Meaning |
|---|---|---|
| `ValidationException` | `400` | Request data failed validation |
| `NotFoundException` | `404` | The requested note doesn't exist |
| `DomainRuleException` | `400` | A domain rule rejected the change |
| `DbUpdateConcurrencyException` | `409` | The record was changed by someone else |
| Anything else | `500` | Logged as a server error, generic message returned |

## Database migrations

Create a migration after changing entities or their configuration:

```bash
dotnet ef migrations add <MigrationName> -p NotesApplication.Infrastructure
dotnet ef database update -p NotesApplication.Infrastructure
```

`NotesDbContextFactory` supplies the DbContext to the EF tools, so no startup project flag is needed. It reads the `NOTES_DB` environment variable if set, and otherwise falls back to the local development connection string.

## Design notes

- **Entities and value objects.** `Entity` compares by id, and `ValueObject` compares by value (for example `NoteTitle`).
- **Domain events.** Entities record events such as note created or updated, and the Application layer maps them to stored event rows.
- **Shadow properties.** `created_at`, `updated_at` and a `version` concurrency token are managed by the DbContext and aren't on the domain classes.
- **Read and write separation.** Writes go through `INoteService` and the unit of work. Reads go through `INoteQueries`, which returns DTOs.
- **Paging.** The list query uses offset paging with a `HasMore` flag instead of a total count.

## Troubleshooting

| Problem | Fix |
|---|---|
| Can't connect to PostgreSQL | Check that the PostgreSQL service is running, and that the port and connection string match your install |
| `dotnet ef` not found | Install it with `dotnet tool install --global dotnet-ef`, then reopen the terminal |
| Missing `Microsoft.EntityFrameworkCore.Relational` at runtime | Make sure all EF Core packages use the same version, then clean and rebuild |
| Unable to resolve a service | Check the registrations in `Program.cs` |