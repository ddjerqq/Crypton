FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Cypton.WebAPI/Cypton.WebAPI.csproj", "Cypton.WebAPI/"]
RUN dotnet restore "Cypton.WebAPI/Cypton.WebAPI.csproj"
COPY . .
WORKDIR "/src/Cypton.WebAPI"
RUN dotnet build "Cypton.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cypton.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cypton.WebAPI.dll"]
