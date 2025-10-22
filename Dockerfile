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
RUN dotnet publish "Labverse.API/Labverse.API.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# default port for Render; Render will inject PORT at runtime
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Labverse.API.dll"]
