#/usr/bin/env bash

dotnet restore

dotnet build ./TfsAdvanced.sln -c Release

dotnet pack ./TfsAdvanced.sln -c Release -o ./artifacts
