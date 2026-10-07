# How the Paint Tint Calculator Works

This guide explains how to start the application, how its database is created, what happens when you calculate or save a tint, and how to add shades and formulas.

## 1. The short version

There are two applications:

1. The **API** is the backend. It reads configuration, connects to SQLite, creates the database if it is missing, and supplies initial sample data.
2. The **WPF desktop app** is the user interface. It sends HTTP requests to the API; it does not connect directly to the database.

```text
You
 │ click/select
 ▼
WPF desktop app ── HTTP/JSON ──► API ── Entity Framework Core ──► SQLite database file
                                       │
                                       └── validates and calculates tint
```

## 2. Start the application

Open two PowerShell terminals at the repository root.

### Terminal 1: start the API

```powershell
dotnet run --project src\PaintTintCalculator.Api
```

The API uses the `http` launch profile, which listens on `http://localhost:5042`. Keep this terminal open while you use the desktop app.

At startup, the API:

1. Looks for `.env` in the repository or its parent directories and loads its settings.
2. Builds its configuration and registers the database provider and application services.
3. Runs `SeedDataService.InitializeAsync()`.
4. Starts listening for HTTP requests.

### Terminal 2: start the WPF app

```powershell
dotnet run --project src\PaintTintCalculator.Wpf
```

The WPF client reads `PaintTintCalculator__ApiBaseUrl` from `.env`. It uses `http://localhost:5042` if that setting is missing. The client checks the API, loads bases and shades, and requests a formula when a shade, base, or can size is selected.

## 3. Configuration and SQLite

The root `.env` file selects SQLite and gives the connection details:

```dotenv
AppConfig__DatabaseProvider=Sqlite
ConnectionStrings__DefaultConnection=Data Source=painttint.db
PaintTintCalculator__ApiBaseUrl=http://localhost:5042
```

The `AppConfig__DatabaseProvider` value tells the API which Entity Framework Core provider to use. Supported values are `SqlServer` and `Sqlite`. The connection string tells it which database file to use. This configuration uses a local SQLite file named `painttint.db` in the repository root.

SQLite keeps the database in a single file on disk, so you do not need a separate database server process. You can inspect or edit the data with SQLite tools or browser extensions, but the app manages the database automatically at startup.

The real `.env` file is ignored by Git. Keep passwords and other credentials out of source files and use a proper secret store for production.

## 4. How the database is created and seeded

The API registers `PaintTintDbContext` in `ServiceCollectionExtensions.AddInfrastructure`. That code reads the provider and connection string from configuration, then chooses `UseSqlServer` or `UseSqlite`.

At startup, `SeedDataService.InitializeAsync()` calls Entity Framework Core's `EnsureCreatedAsync()`:

- If the configured database does not exist, EF Core creates it and its tables from `PaintTintDbContext`.
- If the database already exists, EF Core leaves its schema in place.
- The seeder adds sample bases, colorants, and shades only when each corresponding table is empty.

The main tables are:

| Table | What it stores |
|---|---|
| `Bases` | Paint base names, maximum tint percentage, and price per litre |
| `Colorants` | Dispenser colorants, their codes, names, and cost per mL |
| `Shades` | Catalog colors: code, name, and display hex color |
| `FormulaItems` | The recipe rows connecting a shade, base, and colorant, with mL per litre |
| `DispenseJobs` | Saved dispense transactions |
| `DispenseJobItems` | The colorants, dispensed quantities, and costs captured in each transaction |

The relationships are important: one shade can have several formula rows; each row refers to one base and one colorant. For example, Ocean Mist can have one recipe for Pastel and a different recipe for Medium.

### Important: seed data is not a database update mechanism

The sample records are written in `SeedDataService`, but they are inserted only when their relevant table is empty. If you add a shade to the seeder after the database already contains shades, restarting the API will **not** insert that new shade. For existing databases, add records through SQL as described below (or implement an admin/API feature).

This project currently uses `EnsureCreated`, not EF Core migrations. `EnsureCreated` is suitable for this sample setup, but it does not evolve an existing database schema when the model changes. Back up important data before changing the schema; a future production version should use migrations.

## 5. End-to-end flow when using the calculator

### Load the catalog

When the WPF window opens, `MainViewModel.InitializeAsync()` asks the API for the available bases and shades. The WPF `ApiClient` sends HTTP requests; the API controllers delegate work to Application handlers; those handlers use repository interfaces implemented by Infrastructure.

### Calculate a tint

When you choose a shade, base, and can size:

