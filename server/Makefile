# ====================================================================================
# Makefile para gerenciar o ambiente de desenvolvimento do Desafio de Créditos
# ====================================================================================

# Comando padrão, exibe a ajuda
help:
	@echo "Comandos disponíveis:"
	@echo "  make up        - Sobe os contêineres em segundo plano."
	@echo "  make down      - Para os contêineres."
	@echo "  make destroy   - Para os contêineres e REMOVE OS VOLUMES (apaga o banco)."
	@echo "  make logs      - Exibe os logs dos contêineres em tempo real."
	@echo "  make build     - Força a reconstrução das imagens (útil para a API)."
	@echo "  make start     - Executa 'up' e 'logs' em sequência."

# Sobe os contêineres em modo "detached" (em segundo plano)
up:
	@echo "🚀 Subindo ambiente Docker..."
	docker-compose up -d

# Para os contêineres sem remover volumes
down:
	@echo "🛑 Parando ambiente Docker..."
	docker-compose down

# Para os contêineres e remove os volumes (ação destrutiva)
destroy:
	@echo "💣 Parando ambiente e DESTRUINDO volumes de dados..."
	docker-compose down -v

# Mostra os logs de todos os serviços em tempo real
logs:
	@echo "📜 Exibindo logs em tempo real (Pressione Ctrl+C para sair)..."
	docker-compose logs -f

# Força a reconstrução das imagens definidas no docker-compose.yml
build:
	@echo "🛠️  Reconstruindo imagens Docker sem cache..."
	docker-compose build --no-cache

# Atalho para subir o ambiente e já começar a ver os logs
start: up logs

# Define quais comandos não geram arquivos com o mesmo nome
.PHONY: help up down destroy logs build start
