# 📋 Gestão de Sinistros - Segfy (Desafio Técnico Backend Pleno)

Este projeto consiste em uma Web API RESTful construída em **.NET 8** para a gestão de apólices e sinistros de seguros, aplicando práticas de **Clean Architecture**, **SOLID**, **Domain-Driven Design (DDD)** e testes automatizados.

---

## 🛠️ Arquitetura e Decisões Técnicas

Para demonstrar domínio sobre arquitetura de software, extensibilidade e separação de preocupações, a aplicação foi estruturada seguindo os princípios de **Clean Architecture**. A estrutura é dividida em quatro camadas principais de responsabilidade bem definida:

```
┌────────────────────────────────────────────────────────┐
│                      SegfyDesafio.API                  │ (Presentation)
│       - Controllers, DTOs, Middlewares, Swagger        │
└───────────┬────────────────────────────────┬───────────┘
            │                                │
            ▼                                ▼
┌────────────────────────┐      ┌────────────────────────┐
│ SegfyDesafio.Application│      │SegfyDesafio.Infrastructure│ (Data & External)
│ - Services, Validators │      │ - EF Core Context      │
│ - Mappings, Use Cases  │      │ - Repositories, Migr.  │
└───────────┬────────────┘      └────────────┬───────────┘
            │                                │
            └───────────────┬────────────────┘
                            ▼
                ┌───────────────────────┐
                │   SegfyDesafio.Domain │ (Core Domain)
                │   - Entities, Enums   │
                │   - Business Rules    │
                └───────────────────────┘
```

### 1. Camadas do Projeto
*   **Domain (Núcleo)**: Contém as entidades de domínio (`Cliente`, `Apolice`, `Sinistro`, `HistoricoSinistro`), enums, regras de negócio puras (validação de transição de status) e exceções de domínio. Não possui nenhuma dependência externa de frameworks ou bibliotecas de acesso a dados.
*   **Application (Aplicação)**: Contém as interfaces de serviços e repositórios, DTOs (Data Transfer Objects), validadores (usando **FluentValidation**) e serviços de aplicação que orquestram os casos de uso.
*   **Infrastructure (Infraestrutura)**: Implementa o acesso a banco de dados utilizando **Entity Framework Core**, as migrações (Migrations), repositórios e configurações específicas do banco de dados (mapeamentos Fluent API).
*   **API (Apresentação)**: Endpoint HTTP construído em ASP.NET Core Web API. Contém os Controllers, tratamento global de erros via Middleware, configurações de injeção de dependência e documentação OpenAPI (Swagger).
*   **Tests (Testes Unitários)**: Projeto separado com **xUnit**, **FluentAssertions** e **NSubstitute** para testar as regras de negócio críticas (criação de sinistro, fluxo de status unidirecional, obrigadoriedades).

### 2. Padrões de Projeto Utilizados
*   **Repository e Unit of Work**: Centraliza o acesso aos dados e garante transações atômicas, especialmente útil para salvar o sinistro e seu histórico na mesma transação.
*   **State / Validador de Transição**: Implementação de uma máquina de estados simplificada dentro da entidade `Sinistro` para validar transições de status permitidas de forma unidirecional.
*   **Rich Domain Model**: As entidades contêm comportamentos e regras de negócio próprias (ex: alterar status do sinistro), evitando o padrão anêmico.
*   **Middleware de Exception Handling**: Captura exceções personalizadas de domínio (`DomainException`) e mapeia automaticamente para status HTTP corretos (ex: `400 Bad Request` ou `422 Unprocessable Entity`), retornando mensagens amigáveis e estruturadas.

---

## 💾 Modelagem de Banco de Dados

### Entidades do Sistema

#### 1. Cliente
*   `Id` (Guid, PK)
*   `Nome` (varchar(150))
*   `CpfCnpj` (varchar(20))

#### 2. Apolice (Policy)
*   `Id` (Guid, PK)
*   `Numero` (varchar(50), Unique)
*   `Status` (varchar(20)) — Valores: `Ativa`, `Inativa`
*   `Ramo` (varchar(50)) — Ex: `Auto`, `Vida`, `Residencial`
*   `ClienteId` (Guid, FK)
*   `DataInicio` (DateTime)
*   `DataFim` (DateTime)

#### 3. Sinistro (Claim)
*   `Id` (Guid, PK)
*   `ApoliceId` (Guid, FK)
*   `Status` (varchar(20)) — Valores: `Aberto`, `EmAnalise`, `Aprovado`, `Encerrado`, `Negado`
*   `ValorEstimado` (decimal(18,2))
*   `ValorAprovado` (decimal(18,2), Nullable) — Obrigatório em `Encerrado`
*   `MotivoNegativa` (varchar(500), Nullable) — Obrigatório em `Negado`
*   `DataOcorrencia` (DateTime)
*   `DataAbertura` (DateTime)
*   `DataAtualizacao` (DateTime)

