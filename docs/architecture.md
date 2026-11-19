# Architecture Decisions

**Project:** CRM Al-Mohtaref (educational institution CRM)
**Task:** SCRUM-9 · **Status:** Draft v1
**Scope:** Core architectural decisions and tech-stack constraints for the web platform.

> **Legend:** ✅ Implemented in the repo · 🟡 Decided, not yet implemented · ⚠️ Open issue / needs a decision

---

## 1. Overview

CRM Al-Mohtaref is a web application split into two independently deployable parts that communicate only through a REST API:

| Part | Location | Stack |
|---|---|---|
| Backend | `src/Server` | ASP.NET Core, Clean Architecture, EF Core, SQL Server |
| Frontend | `src/Client` | React + TypeScript (Vite), feature-based structure |

Business scope (from the SRS in `docs/`): clients, campaigns, communications, issues/complaints, notifications, users and academic records. Requirements are referenced as `FR-xx`.

## 2. Tech Stack

### Backend
| Concern | Choice | Status |
|---|---|---|
| Framework | ASP.NET Core Web API | ✅ |
| API docs | Swashbuckle (Swagger UI) 6.6.2, enabled in Development only | ✅ |
| ORM | Entity Framework Core | 🟡 package not added yet |
| Database | **SQL Server** | 🟡 |
| CI | GitHub Actions (`.github/workflows/dotnet-ci.yml`) | ✅ |

