---
name: dotnet-documentation-strategy
category: developer-experience
subcategory: docs
description: Chooses documentation tooling. Starlight, Docusaurus, DocFX decision tree, migration paths.
license: MIT
targets: ['*']
tags: [csharp, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for csharp tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-documentation-strategy

Documentation tooling recommendation for .NET projects: decision tree for selecting Starlight (Astro-based, modern
default), Docusaurus (React-based, plugin-rich), or DocFX (community-maintained, .NET-native XML doc integration).
Covers MarkdownSnippets for verified code inclusion from source files, Mermaid rendering support across all platforms,
migration paths between tools, and project-context-driven recommendation based on team size, project type, and existing
ecosystem.

**Version assumptions:** Starlight v0.x+ (Astro 4+). Docusaurus v3.x (React 18+). DocFX v2.x (community-maintained).
MarkdownSnippets as `dotnet tool` (.NET 8.0+ baseline). Mermaid v10+ (GitHub, Starlight, Docusaurus render natively).

## Scope

- Decision tree for selecting Starlight, Docusaurus, or DocFX
- Initial setup and configuration for each platform
- MarkdownSnippets for verified code inclusion
- Mermaid rendering support across platforms
- Migration paths between documentation tools

## Out of scope

- CI/CD deployment pipelines for doc sites -- see [skill:dotnet-gha-deploy]
- API documentation generation (DocFX API reference, OpenAPI-as-docs) -- see [skill:dotnet-api-docs]
- XML documentation comment authoring -- see [skill:dotnet-xml-docs]
- Mermaid diagram syntax and .NET-specific patterns -- see [skill:dotnet-mermaid-diagrams]

Cross-references: [skill:dotnet-gha-deploy] for doc site deployment pipelines, [skill:dotnet-api-docs] for API reference
generation, [skill:dotnet-xml-docs] for XML doc comment authoring, [skill:dotnet-mermaid-diagrams] for .NET-specific
Mermaid diagrams.

---

## Documentation Tooling Decision Tree

Choose documentation tooling based on project context, team capabilities, and existing ecosystem investments.

### Quick Decision Matrix

| Factor                 | Starlight                 | Docusaurus                | DocFX                                |
| ---------------------- | ------------------------- | ------------------------- | ------------------------------------ |
| Best for               | New projects, static docs | React teams, blog + docs  | Existing .NET projects with XML docs |
| Learning curve         | Low (Markdown + MDX)      | Medium (React + MDX)      | Medium (.NET toolchain)              |
| Built-in search        | Yes (Pagefind)            | Yes (Algolia plugin)      | Yes (Lunr.js)                        |
| Versioned docs         | Yes (manual setup)        | Yes (built-in)            | Yes (built-in)                       |
| i18n support           | Yes (built-in)            | Yes (built-in)            | Limited                              |
| Mermaid support        | Native (remark plugin)    | Native (MDX plugin)       | Plugin required                      |
| API reference from XML | Manual integration        | Manual integration        | Native (`docfx metadata`)            |
| Hosting                | Any static host           | Any static host           | Any static host                      |
| Build speed            | Fast (Astro)              | Moderate (Webpack/Rspack) | Moderate (.NET toolchain)            |

### Decision Flowchart

`````text

Start: New documentation site for .NET project
  |
  +-- Do you have existing DocFX content?
  |     |
  |     +-- Yes --> Do you need XML doc API reference integration?
  |     |            |
  |     |            +-- Yes --> Stay with DocFX (lowest migration cost)
  |     |            |
  |     |            +-- No --> Migrate to Starlight (see Migration Paths below)
  |     |
  |     +-- No --> Is your team heavily invested in React?
  |                 |
  |                 +-- Yes --> Docusaurus (leverage React skills, plugin ecosystem)
  |                 |
  |                 +-- No --> Starlight (modern default, best DX)

```text

### Project Context Factors

**Library vs Application:**

- Libraries benefit from API reference integration -- DocFX excels here with `docfx metadata` generating API docs
  directly from XML comments
- Applications typically need guides, tutorials, and architectural docs -- Starlight or Docusaurus are better fits
- Hybrid (library + app docs) -- consider Starlight with separate API reference section linking to DocFX-generated
  content

**Team Size:**

- Solo / small team (1-3): Starlight -- minimal configuration, fast iteration
- Medium team (4-10): Starlight or Docusaurus -- both handle multiple contributors well with built-in versioning
- Large team (10+): Docusaurus -- plugin ecosystem handles complex multi-author workflows, custom review integrations

**Existing Ecosystem:**

- React ecosystem: Docusaurus integrates naturally with existing React component libraries and storybook
- .NET-only toolchain: DocFX avoids JavaScript build dependencies entirely
- Polyglot / modern: Starlight works with any tech stack, minimal JavaScript knowledge required

---

## Starlight (Astro-Based) -- Modern Default

Starlight is an Astro-based documentation framework. It is the recommended default for new .NET documentation sites due
to fast build times, built-in search (Pagefind), i18n, and native Mermaid support.

### Initial Setup

```bash

# Create a new Starlight project
npm create astro@latest -- --template starlight my-docs

cd my-docs
npm install
npm run dev

```text

### Project Structure

```text

my-docs/
  astro.config.mjs      # Starlight configuration
  src/
    content/
      docs/              # Markdown/MDX documentation pages
        index.mdx        # Landing page
        getting-started/
          installation.md
          quick-start.md
        guides/
          configuration.md
          architecture.md
        reference/
          api.md
          cli.md
    assets/              # Images, diagrams
  public/                # Static assets (favicon, robots.txt)

```markdown

### Configuration

```javascript

// astro.config.mjs
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

export default defineConfig({
  integrations: [
    starlight({
      title: 'My .NET Library',
      social: {
        github: 'https://github.com/mycompany/my-library',
      },
      sidebar: [
        {
          label: 'Getting Started',
          items: [
            { label: 'Installation', slug: 'getting-started/installation' },
            { label: 'Quick Start', slug: 'getting-started/quick-start' },
          ],
        },
        {
          label: 'Guides',
          autogenerate: { directory: 'guides' },
        },
        {
          label: 'Reference',
          autogenerate: { directory: 'reference' },
        },
      ],
    }),
  ],
});

```text

### Mermaid Support in Starlight

```bash

# Install Mermaid remark plugin
npm install remark-mermaidjs

```bash

```javascript

// astro.config.mjs
import remarkMermaid from 'remark-mermaidjs';

export default defineConfig({
  markdown: {
    remarkPlugins: [remarkMermaid],
  },
  integrations: [
    starlight({
      /* ... */
    }),
  ],
});

```text

After configuration, use standard Mermaid fenced code blocks in any Markdown file. See [skill:dotnet-mermaid-diagrams]
for .NET-specific diagram patterns.

### Versioned Documentation

Use the `@lorenzo_lewis/starlight-utils` plugin for version dropdown navigation -- this is the recommended approach for
Starlight versioned docs.

Alternatively, use directory-based versioning with explicit routing in `astro.config.mjs`:

```text

src/content/docs/
  v1/
    getting-started.md
    api-reference.md
  v2/
    getting-started.md
    api-reference.md

```markdown

Configure the sidebar to point to the current version directory and add a version selector via the plugin or custom
Astro component.

---

## Docusaurus (React-Based)

Docusaurus is a React-based documentation framework maintained by Meta. It is a strong choice for teams already invested
in the React ecosystem, offering a rich plugin system, built-in blog, and versioned docs.

### Initial Setup

```bash

npx create-docusaurus@latest my-docs classic

cd my-docs
npm install
npm start

```text

### Project Structure

```text

my-docs/
  docusaurus.config.js   # Docusaurus configuration
  docs/                   # Markdown/MDX documentation
    intro.md
    getting-started/
      installation.md
    guides/
      configuration.md
  blog/                   # Optional blog posts
  src/
    components/           # Custom React components
    pages/                # Custom pages
  static/                 # Static assets
  sidebars.js             # Sidebar configuration

```javascript

### Configuration

```javascript

// docusaurus.config.js
/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'My .NET Library',
  tagline: 'High-performance widgets for .NET',
  url: 'https://mycompany.github.io',
  baseUrl: '/my-library/',
  organizationName: 'mycompany',
  projectName: 'my-library',

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl: 'https://github.com/mycompany/my-library/tree/main/docs/',
        },
        blog: {
          showReadingTime: true,
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      }),
    ],
  ],
};

module.exports = config;

```text

### Mermaid Support in Docusaurus

```bash

npm install @docusaurus/theme-mermaid

```bash

```javascript

// docusaurus.config.js
const config = {
  markdown: {
    mermaid: true,
  },
  themes: ['@docusaurus/theme-mermaid'],
  // ...
};

```text

### Versioned Documentation

Docusaurus has first-class versioning support:

```bash

# Snapshot current docs as a version
npx docusaurus docs:version 1.0

# Creates versioned_docs/version-1.0/ and versions.json

```json

This creates a snapshot of the current `docs/` directory. The `docs/` directory continues to represent the "next"
unreleased version.

---

## DocFX (Community-Maintained, .NET-Native)

DocFX is a .NET-native documentation generator that integrates directly with XML documentation comments. Microsoft
dropped official support in November 2022, and the project is now community-maintained. It remains widely used in the
.NET ecosystem, particularly for projects with heavy API reference documentation needs.

### Initial Setup

```bash

# Install DocFX as a .NET tool
dotnet tool install -g docfx

# Initialize a new DocFX project
docfx init

# Build the documentation
docfx build

# Serve locally
docfx serve _site

```text

### Project Structure

```text

docs/
  docfx.json              # DocFX configuration
  toc.yml                 # Table of contents
  index.md                # Landing page
  articles/               # Conceptual documentation

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
