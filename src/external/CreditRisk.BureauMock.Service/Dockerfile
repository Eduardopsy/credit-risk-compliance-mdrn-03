FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/external/CreditRisk.BureauMock.Service/CreditRisk.BureauMock.Service.csproj", "src/external/CreditRisk.BureauMock.Service/"]

RUN dotnet restore "src/external/CreditRisk.BureauMock.Service/CreditRisk.BureauMock.Service.csproj"

COPY . .

RUN dotnet build "src/external/CreditRisk.BureauMock.Service/CreditRisk.BureauMock.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/external/CreditRisk.BureauMock.Service/CreditRisk.BureauMock.Service.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8081
ENTRYPOINT ["dotnet", "CreditRisk.BureauMock.Service.dll"]
