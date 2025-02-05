FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/WCCG.PAS.ReferralsAPI.Web/WCCG.PAS.ReferralsAPI.Web.csproj", "WCCG.PAS.ReferralsAPI.Web/"]
RUN dotnet restore "WCCG.PAS.ReferralsAPI.Web/WCCG.PAS.ReferralsAPI.Web.csproj"
COPY . .
WORKDIR "/src/WCCG.PAS.ReferralsAPI.Web"
RUN dotnet build "WCCG.PAS.ReferralsAPI.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WCCG.PAS.ReferralsAPI.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WCCG.PAS.ReferralsAPI.Web.dll"]
