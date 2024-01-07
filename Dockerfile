FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Front/Front.csproj", "Front/"]
RUN dotnet restore "Front/Front.csproj"
COPY . .
WORKDIR "/src/Front"
RUN dotnet build "Front.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Front.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS="http://*:7000;"
EXPOSE 7000
WORKDIR /app
COPY --from=publish /app/publish .
# Copie du certificat CA Database
COPY ${DATABASE_CA_CERTIFICATE} /app/database_ca_certificate.crt
ENTRYPOINT ["dotnet", "Front.dll"]
