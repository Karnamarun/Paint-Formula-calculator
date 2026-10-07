# Paint Tint Formula Calculator

A professional, production-grade paint tinting formula calculation and dispensing system built using **Clean Architecture**, **SOLID principles**, and **.NET**.

---

## 🎨 System Highlights

- **Clean Architecture**: Strict inward dependency flow with 100% decoupled Domain layer.
- **Precision Formula Engine**: Scaled to can size (`1L`, `4L`, `10L`, `20L`) with dispenser precision rounding (`0.05 ml`).
- **Base Tint Constraints**: Automated enforcement of base tint ceilings (Pastel 2%, Medium 6%, Deep 12%).
- **Financial Precision**: All monetary values and measurements utilize C# `decimal` (zero floating point inaccuracies).
- **Server-Side Recalculation**: Server independently validates and recalculates formulas before recording dispense jobs with historical cost snapshots.
- **WPF MVVM Desktop Client**: Modern responsive interface with live preview swatches, async API communication, and centralized styling.
- **Comprehensive Test Suite**: 62 automated unit and integration tests across Domain, Application, and API layers.

---

## 🏛️ Clean Architecture Breakdown

The solution consists of decoupled projects respecting the Clean Architecture dependency rule:

```
PaintTintCalculator/
├── src/
│   ├── PaintTintCalculator.Domain/           # Pure C# Business Rules, Entities, Value Objects, Exceptions
│   ├── PaintTintCalculator.Application/      # Use Cases, Handlers, DTOs, Persistence Abstractions, Services
│   ├── PaintTintCalculator.Infrastructure/   # EF Core DbContext, Repositories, SQLite/SQL Server, Seeder
│   ├── PaintTintCalculator.Api/              # ASP.NET Core Web API Composition Root, Controllers, Middleware
│   └── PaintTintCalculator.Wpf/              # WPF Desktop App (MVVM, ApiClient, Theme Resources)
└── tests/
    ├── PaintTintCalculator.Domain.Tests/      # xUnit tests for core calculation and domain rules
    ├── PaintTintCalculator.Application.Tests/ # xUnit tests for use case orchestration
    └── PaintTintCalculator.Api.Tests/         # xUnit WebApplicationFactory API integration tests
```

### Dependency Flow
```
WPF Client ──[HTTP/JSON]──► ASP.NET Core API
                                 │
                                 ├──► Application ──► Domain
                                 │         ▲
                                 └──► Infrastructure
```

- **Domain** knows nothing about Infrastructure, API, or WPF.
- **Application** depends solely on Domain.
- **Infrastructure** implements Application repository interfaces.
- **API** controllers are thin, delegating directly to use cases without touching `DbContext`.
- **WPF** connects exclusively over HTTP/JSON (no database or backend library references).

---

## 🚀 Quick Start Guide

