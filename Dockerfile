FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env

WORKDIR /app/

# Copy csproj and restore as distinct layers
COPY Rem/*.csproj ./Rem/
COPY MemeGenerator/*.csproj ./MemeGenerator/
COPY BabelJam/*.csproj ./BabelJam/
WORKDIR /app/Rem/
RUN dotnet restore

# Copy everything else and build
WORKDIR /app/
COPY Rem/. ./Rem/
COPY MemeGenerator/. ./MemeGenerator/
COPY BabelJam/. ./BabelJam/
WORKDIR /app/Rem/
RUN dotnet publish -c Release -o out

# Run tests
FROM build-env AS testrunner
WORKDIR /app/Tests
COPY Tests/. .
ENTRYPOINT ["dotnet", "test", "--logger:trx"]

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app/
COPY --from=build-env /app/Rem/out .
ENTRYPOINT ["dotnet", "Rem.dll"]