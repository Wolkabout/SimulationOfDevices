#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["/SimulationOfDevices.DAL/SimulationOfDevices.DAL.csproj", "SimulationOfDevices.DAL/"]
RUN dotnet restore "SimulationOfDevices.DAL/SimulationOfDevices.DAL.csproj"

COPY ["/SimulationOfDevices.Services/SimulationOfDevices.Services.csproj", "SimulationOfDevices.Services/"]
RUN dotnet restore "SimulationOfDevices.Services/SimulationOfDevices.Services.csproj"
COPY . .
WORKDIR "/src/SimulationOfDevices.API"
RUN dotnet build "SimulationOfDevices.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimulationOfDevices.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimulationOfDevices.API.dll"]