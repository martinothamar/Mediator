alias c := clean
alias b := build
alias t := test

clean:
    git clean -fxd
    dotnet build-server shutdown

build:
    dotnet build Mediator.sln

test:
    dotnet clean
    dotnet build -p:ExtraDefineConstants=TASKWHENALLPUBLISHER
    dotnet test --no-build --logger "console;verbosity=detailed"
    dotnet clean
    dotnet build
    dotnet test --no-build --logger "console;verbosity=detailed"
