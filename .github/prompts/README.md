# GitHub Copilot Chat Prompts

This folder contains **GitHub Copilot Chat prompts** that make it easy to run converter development tasks directly from the chat window.

## Available Prompts

### @workspace /new-converter

Creates a new converter project from template.

**Usage:**

```
@workspace /new-converter
```

Runs `tools/new-converter.ps1` interactively and guides you through setup.

---

### @workspace /test-converter

Runs tests on a converter project.

**Usage:**

```
@workspace /test-converter
```

or with specific path:

```
@workspace /test-converter templates/dotnet/MyConverter
```

Runs `tools/test-converter.ps1` and interprets results.

---

## How to Use

1. **Open this workspace** in VS Code
2. **Open GitHub Copilot Chat** (Ctrl+Alt+I or Cmd+Alt+I)
3. **Type the prompt** (e.g., `@workspace /new-converter`)
4. **Follow the interactive guidance**

Copilot will:

- Run the PowerShell scripts for you
- Interpret the output
- Suggest next steps
- Help debug issues

## Benefits

✅ **No need to remember command syntax** - Just use natural language
✅ **Interactive guidance** - Copilot walks you through each step  
✅ **Result interpretation** - Copilot explains what the output means
✅ **Smart suggestions** - Get help based on your current context

## Examples

**Create a converter:**

```
User: @workspace /new-converter
Copilot: I'll help you create a new converter. Running tools/new-converter.ps1...
         [Runs script interactively]
         ✅ Project created! Next, add test files to Data/ folder.
```

**Run tests:**

```
User: @workspace /test-converter
Copilot: Which converter would you like to test?
User: MyCompanyConverter
Copilot: Running tests on MyCompanyConverter...
         [Runs dotnet test]
         ⚠️ 3 of 10 tests failed. Let me help you debug...
```

---

**Note:** These prompts are designed for the WATS Converter Development Kit and are simplified versions of the internal automation used by The WATS Company.
