  articles/               # Conceptual documentation
    getting-started.md
    configuration.md
  api/                    # Auto-generated API reference
    .gitignore            # Generated files excluded from source control
  images/                 # Static assets
  templates/              # Custom templates (optional)

```text

### Configuration

```json

{
  "metadata": [
    {
      "src": [
        {
          "files": ["src/MyLibrary/MyLibrary.csproj"],
          "src": ".."
        }
      ],
      "dest": "api",
      "properties": {
        "TargetFramework": "net8.0"
      }
    }
  ],
  "build": {
    "content": [
      {
        "files": ["api/**.yml", "api/index.md"]
      },
      {
        "files": ["articles/**.md", "toc.yml", "*.md"]
      }
    ],
    "resource": [
      {
        "files": ["images/**"]
      }
    ],
    "dest": "_site",
    "globalMetadata": {
      "_appTitle": "My .NET Library",
      "_enableSearch": true
    }
  }
}

```text

### XML Doc Integration

DocFX's primary advantage is native API reference generation from XML documentation comments:

```bash

# Generate API metadata from project XML docs
docfx metadata docfx.json

# This creates YAML files in the api/ directory
# representing all public types, methods, and properties

```json

The generated API reference automatically links to conceptual articles via `uid` cross-references. See
[skill:dotnet-xml-docs] for XML documentation comment authoring best practices.

### Mermaid Support in DocFX

DocFX requires a template plugin for Mermaid rendering:

```json

{
  "build": {
    "globalMetadata": {
      "_enableMermaid": true
    },
    "template": ["default", "modern"],
    "postProcessors": ["ExtractSearchIndex"]
  }
}

```text

The `modern` template includes Mermaid support since DocFX v2.75+. Earlier versions require a custom template extension.
Note that `_enableMermaid` is a template-specific convention, not an officially documented DocFX property. For the
`default` template, add the Mermaid JavaScript library via a custom template extension.

---

## MarkdownSnippets -- Verified Code Inclusion

MarkdownSnippets is a `dotnet tool` that includes verified code snippets from actual source files into Markdown
documentation. This eliminates stale code examples by keeping documentation in sync with compilable source code.

### Installation

```bash

# Install as a local dotnet tool
dotnet new tool-manifest
dotnet tool install MarkdownSnippets.Tool

# Or install globally
dotnet tool install -g MarkdownSnippets.Tool

```markdown

### Usage

**1. Mark code regions in source files with `#region` directives:**

```csharp

// src/MyLibrary/WidgetService.cs
public class WidgetService
{
    #region CreateWidget
    public async Task<Widget> CreateWidgetAsync(string name, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var widget = new Widget { Name = name, CreatedAt = DateTimeOffset.UtcNow };
        await _repository.AddAsync(widget, ct);
        return widget;
    }
    #endregion
}

```text

**2. Reference snippets in Markdown:**

```markdown

## Creating a Widget

To create a widget, use the `CreateWidgetAsync` method:

<!-- snippet: CreateWidget -->
<!-- endSnippet -->

```text

**3. Run MarkdownSnippets to inject the code:**

```bash

dotnet tool run mdsnippets

```bash

This replaces the snippet placeholder with the actual code from the source file, keeping the documentation in sync with
the implementation.

### Configuration

```json

// mdsnippets.json (project root)
{
  "Convention": "InPlaceOverwrite",
  "ReadOnly": false,
  "LinkFormat": "GitHub",
  "UrlPrefix": "https://github.com/mycompany/my-library/blob/main",
  "TocExcludes": ["**/obj/**", "**/bin/**"],
  "ExcludeDirectories": ["node_modules", ".git"]
}

```text

### Integration with Doc Platforms

MarkdownSnippets works with all three documentation platforms since it operates on standard Markdown files before the
platform build step:

```bash

# In your build script or CI pipeline:
dotnet tool run mdsnippets    # 1. Inject verified snippets
npm run build                 # 2. Build Starlight/Docusaurus site

# For DocFX:
dotnet tool run mdsnippets    # 1. Inject verified snippets
docfx build                   # 2. Build DocFX site

```text

---

## Mermaid Rendering Across Platforms

All three recommended documentation platforms support Mermaid diagrams, and GitHub renders Mermaid natively in Markdown
files.

