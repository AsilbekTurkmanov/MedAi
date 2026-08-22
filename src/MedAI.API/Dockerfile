FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/MedAI.Domain/MedAI.Domain.csproj", "MedAI.Domain/"]
COPY ["src/MedAI.Application/MedAI.Application.csproj", "MedAI.Application/"]
COPY ["src/MedAI.Infrastructure/MedAI.Infrastructure.csproj", "MedAI.Infrastructure/"]
COPY ["src/MedAI.API/MedAI.API.csproj", "MedAI.API/"]

RUN dotnet restore "MedAI.API/MedAI.API.csproj"

# Copy full source and publish
COPY src/ .
WORKDIR "/src/MedAI.API"
RUN dotnet publish "MedAI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "MedAI.API.dll"]
