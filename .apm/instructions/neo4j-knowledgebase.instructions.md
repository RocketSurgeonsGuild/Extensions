---
paths:
    - '**'
description: Instructions for using the local Neo4j knowledge base to help understand and work with code while saving tokens.
---

Use the `neo4j-local` MCP server to query and manage the local Neo4j knowledge graph. This is a fully local graph database populated by ingesting unstructured documents via llm-graph-builder.

**When to use**: When the user asks about topics that may be in the knowledge base, wants to find relationships between concepts, asks to store new information, or wants to explore the graph.

**Do not use**: For general programming questions, library docs, or anything not likely to be in the local knowledge base.

## Available MCP Tools

- `get_neo4j_schema` — inspect current node labels, relationship types, and property keys
- `read_neo4j_cypher` — run read-only Cypher queries (MATCH, RETURN, etc.)
- `write_neo4j_cypher` — run write queries (CREATE, MERGE, SET, DELETE)

## Query Patterns

### Explore what's in the graph

```cypher
// See all node labels and counts
CALL apoc.meta.stats() YIELD labels RETURN labels

// Browse entities of a type
MATCH (n:Entity) RETURN n LIMIT 25

// Find connections around a concept
MATCH (n)-[r]-(m)
WHERE n.id CONTAINS 'keyword' OR n.name CONTAINS 'keyword'
RETURN n, r, m LIMIT 50
```

### Find related concepts

```cypher
// Shortest path between two concepts
MATCH path = shortestPath((a)-[*..6]-(b))
WHERE a.name = 'ConceptA' AND b.name = 'ConceptB'
RETURN path

// All neighbors of a node
MATCH (n {name: 'ConceptName'})-[r]-(neighbor)
RETURN type(r), neighbor.name, neighbor
```

### Check schema before querying

```cypher
CALL apoc.meta.data()
```

## Service Status

The knowledgebase requires these services to be running:

- Neo4j + Neo4j Cypher MCP: `cd <knowledgebase-dir> && ./run.sh --start`
- MCP endpoint: http://localhost:8000/mcp/
- Neo4j Browser UI: http://localhost:7474

If MCP tools are unavailable, the services may need to be started. Inform the user rather than guessing.

## Ingesting New Documents

To add new knowledge:

1. Open http://localhost:8080 (llm-graph-builder UI)
2. Select `ollama_llama3.2` from the model dropdown
3. Upload documents (PDF, TXT, Markdown, HTML)
4. Click **Generate Graph**
5. Use `get_neo4j_schema` to confirm new nodes appeared

## Windows-Specific Notes

- Ollama runs natively on Windows (not in Docker) — start with `ollama serve` or via system tray
- Docker services: `./run.sh --start` (or equivalent PowerShell/batch wrapper)
- `OLLAMA_BASE_URL` in `.env` should point to `http://host.docker.internal:11434`
