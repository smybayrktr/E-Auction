#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
WORKDIR /src
COPY ["src/Services/Sourcing/ESourcing.Sourcing/ESourcing.Sourcing.csproj", "src/Services/Sourcing/ESourcing.Sourcing/"]
COPY ["src/Services/Common/EventBusRabbitMQ/EventBusRabbitMQ.csproj", "src/Services/Common/EventBusRabbitMQ/"]
RUN dotnet restore "src/Services/Sourcing/ESourcing.Sourcing/ESourcing.Sourcing.csproj"
COPY . .
WORKDIR "/src/src/Services/Sourcing/ESourcing.Sourcing"
RUN dotnet build "ESourcing.Sourcing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ESourcing.Sourcing.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ESourcing.Sourcing.dll"]