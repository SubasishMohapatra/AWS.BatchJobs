#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AWS.SimpleLinuxJob/AWS.SimpleLinuxJob.csproj", "AWS.SimpleLinuxJob/"]
COPY ["AWS.S3.Core/AWS.S3.Core.csproj", "AWS.S3.Core/"]
RUN dotnet restore "AWS.SimpleLinuxJob/AWS.SimpleLinuxJob.csproj"
COPY . .
WORKDIR "/src/AWS.SimpleLinuxJob"
RUN dotnet build "AWS.SimpleLinuxJob.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AWS.SimpleLinuxJob.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AWS.SimpleLinuxJob.dll"]