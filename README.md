# Notes Application

A notes REST API built with ASP.NET Core, structured in layers (Domain, Application, Infrastructure, API) and backed by PostgreSQL. Creating a note queues a confirmation email through the outbox pattern, and a background worker sends it.

## Tech stack

- .NET 10 / ASP.NET Core (controllers)
- Entity Framework Core with Npgsql (PostgreSQL)
- FluentValidation
- OpenAPI with Swagger UI
- Background worker with the transactional outbox pattern for emails
- Docker (PostgreSQL and a fake mail server for local development)

## Project structure

```
NotesApplication/
├── NotesApplication.Domain/          # Entities, value objects, domain events, outbox message, rules
├── NotesApplication.Application/     # Services, queries, DTOs, validators, outbox and email contracts
├── NotesApplication.Infrastructure/  # EF Core DbContext, configurations, migrations, repositories,
│                                     # outbox worker/processor, SMTP sender
└── NotesApplication.Api/             # Controllers, middleware, API models, startup
```

Dependencies point inward: `Api` and `Infrastructure` depend on `Application`, and `Application` depends on `Domain`. The Domain has no dependency on any other project.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop/)
- EF Core CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- Optional: [pgAdmin 4](https://www.pgadmin.org/) to inspect the database

## Getting started

### 1. Start PostgreSQL and the mail server

Create a `docker-compose.yml` in the solution folder:

```yaml
services:
  db:
    image: postgres:17
    container_name: notes-db
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: notesdb
    ports:
      - "5432:5432"
    volumes:
      - notes-db-data:/var/lib/postgresql/data

  mail:
    image: axllent/mailpit
    container_name: notes-mail
    ports:
      - "1025:1025"   # SMTP
      - "8025:8025"   # inbox UI

volumes:
  notes-db-data:
```

```bash
docker compose up -d
```

- PostgreSQL: `localhost:5432`
- Mailpit inbox: http://localhost:8025 (SMTP on port `1025`)

If you already created a container named `notes-db` with `docker run`, remove it first with `docker rm -f notes-db`, or the container name will clash.

### 2. Configure the application

In `NotesApplication.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "NotesDb": "Host=localhost;Port=5432;Database=notesdb;Username=postgres;Password=postgres"
  },
  "Email": {
    "Host": "localhost",
    "Port": 1025,
    "EnableSsl": false,
    "From": "notes@localhost",
    "DefaultRecipient": "you@example.com"
  },
  "Outbox": {
    "PollIntervalSeconds": 5,
    "BatchSize": 20,
    "MaxAttempts": 10
  }
}
```

These values are for local development only. Don't commit real secrets. Use user secrets or environment variables instead.

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
| `POST` | `/api/notes` | Create a note and queue a confirmation email |
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

Returns `201 Created` with a `Location` header pointing to the new note. A confirmation email is queued in the same transaction and sent shortly after by the background worker (see [Email queue](#email-queue-outbox-pattern)).

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
  "timestamp": "2026-11-22T08:30:00Z"
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

## Email queue (outbox pattern)

Creating a note must not fail because a mail server is down, and it must not silently lose an email. The outbox pattern handles both.

```
POST /api/notes
  → NoteService.CreateAsync
      one transaction: Note row + OutboxMessage row

OutboxWorker (every 5 seconds)
  → OutboxProcessor: due Pending messages → send email → Done, or retry later
```

How it works:

1. **Write.** `NoteService` saves the note and an `outbox_messages` row in the same unit of work. If the save fails, both roll back. If it succeeds, both exist.
2. **Poll.** `OutboxWorker` is a `BackgroundService` that wakes every `PollIntervalSeconds`. It creates a new DI scope per poll, because the `DbContext` is scoped.
3. **Send.** `OutboxProcessor` loads `Pending` messages whose `next_attempt_at` is due, sends each through `IEmailSender` (SMTP), and saves after every message.
4. **Success.** The message is marked `Done`.
5. **Failure.** `attempts` increases, the error is stored in `last_error`, and the next attempt is scheduled with backoff: 5s, 10s, 20s, 40s, then 60s each.
6. **Give up.** After `MaxAttempts` the message is marked `Failed` and stays in the table. Retrying failed messages from an API endpoint is not implemented yet.

### Testing it locally

1. Start the stack (`docker compose up -d`) and the API, then create a note. The email appears in the Mailpit inbox at http://localhost:8025 within about 5 seconds.
2. Stop the mail server and create 3 notes:
   ```bash
   docker compose stop mail
   ```
   The rows stay `Pending`, `attempts` climbs, and `last_error` shows the connection failure.
3. Start it again and watch all 3 emails arrive:
   ```bash
   docker compose start mail
   ```
4. Inspect the queue in pgAdmin or `psql`:
   ```sql
   SELECT id, status, attempts, next_attempt_at, last_error
   FROM outbox_messages
   ORDER BY occurred_at;
   ```

With the default settings, a message is retried for about 6 minutes before it becomes `Failed`. Lower `MaxAttempts` if you want to see the `Failed` state quickly.

### Delivery guarantees

- **No email is lost.** The outbox row commits with the note, and the worker retries until the send succeeds or the message is marked `Failed`.
- **Delivery is at-least-once.** If the app stops after the mail server accepted a message but before the `Done` status is saved, that message is sent again after restart. The worker narrows this window by saving after every message, saving `Done` without the shutdown cancellation token, and running as a single instance. Every retry carries the same `Message-ID` header, so a duplicate is easy to spot.
- **One instance only.** Running several API instances could make two workers pick up the same row. That would need row claiming (`FOR UPDATE SKIP LOCKED`).

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
- **Shadow properties.** `created_at`, `updated_at` and a `version` concurrency token are managed by the DbContext and aren't on the domain classes. `outbox_messages` manages its own timestamps and has no shadow columns.
- **Read and write separation.** Writes go through `INoteService` and the unit of work. Reads go through `INoteQueries`, which returns DTOs.
- **Paging.** The list query uses offset paging with a `HasMore` flag instead of a total count.
- **Outbox.** `IOutbox` and `IEmailSender` are defined in the Application layer. Infrastructure implements them, together with the worker (when to run) and the processor (what to do).

## Troubleshooting

| Problem | Fix |
|---|---|
| Can't connect to PostgreSQL | Run `docker ps` to check the `notes-db` container, and check the port and connection string |
| `dotnet ef` not found | Install it with `dotnet tool install --global dotnet-ef`, then reopen the terminal |
| Missing `Microsoft.EntityFrameworkCore.Relational` at runtime | Make sure all EF Core packages use the same version, then clean and rebuild |
| Unable to resolve a service | Check the registrations in `Program.cs` and `AddInfrastructure` |
| Emails never arrive | Check that the `notes-mail` container is running, that `Email:Port` matches (`1025` for Mailpit), and that `AddHostedService<OutboxWorker>()` is registered. Look at `status` and `last_error` in `outbox_messages` |
| Messages stay `Pending` | The mail server is unreachable, so the worker is retrying with backoff. Check `last_error` and `next_attempt_at` |
