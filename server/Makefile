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

.PHONY: help env up down destroy logs build start migrate test

# Comando padrão, exibe a ajuda
help:
	@echo "Comandos disponíveis:"
	@echo "  make up        - Sobe os contêineres em segundo plano."
	@echo "  make migrate   - Aplica as migrações do EF Core ao banco (requer 'make up')."
	@echo "  make test      - Roda todos os testes da solução."
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

# Roda toda a suíte: unitários e integração
test:
	dotnet test Deullam.Credit.Inquiry.Challenge.sln
