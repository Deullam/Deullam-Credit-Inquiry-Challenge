# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Deullam.Credit.Inquiry.Challenge.API/Deullam.Credit.Inquiry.Challenge.API.csproj", "Deullam.Credit.Inquiry.Challenge.API/"]
COPY ["Deullam.Credit.Inquiry.Challenge.Application/Deullam.Credit.Inquiry.Challenge.Application.csproj", "Deullam.Credit.Inquiry.Challenge.Application/"]
COPY ["Deullam.Credit.Inquiry.Challenge.Domain/Deullam.Credit.Inquiry.Challenge.Domain.csproj", "Deullam.Credit.Inquiry.Challenge.Domain/"]
COPY ["Deullam.Credit.Inquiry.Challenge.Infra/Deullam.Credit.Inquiry.Challenge.Infra.csproj", "Deullam.Credit.Inquiry.Challenge.Infra/"]
COPY ["Deullam.Credit.Inquiry.Challenge.Infra.Data/Deullam.Credit.Inquiry.Challenge.Infra.Data.csproj", "Deullam.Credit.Inquiry.Challenge.Infra.Data/"]
COPY ["Deullam.Credit.Inquiry.Challenge.IoC/Deullam.Credit.Inquiry.Challenge.IoC.csproj", "Deullam.Credit.Inquiry.Challenge.IoC/"]

RUN dotnet restore "Deullam.Credit.Inquiry.Challenge.API/Deullam.Credit.Inquiry.Challenge.API.csproj"

COPY . .
WORKDIR "/src/Deullam.Credit.Inquiry.Challenge.API"
RUN dotnet build "Deullam.Credit.Inquiry.Challenge.API.csproj" -c $BUILD_CONFIGURATION -o /app/build


# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Deullam.Credit.Inquiry.Challenge.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# Usa a imagem de runtime do ASP.NET, que é muito menor que a do SDK.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia os arquivos publicados do estágio 'publish' para o diretório de trabalho da imagem final.
COPY --from=publish /app/publish .

# Define o comando de entrada para executar a aplicação quando o contêiner iniciar.
ENTRYPOINT ["dotnet", "Deullam.Credit.Inquiry.Challenge.API.dll"]