FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app/

# Copy csproj and restore as distinct layers
COPY Rem/*.csproj ./Rem/
WORKDIR /app/Rem/
RUN dotnet restore

# Copy everything else and build
WORKDIR /app/
COPY Rem/. ./Rem/
WORKDIR /app/Rem/
RUN dotnet publish -c Release -o out

# test application -- see: dotnet-docker-unit-testing.md
# FROM build AS testrunner
# WORKDIR /app/tests
# COPY tests/. .
# ENTRYPOINT ["dotnet", "test", "--logger:trx"]

# Build runtime image
FROM microsoft/dotnet:2.1-runtime AS runtime
WORKDIR /app/
COPY --from=build-env /app/Rem/out .
ENTRYPOINT ["dotnet", "Rem.dll"]