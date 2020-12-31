#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TermuxBot/TermuxBot.csproj", "TermuxBot/"]
RUN dotnet restore "TermuxBot/TermuxBot.csproj"
COPY . .
WORKDIR "/src/TermuxBot"
RUN dotnet build "TermuxBot.csproj" -c Release -o /app/build

FROM build AS publish
COPY ["bin/Debug/Plugins/net5.0", "bin/Debug/net5.0/Plugins"]
RUN dotnet publish "TermuxBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TermuxBot.dll"]