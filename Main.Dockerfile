FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SecureBank/SecureBank.csproj", "SecureBank/"]
COPY ["SecureBank.API/SecureBank.API.Authentication/SecureBank.API.Authentication.csproj", "SecureBank.API/SecureBank.API.Authentication/"]
COPY ["SecureBank.Database/SecureBank.Database.csproj", "SecureBank.Database/"]
COPY ["SecureBank.API/SecureBank.API.Controllers/SecureBank.API.Controllers.csproj", "SecureBank.API/SecureBank.API.Controllers/"]
COPY ["SecureBank.Common/SecureBank.Common.csproj", "SecureBank.Common/"]
COPY ["SecureBank.API/SecureBank.API.Services/SecureBank.API.Services.csproj", "SecureBank.API/SecureBank.API.Services/"]
COPY ["SecureBank.Extensions/SecureBank.Extensions.csproj", "SecureBank.Extensions/"]
COPY ["SecureBank.Authentication/SecureBank.Authentication.csproj", "SecureBank.Authentication/"]
COPY ["SecureBank.API/SecureBank.API.Helpers/SecureBank.API.Helpers.csproj", "SecureBank.API/SecureBank.API.Helpers/"]
COPY ["SecureBank.API/SecureBank.API.Encryption/SecureBank.API.Encryption.csproj", "SecureBank.API/SecureBank.API.Encryption/"]
COPY ["SecureBank.Website/SecureBank.Website.Authentication/SecureBank.Website.Authentication.csproj", "SecureBank.Website/SecureBank.Website.Authentication/"]
COPY ["SecureBank.Website/SecureBank.Website.Services/SecureBank.Website.Services.csproj", "SecureBank.Website/SecureBank.Website.Services/"]
COPY ["SecureBank.Website/SecureBank.Website.API/SecureBank.Website.API.csproj", "SecureBank.Website/SecureBank.Website.API/"]
RUN dotnet restore "./SecureBank/./SecureBank.csproj"
COPY . .
WORKDIR "/src/SecureBank"
RUN dotnet build "./SecureBank.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SecureBank.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY database-default.db database.db
USER root
ENTRYPOINT ["dotnet", "SecureBank.dll"]