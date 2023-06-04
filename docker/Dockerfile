# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY DashboardIoT/*.csproj DashboardIoT/
COPY Common.ChartJS/*.csproj Common.ChartJS/
COPY ChartMaker/*.csproj ChartMaker/
COPY *.sln .
RUN dotnet restore

# copy everything else and build app
COPY . .
RUN dotnet publish --self-contained false -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "DashboardIoT.dll"]
