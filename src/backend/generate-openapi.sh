#!/bin/bash
# Script to generate OpenAPI JSON for the frontend

cd "$(dirname "$0")"

# Build the API project first
echo "Building API project..."
dotnet build Zeitung.Api/Zeitung.Api.csproj --configuration Release

# Generate OpenAPI JSON using Swagger CLI
echo "Generating OpenAPI JSON..."
cd Zeitung.Api
dotnet tool restore
dotnet swagger tofile --output ../../frontend/openapi.json bin/Release/net9.0/Zeitung.Api.dll v1

if [ -f "../../frontend/openapi.json" ]; then
    echo "OpenAPI JSON generated successfully at src/frontend/openapi.json"
else
    echo "Warning: OpenAPI JSON generation may have failed"
fi
