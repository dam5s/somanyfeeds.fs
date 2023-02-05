#!/usr/bin/env bash

set -e

source .env

dotnet build Build

rm -rf ./bin
mkdir bin
cp -r Build/bin/Debug/net7.0/* bin/

dotnet bin/Build.dll "$@"
