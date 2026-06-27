---
title: API Reference
description: Auto-generated API reference for the Indago public surface, covering the Indago assembly and Indago.Abstractions.
tags:
    - api
    - source-generator
---

# API Reference

This section is the **auto-generated API reference** for Indago's public surface. The pages below are produced from the library's XML doc comments and compiled metadata, so they always reflect the shipped assemblies rather than hand-written prose.

Indago's public API is intentionally small — nearly all of the work happens in the source generator at build time. The reference is organized by the two top-level namespaces:

- [**Indago**](./Indago/) — the Indago assembly: the core public surface, including `IIndagoProvider`, the opt-out attributes, and the dependency-injection extensions.
- [**Indago.Abstractions**](./Indago.Abstractions/) — the fluent selector interfaces and type/assembly filters you compose inside a selector expression.

> The individual member pages under each namespace are generated from XML doc comments and compiled assembly metadata. Do not edit them by hand — regenerate them from the source instead.

If you are new to Indago, start with the [guide](../guide/) for a conceptual overview before diving into the reference.
