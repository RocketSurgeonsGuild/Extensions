---
name: dotnet-github-docs
category: developer-experience
subcategory: docs
description: Creates GitHub-native docs. README badges, CONTRIBUTING, issue/PR templates, repo metadata.
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for foundation tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-github-docs

GitHub-native documentation patterns for .NET projects: README structure with NuGet/CI/coverage badges and installation
instructions, CONTRIBUTING.md with fork-PR workflow and development setup, issue templates (bug report with .NET version
and repro steps, feature request with problem/solution/alternatives), PR templates with testing checklist and breaking
changes section, GitHub Pages setup for documentation sites, repository metadata (CODEOWNERS, FUNDING.yml, social
preview, topics/tags), and Mermaid diagram embedding in README files.

**Version assumptions:** .NET 8.0+ baseline for code examples. GitHub Actions for CI badges. NuGet.org for package
badges.

## Scope

- README structure with NuGet/CI/coverage badges
- CONTRIBUTING.md with fork-PR workflow
- Issue templates (bug report, feature request)
- PR templates with testing checklist
- Repository metadata (CODEOWNERS, FUNDING.yml, topics/tags)
- Mermaid diagram embedding in README files

## Out of scope

- CI/CD deployment workflows for GitHub Pages -- see [skill:dotnet-gha-deploy]
- Changelog generation and release versioning -- see [skill:dotnet-release-management]
- Documentation platform selection -- see [skill:dotnet-documentation-strategy]
- Mermaid diagram syntax details -- see [skill:dotnet-mermaid-diagrams]
- Project file structure and solution organization -- see [skill:dotnet-project-structure]

Cross-references: [skill:dotnet-gha-deploy] for GitHub Pages deployment pipelines, [skill:dotnet-release-management] for
changelog format and versioning, [skill:dotnet-mermaid-diagrams] for .NET-specific Mermaid diagrams in READMEs,
[skill:dotnet-project-structure] for project metadata context, [skill:dotnet-documentation-strategy] for doc platform
selection.

---

## README Structure for .NET Projects

A well-structured README provides immediate context for contributors and consumers of a .NET project.

### Badges

Place badges at the top of the README, grouped by category:

``````markdown
# My.Library

