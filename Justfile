# Aliases
alias b := build
alias c := clean
alias t := test

# Matrix dimensions
frameworks := "net8.0 net9.0 net10.0"
lifetimes := "Singleton Scoped Transient"
publishers := "ForeachAwait TaskWhenAll"
sizes := "Default Large"
cachingModes := "Eager Lazy"

# Main recipes
build:
    dotnet build

clean:
    git clean -fxd
    dotnet build-server shutdown

restore:
    dotnet tool restore
    dotnet restore -v q

format:
    dotnet csharpier format .

format-check:
    dotnet csharpier check .

# Main test recipe - matches CI workflow
test: restore test-sourcegen test-memory test-matrix

# Test source generator (net10.0 only)
test-sourcegen:
    @echo "=== Running Source Generator Tests (net10.0) ==="
    dotnet build --no-restore -v q -f net10.0 ./test/Mediator.SourceGenerator.Tests/
    dotnet test --no-restore --no-build -f net10.0 ./test/Mediator.SourceGenerator.Tests/

# Test memory allocation tests
test-memory:
    @echo "=== Running Memory Allocation Tests ==="
    #!/usr/bin/env sh
    set -eu
    for framework in {{frameworks}}; do \
        echo "Testing memory allocation on $framework"; \
        dotnet build --no-restore -v q -f "$framework" ./test/Mediator.MemAllocationTests/; \
        dotnet test --no-restore --no-build -f "$framework" ./test/Mediator.MemAllocationTests/; \
    done

# Test all matrix combinations for Mediator.Tests
test-matrix:
    @echo "=== Running Matrix Tests (Mediator.Tests) ==="
    #!/usr/bin/env sh
    set -eu
    for framework in {{frameworks}}; do \
        for lifetime in {{lifetimes}}; do \
            for publisher in {{publishers}}; do \
                for size in {{sizes}}; do \
                    for cachingMode in {{cachingModes}}; do \
                        just _test-config "$framework" "$lifetime" "$publisher" "$size" "$cachingMode"; \
                    done; \
                done; \
            done; \
        done; \
    done

# Helper recipe for individual configuration testing
_test-config framework lifetime publisher size cachingMode:
    @echo "Testing {{framework}} - {{lifetime}}/{{publisher}}/{{size}}/{{cachingMode}}"
    dotnet clean -v q ./test/Mediator.Tests/
    dotnet build --no-restore -f {{framework}} -p:ExtraDefineConstants=\"Mediator_Lifetime_{{lifetime}}%3BMediator_Publisher_{{publisher}}%3BMediator_{{size}}_Project%3BMediator_CachingMode_{{cachingMode}}\" -v q ./test/Mediator.Tests/
    dotnet test --no-restore --no-build -f {{framework}} ./test/Mediator.Tests/

# Convenience recipes

# Test a specific framework with all matrix combinations
test-framework framework="net10.0":
    @echo "=== Testing framework {{framework}} with all matrix combinations ==="
    #!/usr/bin/env sh
    set -eu
    for lifetime in {{lifetimes}}; do \
        for publisher in {{publishers}}; do \
            for size in {{sizes}}; do \
                for cachingMode in {{cachingModes}}; do \
                    just _test-config "{{framework}}" "$lifetime" "$publisher" "$size" "$cachingMode"; \
                done; \
            done; \
        done; \
    done

# Test a specific lifetime across all frameworks
test-lifetime lifetime="Singleton":
    @echo "=== Testing lifetime {{lifetime}} across all frameworks ==="
    #!/usr/bin/env sh
    set -eu
    for framework in {{frameworks}}; do \
        for publisher in {{publishers}}; do \
            for size in {{sizes}}; do \
                for cachingMode in {{cachingModes}}; do \
                    just _test-config "$framework" "{{lifetime}}" "$publisher" "$size" "$cachingMode"; \
                done; \
            done; \
        done; \
    done

# Test a specific publisher across all frameworks
test-publisher publisher="ForeachAwait":
    @echo "=== Testing publisher {{publisher}} across all frameworks ==="
    #!/usr/bin/env sh
    set -eu
    for framework in {{frameworks}}; do \
        for lifetime in {{lifetimes}}; do \
            for size in {{sizes}}; do \
                for cachingMode in {{cachingModes}}; do \
                    just _test-config "$framework" "$lifetime" "{{publisher}}" "$size" "$cachingMode"; \
                done; \
            done; \
        done; \
    done

# Test a specific configuration
test-config framework lifetime publisher size cachingMode:
    @echo "=== Testing specific configuration ==="
    just _test-config {{framework}} {{lifetime}} {{publisher}} {{size}} {{cachingMode}}
