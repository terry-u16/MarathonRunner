#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MarathonRunner.CloudRun/MarathonRunner.CloudRun.csproj", "MarathonRunner.CloudRun/"]
RUN dotnet restore "MarathonRunner.CloudRun/MarathonRunner.CloudRun.csproj"
COPY . .
WORKDIR "/src/MarathonRunner.CloudRun"
RUN dotnet build "MarathonRunner.CloudRun.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarathonRunner.CloudRun.csproj" -c Release -o /app/publish

FROM base AS final
ENV $env:DURATION_MUL 2.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TerryU16.MarathonRunner.CloudRun.dll"]