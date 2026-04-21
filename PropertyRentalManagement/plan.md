## Problem
Audit full ASP.NET Core MVC project against final-brief checklist, then produce requirement-by-requirement status matrix (`✅ DONE`, `⚠️ PARTIAL`, `❌ MISSING`) with exact file/line references and prioritized fix list for presentation.

## Proposed approach
1. Build traceability matrix from brief requirements grouped by role and technical stack.
2. For each requirement, gather concrete evidence from Controllers, Models, Views, Program/Startup, Data/EF configuration, and Migrations.
3. Assign status:
   - `✅ DONE`: requirement implemented end-to-end in current code.
   - `⚠️ PARTIAL`: some capability exists but incomplete scope, weak enforcement, or missing UI/API path.
   - `❌ MISSING`: no meaningful implementation found.
4. Produce final report in user-required format with:
   - status per requirement,
   - exact file path + approximate line,
   - short technical note.
5. Add final “what is missing” summary ranked by presentation impact (highest first).

## Todos
- Create requirement matrix covering all Owner/Manager/Tenant/Technical checklist items.
- Map each matrix row to implementation evidence (or absence) with file + line.
- Validate role/authorization boundaries and account-management flows by role.
- Validate CRUD/search/list coverage per domain area (users, buildings, apartments, appointments, messages, events).
- Validate technical criteria (MVC architecture, EF Core, SQL Server schema/migrations, authN/authZ, RBAC, API endpoints, client validation, responsive UI, ViewModels, data annotations).
- Draft final compliance report in exact output format requested.
- Draft prioritized remediation list for items marked `⚠️` or `❌`.

## Notes / considerations
- “Exact file and line” for missing functionality will be represented by closest relevant file where feature would normally exist, with explicit note that implementation is absent.
- Search/list alone is not enough for CRUD requirements; each action path must exist and be reachable under intended roles.
- “Web API endpoints” will only be marked done if API-style controllers/routes exist (not only MVC views/actions).
