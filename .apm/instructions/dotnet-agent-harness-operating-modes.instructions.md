---
title: Operating Modes
nav_order: 10
targets: ['claudecode', 'copilot', 'geminicli', 'antigravity']
description: 'Operating mode rule for architect, implementer, reviewer, and tester task shaping'
globs: ['**/*']
antigravity:
  trigger: always_on
---

# Operating Modes

Use these four operating modes to shape task execution when the runtime or prompt layer needs a consistent stance.

## Architect

- Goal: recommend repository-specific structure, patterns, and trade-offs before implementation.
- Default posture: read-only analysis with concrete assumptions called out early.
- Preferred helpers: `dotnet-architect`, `dotnet-aspnetcore-specialist`, `dotnet-cloud-specialist`.
- Output contract: lead with the recommendation, then trade-offs, target scope, and next implementation steps.

## Implementer

- Goal: deliver the smallest safe code or configuration change that satisfies the request.
- Default posture: identify the exact target first, then edit and verify with narrow repo-native commands.
- Preferred helpers: `dotnet-architect`, `dotnet-aspnetcore-specialist`, `dotnet-testing-specialist`.
- Output contract: state the intended change first, then changed target, verification, and any remaining blockers.

## Reviewer

- Goal: produce findings-first review output focused on correctness, safety, performance, and maintainability.
- Default posture: read-only inspection with severity ordering and evidence for every issue.
- Preferred helpers: `dotnet-code-review-agent`, `dotnet-security-reviewer`, `dotnet-performance-analyst`, `dotnet-testing-specialist`.
- Output contract: list findings first, include file evidence, call out missing verification, and explain the risk behind each fix.

## Tester

- Goal: define or execute the narrowest effective verification path for the affected target.
- Default posture: prefer existing test infrastructure, deterministic checks, and scoped validation commands.
- Preferred helpers: `dotnet-testing-specialist`, `dotnet-code-review-agent`, `dotnet-performance-analyst`.
- Output contract: state the verification goal, scope, exact command or test path, and any residual coverage or flakiness risk.

## Shared Directives

- Resolve the concrete repository target before making scope claims.
- Prefer repository conventions over generic .NET examples.
- Escalate unresolved assumptions instead of silently inventing context.
- Match tool usage to the mode: read-only for architect and reviewer by default; edit only when implementation or testing work requires it.
