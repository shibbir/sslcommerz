FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
LABEL maintainer="Shibbir Ahmed"
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY SSLCommerz/SSLCommerz.csproj ./SSLCommerz/
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /app/SSLCommerz
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/SSLCommerz/out ./
CMD ASPNETCORE_URLS=http://*:$PORT dotnet SSLCommerz.dll
