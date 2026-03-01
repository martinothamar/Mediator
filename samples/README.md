## Samples

This directory contains various samples of Mediator usage

* [basic/](/samples/basic/) shows basic usage of the primitives - requests, notifications, pipelinebehaviors
* [apps/](/samples/apps/) shows more full-featured usage in APIs/applications using various architectures, such as Clean Architecture
* [use-cases/](/samples/use-cases/) shows more specific usecases and patterns, such as Autofac, query caching pattern

In [Showcase/](/samples/Showcase/) you will find the code for what is shown in the root README.

## Telemetry backend for samples

Server-hosted samples under `samples/apps/` are wired for OpenTelemetry metrics/tracing with OTLP export.
The Blazor WebAssembly client sample is intentionally excluded.
To run a local LGTM stack for viewing telemetry:

```sh
docker compose up -d
```

Endpoints:
* OTLP gRPC: `localhost:4317`
* OTLP HTTP: `localhost:4318`
* Grafana UI: [http://localhost:3000](http://localhost:3000)

Stop and remove:

```sh
docker compose down -v
```