### Frontend
| Concern | Choice |
|---|---|
| Build / language | Vite 8, TypeScript, React 19 |
| Routing | react-router-dom 7 |
| Server state | TanStack Query |
| Client state | Zustand (session and UI state only) |
| Forms / validation | React Hook Form + Zod (types derived from schemas) |
| HTTP | axios (isolated in `src/api`) |
| Tables | TanStack Table |
| Styling / UI | Tailwind CSS 4, shadcn/ui (`components.json`), `tw-animate-css`, lucide-react, sonner (toasts) |
| i18n | i18next + react-i18next (Arabic and English), Cairo font |
| API mocking | MSW, with `@faker-js/faker` for data |
| API types | `openapi-typescript` (generate types from the backend's Swagger) |
| Quality | oxlint, prettier |

## 3. Backend Architecture (Clean Architecture)

Solution: `src/CRM-Al-Mohtaref.sln`

```
Api ───────► Application ───────► Domain
 │                ▲                  ▲
 └─► Infrastructure ─────────────────┘
```

Dependency rule: dependencies point inward. `Domain` depends on nothing. `Application` depends on `Domain` only. `Infrastructure` implements the interfaces defined in `Domain` / `Application`. `Api` is the composition root and wires everything together.

| Project | Responsibility | Current folders |
|---|---|---|
| `CRM.AlMohtaref.Domain` | Entities and repository interfaces. No external packages. | `Entities/`, `Interfaces/` |
| `CRM.AlMohtaref.Application` | Use cases and business logic, organized by feature. | `Features/`, `Common/Behaviors/`, `DependencyInjection.cs` |
| `CRM.AlMohtaref.Infrastructure` | EF Core `DbContext`, migrations, repository implementations, external services. | `Data/`, `Migrations/`, `Repositories/`, `ExternalServices/`, `DependencyInjection.cs` |
| `CRM.AlMohtaref.Api` | HTTP layer: controllers, request/response contracts, middleware, startup. | `Controllers/`, `Contracts/`, `Middlewares/`, `Program.cs` |

Each of `Application` and `Infrastructure` exposes a `DependencyInjection.cs` extension (e.g. `AddApplication()`, `AddInfrastructure()`) that `Program.cs` calls.

**Current state:** the solution is a scaffold. Folders exist but contain only `.gitkeep`. `Program.cs` is the default template (controllers, Swagger, HTTPS redirection, authorization middleware). No entities, controllers, DbContext or migrations exist yet.

### Known issues to resolve ⚠️
1. **Target framework mismatch.** `Api` targets `net8.0` while `Domain`, `Application` and `Infrastructure` target `net10.0`. A `net8.0` project cannot reference `net10.0` projects. Decision needed: align everything to one version (recommended: `net10.0`, since three of four projects already use it).
2. **`Api` has no project references yet.** It must reference `Application` and `Infrastructure` (the composition root) before it can register services.
3. **`Domain/DependencyInjection.cs`** is an empty placeholder class. The Domain layer should not register services. Remove it.
4. **Infrastructure has no EF Core packages yet.** Add `Microsoft.EntityFrameworkCore.SqlServer` and `Microsoft.EntityFrameworkCore.Design` (the latter in the startup project for migrations).

## 4. API Structure

Decisions for the API layer (🟡 to be applied as controllers are written):

- **Style:** RESTful JSON over HTTPS.
- **Routing:** `api/v1/<resource>` with plural, lowercase resource names, e.g. `GET /api/v1/clients`, `POST /api/v1/campaigns`.
- **Verbs:** `GET` read, `POST` create, `PUT`/`PATCH` update, `DELETE` remove (or soft-delete where the domain requires history).
- **Contracts:** controllers accept and return DTOs from `Api/Contracts`. Domain entities are never exposed directly.
- **Controllers stay thin:** they map the request, call the Application layer, and map the result. No business logic in controllers.
- **Status codes:** `200` OK, `201` Created (with `Location`), `204` No Content, `400` validation, `401` unauthenticated, `403` unauthorized, `404` not found, `409` conflict, `500` unexpected.
- **Errors:** a single global exception-handling middleware in `Api/Middlewares` returning a consistent `ProblemDetails` (RFC 7807) body. The frontend maps these in `src/api/errors.ts`.
- **Pagination / filtering:** list endpoints take `page`, `pageSize`, and filter and sort query parameters, and return items plus total count (needed for client filtering/distribution, FR-07, FR-08).
- **Authentication:** token-based (JWT bearer) 🟡. The frontend already has a login flow and auth store; the backend currently only has `UseAuthorization()` with no scheme configured. ⚠️ Confirm the mechanism before implementing.
- **Authorization:** role-based, matching the frontend role model (see section 6). The backend is the source of truth; frontend checks are for UX only.
- **Documentation:** Swagger/OpenAPI is the contract. The frontend generates TypeScript types from it with `openapi-typescript`.
- **Versioning:** URL segment (`/v1`).

## 5. Entity Framework Core Guidelines

- **Approach:** Code-First with migrations.
- **Location:** `DbContext` and entity configurations in `Infrastructure/Data`, migrations in `Infrastructure/Migrations`. Entities live in `Domain/Entities` and stay free of EF attributes.
- **Mapping:** use the Fluent API via `IEntityTypeConfiguration<T>` classes, one per entity. Do not annotate Domain entities.
- **Connection string:** stored in `appsettings.json` under `ConnectionStrings`. Real credentials never go in source control. Use user-secrets locally and environment variables or a secret store in deployed environments.
- **Registration:** `AddDbContext` with `UseSqlServer` inside `Infrastructure/DependencyInjection.cs`, called from `Program.cs`.
- **Repositories:** interfaces in `Domain/Interfaces`, implementations in `Infrastructure/Repositories`.
- **Async only:** use `ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`, and pass `CancellationToken` through.
- **Reads:** use `AsNoTracking()` for read-only queries. Project to DTOs rather than loading whole graphs.
- **Migrations:** descriptive names (e.g. `AddClientsTable`), created and reviewed in pull requests, never edited after being merged. Commands run from `src/Server`:
  ```
  dotnet ef migrations add <Name> -p CRM.AlMohtaref.Infrastructure -s CRM.AlMohtaref.Api
  dotnet ef database update -p CRM.AlMohtaref.Infrastructure -s CRM.AlMohtaref.Api
  ```
- **Auditing:** common audit fields (`CreatedAt`, `UpdatedAt`, and so on) are set centrally, not per-entity.
- **Schema docs:** database diagrams and scripts go in `docs/db-design`.

## 6. Frontend Architecture

Feature-based structure under `src/Client/src`:

| Folder | Purpose |
|---|---|
| `app/` | Providers, router, route guards (`RequireAuth`, `RequirePermission`), 403/404 pages |
| `features/` | One folder per business area: `auth`, `clients`, `campaigns`, `communications`, `issues`, `notifications`, `users`, `academic`. Each exposes only its `index.ts` |
| `api/` | axios client, interceptors, error normalization, file download helper |
| `permissions/` | Roles, permissions, `can()` helper, `<Can>` component, `usePermission` hook |
| `stores/` | Zustand stores (auth/session) |
| `components/` | Shared UI (`ui/` for shadcn components, `common/`) |
| `layouts/` | `AuthLayout`, `AppLayout` and navigation config |
| `i18n/` | Arabic and English locale files |
| `mocks/` | MSW handlers so the UI can be built before the backend is ready |
| `lib/`, `types/`, `styles/` | Env access, utilities, shared types, brand CSS |

### Frontend rules
The full rules are in `src/Client/docs/conventions.md`. The key ones:
1. Features import each other only through `index.ts`.
2. Within a feature, use relative imports.
3. Shared `components`, `hooks` and `lib` never import from `features`.
4. `axios` is used only inside `src/api`. Components use `*.queries.ts`.
5. Server data lives in TanStack Query. Zustand holds session and UI state only.
6. Every form is a Zod schema plus React Hook Form, with types inferred from the schema.
7. No hard-coded Arabic text in components. All strings come from `src/i18n/locales`.
8. RTL-safe utilities only (`ms-`, `me-`, `ps-`, `pe-`, `text-start`, `text-end`). No `ml-`, `mr-`, `pl-`, `pr-`, `text-left` or `text-right`.
9. All routes come from `app/router/paths.ts`, and each route declares its permission via `handle.permission`.

## 7. Roles and Permissions (RBAC)

Defined in `src/Client/src/permissions` and mirroring the use-case diagrams in `docs/diagrams`.

### Role inheritance
| Role | Inherits from |
|---|---|
| `employee` | none |
| `salesEmployee` | `employee` |
| `salesManager` | `salesEmployee` |
| `marketingEmployee` | `employee` |
| `marketingManager` | `marketingEmployee` |
| `academicAdvisor` | `employee` |
| `customerService` | `employee` |
| `admin` | `salesManager`, `marketingManager`, `academicAdvisor`, `customerService` |

### Permissions declared per role (inherited ones are merged automatically)
| Role | Own permissions | Requirements |
|---|---|---|
| `employee` | `account.changePassword`, `notifications.view`, `clients.viewProfile` | FR-02, FR-03, FR-10 |
| `salesEmployee` | `clients.view`, `communications.manage` | FR-09, FR-11, FR-12 |
| `salesManager` | `clients.import`, `clients.filter`, `clients.distribute` | FR-06, FR-07, FR-08 |
| `marketingEmployee` | `clients.view` | FR-09 |
| `marketingManager` | `campaigns.create`, `campaigns.updateStatus`, `campaigns.reports` | FR-13..17 |
| `academicAdvisor` | `academic.viewSubscribed`, `academic.records` | FR-18..20 |
| `customerService` | `issues.create`, `issues.escalate`, `issues.follow` | FR-21..23 |
| `admin` | `users.create`, `users.suspend` | FR-04, FR-05 |

Rules:
- The frontend uses permissions to hide or show UI and guard routes (`RequirePermission`, `<Can>`).
- ⚠️ The backend must enforce the same rules independently. Frontend checks are not security.
- ⚠️ Role names in the frontend must match what the backend returns. A mapping is applied at login if they differ.

## 8. Cross-Cutting Decisions

| Topic | Decision |
|---|---|
| Internationalization | Arabic (RTL) and English from day one, with all strings in locale files |
| Configuration | Environment-specific via `appsettings.*.json` (backend) and `.env.*` (frontend). `.env.example` is committed, `.env.local` is not |
| Parallel development | Frontend develops against MSW mocks, then switches to the real API |
| API contract | Swagger/OpenAPI is the single source of truth, with TypeScript types generated from it |
| Validation | Zod on the client, and a server-side validation layer in `Application` (library 🟡 not yet chosen) that always runs regardless of client checks |
| Secrets | Never committed. Use user-secrets, environment variables or a secret store |

## 9. Planned, Not Yet Decided ⚠️

- Mediator/CQRS approach in `Application/Features` (`Common/Behaviors` suggests pipeline behaviors for validation and logging, but no library is referenced yet).
- Server-side validation library.
- Logging and observability (structured logging).
- Authentication mechanism details (token lifetime, refresh strategy).
- Automated tests (backend and frontend). The CI currently runs `dotnet-ci.yml` only.

## 10. Repository Layout

```
crm-al-mohtaref/
├── .github/workflows/     CI (dotnet-ci.yml)
├── assets/
├── docs/
│   ├── architecture.md    ← this file
│   ├── db-design/         diagrams/, scripts/
│   ├── diagrams/          use-case diagrams
│   ├── ui-designs/
│   └── Al-Mohtaref_CRM_SRS_Arabic_Functional_Requirements.docx
└── src/
    ├── CRM-Al-Mohtaref.sln
    ├── Client/            React + TypeScript
    └── Server/            ASP.NET Core (Api, Application, Domain, Infrastructure)
```

## 11. Change Policy

Any change to the layers, the stack, or the rules above is made through a pull request that also updates this document.
