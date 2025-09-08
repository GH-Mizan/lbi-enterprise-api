FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/LbI.Web.Host/LbI.Web.Host.csproj", "src/LbI.Web.Host/"]
COPY ["src/LbI.Web.Core/LbI.Web.Core.csproj", "src/LbI.Web.Core/"]
COPY ["src/LbI.Application/LbI.Application.csproj", "src/LbI.Application/"]
COPY ["src/LbI.Core/LbI.Core.csproj", "src/LbI.Core/"]
COPY ["src/LbI.EntityFrameworkCore/LbI.EntityFrameworkCore.csproj", "src/LbI.EntityFrameworkCore/"]
WORKDIR "/src/src/LbI.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/LbI.Web.Host", "src/LbI.Web.Host"]
COPY ["src/LbI.Web.Core", "src/LbI.Web.Core"]
COPY ["src/LbI.Application", "src/LbI.Application"]
COPY ["src/LbI.Core", "src/LbI.Core"]
COPY ["src/LbI.EntityFrameworkCore", "src/LbI.EntityFrameworkCore"]
WORKDIR "/src/src/LbI.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "LbI.Web.Host.dll"]
