# Property Rental Management

Sistema web em **ASP.NET Core MVC + Entity Framework Core + SQL Server** para gerenciamento de locação de imóveis.

## O que este projeto faz

O sistema conecta 3 perfis:

- **Owner (proprietário/admin):** controla contas, imóveis, apartamentos, mensagens, eventos e agendamentos.
- **Manager (gerente):** gerencia prédios/apartamentos, agenda visitas, troca mensagens e reporta eventos.
- **Tenant (inquilino/potencial locatário):** cria conta, vê apartamentos, agenda visita e envia mensagens.

Módulos principais já no projeto:

- Buildings (prédios)
- Apartments (apartamentos)
- Users (usuários)
- Appointments (agendamentos)
- Messages (mensagens)
- Events (ocorrências/incidentes)

## Segurança e autenticação (implementado)

- Login com **cookie authentication**.
- Logout e registro de tenant.
- Controle de acesso por papel (Owner/Manager/Tenant).
- Proteção de rotas com `[Authorize]`.
- Senhas com hash (`PasswordHasher`), sem armazenamento em texto puro para novos logins/cadastros.
- Impersonação bloqueada em fluxos críticos:
  - mensagem usa usuário logado como remetente
  - evento usa usuário logado como reportante
  - tenant só vê/gera seus próprios agendamentos/mensagens

## Primeiro acesso

Se não existir nenhum Owner no banco, o sistema cria automaticamente:

- **Email:** `owner@propertyrental.local`
- **Senha:** `Owner@123`

Depois do primeiro login, troque essa senha.

## Como rodar

1. Configure `ConnectionStrings:DefaultConnection` em `appsettings.json`.
2. Aplique migrations no seu SQL Server.
3. Execute:

```bash
dotnet restore
dotnet build
dotnet run
```

4. Abra URL mostrada no terminal e faça login.

## Estrutura resumida

- `Controllers/`: regras MVC e autorização por papel
- `Models/`: entidades de domínio
- `Data/ApplicationDbContext.cs`: mapeamento EF Core
- `Views/`: interface Razor
- `Migrations/`: histórico de banco
