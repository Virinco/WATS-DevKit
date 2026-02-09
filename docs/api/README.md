# ⚠️ CRITICAL - API Reference for Agents

**Location:** `docs/api/API_REFERENCE.md`

This file contains the **authoritative WATS Report API documentation** for AI agents helping with converter development.

## Why This Matters

AI agents MUST reference this documentation when:
- Implementing converter code
- Debugging API usage issues
- Answering questions about WATS API
- Suggesting code patterns

## What It Contains

✅ Complete API method signatures  
✅ Required vs optional parameters  
✅ Correct initialization patterns  
✅ Operation type handling (server-specific)  
✅ All step types with examples  
✅ Validation modes and test modes  
✅ Best practices and anti-patterns  
✅ Common errors and solutions  

## How to Use

Before suggesting ANY WATS API code:

1. **Read** the relevant section in API_REFERENCE.md
2. **Verify** method signatures and parameters
3. **Check** for common pitfalls in that section
4. **Use** the exact patterns shown in examples

**DO NOT:**
- Guess at API method names
- Assume operation types are hardcoded
- Invent properties that don't exist
- Skip validation modes

## References in Code

All workflow docs now reference this file:
- ✅ `docs/README.md` - Documentation hub
- ✅ `docs/guides/QUICKSTART.md` - Quick start guide
- ✅ `docs/guides/METHODOLOGY.md` - Development methodology
- ✅ `docs/guides/API_GUIDE.md` - User-facing guide
- ✅ `.github/prompts/README.md` - Copilot prompts
- ✅ `.github/prompts/new-converter.prompt.md` - New converter prompt
- ✅ `.github/prompts/test-converter.prompt.md` - Test converter prompt
- ✅ `README.md` - Main repository README

This ensures agents cannot miss this critical resource!
