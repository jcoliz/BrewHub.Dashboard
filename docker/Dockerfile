# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# Install Node
RUN apt-get update -yq \
    && apt-get -yq install curl gnupg ca-certificates \
    && curl -L https://deb.nodesource.com/setup_20.x | bash \
    && apt-get update -yq \
    && apt-get install -yq \
        nodejs

# copy csproj and restore as distinct layers
COPY BrewHub.Protocol.Mqtt/Protocol.Mqtt/*.csproj BrewHub.Protocol.Mqtt/Protocol.Mqtt/
COPY Dashboard.Services.DeviceMessaging/*.csproj Dashboard.Services.DeviceMessaging/
COPY Dashboard.AspNet/*.csproj Dashboard.AspNet/
COPY Dashboard.Core/*.csproj Dashboard.Core/
COPY Common.ChartJS/*.csproj Common.ChartJS/
WORKDIR /source/Dashboard.AspNet
RUN dotnet restore

# Software version number
#   - Should correspond to tag
#   - Including default value so if someone just runs "docker build", it will work
ARG SOLUTION_VERSION=docker

# copy everything else and build app
WORKDIR /source
COPY . .
WORKDIR /source/Dashboard.AspNet
RUN dotnet publish --self-contained false -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Install Curl for healthcheck
RUN apt-get update -yq && apt-get install -yq curl

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Dashboard.AspNet.dll"]
HEALTHCHECK --interval=10s --timeout=30s --start-period=5s --retries=3 CMD [ "curl", "http://localhost/health" ]
