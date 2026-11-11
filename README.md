# TJ Digital — Sistema de Gestão Processual (Demonstração)

Projeto de portfólio em **C# / ASP.NET Core 10**, com API REST, interface web responsiva e persistência de dados com **Entity Framework Core 10**.

> **Aviso importante:** este é um projeto **100% fictício**, desenvolvido exclusivamente para fins de estudo, demonstração técnica e portfólio. Não pertence, não representa e não possui vínculo com nenhum Tribunal de Justiça ou órgão público. Os registros, nomes, numerações, localidades e fluxos são inventados e **não devem ser usados para decisões judiciais**.

## Funcionalidades

- Dashboard com totais de processos e audiências próximas.
- Consulta processual por número fictício, assunto e comarca; filtros e paginação.
- Cadastro de processos e das partes autora/ré com validação.
- Visualização do histórico de movimentações e registro de novos andamentos.
- Alteração da situação processual, com registro de movimentação.
- Agenda de audiências com agendamento e conclusão.
- API REST em ASP.NET Core e banco SQLite criado automaticamente com dados de demonstração.
- Suporte opcional a SQL Server usando EF Core.
- Testes de validação e GitHub Actions para build/test.

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Backend | C#, .NET 10, ASP.NET Core Minimal APIs |
| ORM | Entity Framework Core 10.0.15 |
| Banco padrão | SQLite |
| Banco opcional | Microsoft SQL Server |
| Interface | HTML5, CSS3, JavaScript sem frameworks |
| Testes | xUnit |
| CI | GitHub Actions |

## Rodar no Linux, Windows ou macOS

**Pré-requisito:** .NET SDK 10, disponível em <https://dotnet.microsoft.com/download/dotnet/10.0>.

Na raiz do repositório:

```bash
dotnet restore src/TJDigital.Api/TJDigital.Api.csproj
dotnet run --project src/TJDigital.Api --urls http://localhost:5088
```

Abra **http://localhost:5088**. O banco SQLite `tjdigital.db` será criado automaticamente, com processos e audiências fictícios. Os arquivos `.db` são ignorados no Git.

Para executar os testes:

```bash
dotnet test tests/TJDigital.Tests/TJDigital.Tests.csproj
```

## Banco SQL Server (opcional)

A aplicação utiliza SQLite por padrão, mas também pode usar SQL Server. Crie um banco de dados de testes vazio e configure as variáveis de ambiente (não versione credenciais):

```bash
export Database__Provider=SqlServer
export ConnectionStrings__SqlServer='Server=localhost,1433;Database=TJDigitalDemo;User Id=sa;Password=SUA_SENHA_FORTE;Encrypt=True;TrustServerCertificate=True'
dotnet run --project src/TJDigital.Api --urls http://localhost:5088
```

`TrustServerCertificate=True` é **somente para desenvolvimento local**; em outros ambientes, utilize certificados válidos. Não aponte esta aplicação para bases com dados reais.

> **Observação:** a inicialização utiliza `EnsureCreated()` para facilitar a demo. Ela **não implementa migrações de produção**. Para evoluir o esquema em um ambiente real, substitua por migrações do EF Core e adote autenticação, autorização, logs de auditoria, política de segurança, proteção de dados e revisão técnica.

## URLs para testar

| Método | URL | Finalidade |
|---|---|---|
| GET | `http://localhost:5088/` | Interface web |
| GET | `http://localhost:5088/api/health` | Verificar API |
| GET | `http://localhost:5088/api/dashboard` | Indicadores |
| GET | `http://localhost:5088/api/processos` | Listar processos |
| GET | `http://localhost:5088/api/processos?busca=DEMO-2026-0001` | Buscar pelo número |
| GET | `http://localhost:5088/api/processos/1` | Consultar detalhes |
| POST | `http://localhost:5088/api/processos` | Cadastrar processo |
| PUT | `http://localhost:5088/api/processos/1/status` | Alterar situação |
| POST | `http://localhost:5088/api/processos/1/movimentacoes` | Registrar movimentação |
| GET | `http://localhost:5088/api/audiencias` | Agenda |
| POST | `http://localhost:5088/api/audiencias` | Agendar audiência |
| PATCH | `http://localhost:5088/api/audiencias/1/status` | Alterar situação de audiência |
| GET | `http://localhost:5088/api/opcoes` | Classes e situações disponíveis |

Há exemplos das requisições em `docs/requisicoes.http`.

## Organização

```text
src/TJDigital.Api/
  Contracts/          Requests tipadas
  Data/               DbContext, mapeamento e carga de demonstração
  Domain/             Entidades e regras de validação
  Endpoints/          Endpoints REST
  wwwroot/            Interface web responsiva
  Program.cs          Configuração e inicialização

tests/TJDigital.Tests/ Regras automatizadas (xUnit)
docs/                  Exemplos de requisições
.github/workflows/      Pipeline GitHub Actions
```

## Publicar no GitHub

Crie um repositório **novo e vazio** chamado `TJDigital-DotNet-Portfolio` na sua conta GitHub, sem adicionar README ou `.gitignore` pelo formulário. Na pasta extraída:

```bash
git init
git add .
git commit -m "feat: sistema demonstrativo TJ Digital em ASP.NET Core 10"
git branch -M main
git remote add origin https://github.com/mauricioapacheco/TJDigital-DotNet-Portfolio.git
git push -u origin main
```

A publicação no GitHub é de **código-fonte**, não uma hospedagem web. Para disponibilizar a aplicação online, ainda será necessário provisionar hospedagem ASP.NET Core, banco e HTTPS.

## Escopo e segurança

- Registros exclusivamente fictícios e numeração **DEMO-**.
- Projeto de portfólio **sem autenticação ou autorização**; endpoints de escrita são acessíveis a quem conseguir acessar o servidor. **Não publique a instância diretamente na internet.**
- Não recebe nem consulta dados do Poder Judiciário.
- Não utiliza símbolos, brasões, identidades oficiais ou integrações de tribunais.
- Para uso real, seriam necessários requisitos jurídicos, de acesso e de LGPD não cobertos nesta demo.

**Autor:** Maurício Andrade Pacheco — portfólio de desenvolvimento Full Stack.