### Prerequisites
- [.NET SDK 8.0 or later](https://dotnet.microsoft.com/download) (Compatible with .NET 8, 9, and 10).

### 1. Run the Automated Test Suite
To execute all 62 unit and integration tests:

```bash
dotnet test PaintTintCalculator.sln
```

### 2. Start the ASP.NET Core Web API
Run the backend server (starts on `http://localhost:5000` or `https://localhost:5001`):

```bash
dotnet run --project src/PaintTintCalculator.Api
```

> **Note**: On startup, the database `painttint.db` is automatically created and seeded with default Bases, Colorants, and Shades.

### 3. Start the WPF Desktop Client
In a separate terminal (on Windows, or via cross-targeting):

```bash
dotnet run --project src/PaintTintCalculator.Wpf
```

---

## 📋 API Endpoints Specification

### 1. Search Shades
```http
GET /api/shades?search=Ocean
```
**Response (200 OK):**
```json
[
  {
    "id": 1,
    "code": "OM-201",
    "name": "Ocean Mist",
    "hexColor": "#7FA7B5"
  }
]
```

### 2. Get Available Bases
```http
GET /api/shades/bases
```
**Response (200 OK):**
```json
[
  { "id": 1, "name": "Pastel", "maxTintPercent": 2.0, "pricePerLitre": 250.0 },
  { "id": 2, "name": "Medium", "maxTintPercent": 6.0, "pricePerLitre": 270.0 },
  { "id": 3, "name": "Deep", "maxTintPercent": 12.0, "pricePerLitre": 290.0 }
]
```

### 3. Calculate Tint Formula
```http
POST /api/tint/calculate
Content-Type: application/json

{
  "shadeId": 1,
  "baseId": 1,
  "canSizeLitres": 4.0
}
```
**Response (200 OK):**
```json
{
  "shadeId": 1,
  "shadeCode": "OM-201",
  "shadeName": "Ocean Mist",
  "hexColor": "#7FA7B5",
  "baseId": 1,
  "baseName": "Pastel",
  "basePricePerLitre": 250.00,
  "canSizeLitres": 4.0,
  "items": [
    {
      "colorantId": 3,
      "colorantCode": "C03",
      "colorantName": "Phthalo Blue",
      "mlPerLitre": 5.0,
      "dispensedMl": 20.0,
      "costPerMl": 1.2,
      "totalCost": 24.0
    },
    {
      "colorantId": 1,
      "colorantCode": "C01",
      "colorantName": "Black",
      "mlPerLitre": 2.5,
      "dispensedMl": 10.0,
      "costPerMl": 0.8,
      "totalCost": 8.0
    }
  ],
  "totalColorantMl": 30.0,
  "tintPercent": 0.75,
  "maxTintPercent": 2.0,
  "baseCost": 1000.0,
  "colorantCost": 32.0,
  "totalPrice": 1032.0
}
```

### 4. Create Dispense Job
```http
POST /api/dispense-jobs
Content-Type: application/json

{
  "shadeId": 1,
  "baseId": 1,
  "canSizeLitres": 4.0
}
```
**Response (201 Created):**
```json
{
  "id": 1,
  "shadeId": 1,
  "shadeCode": "OM-201",
  "shadeName": "Ocean Mist",
  "baseId": 1,
  "baseName": "Pastel",
  "canSizeLitres": 4.0,
  "totalColorantMl": 30.0,
  "tintPercent": 0.75,
  "totalPrice": 1032.0,
  "createdAt": "2026-10-07T06:20:00Z",
  "items": [...]
}
```

---

## 📌 Documented Business Assumptions

1. **Currency**:
   - The UI display and assignment mockups display `₹` while specifications mention SAR.
   - The system supports configurable currency formatting (defaulting to `₹`) configured in `appsettings.json` and WPF resources.
2. **Downstream Rounding Propagation**:
   - The formula scaling scales `MlPerLitre × CanSizeLitres`, which is rounded to the nearest `0.05 ml` dispenser increment.
   - All subsequent calculations (total colorant, tint percentage, cost per colorant, and persisted historical records) use the **actual rounded dispensed amount**.
3. **Missing Formula**:
   - When a shade does not have a formula configured for a selected base, the API returns a structured `404 Not Found` with code `FORMULA_NOT_FOUND` rather than silently computing zero.
4. **Historical Price Snapshot**:
   - When saving `DispenseJobItem`, the dispensed milliliters and historical cost are preserved permanently, ensuring future colorant price updates do not alter past transactions.

---

## 🧪 Live Demo Customization Quick Reference

| Demo Request | File to Edit | Modification |
|---|---|---|
| **Change dispenser precision to 0.10 ml** | `src/PaintTintCalculator.Domain/Rules/TintCalculationRules.cs` | Change `DispenserPrecisionMl = 0.10m;` |
| **Add a new can size (e.g. 2.5L)** | `src/PaintTintCalculator.Domain/ValueObjects/CanSize.cs` | Add `2.5m` to `SupportedLitres` array |
| **Change base tint limit** | `src/PaintTintCalculator.Infrastructure/Seed/SeedDataService.cs` | Update `MaxTintPercent` for base |

---

## 📑 Architecture Checklist Verification

- [x] Domain has zero Infrastructure / API / WPF dependencies
- [x] Application depends only on Domain
- [x] Infrastructure implements Application interfaces
- [x] API controllers contain no business logic and no DbContext references
- [x] WPF connects exclusively over HTTP/JSON using MVVM
- [x] Strictly `decimal` used for measurements and currency
- [x] Server recalculates all figures before recording jobs
- [x] 62 automated unit and integration tests passing

