---
name: dotnet-gha-patterns
category: operations
subcategory: ci-cd
description: Composes GitHub Actions workflows. Reusable workflows, composite actions, matrix, caching.
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

# dotnet-gha-patterns

Composable GitHub Actions workflow patterns for .NET projects: reusable workflows with `workflow_call`, composite
actions for shared step sequences, matrix builds across TFMs and operating systems, path-based triggers, concurrency
groups for duplicate run cancellation, environment protection rules, NuGet and SDK caching strategies, and
`workflow_dispatch` inputs for manual triggers.

**Version assumptions:** GitHub Actions workflow syntax v2. `actions/setup-dotnet@v4` for .NET 8/9/10 support.
`actions/cache@v4` for dependency caching.

## Scope

- Reusable workflows with workflow_call
- Composite actions for shared step sequences
- Matrix builds across TFMs and operating systems
- Path-based triggers and concurrency groups
- NuGet and SDK caching strategies
- workflow_dispatch inputs for manual triggers

## Out of scope

- Starter CI/CD templates -- see [skill:dotnet-add-ci]
- CLI release pipelines (tag-triggered build-package-release for CLI tools) -- see [skill:dotnet-cli-release-pipeline]
- Benchmark CI workflows -- see [skill:dotnet-ci-benchmarking]
- Azure DevOps pipeline patterns -- see [skill:dotnet-ado-patterns]
- Build/test specifics -- see [skill:dotnet-gha-build-test]
- Publishing workflows -- see [skill:dotnet-gha-publish]
- Deployment patterns -- see [skill:dotnet-gha-deploy]

Cross-references: [skill:dotnet-add-ci] for starter templates that these patterns extend,
[skill:dotnet-cli-release-pipeline] for CLI-specific release automation, [skill:dotnet-ci-benchmarking] for
benchmark-specific CI integration.

---

## Reusable Workflows (`workflow_call`)

### Defining a Reusable Workflow

Reusable workflows allow callers to invoke an entire workflow as a single step. Define inputs, outputs, and secrets for
a clean contract:

````yaml

# .github/workflows/build-reusable.yml
name: Build (Reusable)

on:
  workflow_call:
    inputs:
      dotnet-version:
        description: '.NET SDK version to install'
        required: false
        type: string
        default: '8.0.x'
      configuration:
        description: 'Build configuration'
        required: false
        type: string
        default: 'Release'
      project-path:
        description: 'Path to solution or project file'
        required: true
        type: string
    outputs:
      artifact-name:
        description: 'Name of the uploaded build artifact'
        value: ${{ jobs.build.outputs.artifact-name }}
    secrets:
      NUGET_AUTH_TOKEN:
        description: 'NuGet feed authentication token'
        required: false

jobs:
  build:
    runs-on: ubuntu-latest
    outputs:
      artifact-name: build-${{ github.sha }}
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ inputs.dotnet-version }}

      - name: Restore
        run: dotnet restore ${{ inputs.project-path }}

      - name: Build
        run: dotnet build ${{ inputs.project-path }} -c ${{ inputs.configuration }} --no-restore

      - name: Upload build artifact
        uses: actions/upload-artifact@v4
        with:
          name: build-${{ github.sha }}
          path: |
            **/bin/${{ inputs.configuration }}/**
          retention-days: 7

```text

### Calling a Reusable Workflow

```yaml

# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    uses: ./.github/workflows/build-reusable.yml
    with:
      dotnet-version: '8.0.x'
      project-path: MyApp.sln
    secrets:
      NUGET_AUTH_TOKEN: ${{ secrets.NUGET_AUTH_TOKEN }}

  test:
    needs: build
    uses: ./.github/workflows/test-reusable.yml
    with:
      dotnet-version: '8.0.x'
      project-path: MyApp.sln

```yaml

### Cross-Repository Reusable Workflows

Reference workflows from other repositories using the full path:

```yaml

jobs:
  build:
    uses: my-org/.github-workflows/.github/workflows/dotnet-build.yml@v1
    with:
      dotnet-version: '9.0.x'
    secrets: inherit # pass all secrets from caller

```yaml

Use `secrets: inherit` when the reusable workflow needs access to the same secrets as the calling workflow without
explicit enumeration.

---

## Composite Actions

### Creating a Composite Action

Composite actions bundle multiple steps into a single reusable action. Use them for shared step sequences that appear
across multiple workflows:

```yaml

# .github/actions/dotnet-setup/action.yml
name: 'Setup .NET Environment'
description: 'Install .NET SDK and restore NuGet packages with caching'

inputs:
  dotnet-version:
    description: '.NET SDK version'
    required: false
    default: '8.0.x'
  project-path:
    description: 'Path to solution or project'
    required: true

runs:
  using: 'composite'
  steps:
    - name: Setup .NET SDK
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ inputs.dotnet-version }}

    - name: Cache NuGet packages
      uses: actions/cache@v4
      with:
        path: ~/.nuget/packages
        key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj', '**/Directory.Packages.props') }}
        restore-keys: |
          nuget-${{ runner.os }}-

    - name: Restore dependencies
      shell: bash
      run: dotnet restore ${{ inputs.project-path }}