#### 4. HistoricoSinistro
*   `Id` (Guid, PK)
*   `SinistroId` (Guid, FK)
*   `StatusAnterior` (varchar(20), Nullable)
*   `StatusNovo` (varchar(20))
*   `DataAlteracao` (DateTime)
*   `Motivo` (varchar(500), Nullable)

---

## 🚦 Regras de Negócio e Máquina de Estados

### Fluxo de Status
O status do sinistro segue obrigatoriamente o seguinte fluxo unidirecional:
```
Aberto ➔ EmAnalise ➔ Aprovado ➔ Encerrado (ou Negado, com motivo obrigatório)
```

As transições válidas de status implementadas no domínio são:
1.  `Aberto` ➔ `EmAnalise`
2.  `EmAnalise` ➔ `Aprovado` ou `Negado`
3.  `Aprovado` ➔ `Encerrado` ou `Negado`

### Regras Implementadas
1.  **Abertura**: O sinistro só pode ser aberto se a apólice associada estiver com status `Ativa`.
2.  **Transição**: Não é permitida a alteração de status fora das transições acima. Uma vez que o sinistro atinge `Encerrado` ou `Negado`, nenhuma outra alteração é permitida.
3.  **Registro de Histórico**: Qualquer transição de status gera automaticamente um registro na tabela `HistoricoSinistros` contendo os status anterior/novo, data e motivo (opcional).
4.  **Encerramento**: Ao transitar para o status `Encerrado`, o preenchimento do campo `ValorAprovado` é obrigatório.
5.  **Negativa**: Ao transitar para o status `Negado`, o preenchimento do campo `MotivoNegativa` é obrigatório.

---

## 📝 Consultas SQL (`queries.sql`)

As consultas solicitadas para extração de indicadores estão estruturadas abaixo (será mantido um arquivo `queries.sql` na raiz do projeto):

### 1. Ranking de ramos por maior percentual de sinistros negados nos últimos 6 meses
```sql
-- PostgreSQL
SELECT 
    a.Ramo,
    COUNT(CASE WHEN s.Status = 'Negado' THEN 1 END) * 100.0 / COUNT(*) AS PercentualNegados,
    COUNT(CASE WHEN s.Status = 'Negado' THEN 1 END) AS QtdNegados,
    COUNT(*) AS TotalSinistros
FROM Sinistros s
JOIN Apolices a ON s.ApoliceId = a.Id
WHERE s.DataAbertura >= CURRENT_DATE - INTERVAL '6 months'
GROUP BY a.Ramo
ORDER BY PercentualNegados DESC;

-- SQL Server
SELECT 
    a.Ramo,
    CAST(COUNT(CASE WHEN s.Status = 'Negado' THEN 1 END) AS DECIMAL(18,2)) * 100.0 / COUNT(*) AS PercentualNegados,
    COUNT(CASE WHEN s.Status = 'Negado' THEN 1 END) AS QtdNegados,
    COUNT(*) AS TotalSinistros
FROM Sinistros s
JOIN Apolices a ON s.ApoliceId = a.Id
WHERE s.DataAbertura >= DATEADD(month, -6, GETDATE())
GROUP BY a.Ramo
ORDER BY PercentualNegados DESC;
```

### 2. Top 10 clientes com maior soma de ValorEstimado em sinistros em análise ou aprovados
```sql
-- PostgreSQL
SELECT 
    c.Id AS ClienteId,
    c.Nome AS ClienteNome,
    SUM(s.ValorEstimado) AS TotalValorEstimado
FROM Sinistros s
JOIN Apolices a ON s.ApoliceId = a.Id
JOIN Clientes c ON a.ClienteId = c.Id
WHERE s.Status IN ('EmAnalise', 'Aprovado')
GROUP BY c.Id, c.Nome
ORDER BY TotalValorEstimado DESC
LIMIT 10;

-- SQL Server
SELECT TOP 10
    c.Id AS ClienteId,
    c.Nome AS ClienteNome,
    SUM(s.ValorEstimado) AS TotalValorEstimado
FROM Sinistros s
JOIN Apolices a ON s.ApoliceId = a.Id
JOIN Clientes c ON a.ClienteId = c.Id
WHERE s.Status IN ('EmAnalise', 'Aprovado')
GROUP BY c.Id, c.Nome
ORDER BY TotalValorEstimado DESC;
```

