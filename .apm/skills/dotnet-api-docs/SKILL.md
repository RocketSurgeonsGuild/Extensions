---
name: dotnet-api-docs
category: web
subcategory: minimal-apis
description: Generates API documentation. DocFX setup, OpenAPI-as-docs, doc-code sync, versioned docs.
license: MIT
targets: ['*']
tags: [api, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for api tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-api-docs

API documentation generation for .NET projects: DocFX setup for API reference from assemblies (`docfx.json`
configuration, metadata extraction, template customization, cross-referencing), OpenAPI spec as living API documentation
(Scalar and Swagger UI embedding, versioned OpenAPI documents), documentation-code synchronization (CI validation with
`-warnaserror:CS1591`, broken link detection, automated doc builds on PR), API changelog patterns (breaking change
documentation, migration guides, deprecated API tracking), and versioned API documentation (version selectors,
multi-version maintenance, URL patterns).

**Version assumptions:** DocFX v2.x (community-maintained). OpenAPI 3.x via `Microsoft.AspNetCore.OpenApi` (.NET 9+
built-in). Scalar UI for modern OpenAPI visualization. .NET 8.0+ baseline for code examples.

## Scope

- DocFX setup for API reference (metadata extraction, template customization, cross-referencing)
- OpenAPI spec as living documentation (Scalar and Swagger UI embedding)
- Documentation-code synchronization (CI validation, broken link detection)
- API changelog patterns (breaking changes, migration guides, deprecated API tracking)
- Versioned API documentation (version selectors, multi-version maintenance)

## Out of scope

- XML documentation comment syntax and authoring -- see [skill:dotnet-xml-docs]
- OpenAPI spec generation and configuration -- see [skill:dotnet-openapi]
- CI/CD deployment pipelines for documentation sites -- see [skill:dotnet-gha-deploy]
- Documentation platform selection and initial setup -- see [skill:dotnet-documentation-strategy]
- Changelog generation tooling and SemVer versioning -- see [skill:dotnet-release-management]

Cross-references: [skill:dotnet-xml-docs] for XML doc comment authoring, [skill:dotnet-openapi] for OpenAPI generation,
[skill:dotnet-gha-deploy] for doc site deployment pipelines, [skill:dotnet-documentation-strategy] for platform
selection, [skill:dotnet-release-management] for changelog tooling and versioning.

---

## DocFX Setup for .NET API Reference

DocFX generates API reference documentation directly from .NET assemblies and XML documentation comments. It is the only
documentation tool with native `docfx metadata` extraction from .NET projects.

### Installation

`````bash

# Install DocFX as a .NET global tool
dotnet tool install -g docfx

# Or as a local tool (recommended for team consistency)
dotnet new tool-manifest
dotnet tool install docfx

```text

### Configuration (`docfx.json`)

```json

{
  "metadata": [
    {
      "src": [
        {
          "files": ["src/**/*.csproj"],
          "exclude": ["**/bin/**", "**/obj/**"],
          "src": ".."
        }
      ],
      "dest": "api",
      "properties": {
        "TargetFramework": "net8.0"
      },
      "disableGitFeatures": false,
      "disableDefaultFilter": false
    }
  ],
  "build": {
    "content": [
      {
        "files": ["api/**.yml", "api/index.md"]
      },
      {
        "files": ["articles/**.md", "articles/**/toc.yml", "toc.yml", "*.md"]
      }
    ],
    "resource": [
      {
        "files": ["images/**"]
      }
    ],
    "dest": "_site",
    "globalMetadataFiles": [],
    "fileMetadataFiles": [],
    "template": ["default", "modern"],
    "postProcessors": ["ExtractSearchIndex"],
    "markdownEngineName": "markdig",
    "noLangKeyword": false,
    "keepFileLink": false,
    "cleanupCacheHistory": false,
    "disableGitFeatures": false,
    "globalMetadata": {
      "_appTitle": "My.Library API Reference",
      "_appFooter": "Copyright 2024 My Company",
      "_enableSearch": true,
      "_enableNewTab": true
    }
  }
}

```text

### Metadata Extraction

The `metadata` section controls how DocFX extracts API information from .NET projects:

```bash

# Generate API metadata YAML files from projects
docfx metadata docfx.json

# This creates YAML files in the api/ directory:
#   api/MyLibrary.WidgetService.yml
#   api/MyLibrary.Widget.yml
#   api/toc.yml

```yaml

**Key metadata configuration options:**

| Property                     | Purpose                       | Default                |
| ---------------------------- | ----------------------------- | ---------------------- |
| `src.files`                  | Project files to extract from | Required               |
| `dest`                       | Output directory for YAML     | `api`                  |
| `properties.TargetFramework` | TFM to build against          | Project default        |
| `disableGitFeatures`         | Skip git blame info           | `false`                |
| `filter`                     | Path to API filter YAML       | None (all public APIs) |

### API Filtering

Exclude internal types from the generated documentation:

```yaml

# filterConfig.yml
apiRules:
  - exclude:
      uidRegex: ^MyLibrary\.Internal\.
      type: Namespace
  - exclude:
      hasAttribute:
        uid: System.ComponentModel.EditorBrowsableAttribute
        ctorArguments:
          - System.ComponentModel.EditorBrowsableState.Never

```text

Reference the filter in `docfx.json`:

```json

{
  "metadata": [
    {
      "filter": "filterConfig.yml"
    }
  ]
}

```yaml

### Template Customization

DocFX supports template overrides for custom branding:

```text

docs/
  templates/
    custom/
      styles/
        main.css          # Custom CSS overrides
      partials/
        head.tmpl.partial # Custom head section (analytics, fonts)
        footer.tmpl.partial

```csharp

Reference custom templates in `docfx.json`:

```json

{
  "build": {
    "template": ["default", "modern", "templates/custom"]
  }
}

```text

### Cross-Referencing Between Pages

DocFX supports `uid`-based cross-references between API pages and conceptual articles:

```markdown

<!-- In a conceptual article -->

See the @MyLibrary.WidgetService.CreateWidgetAsync(System.String) method for details.

For the full API, see <xref:MyLibrary.WidgetService>.

```text

```yaml

# In an API YAML override file (api/MyLibrary.WidgetService.yml)
# Add links to conceptual articles
references:
  - uid: MyLibrary.WidgetService
    seealso:
      - linkId: ../articles/getting-started.md
        commentId: getting-started

```markdown

---

## OpenAPI Spec as Documentation

Generated OpenAPI specifications serve as living API documentation that stays in sync with the code. This section covers
using OpenAPI output as documentation; for OpenAPI generation and configuration, see [skill:dotnet-openapi].

### Scalar UI Embedding

Scalar provides a modern, interactive API documentation viewer:

```csharp

// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();         // Serves OpenAPI JSON at /openapi/v1.json
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("My API Documentation")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.Run();

```text

Scalar renders the OpenAPI spec as an interactive documentation page with:

- Endpoint grouping by tags
- Request/response examples
- Authentication configuration
- "Try it" functionality for testing endpoints

### Swagger UI Embedding

For projects using Swashbuckle or requiring the classic Swagger UI:

```csharp

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
        options.RoutePrefix = "api-docs";
        options.DocumentTitle = "My API Documentation";
        options.DefaultModelsExpandDepth(-1); // Hide schemas by default
    });
}

```text

### Versioned OpenAPI Documents

Serve multiple OpenAPI documents for different API versions:

```csharp

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Version = "1.0";
        document.Info.Title = "My API";
        return Task.CompletedTask;
    });
});

builder.Services.AddOpenApi("v2", options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Version = "2.0";
        document.Info.Title = "My API";
        return Task.CompletedTask;
    });
});

// Serves /openapi/v1.json and /openapi/v2.json
app.MapOpenApi();

```json

### Exporting OpenAPI for Static Documentation

Export the OpenAPI spec at build time for use in static documentation sites:

```bash

# Generate OpenAPI spec from the running application
dotnet run -- --urls "http://localhost:5099" &
APP_PID=$!
sleep 3
curl -s http://localhost:5099/openapi/v1.json > docs/openapi/v1.json
kill $APP_PID

```json

Alternatively, use the `Microsoft.Extensions.ApiDescription.Server` package to generate at build time:

```xml

<PackageReference Include="Microsoft.Extensions.ApiDescription.Server" Version="8.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>

<PropertyGroup>
  <OpenApiGenerateDocuments>true</OpenApiGenerateDocuments>
  <OpenApiDocumentsDirectory>$(MSBuildProjectDirectory)/../docs/openapi</OpenApiDocumentsDirectory>
</PropertyGroup>

```text

For OpenAPI generation setup and Swashbuckle migration details, see [skill:dotnet-openapi].

---

## Doc Site Generation from XML Comments

### XML Docs to DocFX (Static HTML)

The primary pipeline for library API reference documentation:

```xml

Source Code (.cs files)
    |
    v
XML Doc Comments (/// <summary>...)
    |
    v
Build with GenerateDocumentationFile=true
    |
    v
XML Doc File (MyLibrary.xml)
    |
    v
docfx metadata (extracts API structure)
    |
    v
YAML Files (api/*.yml)
    |
    v
docfx build (generates HTML)
    |

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
