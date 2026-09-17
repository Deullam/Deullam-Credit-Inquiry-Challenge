# ====================================================================================
# Makefile para gerenciar o ambiente de desenvolvimento do Desafio de Créditos
# ====================================================================================

COMPOSE := docker compose
EF_PROJECT := Deullam.Credit.Inquiry.Challenge.Infra.Data
EF_STARTUP := Deullam.Credit.Inquiry.Challenge.API

# Connection string usada pelo `make migrate`, que roda no host e alcança o PostgreSQL
# pela porta que o compose publica. Bate com server/.env.example.
# Para apontar para outro banco: make migrate DB_CONNECTION="Host=...;Port=..."
DB_CONNECTION ?= Host=localhost;Port=5432;Database=credit_inquiry_db;Username=credit_user;Password=credit_local_dev

.DEFAULT_GOAL := help

.PHONY: help env up down destroy logs build start migrate test test-e2e

# Comando padrão, exibe a ajuda
help:
	@echo "Comandos disponíveis:"
	@echo "  make up        - Sobe os contêineres em segundo plano."
	@echo "  make migrate   - Aplica as migrações do EF Core ao banco (requer 'make up')."
	@echo "  make test      - Roda os testes unitários e de integração (exclui os E2E)."
	@echo "  make test-e2e  - Roda os testes E2E contra a pilha real (requer make up + make migrate)."
	@echo "  make down      - Para os contêineres."
	@echo "  make destroy   - Para os contêineres e REMOVE OS VOLUMES (apaga o banco)."
	@echo "  make logs      - Exibe os logs dos contêineres em tempo real."
	@echo "  make build     - Força a reconstrução das imagens (útil para a API)."
	@echo "  make start     - Executa 'up' e 'logs' em sequência."

# Cria o .env a partir do .env.example na primeira execução.
# O docker-compose.yml declara `env_file: .env` e falha se o arquivo não existir.
env:
	@test -f .env || (cp .env.example .env && echo "✅ .env criado a partir de .env.example")

# Sobe os contêineres em modo "detached" (em segundo plano)
up: env
	@echo "🚀 Subindo ambiente Docker..."
	$(COMPOSE) up -d

# Para os contêineres sem remover volumes
down:
	@echo "🛑 Parando ambiente Docker..."
	$(COMPOSE) down

# Para os contêineres e remove os volumes (ação destrutiva)
destroy:
	@echo "💣 Parando ambiente e DESTRUINDO volumes de dados..."
	$(COMPOSE) down -v

# Mostra os logs de todos os serviços em tempo real
logs:
	@echo "📜 Exibindo logs em tempo real (Pressione Ctrl+C para sair)..."
	$(COMPOSE) logs -f

# Força a reconstrução das imagens definidas no docker-compose.yml
build: env
	@echo "🛠️  Reconstruindo imagens Docker sem cache..."
	$(COMPOSE) build --no-cache

# Atalho para subir o ambiente e já começar a ver os logs
start: up logs

## migrate: Aplica todas as migrações pendentes ao banco de dados.
## Requer que o contêiner do banco de dados esteja rodando (use 'make up' primeiro).
## O dotnet-ef vem do manifesto local em .config/dotnet-tools.json: nada é instalado
## globalmente e a versão fica travada junto com a do EF Core.
migrate:
	@echo "🔧 Restaurando o dotnet-ef local..."
	@dotnet tool restore
	@echo "🗃️  Aplicando migrações do EF Core ao banco de dados..."
	dotnet ef database update --project $(EF_PROJECT) --startup-project $(EF_STARTUP) --connection "$(DB_CONNECTION)"

# Roda unitários e integração. Exclui a suíte E2E (categoria "E2E"), que exige a pilha do
# docker-compose no ar e falha alto sem ela; para rodá-la, use `make test-e2e`.
test:
	dotnet test Deullam.Credit.Inquiry.Challenge.sln --filter "TestCategory!=E2E"

## test-e2e: Roda a suíte E2E contra a pilha REAL (API em localhost:8080, Kafka em localhost:29092).
## Requer `make up` e `make migrate` antes. Sem a pilha, a suíte falha explicando o que subir.
## Para apontar para outra pilha: E2E_API_BASE_URL=... E2E_KAFKA_BOOTSTRAP=... make test-e2e
test-e2e:
	dotnet test Deullam.Credit.Inquiry.Challenge.E2E.Tests
