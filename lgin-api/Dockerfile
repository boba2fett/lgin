FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY --from=build /app/bin/Release/netcoreapp5.0/publish/ /app
WORKDIR /app
EXPOSE 4000/tcp
CMD ["dotnet","WebApi.dll"]