FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test
WORKDIR /app

COPY . .
WORKDIR /app/UrlShortener.Tests

RUN dotnet restore
RUN dotnet build --configuration Release
CMD dotnet test --logger "trx;LogFileName=integration_tests.trx" --verbosity normal --configuration Release

