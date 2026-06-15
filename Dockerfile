FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.slnx ./
COPY src/HomeFlow.Domain/HomeFlow.Domain.csproj src/HomeFlow.Domain/
COPY src/HomeFlow.Application/HomeFlow.Application.csproj src/HomeFlow.Application/
COPY src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj src/HomeFlow.Infrastructure/
COPY src/HomeFlow.API/HomeFlow.API.csproj src/HomeFlow.API/
COPY tests/HomeFlow.API.Tests/HomeFlow.API.Tests.csproj tests/HomeFlow.API.Tests/
COPY tests/HomeFlow.Application.Tests/HomeFlow.Application.Tests.csproj tests/HomeFlow.Application.Tests/
COPY tests/HomeFlow.Infrastructure.Tests/HomeFlow.Infrastructure.Tests.csproj tests/HomeFlow.Infrastructure.Tests/
RUN dotnet restore

COPY src/ src/
RUN dotnet publish src/HomeFlow.API/HomeFlow.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "HomeFlow.API.dll"]
