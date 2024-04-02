alias c := clean
alias b := build
alias t := test

clean:
    git clean -fxd
    dotnet build-server shutdown

build:
    dotnet build Mediator.sln

_test constants:
    dotnet clean
    dotnet build --no-restore -p:ExtraDefineConstants=\"{{constants}}\"
    dotnet test --no-restore --no-build --logger "console;verbosity=detailed"

test:
    dotnet restore

    just -f {{ justfile() }} _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait'
    just -f {{ justfile() }} _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait'
    just -f {{ justfile() }} _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait'
    just -f {{ justfile() }} _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll'
    just -f {{ justfile() }} _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll'
    just -f {{ justfile() }} _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll'

    dotnet clean
    dotnet build --no-restore
    dotnet test --no-restore --no-build --logger "console;verbosity=detailed"
