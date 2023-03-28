FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Muuzika.Server/Muuzika.Server.csproj", "Muuzika.Server/"]
RUN dotnet restore "Muuzika.Server/Muuzika.Server.csproj"
COPY . .
WORKDIR "/src/Muuzika.Server"
RUN dotnet build "Muuzika.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Muuzika.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Muuzika.Server.dll"]
