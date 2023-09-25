#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AspNetCore.Testing.SwissWebApplicationFactory.Stand/AspNetCore.Testing.SwissWebApplicationFactory.Stand.csproj", "AspNetCore.Testing.SwissWebApplicationFactory.Stand/"]
RUN dotnet restore "AspNetCore.Testing.SwissWebApplicationFactory.Stand/AspNetCore.Testing.SwissWebApplicationFactory.Stand.csproj"
COPY . .
WORKDIR "/src/AspNetCore.Testing.SwissWebApplicationFactory.Stand"
RUN dotnet build "AspNetCore.Testing.SwissWebApplicationFactory.Stand.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspNetCore.Testing.SwissWebApplicationFactory.Stand.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNetCore.Testing.SwissWebApplicationFactory.Stand.dll"]