1. WPF sends `POST /api/tint/calculate` with the selected IDs and can size.
2. `TintController` checks that the IDs and can size are positive.
3. `CalculateTintCommandHandler` loads the shade, its formula rows, and the selected base.
4. `TintCalculationService` selects formula rows for that base. If no rows exist, the API returns a formula-not-found error; it does not invent a zero formula.
5. For every colorant row, it multiplies mL per litre by can size, then rounds to the dispenser increment of `0.05 mL`.
6. It calculates total tint percentage and cost, and checks that the base's maximum tint percentage is not exceeded.
7. The API returns the complete breakdown to WPF for display.

For example, a formula row of `5 mL/L` scales to `20 mL` for a `4 L` can before dispenser rounding. The exact output always uses the formula in the database and the chosen base.

### Save a dispense job

When you press the dispense/save button:

1. WPF sends `POST /api/dispense-jobs` with the shade ID, base ID, and can size.
2. The API loads the current data and recalculates the formula itself. It does not trust totals supplied by the desktop client.
3. It saves a `DispenseJobs` record plus `DispenseJobItems` containing the actual dispensed amounts and costs.
4. The API returns the saved job ID and summary to WPF.

The saved item costs are historical snapshots; later changes to colorant pricing do not rewrite older dispense jobs.

## 6. Add a new shade and formula to the existing database

Open the SQLite database in a SQLite browser or run `sqlite3 painttint.db` from the repository root. First check which base and colorant IDs exist:

```sql
SELECT Id, Name, MaxTintPercent, PricePerLitre FROM Bases ORDER BY Id;
SELECT Id, Code, Name, CostPerMl FROM Colorants ORDER BY Id;
```

Then insert the shade and its formula rows. Replace the sample code, name, hex value, IDs, and quantities with your actual data. The example uses existing base and colorant IDs from the default seed data.

```sql
INSERT INTO Shades (Code, Name, HexColor)
VALUES ('NEW-001', 'New Shade', '#AABBCC');

SELECT last_insert_rowid() AS ShadeId;
```

Use the returned `ShadeId` value in the next query:

```sql
INSERT INTO FormulaItems (ShadeId, BaseId, ColorantId, MlPerLitre)
VALUES
    (1, 1, 3, 5.0000),
    (1, 1, 1, 2.5000);
```

Each `FormulaItems` row is one colorant in one base recipe:

- `ShadeId`: the shade being formulated.
- `BaseId`: the paint base this recipe works with.
- `ColorantId`: one existing dispenser colorant.
- `MlPerLitre`: amount of that colorant for each litre of paint.

To add a recipe for another base to the **same shade**, keep its `ShadeId` but use the other base's ID and the corresponding colorant rows:

```sql
INSERT INTO FormulaItems (ShadeId, BaseId, ColorantId, MlPerLitre)
VALUES
    (1, 2, 3, 35.0000),
    (1, 2, 1, 15.0000);
```

### Check the tint limit before adding a formula

The formula amounts are per litre. Add the colorants' `MlPerLitre` values for a base recipe and check the total against that base's limit. A 2% limit permits up to 20 mL/L; 6% permits up to 60 mL/L; 12% permits up to 120 mL/L. The API checks the final scaled and rounded amounts again during calculation.

### Add a completely new colorant (if needed)

First insert it in `Colorants`, then use the generated ID in the formula:

```sql
INSERT INTO Colorants (Code, Name, CostPerMl)
VALUES ('C05', 'New Colorant', 1.0500);

SELECT Id, Code, Name FROM Colorants WHERE Code = 'C05';
```

Use the returned ID in `FormulaItems`. The colorant code must be unique.

After inserting records, restart the API if it is not already running, then use **Refresh** or restart the WPF app. Shade search reads from the database, so the new shade should appear. A formula is only available for the base IDs for which you inserted `FormulaItems`; other bases will report that no formula exists.

## 7. Where to look in the source

| Concern | File |
|---|---|
| API startup and `.env` loading | `src/PaintTintCalculator.Api/Program.cs` |
| Database provider and service registration | `src/PaintTintCalculator.Api/Extensions/ServiceCollectionExtensions.cs` |
| Entity/table relationships | `src/PaintTintCalculator.Infrastructure/Persistence/PaintTintDbContext.cs` |
| Database creation and initial sample data | `src/PaintTintCalculator.Infrastructure/Seed/SeedDataService.cs` |
| HTTP endpoints | `src/PaintTintCalculator.Api/Controllers/` |
| Formula calculation | `src/PaintTintCalculator.Application/Services/TintCalculationService.cs` |
| WPF API communication | `src/PaintTintCalculator.Wpf/Services/ApiClient.cs` |
| WPF screen state and user actions | `src/PaintTintCalculator.Wpf/ViewModels/MainViewModel.cs` |

