alias b := build
alias c := clean
alias t := test

build:
    dotnet build Mediator.sln

clean:
    git clean -fxd
    dotnet build-server shutdown

_test constants:
    dotnet clean -v q
    dotnet build --no-restore -p:ExtraDefineConstants=\"{{constants}}\" -v q ./test/Mediator.Tests/
    dotnet test --no-restore --no-build ./test/Mediator.Tests/

test:
    dotnet restore -v m

    # Small projects - Singleton
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager'

    # Small projects - Scoped
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager'

    # Small projects - Transient
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager'

    # Large projects - Singleton
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager%3BMediator_Large_Project'

    # Large projects - Scoped
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager%3BMediator_Large_Project'

    # Large projects - Transient
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager%3BMediator_Large_Project'

    dotnet clean -v q
    dotnet build --no-restore -v q
    dotnet test --no-restore --no-build

test-transient:
    dotnet restore -v m

    # Small projects
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager'

    # Large projects
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager%3BMediator_Large_Project'

test-scoped:
    dotnet restore -v m

    # Small projects
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager'

    # Large projects
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager%3BMediator_Large_Project'

test-singleton:
    dotnet restore -v m

    # Small projects
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager'

    # Large projects
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_CachingMode_Eager%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Lazy%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_CachingMode_Eager%3BMediator_Large_Project'
