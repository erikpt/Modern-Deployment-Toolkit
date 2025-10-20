FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ModernDeploymentToolkit.sln", "./"]
COPY ["MDT.Core/MDT.Core.csproj", "MDT.Core/"]
COPY ["MDT.TaskSequence/MDT.TaskSequence.csproj", "MDT.TaskSequence/"]
COPY ["MDT.Plugins/MDT.Plugins.csproj", "MDT.Plugins/"]
COPY ["MDT.WebUI/MDT.WebUI.csproj", "MDT.WebUI/"]
COPY ["MDT.Engine/MDT.Engine.csproj", "MDT.Engine/"]

RUN dotnet restore

COPY . .
RUN dotnet build -c Release --no-restore
RUN dotnet publish MDT.WebUI/MDT.WebUI.csproj -c Release -o /app/webui --no-restore
RUN dotnet publish MDT.Engine/MDT.Engine.csproj -c Release -o /app/engine --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS webui
WORKDIR /app
COPY --from=build /app/webui .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "MDT.WebUI.dll"]

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS engine
WORKDIR /app
COPY --from=build /app/engine .
ENTRYPOINT ["dotnet", "MDT.Engine.dll"]
