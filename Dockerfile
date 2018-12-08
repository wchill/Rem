FROM microsoft/dotnet:2.2-sdk AS build-env

WORKDIR /app/

# Copy csproj and restore as distinct layers
COPY Rem/*.csproj ./Rem/
COPY MemeGenerator/*.csproj ./MemeGenerator/
WORKDIR /app/Rem/
RUN dotnet restore

# Copy everything else and build
WORKDIR /app/
COPY Rem/. ./Rem/
COPY MemeGenerator/. ./MemeGenerator/
WORKDIR /app/Rem/
RUN dotnet publish -c Release -o out

# Run tests
FROM build-env AS testrunner
WORKDIR /app/Tests
COPY Tests/. .
ENTRYPOINT ["dotnet", "test", "--logger:trx"]

# Build runtime image
FROM microsoft/dotnet:2.2-runtime AS runtime
WORKDIR /app/
COPY --from=build-env /app/Rem/out .
ENTRYPOINT ["dotnet", "Rem.dll"]