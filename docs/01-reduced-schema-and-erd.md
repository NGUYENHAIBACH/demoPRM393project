# Dormio (PRN232) — Reduced schema and ERD

Reduced from the original Dormio schema (44 models, 34 enums). The original code is untouched.

## 1. Tables kept (14 tables = 3 Identity + 11 domain)

| # | Table | Source (Prisma) | Notes |
|---|-------|-----------------|-------|
| 1 | AspNetUsers | User (+UserIdentification, BankAccount dropped) | Extends `IdentityUser<Guid>`: FullName, IsActive |
| 2 | AspNetRoles | `UserRole` enum | Roles: Admin, Landlord, Tenant (leasing_agent, employee dropped) |
| 3 | AspNetUserRoles | — | Identity many-to-many |
| 4 | BoardingHouses | BoardingHouse | Address collapsed into one column |
| 5 | Rooms | Room (+RoomType merged) | `UNIQUE(BoardingHouseId, RoomNumber)`, BasePrice added |
| 6 | Services | Service | Electricity/water (metered) and flat fees |
| 7 | RoomServices | RoomService | M:N Room–Service |
| 8 | Contracts | Contract (+Deposit merged) | `DepositAmount` column replaces the Deposit table |
| 9 | TenantContracts | TenantContract | M:N Contract–Tenant, `IsPrimary` |
| 10 | MeterReadings | MeterReading (+History dropped) | `UNIQUE(Room, Service, Month, Year)` |
| 11 | Invoices | Invoice | `UNIQUE(Contract, Month, Year)` |
| 12 | InvoiceItems | InvoiceItem | Rent line has `ServiceId = NULL` |
| 13 | Payments | Payment | payOS/VietQR fields reduced to Method + TransactionRef |
| 14 | Grievances | Grievance (+GrievanceImage dropped) | Complaint / inquiry / feedback workflow |

Dropped for scope: Post/BHRP listing module, Subscription, Chat/AI, Employee/Schedule/Attendance, Notification, Asset, Expense, Otp, AuditLog.

Rules kept: money is `decimal(12,2)`; Invoice/Payment/Contract are never hard-deleted (FK `Restrict`, status transitions only); boarding houses are soft-deleted via `IsActive`.

## 2. ERD

```mermaid
erDiagram
    AspNetUsers ||--o{ AspNetUserRoles : has
    AspNetRoles ||--o{ AspNetUserRoles : has
    AspNetUsers ||--o{ BoardingHouses : owns
    BoardingHouses ||--o{ Rooms : contains
    BoardingHouses ||--o{ Services : defines
    Rooms ||--o{ RoomServices : uses
    Services ||--o{ RoomServices : "applied to"
    Rooms ||--o{ Contracts : "rented by"
    Contracts ||--o{ TenantContracts : has
    AspNetUsers ||--o{ TenantContracts : signs
    Rooms ||--o{ MeterReadings : measured
    Services ||--o{ MeterReadings : "metered by"
    Invoices |o--o{ MeterReadings : bills
    Contracts ||--o{ Invoices : generates
    Invoices ||--|{ InvoiceItems : contains
    Services |o--o{ InvoiceItems : "line for"
    Invoices ||--o{ Payments : "paid by"
    AspNetUsers |o--o{ Payments : pays
    AspNetUsers ||--o{ Grievances : submits
    Rooms |o--o{ Grievances : about
    AspNetUsers |o--o{ Grievances : resolves

    AspNetUsers { guid Id PK  string FullName  string Email  string PhoneNumber  bool IsActive  datetime CreatedAt }
    AspNetRoles { guid Id PK  string Name }
    BoardingHouses { guid Id PK  guid OwnerId FK  string Name  string Address  string Description  bool IsActive }
    Rooms { guid Id PK  guid BoardingHouseId FK  string RoomNumber  int Floor  decimal Area  int MaxOccupants  decimal BasePrice  int Status }
    Services { guid Id PK  guid BoardingHouseId FK  string Name  decimal Price  string Unit  bool IsMetered  bool IsActive }
    RoomServices { guid RoomId PK  guid ServiceId PK }
    Contracts { guid Id PK  guid RoomId FK  decimal RentPrice  decimal DepositAmount  int MonthlyPaymentDay  date StartDate  date EndDate  int Status }
    TenantContracts { guid ContractId PK  guid TenantId PK  bool IsPrimary }
    MeterReadings { guid Id PK  guid RoomId FK  guid ServiceId FK  int BillingMonth  int BillingYear  decimal PreviousValue  decimal CurrentValue  guid InvoiceId FK }
    Invoices { guid Id PK  guid ContractId FK  int BillingMonth  int BillingYear  decimal TotalAmount  int Status  date DueDate }
    InvoiceItems { guid Id PK  guid InvoiceId FK  guid ServiceId FK  string Description  decimal Quantity  decimal UnitPrice  decimal Amount }
    Payments { guid Id PK  guid InvoiceId FK  guid PayerId FK  decimal Amount  int Method  int Status  string TransactionRef  datetime PaidAt }
    Grievances { guid Id PK  guid TenantId FK  guid RoomId FK  int Type  string Title  int Priority  int Status  guid ResolvedById FK }
```

## 3. Use cases kept (mapped to the original spec)

| Actor | Use cases (original UC) |
|-------|-------------------------|
| Landlord | Boarding house CRUD (UC-L-01..), rooms, services, contracts, meter readings, invoices, record payments, handle grievances |
| Tenant | View contract and invoices, pay, submit grievance (UC-T-01..07 subset) |
| Admin | User management, view all grievances (UC-A subset) |

## 4. Architecture

- `src/Dormio.Api` — ASP.NET Core 8 Web API, EF Core 8 (SQL Server), ASP.NET Identity, JWT + role policies, OData (`/odata/Rooms`), Swagger.
- `src/Dormio.Web` — ASP.NET Core MVC, Bootstrap 5, TagHelper (`<status-badge>`), PartialView (`_BoardingHouseCard`), cookie sign-in whose claims/roles come from the API's JWT.
- Implemented vertical slice: Auth (register/login), BoardingHouses (REST CRUD), Rooms (OData read). Remaining modules follow the same pattern.