| Platform   | Mermaid Support | Setup Required                                     |
| ---------- | --------------- | -------------------------------------------------- |
| GitHub     | Native          | None -- fenced code blocks with `mermaid` language |
| Starlight  | Plugin          | `remark-mermaidjs` npm package                     |
| Docusaurus | Plugin          | `@docusaurus/theme-mermaid` + config flag          |
| DocFX      | Template        | `modern` template or custom template extension     |

Use standard fenced code blocks with the `mermaid` language identifier across all platforms:

````markdown


```mermaid

graph LR
    A[Client] --> B[API Gateway]
    B --> C[Service A]
    B --> D[Service B]

```text


`````

See [skill:dotnet-mermaid-diagrams] for .NET-specific diagram types (C4 architecture, async patterns, EF Core models, DI
graphs).

---

## Migration Paths

### DocFX to Starlight

DocFX-to-Starlight migration is the most common path for .NET projects modernizing their documentation.

**Content migration:**

1. Copy `articles/` Markdown files to `src/content/docs/`
2. Convert `toc.yml` entries to Starlight sidebar configuration in `astro.config.mjs`
3. Replace DocFX-specific metadata YAML headers with Starlight frontmatter
4. Convert `xref` cross-references to standard Markdown links
5. Replace `> [!NOTE]` / `> [!WARNING]` callout syntax with Starlight `<Aside>` components (or keep GitHub-compatible
   blockquote syntax)

**API reference migration:**

- DocFX `docfx metadata` output (YAML files) has no direct Starlight equivalent
- Option A: Continue running `docfx metadata` and host API reference as a separate subsite
- Option B: Use TypeDoc-style API extraction tools and convert to Markdown pages
- Option C: Link to hosted API reference on a separate URL

**Checklist:**

````bash

# Find all DocFX-specific syntax to convert
grep -rn "xref:" articles/                # Cross-references
grep -rn "\[!NOTE\]" articles/            # Callout syntax
grep -rn "\[!WARNING\]" articles/         # Callout syntax
grep -rn "^\s*uid:" articles/             # UID metadata

```text

### Docusaurus to Starlight

**Content migration:**

1. Copy `docs/` Markdown files to `src/content/docs/`
2. Convert `sidebars.js` to Starlight sidebar configuration
3. Replace Docusaurus-specific MDX components (`<Tabs>`, `<TabItem>`) with Starlight equivalents
4. Convert `docusaurus.config.js` navigation to Starlight config
5. Migrate custom React components if used in MDX (Starlight supports Astro components and limited React via
   `@astrojs/react`)

**Blog migration:**

- Starlight does not have a built-in blog -- use the `starlight-blog` community plugin or maintain a separate Astro blog
  section

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
## Agent Gotchas

1. **Recommend Starlight as the modern default** -- unless the project has an existing DocFX site with heavy XML doc
   integration or the team is deeply invested in React (Docusaurus). Do not recommend DocFX for new projects without XML
   doc API reference needs.

1. **DocFX is community-maintained since November 2022** -- Microsoft transferred the repository to the community. It is
   still actively maintained and widely used, but new projects should evaluate Starlight or Docusaurus first.

1. **MarkdownSnippets runs BEFORE the doc platform build** -- it is a pre-processing step that modifies Markdown files
   in place. Always run `dotnet tool run mdsnippets` before `npm run build` (Starlight/Docusaurus) or `docfx build`
   (DocFX).

1. **Do not generate CI deployment YAML** -- doc site deployment workflows belong to [skill:dotnet-gha-deploy]. This
   skill covers tooling selection and local authoring setup only.

1. **Do not generate API reference configuration** -- DocFX API reference setup, OpenAPI-as-documentation patterns, and
   doc-code sync belong to [skill:dotnet-api-docs]. This skill helps choose the platform, not configure API reference
   generation.

1. **Mermaid fenced code blocks work identically across GitHub, Starlight, and Docusaurus** -- use the same `mermaid`
   language identifier everywhere. Only DocFX requires additional template configuration.

1. **MarkdownSnippets `#region` names must be unique across the entire solution** -- duplicate region names cause
   ambiguous snippet resolution. Use descriptive names like `CreateWidgetAsync` not generic names like `Example1`.

1. **Starlight versioning is directory-based, not command-based** -- unlike Docusaurus (`npx docusaurus docs:version`),
   Starlight uses directory structure or community plugins for versioning. Do not suggest Docusaurus versioning commands
   for Starlight projects.
````
