# ================================
# Stage 1: Build
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Labverse.sln", "./"]
COPY ["Labverse.API/Labverse.API.csproj", "Labverse.API/"]
COPY ["Labverse.BLL/Labverse.BLL.csproj", "Labverse.BLL/"]
COPY ["Labverse.DAL/Labverse.DAL.csproj", "Labverse.DAL/"]

# Restore dependencies
RUN dotnet restore "Labverse.sln"

# Copy everything else and build
COPY . .

# Build và publish project Web API
RUN dotnet publish "Labverse.API/Labverse.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ================================
# Stage 2: Runtime
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy từ build stage
COPY --from=build /app/publish .

# Expose port cho Render
EXPOSE 8080

# Render sẽ gửi request tới port 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "Labverse.API.dll"]
