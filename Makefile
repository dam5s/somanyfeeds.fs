.PHONY: restore build lint format

restore:
	dotnet tool restore
	dotnet restore

build:
	dotnet run --project ./Build

lint:
	dotnet run --project ./Build lint

format:
	dotnet run --project ./Build format
