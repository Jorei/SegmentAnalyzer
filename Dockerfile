# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY app/*.csproj .
RUN dotnet restore  -r linux-x64

# copy everything else and build app
COPY app/. .
RUN dotnet publish -c Release -o /app -r linux-x64 --self-contained false --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim-amd64
WORKDIR /app
EXPOSE 5000
COPY --from=build /app .
ENV ASPNETCORE_URLS="5000"
ENTRYPOINT ["./app"]