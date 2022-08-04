# .NET OTel Logs

Demonstration of different approaches to dotnet logs when using [OpenTelemetry](https://opentelemetry.io/) and [Serilog](https://serilog.net/).

## Docker Compose setup

There are three services in the the [docker-compose.yml](./docker-compose.yml) file

| Service  | Purpose | Local Url | More Info |
| ------------- | ------------- | ----------- | ---------- |
| seq  | Logging backend that recieves logs and allows advanced searching  | http://localhost:5002 | [Seq product](https://datalust.co/seq) |
| otel-collector  | Collector for OpenTelemetry which receives traces/metrics/logs and writes to the console, as well as optionally to Honeycomb | N/A | [Otel Collector docs](https://opentelemetry.io/docs/collector/) |
| webhost  | Simple API for testing OTel logging approaches  | http://localhost:5001/swagger/index.html | See the source code |


If you want to send trace telemetry to Honeycomb you need to modify the [otel-collector-config.yaml](./dockercompose/otel-collector/otel-collector-config.yaml) file by updating the `"x-honeycomb-team` and `x-honeycomb-dataset` values with your own information. You also need to adjust the service pipeline configuration to ensure that the `otlp` exporter is present for the traces pipeline.

## Running

Open a terminal in the repo root and run
>docker compose up

The console will print output, mostly from the otel-collector service with the raw OTel data for traces, metrics and logs.

To generate trace and log data do the following:
1. Open the [webhost swagger UI](http://localhost:5001/swagger/index.html)
2. Expand the `GET /carts/{id}/` method
3. Click the `Try it out` button
4. Enter a value for `id` between 1 and 10 inclusive
5. Click the `Execute` button

Repeat the above a number of times with different values for the cart Id, ensuring you have a spread of values below and above cart Id = 5.

As the requests are processed, telemetry data is sent to the OTel collector which batch processes it and then flushes to the console and optionally to Honeycomb too (if configured).

## Approaches to sending logs

There are two alternative approaches explored here:

1. Augmenting the current trace span to include log information in the span events array
   1. This is achieved via a [custom Serilog sink](./WebHost/Telemetry/Logging/ActivityEventWriterSink.cs) which adds logs to the current span by way of `Activity.Current.AddEvent(...)`
   2. In this approach logs are transformed into information attached to telemetry traces and sent to the OTel collector via the traces gRPC method
2. Using the OpenTelemetry logs signal directly
   1. In this approach logs are converted to OTLP log format and sent to the OTel Collector via the logs gRPC method
   2. Note that since the OTel logs signal type is not fully stable this approach is currently considered experimental

## Investigating the data

Suppose our goal is to find all log records where the number of items in the cart is greater than 5.

### Seq

1. Open the [Seq dashboard](http://localhost:5002) 
2. In the search bar enter `Cart.ItemsCount > 5` 
3. Click the Go button
4. All logs are displayed
5. Easy!

### Honeycomb

1. Open the Honeycomb dashboard
2. Select `New Query`
3. Select the correct dataset where the trace data was sent
4. Click `Run Query` to see recent Raw Data
5. Observe that there is a field called `Cart` with content of the form `{ CartId: 9, ItemsCount: 9 }`
6. This data was sent to Honeycomb as a span event attribute with the format `Cart: STRING({ CartId: 9, ItemsCount: 9 })` so Honeycomb records the field as string content
7. There is no immediate way to ad-hoc query the contents of this field
8. There is a setting at the Honeycomb dataset level to [automatically unpack JSON objects](https://docs.honeycomb.io/getting-data-in/logs/json/) but this doesn't appear to apply here
9. It is possible to use [Derived Columns](https://docs.honeycomb.io/working-with-your-data/use-advanced-operators/derived-columns/) to extract this information, however that requires setup of a new derived column for each JSON property that you want to query on