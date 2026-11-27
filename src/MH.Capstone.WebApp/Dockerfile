# Multi-arch build: runs natively on the Raspberry Pi 5 (linux/arm64) and on x64.
# Build context is ./src (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props ./
COPY MH.Capstone.Domain/ MH.Capstone.Domain/
COPY MH.Capstone.Migrations.SqlServer/ MH.Capstone.Migrations.SqlServer/
COPY MH.Capstone.Migrations.Postgres/ MH.Capstone.Migrations.Postgres/
COPY MH.Capstone.WebApp/ MH.Capstone.WebApp/
RUN dotnet publish MH.Capstone.WebApp -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
# Run as the image's built-in non-root user. The data-protection key folder is created
# up front so a mounted volume inherits the right owner (keeps logins valid across redeploys).
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && chown -R app:app /home/app/.aspnet
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "MH.Capstone.WebApp.dll"]
