FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

RUN apt-get update && apt-get install -y tzdata \
    && ln -fs /usr/share/zoneinfo/Europe/Istanbul /etc/localtime \
    && dpkg-reconfigure --frontend noninteractive tzdata

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore 

COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY ./wwwroot/assets /app/wwwroot/assets
ENTRYPOINT ["dotnet", "pulseui.dll"]