```bash

### Using a Composite Action

```yaml

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET environment
        uses: ./.github/actions/dotnet-setup
        with:
          dotnet-version: '9.0.x'
          project-path: MyApp.sln

      - name: Build
        run: dotnet build MyApp.sln -c Release --no-restore

```text

### Reusable Workflow vs Composite Action

| Feature          | Reusable Workflow               | Composite Action                |
| ---------------- | ------------------------------- | ------------------------------- |
| Scope            | Entire job with runner          | Steps within a job              |
| Runner selection | Own `runs-on`                   | Caller's runner                 |
| Secrets access   | Explicit or `inherit`           | Caller's context                |
| Outputs          | Job-level outputs               | Step-level outputs              |
| Best for         | Complete build/test/deploy jobs | Shared setup/teardown sequences |

---

## Matrix Builds

### Multi-TFM and Multi-OS Matrix

```yaml

jobs:
  test:
    strategy:
      fail-fast: false
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
        dotnet-version: ['8.0.x', '9.0.x']
        include:
          - os: ubuntu-latest
            dotnet-version: '10.0.x'
        exclude:
          - os: macos-latest
            dotnet-version: '8.0.x'
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET ${{ matrix.dotnet-version }}
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ matrix.dotnet-version }}

      - name: Test
        run:
          dotnet test --framework net${{ matrix.dotnet-version == '8.0.x' && '8.0' || matrix.dotnet-version == '9.0.x'
          && '9.0' || '10.0' }}

```text

**Key decisions:**

- `fail-fast: false` ensures all matrix combinations run even if one fails, giving full signal on which platforms/TFMs
  are broken
- `include` adds specific combinations not in the Cartesian product
- `exclude` removes combinations that are unnecessary or unsupported

### Dynamic Matrix from JSON

Generate matrix values dynamically for complex scenarios:

```yaml

jobs:
  compute-matrix:
    runs-on: ubuntu-latest
    outputs:
      matrix: ${{ steps.set-matrix.outputs.matrix }}
    steps:
      - uses: actions/checkout@v4
      - id: set-matrix
        shell: bash
        run: |
          set -euo pipefail
          # Extract TFMs from Directory.Build.props or csproj files
          TFMS=$(grep -rh '<TargetFrameworks\?>' **/*.csproj | \
            sed 's/.*<TargetFrameworks\?>//' | sed 's/<.*//' | \
            tr ';' '\n' | sort -u | jq -R . | jq -sc .)
          echo "matrix={\"tfm\":$TFMS}" >> "$GITHUB_OUTPUT"

  test:
    needs: compute-matrix
    strategy:
      matrix: ${{ fromJson(needs.compute-matrix.outputs.matrix) }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: dotnet test --framework ${{ matrix.tfm }}

```text

---

## Path-Based Triggers

### Selective Workflow Execution

Trigger workflows only when relevant files change. Reduces CI cost and feedback time:

```yaml

on:
  push:
    branches: [main]
    paths:
      - 'src/**'
      - 'tests/**'
      - '*.sln'
      - 'Directory.Build.props'
      - 'Directory.Packages.props'
      - '.github/workflows/ci.yml'
  pull_request:
    branches: [main]
    paths:
      - 'src/**'
      - 'tests/**'
      - '*.sln'
      - 'Directory.Build.props'
      - 'Directory.Packages.props'

```xml

### Ignoring Non-Code Changes

Use `paths-ignore` to skip builds for documentation-only changes:

```yaml

on:
  push:
    branches: [main]
    paths-ignore:
      - 'docs/**'
      - '*.md'
      - 'LICENSE'
      - '.editorconfig'

```markdown

**Choose `paths` or `paths-ignore`, not both.** When both are specified on the same event, `paths-ignore` is ignored.
Use `paths` (allowlist) for focused workflows; use `paths-ignore` (denylist) for broad workflows.

---

## Concurrency Groups

### Cancelling Duplicate Runs

Prevent wasted CI time by cancelling in-progress runs when new commits are pushed to the same branch or PR:

```yaml

concurrency:
  group: ci-${{ github.ref }}
  cancel-in-progress: true

```yaml

### Environment-Scoped Concurrency

Prevent parallel deployments to the same environment:

```yaml

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
