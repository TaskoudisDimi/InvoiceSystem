# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InvoiceSystem/InvoiceSystem/InvoiceSystem/InvoiceSystem.csproj", "InvoicingSystem/"]
RUN dotnet restore "InvoicingSystem/InvoiceSystem.csproj"
COPY . .
WORKDIR "/src/InvoicingSystem"
RUN dotnet build "InvoiceSystem.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "InvoiceSystem.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvoiceSystem.dll"] 