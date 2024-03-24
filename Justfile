alias c := clean
alias b := build
alias t := test

clean:
    git clean -fxd
    dotnet build-server shutdown

build:
    dotnet build Mediator.sln

test:
    dotnet test Mediator.sln