[![NuGet](https://img.shields.io/nuget/v/My.Library.svg)](https://www.nuget.org/packages/My.Library)
[![NuGet Downloads](https://img.shields.io/nuget/dt/My.Library.svg)](https://www.nuget.org/packages/My.Library)
[![Build Status](https://github.com/mycompany/my-library/actions/workflows/ci.yml/badge.svg)](https://github.com/mycompany/my-library/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/mycompany/my-library/branch/main/graph/badge.svg)](https://codecov.io/gh/mycompany/my-library)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

`````yaml

**Badge categories for .NET projects:**

| Badge           | Source                 | Notes                                        |
| --------------- | ---------------------- | -------------------------------------------- |
| NuGet version   | shields.io + nuget.org | Use package ID, not assembly name            |
| NuGet downloads | shields.io + nuget.org | Shows adoption; use `dt` for total downloads |
| Build status    | GitHub Actions         | Link to the CI workflow                      |
| Code coverage   | Codecov / Coveralls    | Requires CI integration                      |
| License         | shields.io             | Match the license in the repo                |
| .NET version    | shields.io             | Optional; shows minimum supported TFM        |

### Recommended README Sections

````markdown

# My.Library

[badges here]

Short one-paragraph description of what the library does and why it exists.

## Installation

```shell

dotnet add package My.Library

```bash

Or via PackageReference in your `.csproj`:

```xml

<PackageReference Include="My.Library" Version="1.0.0" />

```csharp

## Quick Start

```csharp

using My.Library;

var service = new WidgetService();
var widget = await service.CreateWidgetAsync("example");
Console.WriteLine(widget.Id);

```text

## Features

- Feature 1: brief description
- Feature 2: brief description
- Feature 3: brief description

## Documentation

Full documentation is available at [https://mycompany.github.io/my-library](https://mycompany.github.io/my-library).

## Architecture

[Mermaid architecture diagram -- see [skill:dotnet-mermaid-diagrams] for patterns]

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull
requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a history of changes. For changelog format conventions, see
[skill:dotnet-release-management].

### Architecture Diagram in README

Embed a Mermaid architecture diagram directly in the README for visual context. GitHub renders Mermaid fenced code
blocks natively:

````markdown
## Architecture

````mermaid

graph TB
    subgraph Client
        App["Consumer App"]
    end
    subgraph Library["My.Library"]
        API["Public API Surface"]
        Core["Core Engine"]
        Cache["In-Memory Cache"]
    end
    App --> API
    API --> Core
    Core --> Cache

```text

````
````

See [skill:dotnet-mermaid-diagrams] for .NET-specific diagram patterns including C4-style architecture, sequence
diagrams for API flows, and class diagrams for domain models.

---

## CONTRIBUTING.md Patterns

### Fork-PR Workflow

````markdown
# Contributing to My.Library

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR-USERNAME/my-library.git`
3. Create a feature branch: `git checkout -b feature/my-feature`
4. Make your changes
5. Submit a pull request

## Development Setup

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- An IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/) with C#
  Dev Kit, or [JetBrains Rider](https://www.jetbrains.com/rider/)

### Building

````shell

dotnet restore
dotnet build

```bash

````
````

### Running Tests

````shell

dotnet test

```bash

To run tests with coverage:

```shell

dotnet test --collect:"XPlat Code Coverage"

```bash

### Coding Standards

- Follow the
  [.NET coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use `dotnet format` to enforce code style before committing
- All public APIs must have XML documentation comments
- New features must include unit tests

## Pull Request Process

1. Update documentation for any changed public APIs
2. Add or update tests to cover your changes
3. Ensure all tests pass: `dotnet test`
4. Ensure code compiles without warnings: `dotnet build -warnaserror`
5. Update the CHANGELOG.md with your changes under the `[Unreleased]` section
6. The PR will be reviewed by a maintainer

## Reporting Issues

- Use the [Bug Report](.github/ISSUE_TEMPLATE/bug_report.md) template for bugs
- Use the [Feature Request](.github/ISSUE_TEMPLATE/feature_request.md) template for enhancements

````

---

## Issue Templates

### Bug Report Template

```yaml
# .github/ISSUE_TEMPLATE/bug_report.yml
name: Bug Report
description: Report a bug in the library
title: '[Bug]: '
labels: ['bug', 'triage']
body:
  - type: markdown
    attributes:
      value: |
        Thank you for reporting a bug. Please fill out the information below
        to help us diagnose and fix the issue.

  - type: textarea
    id: description
    attributes:
      label: Description
      description: A clear and concise description of the bug.
    validations:
      required: true

  - type: textarea
    id: repro-steps
    attributes:
      label: Steps to Reproduce
      description: Steps to reproduce the behavior.
      value: |
        1. Install package version X
        2. Call method Y with parameters Z
        3. Observe error
    validations:
      required: true

  - type: textarea
    id: expected
    attributes:
      label: Expected Behavior
      description: What you expected to happen.
    validations:
      required: true

  - type: textarea
    id: actual
    attributes:
      label: Actual Behavior
      description: What actually happened. Include any error messages or stack traces.
    validations:
      required: true

  - type: input
    id: dotnet-version
    attributes:
      label: .NET Version
      description: 'Output of `dotnet --version`'
      placeholder: '8.0.100'
    validations:
      required: true

  - type: dropdown
    id: os
    attributes:
      label: Operating System
      options:
        - Windows
        - macOS
        - Linux
    validations:
      required: true

  - type: textarea
    id: additional
    attributes:
      label: Additional Context
      description: Any other context about the problem (project type, related packages, etc.)
```

### Feature Request Template

````yaml

# .github/ISSUE_TEMPLATE/feature_request.yml
name: Feature Request
description: Suggest a new feature or enhancement
title: '[Feature]: '
labels: ['enhancement']
body:
  - type: textarea
    id: problem
    attributes:
      label: Problem Statement
      description: Describe the problem this feature would solve.
      placeholder: "I'm always frustrated when..."
    validations:
      required: true

  - type: textarea
    id: solution
    attributes:
      label: Proposed Solution
      description: Describe the solution you'd like to see.
    validations:
      required: true

  - type: textarea
    id: alternatives
    attributes:
      label: Alternatives Considered
      description: Describe any alternative solutions or features you've considered.

  - type: textarea
    id: api-surface
    attributes:
      label: API Surface (if applicable)
      description: |
        If you have a proposed API design, include it here.
      render: csharp

```csharp

### Question / Discussion Template

```yaml

# .github/ISSUE_TEMPLATE/question.yml
name: Question
description: Ask a question about using the library
title: '[Question]: '
labels: ['question']
body:
  - type: textarea
    id: question

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
