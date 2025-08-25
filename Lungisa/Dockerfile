##See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
#
##Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
##For more information, please see https://aka.ms/containercompat
#
#FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["Lungisa/Lungisa.csproj", "Lungisa/"]
#RUN dotnet restore "./Lungisa/Lungisa.csproj"
#COPY . .
#WORKDIR "/src/Lungisa"
#RUN dotnet build "./Lungisa.csproj" -c %BUILD_CONFIGURATION% -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./Lungisa.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Lungisa.dll"]

## Base image (runtime only)
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#WORKDIR /app
#EXPOSE 8080
#
## Build image (SDK for building/publishing)
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#
## Copy csproj and restore dependencies
#COPY ["Lungisa/Lungisa.csproj", "Lungisa/"]
#RUN dotnet restore "Lungisa/Lungisa.csproj"
#
## Copy everything else and build
#COPY . .
#WORKDIR "/src/Lungisa"
#RUN dotnet build "Lungisa.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## Publish the app
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "Lungisa.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## 👇 Optional check: ensure wwwroot was copied
#RUN dir /app/publish/wwwroot
#
## Copy wwwroot explicitly (in case msbuild skips it)
#COPY Lungisa/wwwroot /app/publish/wwwroot
#
#
## Final runtime image
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Lungisa.dll"]
#

# Use the ASP.NET runtime image (Linux)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Lungisa/Lungisa.csproj", "Lungisa/"]
RUN dotnet restore "Lungisa/Lungisa.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Lungisa"
RUN dotnet build "Lungisa.csproj" -c $BUILD_CONFIGURATION -o /app/build



# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Lungisa.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Double-check wwwroot is present
RUN ls -R /app/publish/wwwroot || echo "wwwroot missing!"


# Copy static files explicitly to make sure wwwroot comes across
COPY Lungisa/wwwroot /app/publish/wwwroot

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lungisa.dll"]
