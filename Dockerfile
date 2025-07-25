# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY src/People.Api/People.Api.csproj ./People.Api/
COPY src/People.Data/People.Data.csproj ./People.Data/

RUN dotnet restore ./People.Api/People.Api.csproj

# Copy everything else
COPY src/. ./

# Publish the API project
WORKDIR /src/People.Api
RUN dotnet publish People.Api.csproj \
    -c Release \
    -r linux-musl-x64 \
    --self-contained true \
    -o /app/publish

# Debug published artifacts
RUN ls -al /app/publish

# Runtime stage - use non-root user
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

# Add non-root user and switch to it
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish/. .

RUN ls -al /app

# Listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
# Honour DOTNET_ENVIRONMENT if passed (no default needed)

EXPOSE 8080

ENTRYPOINT ["./People.Api"]
