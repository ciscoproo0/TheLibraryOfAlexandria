# Use the Microsoft's official build .NET image.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copie csproj e restore as dependências
COPY *.csproj ./
RUN dotnet restore

# Copie tudo o resto e construa o aplicativo
COPY . ./
RUN dotnet publish -c Release -o out

# Use Microsoft's official runtime .NET image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "YourAppName.dll"]
