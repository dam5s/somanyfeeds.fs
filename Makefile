.PHONY: setup build format

setup:
	dotnet tool restore
	dotnet restore

build:
	dotnet run --project ./Build

format:
	dotnet run --project ./Build format

