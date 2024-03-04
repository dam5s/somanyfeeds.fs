.PHONY: restore clean check format damo.io.server.container damo.io.blog.container dev

restore:
	dotnet tool restore
	dotnet restore

clean:
	dotnet clean

check:
	dotnet fantomas --check .
	dotnet build
	dotnet test

format:
	dotnet fantomas .

damo.io.server.container:
	docker build -t damo.io.server -f Damo.Io.Server/deployment/Dockerfile .

damo.io.blog.container:
	docker build -t damo.io.blog -f Damo.Io.Blog/deployment/Dockerfile .

dev:
	dotnet watch run --project Damo.Io.Server