### 3. Tempo médio de resolução (em dias) de sinistros encerrados, agrupado por ramo
```sql
-- PostgreSQL
SELECT 
    a.Ramo,
    AVG(EXTRACT(DAY FROM (h.DataAlteracao - s.DataAbertura))) AS TempoMedioResolucaoDias
FROM Sinistros s
JOIN Apolices a ON s.ApoliceId = a.Id
JOIN HistoricoSinistros h ON s.Id = h.SinistroId
WHERE s.Status = 'Encerrado' AND h.StatusNovo = 'Encerrado'
GROUP BY a.Ramo;

-- SQL Server
SELECT 
    a.Ramo,
    AVG(CAST(DATEDIFF(day, s.DataAbertura, h.DataAlteracao) AS DECIMAL(18,2))) AS TempoMedioResolucaoDias
FROM Sinistros s
JOIN Apolices a ON s.ApoliceId = a.Id
JOIN HistoricoSinistros h ON s.Id = h.SinistroId
WHERE s.Status = 'Encerrado' AND h.StatusNovo = 'Encerrado'
GROUP BY a.Ramo;
```

---

## 🚀 Planejamento de Desenvolvimento (Passo a Passo)

A execução do projeto está planejada nas seguintes etapas incrementais:

### Fase 1: Setup do Workspace e Projetos (.NET 8)
1.  Criar a Solution (.sln) e os 5 projetos correspondentes (`Domain`, `Application`, `Infrastructure`, `API`, `Tests`).
2.  Configurar referências entre os projetos (Domain <- Application <- API/Infrastructure).
3.  Adicionar pacotes NuGet necessários:
    *   `Npgsql.EntityFrameworkCore.PostgreSQL` e `Microsoft.EntityFrameworkCore.Tools` em Infrastructure.
    *   `FluentValidation.DependencyInjectionExtensions` em Application.
    *   `FluentAssertions` e `NSubstitute` em Tests.
4.  Configurar o `docker-compose.yml` contendo um container de **PostgreSQL** para que a aplicação suba sem depender de instalação manual local.

### Fase 2: Implementação do Core Domain
1.  Modelar os Enums (`StatusApolice`, `StatusSinistro`).
2.  Criar entidades `Cliente`, `Apolice`, `Sinistro` e `HistoricoSinistro` com propriedades ricas e construtores apropriados.
3.  Implementar métodos de alteração de status em `Sinistro` com validação de transição, encapsulando as regras de negócio para evitar estados inválidos.

### Fase 3: Infraestrutura e Acesso a Dados
1.  Configurar a persistência via EF Core Fluent API (definir tamanhos de campos, chaves primárias, relacionamentos e restrições de nulidade).
2.  Criar a implementação do `SegfyDbContext`.
3.  Criar um Script de Seed de dados em banco (`Cliente` e `Apolice` em status Ativa/Inativa) para possibilitar a execução de testes rápidos de API.
4.  Gerar as migrações (`dotnet ef migrations add InitialCreate`) e aplicar no banco.

### Fase 4: Camada de Aplicação e Validações
1.  Definir DTOs para inputs e outputs das APIs.
2.  Criar os serviços de aplicação (ou casos de uso) para gerenciar fluxo de abertura de sinistro, busca e listagem.
3.  Implementar validador de DTOs utilizando `FluentValidation`.

### Fase 5: Exposição dos Endpoints (API) e Middlewares
1.  Criar os Controllers:
    *   `POST /api/sinistros` — Abertura.
    *   `GET /api/sinistros/{id}` — Busca por Id.
    *   `GET /api/sinistros` — Filtros por status, data e paginação.
    *   `PATCH /api/sinistros/{id}/status` — Alterar status.
    *   `GET /api/sinistros/{id}/historico` — Listar histórico.
2.  Implementar `GlobalExceptionMiddleware` para capturar exceções de domínio e retornar HTTP Status Codes específicos (ex: 400 Bad Request ao quebrar regra de negócio).

### Fase 6: Testes Unitários e Qualidade de Código
1.  Escrever testes unitários abrangentes para cobrir os fluxos críticos:
    *   Abertura de sinistro em apólices ativas vs. inativas.
    *   Verificação do fluxo de transição de status válido e inválido.
    *   Obrigatoriedade de `ValorAprovado` ao encerrar e `MotivoNegativa` ao negar.
    *   Verificação de que a geração de histórico é disparada nas transições.

---

## 🛠️ Como Executar a Aplicação (Após Conclusão)

### Pré-requisitos
*   [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior.
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional, para rodar o banco sem instalações locais).

### Passos para Rodar:

1.  **Iniciar Banco de Dados**:
    ```bash
    docker-compose up -d
    ```
2.  **Restaurar Pacotes e Rodar Migrations**:
    ```bash
    dotnet restore
    dotnet ef database update --project SegfyDesafio.Infrastructure --startup-project SegfyDesafio.API
    ```
3.  **Executar a API**:
    ```bash
    dotnet run --project SegfyDesafio.API
    ```
    *A API estará acessível em `http://localhost:5000/swagger` ou `https://localhost:5001/swagger` para exploração dos endpoints.*
4.  **Executar Testes**:
    ```bash
    dotnet test
    ```
