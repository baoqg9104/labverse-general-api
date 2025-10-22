# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore as a separate layer
COPY ./Labverse.API/Labverse.API.csproj ./Labverse.API/
COPY ./Labverse.BLL/Labverse.BLL.csproj ./Labverse.BLL/
COPY ./Labverse.DAL/Labverse.DAL.csproj ./Labverse.DAL/

# Restore
RUN dotnet restore ./Labverse.API/Labverse.API.csproj

# Copy everything else and build
COPY . .
RUN dotnet build ./Labverse.API/Labverse.API.csproj -c Release -o /app/build --no-restore

# Publish
RUN dotnet publish ./Labverse.API/Labverse.API.csproj -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Environment
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Entry
ENTRYPOINT ["dotnet", "Labverse.API.dll"]
