- WinForms -> [skill:dotnet-winforms-basics]

### Testing

- Strategy/what to test -> [skill:dotnet-testing-strategy]
- xUnit v3 -> [skill:dotnet-xunit]
- Integration tests -> [skill:dotnet-integration-testing]
- UI testing -> [skill:dotnet-ui-testing-core] + framework-specific skill
- Snapshot testing -> [skill:dotnet-snapshot-testing]
- Coverage/quality -> [skill:dotnet-test-quality]

### Performance Work

- Benchmarking -> [skill:dotnet-benchmarkdotnet]
- Optimization patterns -> [skill:dotnet-performance-patterns]
- Profiling -> [skill:dotnet-profiling]
- CI benchmarks -> [skill:dotnet-ci-benchmarking]
- GC tuning, memory management -> [skill:dotnet-gc-memory]

### Native AOT / Trimming

- AOT compilation -> [skill:dotnet-native-aot]
- Architecting for AOT -> [skill:dotnet-aot-architecture]
- Trimming -> [skill:dotnet-trimming]
- WASM AOT -> [skill:dotnet-aot-wasm]

### CLI Tools

- System.CommandLine -> [skill:dotnet-system-commandline]
- CLI design -> [skill:dotnet-cli-architecture]
- Distribution -> [skill:dotnet-cli-distribution], [skill:dotnet-cli-packaging], [skill:dotnet-cli-release-pipeline]
- Tool install, manifest, restore -> [skill:dotnet-tool-management]

### Containers & Deployment

- Dockerfiles -> [skill:dotnet-containers]
- Kubernetes/Compose -> [skill:dotnet-container-deployment]

### Security

- OWASP compliance -> [skill:dotnet-security-owasp]
- Secrets management -> [skill:dotnet-secrets-management]
- Cryptography -> [skill:dotnet-cryptography]

### Communication Patterns

- gRPC -> [skill:dotnet-grpc]
- Real-time (SignalR, SSE) -> [skill:dotnet-realtime-communication]
- Choosing protocol -> [skill:dotnet-service-communication]
- Messaging, event-driven (Service Bus, RabbitMQ) -> [skill:dotnet-messaging-patterns]

### CI/CD Setup

- GitHub Actions -> [skill:dotnet-gha-patterns], [skill:dotnet-gha-build-test], [skill:dotnet-gha-publish],
  [skill:dotnet-gha-deploy]
- Azure DevOps -> [skill:dotnet-ado-patterns], [skill:dotnet-ado-build-test], [skill:dotnet-ado-publish],
  [skill:dotnet-ado-unique]

### Packaging & Releases

- NuGet publishing -> [skill:dotnet-nuget-authoring]
- MSIX -> [skill:dotnet-msix]
- GitHub Releases -> [skill:dotnet-github-releases]
- Versioning -> [skill:dotnet-release-management]

### Multi-Targeting

- Multi-TFM builds -> [skill:dotnet-multi-targeting]
- Version upgrades -> [skill:dotnet-version-upgrade]

### Localization

- i18n/l10n -> [skill:dotnet-localization]

### Documentation

- Doc strategy -> [skill:dotnet-documentation-strategy]
- Diagrams -> [skill:dotnet-mermaid-diagrams]
- GitHub docs -> [skill:dotnet-github-docs]
- XML docs -> [skill:dotnet-xml-docs]
- API docs -> [skill:dotnet-api-docs]

### Agent Assistance

- Agent making .NET mistakes -> [skill:dotnet-agent-gotchas]
- Build errors -> [skill:dotnet-build-analysis]
- Reading .csproj -> [skill:dotnet-csproj-reading]
- Navigating solutions -> [skill:dotnet-solution-navigation]

### Background Work

- Background services, queues -> [skill:dotnet-background-services]
- Observability/logging -> [skill:dotnet-observability]
- Log pipeline design, aggregation, PII scrubbing -> [skill:dotnet-structured-logging]

### Cloud & Orchestration

- .NET Aspire, service discovery, AppHost -> [skill:dotnet-aspire-patterns]

### AI & LLM Integration

- Microsoft Agent Framework, AI agents, workflows, tools -> [skill:dotnet-microsoft-agent-framework]

### Specialist Agent Routing

For complex analysis that benefits from domain expertise, delegate to specialist agents:

- Async/await performance, ValueTask, ConfigureAwait, IO.Pipelines -> [subagent:dotnet-async-performance-specialist]
- ASP.NET Core middleware, request pipeline, DI lifetimes, diagnostic scenarios -> [subagent:dotnet-aspnetcore-specialist]
- Test architecture, test type selection, test data management, microservice testing ->
  [subagent:dotnet-testing-specialist]
- Cloud deployment, .NET Aspire, AKS, CI/CD pipelines, distributed tracing -> [subagent:dotnet-cloud-specialist]
- Microsoft Agent Framework: agent design, workflow orchestration, multi-agent patterns, tool integration ->
  [subagent:dotnet-microsoft-agent-framework-specialist]
- General code review (correctness, performance, security, architecture) -> [subagent:dotnet-code-review-agent]
