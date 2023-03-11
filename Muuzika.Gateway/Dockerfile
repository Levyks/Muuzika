FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Muuzika.Gateway/Muuzika.Gateway.csproj", "Muuzika.Gateway/"]
RUN dotnet restore "Muuzika.Gateway/Muuzika.Gateway.csproj"
COPY . .
WORKDIR "/src/Muuzika.Gateway"
RUN dotnet build "Muuzika.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Muuzika.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Muuzika.Gateway.dll"]
