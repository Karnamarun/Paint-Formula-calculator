# Paint Tint Formula Calculator — Architecture Specification

## 1. Clean Architecture Overview

The system strictly adheres to **Clean Architecture** and the **Dependency Inversion Principle**. All dependencies point inward toward the Domain model.

```mermaid
flowchart TD
    subgraph Presentation
        WPF["WPF MVVM Desktop App"]
        API["ASP.NET Core Web API"]
    end

    subgraph Application["Application Layer (Use Cases & Orchestration)"]
        UC["Queries / Commands / DTOs / Interfaces / Services"]
    end

    subgraph Domain["Domain Layer (Enterprise Business Rules)"]
        ENT["Entities (Base, Colorant, Shade, FormulaItem, DispenseJob)"]
        VO["Value Objects (CanSize)"]
        RULES["Rules (TintCalculationRules, PricingRules)"]
    end

    subgraph Infrastructure["Infrastructure Layer (Persistence)"]
        EF["EF Core & DbContext"]
        REPO["Repositories & UnitOfWork"]
        SEED["Database Seeder"]
    end

    WPF -- "HTTP / JSON" --> API
    API --> Application
    API --> Infrastructure
    Infrastructure --> Application
    Infrastructure --> Domain
    Application --> Domain
```

### Dependency Rules Enforced
1. **Domain**: Pure C#. Zero dependencies on EF Core, ASP.NET Core, SQL Server, WPF, or external libraries.
2. **Application**: Depends only on Domain. Declares interfaces (`IShadeRepository`, `IBaseRepository`, `IDispenseJobRepository`, `IUnitOfWork`, `ITintCalculationService`) and orchestrates use cases.
3. **Infrastructure**: Implements persistence abstractions defined in Application using EF Core and SQLite/SQL Server.
4. **API**: Composition root registering DI dependencies, exposing thin controllers, and translating domain exceptions into standard HTTP error responses.
5. **WPF**: Autonomous presentation layer using MVVM. Communicates exclusively with the API via asynchronous HTTP JSON calls.

---

## 2. Business Rules & Calculation Flow

```mermaid
flowchart LR
    A["Raw MlPerLitre × CanSizeLitres"] --> B["Round to nearest 0.05 ml (DispensedMl)"]
    B --> C["Total Colorant Ml = Σ DispensedMl"]
    C --> D["Tint % = TotalColorant / (CanSize × 1000) × 100"]
    D --> E{"Tint % ≤ Base Max % ?"}
    E -- Yes --> F["Compute Base Cost & Colorant Costs"]
    E -- No --> G["Throw TintLimitExceededException (400 Bad Request)"]
    F --> H["Total Price = Base Cost + Colorant Cost (Rounded 2 Decimals)"]
    H --> I["Persist Historical Snapshot in DispenseJobItem"]
```

### Dispenser Precision & Downstream Calculations
- **Rounding Step**: Raw formula scaled values are rounded to the nearest `0.05 ml` using `Math.Round(amount / 0.05m, MidpointRounding.AwayFromZero) * 0.05m`.
- **Downstream Propagation**: Downstream calculations (total colorant, tint percentage, item cost, and persisted dispense job items) strictly utilize the **rounded dispensed volume**.

### Maximum Tint Percentages by Base
- **Pastel Base**: Maximum allowed tint is `2.00%` (`20 ml/L`).
- **Medium Base**: Maximum allowed tint is `6.00%` (`60 ml/L`).
- **Deep Base**: Maximum allowed tint is `12.00%` (`120 ml/L`).

### Decimal Accuracy
All quantities, monetary costs, percentages, and measurements strictly utilize C# `decimal` (`128-bit` IEEE 754-2008 precision) to eliminate floating-point rounding errors.

---

## 3. Database Schema & Relationships

```mermaid
erDiagram
    Bases ||--o{ FormulaItems : "has"
    Bases ||--o{ DispenseJobs : "used in"
    Shades ||--o{ FormulaItems : "contains"
    Shades ||--o{ DispenseJobs : "dispensed in"
    Colorants ||--o{ FormulaItems : "referenced in"
    Colorants ||--o{ DispenseJobItems : "dispensed"
    DispenseJobs ||--|{ DispenseJobItems : "contains (cascade delete)"

    Bases {
        int Id PK
        string Name UK
        decimal MaxTintPercent
        decimal PricePerLitre
    }

    Colorants {
        int Id PK
        string Code UK
        string Name
        decimal CostPerMl
    }

    Shades {
        int Id PK
        string Code UK
        string Name
        string HexColor
    }

    FormulaItems {
        int Id PK
        int ShadeId FK
        int BaseId FK
        int ColorantId FK
        decimal MlPerLitre
    }

    DispenseJobs {
        int Id PK
        int ShadeId FK
        int BaseId FK
        decimal CanSizeLitres
        decimal TotalColorantMl
        decimal TintPercent
        decimal TotalPrice
        DateTimeOffset CreatedAt
    }

    DispenseJobItems {
        int Id PK
        int DispenseJobId FK
        int ColorantId FK
        decimal DispensedMl
        decimal Cost
    }
```

---

## 4. Live Demo Readiness Guide

During technical evaluation, small on-the-spot changes can be applied cleanly:

### Scenario 1: Change Dispenser Precision (e.g. from 0.05 ml to 0.10 ml)
- **Single Source of Truth**:
  Open [TintCalculationRules.cs](file:///home/parthiban/Desktop/Assignment/src/PaintTintCalculator.Domain/Rules/TintCalculationRules.cs) and modify:
  ```csharp
  public const decimal DispenserPrecisionMl = 0.10m;
  ```
  Both API and tests immediately utilize the updated precision.

### Scenario 2: Add a New Can Size (e.g. 5L or 2.5L)
- **Single Source of Truth**:
  Open [CanSize.cs](file:///home/parthiban/Desktop/Assignment/src/PaintTintCalculator.Domain/ValueObjects/CanSize.cs) and add the size to `SupportedLitres`:
  ```csharp
  public static readonly IReadOnlyList<decimal> SupportedLitres = new[] { 1.0m, 2.5m, 4.0m, 10.0m, 20.0m };
  ```
  Add the corresponding option in WPF [ClientModels.cs](file:///home/parthiban/Desktop/Assignment/src/PaintTintCalculator.Wpf/Models/ClientModels.cs).

### Scenario 3: Update Medium Base Maximum Tint Percentage
- **Persistence / Seeder**:
  Adjust value in [SeedDataService.cs](file:///home/parthiban/Desktop/Assignment/src/PaintTintCalculator.Infrastructure/Seed/SeedDataService.cs) or update the database record. The calculation logic reads `baseEntity.MaxTintPercent` dynamically and enforces limits without requiring any code changes in controllers or ViewModels.

