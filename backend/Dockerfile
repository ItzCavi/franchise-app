# Build stage: compile and publish with the .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Franchise.Api/Franchise.Api.csproj Franchise.Api/
RUN dotnet restore Franchise.Api/Franchise.Api.csproj

COPY Franchise.Api/ Franchise.Api/
RUN dotnet publish Franchise.Api/Franchise.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage: only the ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Franchise.Api.dll"]