<div align="center">


# Desafio Técnico: API de Consulta de Créditos Constituídos

Este repositório contém a solução para um desafio técnico que consiste na criação de um serviço de back-end para gerenciar e consultar créditos tributários. 
</div>

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet )
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-4169E1?style=for-the-badge&logo=postgresql )
![Kafka](https://img.shields.io/badge/Apache%20Kafka-black?style=for-the-badge&logo=apachekafka )
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker )
![Arquitetura](https://img.shields.io/badge/Arquitetura-Clean-informational?style=for-the-badge )

</div>


---

## ✨ Visão Geral
A aplicação foi desenvolvida utilizando .NET 8, Clean Architecture, Docker, PostgreSQL e Apache Kafka, com foco em boas práticas de desenvolvimento, testabilidade e escalabilidade.
O sistema expõe uma API RESTful para duas operações principais:

1.  **Integração Assíncrona de Créditos:** Um endpoint `POST` recebe uma lista de créditos, publica cada um como uma mensagem em um tópico do Kafka e retorna uma resposta imediata (`202 Accepted`). Um serviço de background consome essas mensagens, valida e persiste os créditos no banco de dados, garantindo que o sistema seja resiliente e responsivo.
2.  **Consulta de Créditos:** Endpoints `GET` permitem a consulta dos créditos já processados e armazenados no banco de dados, retornando os dados em tempo real.

Além disso, a aplicação expõe endpoints de **Health Check** para monitoramento da saúde do serviço e de suas dependências.

## 🏛️ Arquitetura

A solução foi estruturada seguindo os princípios da **Clean Architecture**, promovendo a separação de responsabilidades, baixo acoplamento e alta testabilidade.

-   **Domain:** Contém as entidades de negócio (`Credito`) e as abstrações mais centrais. Não depende de nenhuma outra camada.
-   **Application:** Orquestra o fluxo de dados e contém a lógica de negócio. Define as interfaces dos repositórios e serviços externos (como o `IMessagePublisher`). Depende apenas da camada de Domain.
-   **Infrastructure:** Implementa as interfaces definidas na camada de Application. É aqui que se encontram o acesso ao banco de dados (Entity Framework Core, Repositórios), a comunicação com o message broker (Kafka) e outras dependências de infraestrutura.
-   **API:** A camada de apresentação. Contém os Controllers, a configuração do pipeline de injeção de dependência, middlewares e o `Dockerfile`. É o ponto de entrada da aplicação.

O uso do **Apache Kafka** como intermediário para a escrita de dados desacopla a API do processo de persistência, permitindo que a aplicação absorva picos de requisições sem sobrecarregar o banco de dados e melhorando a experiência do usuário com respostas mais rápidas.

## 🚀 Tecnologias Utilizadas

-   **Framework:** .NET 8
-   **Linguagem:** C# 12
-   **Banco de Dados:** PostgreSQL 15
-   **ORM:** Entity Framework Core 8
-   **Mensageria:** Apache Kafka (com a biblioteca `Confluent.Kafka`)
-   **Containerização:** Docker & Docker Compose
-   **Arquitetura:** Clean Architecture
-   **Validação:** FluentValidation
-   **API Documentation:** Swagger (OpenAPI)
-   **Testes:** xUnit, Moq


## 🏁 Como Executar o Projeto

O projeto é totalmente containerizado e inclui um `Makefile` para simplificar as operações do Docker.

## ⚙️ Pré-requisitos

Para executar este projeto, você precisará ter instalado em sua máquina:

-   [Docker](https://www.docker.com/get-started )
-   [Docker Compose](https://docs.docker.com/compose/install/ ) (geralmente já vem com o Docker Desktop)
-   [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0 ) (para executar os comandos do Entity Framework manually)
-   Um cliente Git (para clonar o repositório)
-   Um editor de código como VS Code ou Visual Studio.


### 1. Clone o Repositório

```bash
git clone https://github.com/Deullam/Deullam-Credit-Inquiry-Challenge
cd Deullam-Credit-Inquiry-Challenge/server
```

### 2. Crie o Arquivo de Ambiente
Na pasta server/, crie um arquivo chamado .env. Ele guardará as configurações dos serviços. Copie e cole o conteúdo abaixo:

# Credenciais de exemplo arquivo .env
``` 
DB_USER=admin
DB_PASSWORD=admin
DB_NAME=credits_db

# String de Conexão do Kafka
KAFKA_CONNECTION_STRING=kafka:9092
```


### 3. Suba o Ambiente com Docker Compose
Execute o seguinte comando no seu terminal, dentro da pasta server/. Este comando irá construir as imagens, criar os contêineres e iniciar todos os serviços em segundo plano.
```Bash
make up
```
A primeira execução pode levar alguns minutos para baixar as imagens do Docker. Ao final do processo, todos os serviços estarão rodando. Para aplicar as migrações do banco de dados, execute ```make migrate```.



### 4. Verifique se tudo está funcionando
Documentação da API (Swagger): Abra seu navegador e acesse http://localhost:8080/swagger.
Health Checks: Acesse http://localhost:8080/health/ready para ver o status da API e de suas dependências.
Banco de Dados (pgAdmin): Acesse http://localhost:443 (ou a porta que você configurou), faça login com admin@admin.com / admin e conecte-se ao servidor credit-inquiry-db para visualizar as tabelas.



### Comandos Úteis do Makefile

| Comando | Descrição |
| :--- | :--- |
| `make up` | Constrói as imagens Docker (se necessário) e inicia todos os serviços em segundo plano. |
| `make down` | Para todos os contêineres em execução relacionados ao projeto. |
| `make destroy` | Para e remover os contêineres, redes e **volumes de dados**. Use para uma limpeza completa. |
| `make logs` | Exibe os logs de todos os serviços em tempo real, útil para depuração. |
| `make migrate` | Aplica manualmente as migrações do EF Core ao banco de dados. Requer que o banco esteja no ar. |
| `make help` | Mostra uma lista de todos os comandos disponíveis e suas descrições. |




## 📖 Documentação da API

A documentação completa e interativa da API está disponível via Swagger. Após iniciar o projeto, acesse:

#### `http://localhost:8080/swagger`

A seguir, um resumo dos endpoints disponíveis.

---

### Integração de Créditos

#### `POST /api/creditos/integrar-credito-constituido`

Enfileira uma lista de créditos para serem processados e salvos de forma assíncrona. Este é o método principal para inserir novos dados no sistema.

**Corpo da Requisição (`application/json`)**

```json
[
  {
    "numeroCredito": "123456",
    "numeroNfse": "7891011",
    "dataConstituicao": "2024-02-25",
    "valorIssqn": 1500.75,
    "tipoCredito": "ISSQN",
    "simplesNacional": "Sim",
    "aliquota": 5.0,
    "valorFaturado": 30000.00,
    "valorDeducao": 5000.00,
    "baseCalculo": 25000.00
  }
]

```

#### Respostas Possíveis

| Código | Status | Descrição |
| :--- | :--- | :--- |
| `202` | `Accepted` | A requisição foi aceita e as mensagens foram enfileiradas para processamento. |
| `400` | `Bad Request` | O corpo da requisição é inválido (nulo, vazio ou com erros de validação). |
| `500` | `Internal Server Error` | Ocorreu um erro inesperado, como falha ao publicar no Kafka. |


### Consulta de Créditos

#### `GET /api/creditos/credito/{numeroCredito}`
Busca um crédito específico pelo seu número de crédito.

##### Parâmetros de Rota
Parâmetro Tipo Descrição numeroCredito string
O número do crédito a ser buscado.

##### Respostas Possíveis

| Código | Status | Descrição |
| :--- | :--- | :--- |
| `200` | `OK` | Retorna o objeto do crédito encontrado. |
| `404` | `Not Found` | Nenhum crédito com o ID fornecido foi encontrado. |
| `500` | `Internal Server Error` | Ocorreu um erro inesperado, como falha ao publicar no Kafka. |


#### `GET /api/creditos/nfse/{numeroNfse}`
Busca um crédito específico pelo seu número nfse.

##### Parâmetros de Rota
Parâmetro Tipo Descrição numeroNfse  string 
O número do crédito a ser buscado.

Retorna o crédito constituído que já foi processado.

Resposta de Sucesso (200 OK)
```JSON
[
  {
    "id": 1,
    "numeroCredito": "123456",
    "numeroNfse": "7891011",
    "dataConstituicao": "2024-02-25T00:00:00",
    "valorIssqn": 1500.75,
    // ... outros campos
  }
]

```

##### Respostas Possíveis
| Código | Status | Descrição |
| :--- | :--- | :--- |
| `200` | `OK` | Retorna o objeto do crédito encontrado. |
| `404` | `Not Found` | Nenhum crédito com o ID fornecido foi encontrado. |
| `500` | `Internal Server Error` | Ocorreu um erro inesperado, como falha ao publicar no Kafka. |



### Monitoramento

#### `GET /health/self`
Endpoint de liveness. Usado para verificar se a aplicação está online e respondendo a requisições.
Resposta de Sucesso (200 OK)
```JSON
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0015486"
} 
```
#### `GET /health/ready`
Endpoint de readiness. Usado para verificar se a aplicação está pronta para receber tráfego, validando a saúde de suas dependências críticas.
Resposta de Sucesso (200 OK)
``` JSON
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0543012",
  "results": {
    "PostgreSQL": {
      "status": "Healthy",
      "description": "PostgreSQL is healthy.",
      "duration": "00:00:00.0123456"
    },
    "Kafka": {
      "status": "Healthy",
      "description": "Kafka is healthy.",
      "duration": "00:00:00.0423456"
    }
  }
}
```


---

<div align="center">
<h2> Desenvolvido por Deullam Justi </h2>
</div>
