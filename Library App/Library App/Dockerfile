# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# STAGE1: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers (paths relative to repo root)
COPY ["Library App/Library App.csproj", "Library App/"]
RUN dotnet restore "Library App/Library App.csproj"

# copy everything and build
COPY . .
WORKDIR "/src/Library App"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# STAGE2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Expose port for local runs; Render sets PORT env and routing
EXPOSE 8080

# Copy published output
COPY --from=build /app/publish .

# Set ASP.NET Core to listen on the Render-provided PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Entrypoint
ENTRYPOINT ["dotnet", "Library App.dll"]
