alias b := build
alias c := check
alias f := format
alias d := dev

build: clean restore check

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

damo-io-server-container:
	docker build -t damo.io.server -f Damo.Io.Server/deployment/Dockerfile .

damo-io-blog-container:
	docker build -t damo.io.blog -f Damo.Io.Blog/deployment/Dockerfile .

dev:
	dotnet watch run --project Damo.Io.Server
