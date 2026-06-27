#!/usr/bin/env node
// Adds Starlight-compatible frontmatter to xmldocmd-generated API reference files.
// xmldocmd emits pure Markdown with no frontmatter; Starlight requires at minimum a `title`.
import { readdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";

const API_DIR = new URL("../src/content/docs/api/", import.meta.url).pathname;

async function processDir(dir) {
  const entries = await readdir(dir, { withFileTypes: true });
  for (const entry of entries) {
    const fullPath = join(dir, entry.name);
    if (entry.isDirectory()) {
      await processDir(fullPath);
    } else if (entry.name.endsWith(".md")) {
      await addFrontmatter(fullPath);
    }
  }
}

async function addFrontmatter(filePath) {
  const content = await readFile(filePath, "utf8");
  if (content.startsWith("---")) return; // already has frontmatter

  const titleMatch = content.match(/^#\s+(.+)$/m);
  const title = titleMatch ? titleMatch[1].trim() : "API Reference";

  const frontmatter = `---\ntitle: "${title.replace(/"/g, '\\"')}"\n---\n\n`;
  await writeFile(filePath, frontmatter + content);
}

await processDir(API_DIR);
console.log("API frontmatter injection complete.");
