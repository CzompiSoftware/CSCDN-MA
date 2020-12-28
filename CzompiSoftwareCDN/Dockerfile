#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CzompiSoftwareCDN/CzompiSoftwareCDN.csproj", "CzompiSoftwareCDN/"]
RUN dotnet restore "CzompiSoftwareCDN/CzompiSoftwareCDN.csproj"
WORKDIR "/src"
COPY /src .
RUN dotnet build "CzompiSoftwareCDN.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "CzompiSoftwareCDN.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CzompiSoftwareCDN.dll"]