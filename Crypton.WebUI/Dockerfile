FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Crypton.WebUI/Crypton.WebUI.csproj", "Crypton.WebUI/"]
RUN dotnet restore "Crypton.WebUI/Crypton.WebUI.csproj"
COPY . .
WORKDIR "/src/Crypton.WebUI"
RUN dotnet build "Crypton.WebUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crypton.WebUI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crypton.WebUI.dll"]
