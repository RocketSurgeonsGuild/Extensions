```text

Run with the observability stack:

```bash

docker compose -f docker-compose.yml -f docker-compose.observability.yml up

```bash

---

## CI/CD Integration

Basic CI/CD patterns for building and pushing .NET container images. Advanced CI patterns (matrix builds, environment
promotion, deploy pipelines) -- see [skill:dotnet-gha-publish], [skill:dotnet-gha-deploy], and
[skill:dotnet-ado-publish].

### GitHub Actions: Build and Push

```yaml

# .github/workflows/docker-publish.yml
name: Build and Push Container

on:
  push:
    branches: [main]
    tags: ['v*']

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write

    steps:
      - uses: actions/checkout@v4

      - name: Log in to container registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=ref,event=branch
            type=semver,pattern={{version}}
            type=semver,pattern={{major}}.{{minor}}
            type=sha

      - name: Build and push
        uses: docker/build-push-action@v6
        with:
          context: .
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

```text

### Image Tagging Strategy

| Tag Pattern | Example             | Use Case                                    |
| ----------- | ------------------- | ------------------------------------------- |
| `latest`    | `myapi:latest`      | Development only -- never use in production |
| Semver      | `myapi:1.2.3`       | Release versions -- immutable               |
| Major.Minor | `myapi:1.2`         | Floating tag for patch updates              |
| SHA         | `myapi:sha-abc1234` | Unique per commit -- traceability           |
| Branch      | `myapi:main`        | CI builds -- latest from branch             |

### dotnet publish Container in CI

For projects using `dotnet publish /t:PublishContainer` instead of Dockerfiles:

```yaml

steps:
  - uses: actions/checkout@v4

  - uses: actions/setup-dotnet@v4
    with:
      dotnet-version: '10.0.x'

  - name: Publish container image
    run: |
      dotnet publish src/OrderApi/OrderApi.csproj \
        --os linux --arch x64 \
        /t:PublishContainer \
        -p:ContainerRegistry=${{ env.REGISTRY }} \
        -p:ContainerRepository=${{ env.IMAGE_NAME }} \
        -p:ContainerImageTag=${{ github.sha }}

```text

---

## Key Principles

- **Use startup probes** to decouple initialization time from liveness detection -- without a startup probe,
  slow-starting apps get killed before they are ready
- **Separate liveness from readiness** -- liveness checks should not include dependency health (see
  [skill:dotnet-observability] for endpoint patterns)
- **Set resource requests and limits** -- without them, pods can starve other workloads or get OOM-killed unpredictably
- **Run as non-root** -- set `runAsNonRoot: true` in the pod security context and use chiseled images (see
  [skill:dotnet-containers])
- **Use `depends_on` with health checks** in Docker Compose -- prevents app startup before dependencies are ready
- **Keep secrets out of manifests** -- use Kubernetes Secrets with external secrets operators, not plain values in
  source control
- **Match ShutdownTimeout to terminationGracePeriodSeconds** -- ensure the app finishes cleanup before Kubernetes sends
  SIGKILL

---

## Agent Gotchas

1. **Do not omit the startup probe** -- without it, the liveness probe runs during initialization and may restart
   slow-starting apps. Calculate startup budget as `failureThreshold * periodSeconds`.
2. **Do not include dependency checks in liveness probes** -- a database outage should not restart your app. Liveness
   endpoints must only check the process itself. See [skill:dotnet-observability] for the liveness vs readiness pattern.
3. **Do not use `latest` tag in Kubernetes manifests** -- `latest` is mutable and `imagePullPolicy: IfNotPresent` may
   serve stale images. Use immutable tags (semver or SHA).
4. **Do not hardcode connection strings in Kubernetes manifests** -- use Secrets or ConfigMaps referenced via
   `secretKeyRef`/`configMapRef`.
5. **Do not set `terminationGracePeriodSeconds` lower than `Host.ShutdownTimeout`** -- the app needs time to drain
   in-flight requests before Kubernetes sends SIGKILL.
6. **Do not forget `condition: service_healthy` in Docker Compose `depends_on`** -- without the condition, Compose
   starts dependent services immediately without waiting for health checks.

---



## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**
- ✅ **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- ✅ **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- ✅ **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**
```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
## References

- [Deploy ASP.NET Core to Kubernetes](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx)
- [Kubernetes Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Kubernetes probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
- [Docker Compose overview](https://docs.docker.com/compose/)
- [ASP.NET Core health checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Graceful shutdown in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host#host-shutdown)
- [GitHub Actions: Publishing Docker images](https://docs.github.com/en/actions/use-cases-and-examples/publishing-packages/publishing-docker-images)
````
