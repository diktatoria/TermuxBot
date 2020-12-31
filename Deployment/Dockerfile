#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Setup App
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build App
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
# COPY ["./TermuxBot.sln", "./"]
COPY . .
RUN dotnet restore "TermuxBot.sln"
COPY . .
RUN dotnet build "TermuxBot.sln" -c Release -o /app/build

# publish app
FROM build AS publish
WORKDIR /src
RUN dotnet publish "TermuxBot.sln" -c Release -o /app/publish

# Copy Plugin directory to publish folder
#  COPY ["/app/build/Plugins", "/app/publish/Plugins"]

# run app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TermuxBot.dll"]