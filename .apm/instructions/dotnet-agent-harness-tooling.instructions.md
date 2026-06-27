---
title: Tooling and MCP Strategy
nav_order: 20
targets: ['*']
version: '0.0.1'
---

# Tooling and MCP Strategy

MCP servers and fallback tooling for the dotnet-agent-harness ecosystem.

## MCP Server Inventory

| Server | Type | Primary Role | Best Use Cases |
|--------|------|--------------|----------------|
| **serena** | stdio | Semantic code analysis | Symbol-level navigation, refactoring, dependency analysis, precise code edits |
| **microsoftdocs-mcp** | http | Official Microsoft documentation | First-party .NET/ASP.NET/Azure docs, authoritative API references |

## Routing Guide

```text
Need to navigate or refactor code?
  YES -> [mcp:serena] for symbol operations
  NO -> Continue...

Need official Microsoft documentation?
  YES -> [mcp:microsoftdocs-mcp] for .NET/Azure docs
  NO -> Continue...

Need documentation?
  Official Microsoft docs -> [mcp:microsoftdocs-mcp]
  Third-party libraries -> Web search + library READMEs
  Project-specific docs -> Read markdown files from docs/, wiki/
  GitHub operations -> Use gh CLI or web interface
  Other -> Traditional tools (Read, Grep, Glob, Bash)
```

## Target-Specific Behaviors

| Target | MCP Access | Notes |
|--------|-----------|-------|
| Claude Code | Full | Native MCP server support, all tools available |
| OpenCode | Filtered | Tab cycles primary agents only, `@mention` for subagents |
| Codex CLI | Full | All configured servers available |
| Copilot | Partial | Uses `execute` tool, MCP via extensions |
| Gemini CLI | Full | Similar to Claude Code for stdio/http |
| Factory Droid | Rules-only | MCP routing through generated rules |
| Antigravity | Portable | Hooks-based, minimal surface |

## Health Checks

Session start validation:
1. Verify server connectivity
2. Report available/unavailable status
3. Suggest fallbacks if needed

**Output format:**
```json
{
  "mcpHealth": {
    "serena": { "status": "available", "type": "stdio" },
    "microsoftdocs-mcp": { "status": "available", "type": "http" }
  },
  "recommendations": []
}
```

**Failure mitigation:**
| Server Down | Mitigation |
|-------------|------------|
| serena | Use Read + Grep for finding code |
| microsoftdocs-mcp | Use web search with microsoft.com/learn |

## Tool Categories

| Category | Tools | Use When |
|----------|-------|----------|
| Code Intelligence | serena | Navigation, refactoring, analysis |
| Documentation | microsoftdocs-mcp, web search | Official docs, research |
| GitHub | gh CLI | Repository operations |
| Traditional | Read, Grep, Glob, Bash | Fallback, offline scenarios |
