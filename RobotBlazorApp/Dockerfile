FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RobotBlazorApp/RobotBlazorApp.csproj", "RobotBlazorApp/"]
RUN dotnet restore "RobotBlazorApp/RobotBlazorApp.csproj"
COPY . .
WORKDIR "/src/RobotBlazorApp"
RUN dotnet build "RobotBlazorApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RobotBlazorApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RobotBlazorApp.dll"]
