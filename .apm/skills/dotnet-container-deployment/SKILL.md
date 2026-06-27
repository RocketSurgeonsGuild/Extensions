---
name: dotnet-container-deployment
category: operations
subcategory: deployment
description: Deploys .NET containers. Kubernetes probes, Docker Compose for local dev, CI/CD integration.
license: MIT
targets: ['*']
tags: [cicd, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for cicd tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-container-deployment

Deploying .NET containers to Kubernetes and local development environments. Covers Kubernetes Deployment + Service +
probe YAML, Docker Compose for local dev workflows, and CI/CD integration for building and pushing container images.

## Scope

- Kubernetes Deployment, Service, and probe YAML for .NET apps
- Docker Compose for local development workflows
- CI/CD integration for building and pushing container images

## Out of scope

- Dockerfile authoring, multi-stage builds, and base image selection -- see [skill:dotnet-containers]
- Advanced CI/CD pipeline patterns (matrix builds, deploy pipelines) -- see [skill:dotnet-gha-deploy] and
  [skill:dotnet-ado-patterns]
- DI and async patterns -- see [skill:dotnet-csharp-dependency-injection] and [skill:dotnet-csharp-async-patterns]
- Testing container deployments -- see [skill:dotnet-integration-testing] and [skill:dotnet-playwright]

Cross-references: [skill:dotnet-containers] for Dockerfile and image best practices, [skill:dotnet-observability] for
health check endpoint patterns used by Kubernetes probes.

---

## Kubernetes Deployment

### Deployment Manifest

A production-ready Kubernetes Deployment for a .NET API:

````yaml

apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-api
  labels:
    app: order-api
    app.kubernetes.io/name: order-api
    app.kubernetes.io/version: '1.0.0'
    app.kubernetes.io/component: api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: order-api
  template:
    metadata:
      labels:
        app: order-api
    spec:
      containers:
        - name: order-api
          image: ghcr.io/myorg/order-api:1.0.0
          ports:
            - containerPort: 8080
              protocol: TCP
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: 'Production'
            - name: OTEL_EXPORTER_OTLP_ENDPOINT
              value: 'http://otel-collector.monitoring:4317'
            - name: OTEL_SERVICE_NAME
              value: 'order-api'
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: order-api-secrets
                  key: connection-string
          resources:
            requests:
              cpu: '100m'
              memory: '128Mi'
            limits:
              cpu: '500m'
              memory: '512Mi'
          livenessProbe:
            httpGet:
              path: /health/live
              port: 8080
            initialDelaySeconds: 10
            periodSeconds: 15
            timeoutSeconds: 3
            failureThreshold: 3
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            initialDelaySeconds: 5
            periodSeconds: 10
            timeoutSeconds: 3
            failureThreshold: 3
          startupProbe:
            httpGet:
              path: /health/live
              port: 8080
            initialDelaySeconds: 0
            periodSeconds: 5
            failureThreshold: 30
      securityContext:
        runAsNonRoot: true
        runAsUser: 1654
        fsGroup: 1654
      terminationGracePeriodSeconds: 30

```text

### Service Manifest

Expose the Deployment within the cluster:

```yaml

apiVersion: v1
kind: Service
metadata:
  name: order-api
  labels:
    app: order-api
spec:
  type: ClusterIP
  selector:
    app: order-api
  ports:
    - port: 80
      targetPort: 8080
      protocol: TCP
      name: http

```text

### ConfigMap for Non-Sensitive Configuration

```yaml

apiVersion: v1
kind: ConfigMap
metadata:
  name: order-api-config
data:
  ASPNETCORE_ENVIRONMENT: 'Production'
  Logging__LogLevel__Default: 'Information'
  Logging__LogLevel__Microsoft.AspNetCore: 'Warning'

```text

Reference in the Deployment:

```yaml

envFrom:
  - configMapRef:
      name: order-api-config

```yaml

### Secrets for Sensitive Configuration

```yaml

apiVersion: v1
kind: Secret
metadata:
  name: order-api-secrets
type: Opaque
stringData:
  connection-string: 'Host=postgres;Database=orders;Username=app;Password=<DB_PASSWORD_PLACEHOLDER>'

```text

In production, use an external secrets operator (e.g., External Secrets Operator, Sealed Secrets) rather than plain
Kubernetes Secrets stored in source control.

---

## Kubernetes Probes

Probes tell Kubernetes how to check application health. They map to the health check endpoints defined in your .NET
application (see [skill:dotnet-observability]).

### Probe Types

| Probe         | Purpose                            | Endpoint        | Failure Action                                          |
| ------------- | ---------------------------------- | --------------- | ------------------------------------------------------- |
| **Startup**   | Has the app finished initializing? | `/health/live`  | Keep waiting (up to `failureThreshold * periodSeconds`) |
| **Liveness**  | Is the process healthy?            | `/health/live`  | Restart the pod                                         |
| **Readiness** | Can the process serve traffic?     | `/health/ready` | Remove from Service endpoints                           |

### Probe Configuration Guidelines

```yaml

# Startup probe: give the app time to initialize
# Total startup budget: failureThreshold * periodSeconds = 30 * 5 = 150s
startupProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 0
  periodSeconds: 5
  failureThreshold: 30

# Liveness probe: detect deadlocks and hangs
# Only runs after startup probe succeeds
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  periodSeconds: 15
  timeoutSeconds: 3
  failureThreshold: 3

# Readiness probe: control traffic routing
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  periodSeconds: 10
  timeoutSeconds: 3
  failureThreshold: 3

```text

### Graceful Shutdown

.NET responds to `SIGTERM` and begins graceful shutdown. Configure `terminationGracePeriodSeconds` to allow in-flight
requests to complete:

```yaml

spec:
  terminationGracePeriodSeconds: 30

```yaml

In your application, use `IHostApplicationLifetime` to handle shutdown:

```csharp

app.Lifetime.ApplicationStopping.Register(() =>
{
    // Perform cleanup: flush telemetry, close connections
    Log.CloseAndFlush();
});

```text

Ensure the `Host.ShutdownTimeout` allows in-flight requests to complete:

```csharp

builder.Host.ConfigureHostOptions(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(25);
});

