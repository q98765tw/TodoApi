FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TodoApi-GrokBot/TodoApi-GrokBot.csproj TodoApi-GrokBot/
RUN dotnet restore TodoApi-GrokBot/TodoApi-GrokBot.csproj
COPY . .
RUN dotnet publish TodoApi-GrokBot/TodoApi-GrokBot.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "TodoApi-GrokBot.dll"]