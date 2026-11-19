# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_HTTP_PORTS=80
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
ARG USER
WORKDIR /src
COPY ["Unite.Analysis/Unite.Analysis.csproj", "Unite.Analysis/"]
COPY ["Unite.Analysis.Web/Unite.Analysis.Web.csproj", "Unite.Analysis.Web/"]
COPY ["Unite.Orchestrator/Unite.Orchestrator.csproj", "Unite.Orchestrator/"]
RUN --mount=type=secret,id=github_token \
    dotnet nuget add source https://nuget.pkg.github.com/dkfz-unite/index.json -n github -u ${USER} -p "$(cat /run/secrets/github_token)" --store-password-in-clear-text \
    && dotnet restore "Unite.Analysis/Unite.Analysis.csproj" \
    && dotnet restore "Unite.Analysis.Web/Unite.Analysis.Web.csproj" \
    && dotnet restore "Unite.Orchestrator/Unite.Orchestrator.csproj" \
    && dotnet nuget remove source github

FROM restore AS build
COPY . .
WORKDIR "/src/Unite.Analysis.Web"
RUN dotnet build --no-restore "Unite.Analysis.Web.csproj" -c Release

FROM build AS publish
RUN dotnet publish --no-build "Unite.Analysis.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Unite.Analysis.Web.dll"]
