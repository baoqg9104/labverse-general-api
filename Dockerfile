# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj files and restore
COPY ["Labverse.API/Labverse.API.csproj", "Labverse.API/"]
COPY ["Labverse.BLL/Labverse.BLL.csproj", "Labverse.BLL/"]
COPY ["Labverse.DAL/Labverse.DAL.csproj", "Labverse.DAL/"]

RUN dotnet restore "Labverse.API/Labverse.API.csproj"

# copy rest of the files and publish
COPY . .
RUN dotnet publish "Labverse.API/Labverse.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# default port for Render; Render will inject PORT at runtime
ENV PORT=10000
ENV ASPNETCORE_URLS=http://+:${PORT}

# copy published output
COPY --from=build /app/publish .

EXPOSE ${PORT}

# Run as non-root user for better security
RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Labverse.API.dll"]
