alias c := clean
alias b := build
alias t := test

clean:
    find . -iname "bin" -print0 | xargs -0 rm -rf
    find . -iname "obj" -print0 | xargs -0 rm -rf
    dotnet build-server shutdown

build:
    dotnet build Mediator.sln

test:
    dotnet test Mediator.sln
