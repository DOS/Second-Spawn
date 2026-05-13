.PHONY: run test build lint vet tidy fmt clean docker

GO ?= go
PKG := ./...
BIN := bin/gateway

run:
	$(GO) run .

test:
	$(GO) test -race -count=1 $(PKG)

build:
	$(GO) build -trimpath -ldflags "-s -w" -o $(BIN) .

vet:
	$(GO) vet $(PKG)

fmt:
	$(GO) fmt $(PKG)

tidy:
	$(GO) mod tidy

lint: vet
	@which staticcheck > /dev/null 2>&1 && staticcheck $(PKG) || echo "staticcheck not installed - run: go install honnef.co/go/tools/cmd/staticcheck@latest"

clean:
	rm -rf bin/

docker:
	docker build -t second-spawn-gateway:local .