```text

Set `ShutdownTimeout` to a value less than `terminationGracePeriodSeconds` to ensure the app shuts down before
Kubernetes sends `SIGKILL`.

---

## Docker Compose for Local Development

Docker Compose provides a local development environment that mirrors production dependencies.

### Basic Compose File

```yaml

# docker-compose.yml
services:
  order-api:
    build:
      context: .
      dockerfile: src/OrderApi/Dockerfile
    ports:
      - '8080:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=orders;Username=app;Password=<DB_PASSWORD_PLACEHOLDER>
      - OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    # Note: CMD-SHELL + curl requires a base image with shell and curl installed.
    # Chiseled/distroless images lack both. For chiseled images, either use a
    # non-chiseled dev target in the Dockerfile or omit the healthcheck and rely
    # on depends_on ordering (acceptable for local dev).
    healthcheck:
      test: ['CMD-SHELL', 'curl -f http://localhost:8080/health/live || exit 1']
      interval: 10s
      timeout: 3s
      retries: 3
      start_period: 10s

  postgres:
    image: postgres:17
    environment:
      POSTGRES_DB: orders
      POSTGRES_USER: app
      POSTGRES_PASSWORD: devpassword
    ports:
      - '5432:5432'
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ['CMD-SHELL', 'pg_isready -U app -d orders']
      interval: 5s
      timeout: 3s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - '6379:6379'
    healthcheck:
      test: ['CMD', 'redis-cli', 'ping']
      interval: 5s
      timeout: 3s
      retries: 5

volumes:
  postgres-data:

```text

### Development Override

Use a separate override file for development-specific settings:

```yaml

# docker-compose.override.yml (auto-loaded by docker compose up)
services:
  order-api:
    build:
      target: build # Stop at build stage for faster rebuilds
    volumes:
      - .:/src # Mount source for hot reload
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - DOTNET_USE_POLLING_FILE_WATCHER=true
    command: ['dotnet', 'watch', 'run', '--project', 'src/OrderApi/OrderApi.csproj']

```bash

### Observability Stack

Add an OpenTelemetry collector and Grafana for local observability:

```yaml

# docker-compose.observability.yml
services:
  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    command: ['--config=/etc/otelcol-config.yaml']
    volumes:
      - ./infra/otelcol-config.yaml:/etc/otelcol-config.yaml
    ports:
      - '4317:4317' # OTLP gRPC
      - '4318:4318' # OTLP HTTP

  grafana:
    image: grafana/grafana:latest
    ports:
      - '3000:3000'
    volumes:
      - grafana-data:/var/lib/grafana

volumes:
  grafana-data:

```text

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
