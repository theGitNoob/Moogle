.PHONY: build
build:
	dotnet build

.PHONY: dev
run:
	dotnet run --project MoogleServer
dev:
	CONTENT_PATH="/home/rafa/Documentos/Codes/C#/moogle-2021/Content" dotnet watch run --project MoogleServer
