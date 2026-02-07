# Brainboxes.IO Documentation

DocFX-generated API documentation for the Brainboxes.IO library.

## Structure

```
docfx.json              # DocFX configuration
dfmg-config.yaml        # DocFxMarkdownGen config for Docusaurus output
postprocess-dfmg.py     # Post-processor: injects remarks, examples into markdown
api/                    # Generated YAML metadata (auto-generated, do not edit)
articles/               # Manual documentation articles
docusaurus/             # Docusaurus-compatible output (auto-generated)
_site/                  # Generated HTML site (legacy)
```

## Generating Documentation

The documentation pipeline has 3 stages:

```bash
# Stage 1: Extract XML doc comments into YAML metadata
docfx metadata docfx.json

# Stage 2: Convert YAML to Docusaurus-compatible markdown
dfmg    # reads dfmg-config.yaml

# Stage 3: Post-process to add remarks, examples, and cleanup
python3 postprocess-dfmg.py

# Or run all 3 stages via VSCode task: "docs-build"
```

## Configuration

- `docfx.json` - Main DocFX config, targets net10.0 framework
- `dfmg-config.yaml` - DocFxMarkdownGen config for Docusaurus output
- `postprocess-dfmg.py` - Augments dfmg output with YAML fields it skips (remarks, example)

## Pipeline Notes

DocFxMarkdownGen (dfmg) v0.5.0 does not render `remarks`, `example`, or `seealso` YAML fields.
The post-processing script reads these fields from the YAML and injects them into the generated
markdown. It also cleans up HTML-escaped XML tags that dfmg leaves in summaries.

XML doc tags that flow through the full pipeline:
- `<summary>` - Rendered by dfmg
- `<param>`, `<returns>` - Rendered by dfmg
- `<exception>` - Rendered by dfmg
- `<remarks>` - Rendered by postprocess-dfmg.py
- `<example>` - Rendered by postprocess-dfmg.py (must be sibling of summary, not nested)

## Output

Documentation is generated in Docusaurus-compatible markdown format.

Published to: https://docs.brainboxes.com/api/dotnet/Brainboxes.IO

## Adding Articles

1. Create markdown file in `articles/`
2. Update `toc.yml` to include the new article
3. Regenerate documentation

## Notes

- `api/` folder is auto-generated from XML comments - do not edit directly
- `_site/` is the build output - do not commit
- Ensure XML documentation is enabled in Brainboxes.IO.csproj for API docs
