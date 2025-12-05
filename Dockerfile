# Etapa 1 — Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo
COPY . .

# Restaura dependências
RUN dotnet restore

# Publica em modo Release
RUN dotnet publish -c Release -o /app/publish

# Etapa 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia arquivos do build
COPY --from=build /app/publish .

# Render define a porta pela variável $PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT

# Inicia a API
ENTRYPOINT ["dotnet", "As.Api.dll"]
