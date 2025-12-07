# GitHub Copilot Instructions for Rndr

## Overview

When working on the Rndr project, you must ensure all changes align with the project's constitution defined in `.specify/memory/constitution.md`. This document contains the core principles, technical constraints, and development workflow that govern all development decisions.

## Pre-Change Review

Before making any changes, especially those **outside of the spec kit workflow** (i.e., not using official specs via `/speckit.specify`, `/speckit.plan`, or `/speckit.implement`), you MUST:

1. **Read the Constitution**: Review `.specify/memory/constitution.md` to understand the 8 core principles:
   - I. Minimal API Ergonomics (ASP.NET Minimal API patterns)
   - II. Vue-like Component Model (.tui single-file components)
   - III. Beautiful by Default (polished visual output)
   - IV. AOT-Friendly by Design (no reflection in core paths)
   - V. Testability First (abstracted I/O, pure Build() methods)
   - VI. Generic Host Integration (standard .NET DI patterns)
   - VII. Observability Ready (System.Diagnostics APIs only)
   - VIII. Phased Development (incremental milestones)

2. **Understand Technical Constraints**:
   - Target: .NET 8+ (design for .NET 10 future-proofing)
   - Language: C# 12+ (records, primary constructors, collection expressions)
   - Dependencies: Microsoft.Extensions.* and System.Diagnostics.* allowed
   - Forbidden in Core: OpenTelemetry packages, Spectre.Console, Terminal.Gui

3. **Check Project Structure**:
   - Core types in `Rndr` namespace
   - Layout primitives in `Rndr.Layout`
   - Rendering in `Rndr.Rendering`
   - Follow the established namespace conventions

## During Development

While making changes:

1. **Follow API Design Rules**:
   - Use extension methods for optional functionality
   - Use `Action<T>` callbacks for builder configuration
   - Expose `IServiceCollection` and `IConfiguration` for user customization
   - Mark internal details with `internal` visibility
   - Use `sealed` on classes not designed for inheritance

2. **Maintain Testability**:
   - Abstract all I/O through interfaces (`IConsoleAdapter`, `IInputSource`, `IClock`, `IEventLoop`)
   - Keep `Build()` methods pure (no side effects, no I/O)
   - Ensure components can be tested via `RndrTestHost`

3. **Preserve AOT Compatibility**:
   - Avoid reflection-based discovery in core runtime paths
   - Ensure `.tui` files compile to plain C# classes
   - Keep service registrations explicit (no assembly scanning)

## Post-Change Constitutional Review

**MANDATORY STEP**: Before finalizing any changes (committing, creating PR, or marking work complete), you MUST perform a Constitutional Review:

### Constitutional Review Checklist

Review your changes against each principle and document compliance:

#### ✓ I. Minimal API Ergonomics
- [ ] Does the API feel like ASP.NET Minimal APIs?
- [ ] Are patterns like `CreateBuilder()`, `Build()`, `MapView()`, `RunAsync()` followed?
- [ ] Are extension methods used appropriately?
- [ ] If violated, is there explicit justification?

#### ✓ II. Vue-like Component Model
- [ ] Do `.tui` files maintain the single-file component structure?
- [ ] Are declarative markup patterns preserved?
- [ ] Does state management follow `Signal<T>` patterns?
- [ ] If violated, is there explicit justification?

#### ✓ III. Beautiful by Default
- [ ] Are semantic layout primitives used over raw positioning?
- [ ] Is theming consistent with design tokens?
- [ ] Are Unicode box-drawing characters used appropriately?
- [ ] If violated, is there explicit justification?

#### ✓ IV. AOT-Friendly by Design
- [ ] Is reflection avoided in core runtime paths?
- [ ] Do `.tui` files compile to plain C# classes?
- [ ] Are service registrations explicit?
- [ ] Test: Does `dotnet publish -c Release --self-contained -r linux-x64 -p:PublishAot=true` produce no trim warnings? (use appropriate runtime: `win-x64`, `osx-x64`, etc.)
- [ ] If violated, is there explicit justification?

#### ✓ V. Testability First
- [ ] Are all I/O operations abstracted through interfaces?
- [ ] Are `Build()` methods pure (no side effects)?
- [ ] Can components be tested via `RndrTestHost`?
- [ ] Are appropriate tests added for the changes?
- [ ] If violated, is there explicit justification?

#### ✓ VI. Generic Host Integration
- [ ] Does the change integrate with `IServiceCollection`?
- [ ] Are standard .NET patterns (`IConfiguration`, `ILogger<T>`, `IOptions<T>`) used?
- [ ] Is `IHostApplicationLifetime` respected?
- [ ] If violated, is there explicit justification?

#### ✓ VII. Observability Ready
- [ ] Are only `System.Diagnostics` APIs used (no OpenTelemetry packages)?
- [ ] Are `ActivitySource` or `Meter` used appropriately if adding tracing/metrics?
- [ ] If violated, is there explicit justification?

#### ✓ VIII. Phased Development
- [ ] Does the change fit within the appropriate phase?
- [ ] Is the change incremental and demonstrable?
- [ ] Does it maintain backward compatibility with earlier phases?
- [ ] If violated, is there explicit justification?

### Technical Constraints Review

- [ ] Target runtime is .NET 8+ compatible
- [ ] Code uses C# 12+ features appropriately
- [ ] No forbidden dependencies added (OpenTelemetry, Spectre.Console, Terminal.Gui in core)
- [ ] Namespace conventions followed
- [ ] API design rules followed (extension methods, `Action<T>` callbacks, etc.)

### Code Quality Gates

Before committing, verify:

- [ ] `dotnet build` succeeds with no warnings (TreatWarningsAsErrors)
- [ ] All tests pass
- [ ] Code is formatted per `.editorconfig` rules
- [ ] `dotnet publish -c Release --self-contained -r linux-x64 -p:PublishAot=true` produces no trim warnings (if AOT-related changes; use appropriate runtime: `win-x64`, `osx-x64`, etc.)

### Commit Message Format

Use the following format:
```
<type>(<scope>): <description>

Types: feat, fix, refactor, test, docs, chore
Scopes: core, layout, render, input, nav, razor, samples
```

Examples:
- `feat(layout): add flexbox-like justify-content support`
- `fix(render): correct panel border rendering on narrow terminals`
- `refactor(core): simplify signal update propagation logic`
- `test(nav): add integration tests for navigation stack`
- `docs(readme): update quick start guide with new syntax`
- `chore(build): update .NET SDK to 8.0.400`

## Handling Constitution Violations

If your change **must** violate a constitutional principle:

1. **Document the violation** in your commit message or PR description
2. **Justify why it's needed** - explain the specific problem being solved
3. **Explain why simpler alternatives were rejected** - what did you consider?
4. **Get explicit review approval** from maintainers

Example:
```
Violation: Added repository pattern (violates simplicity principle)
Why Needed: Required for multi-tenant data isolation
Simpler Alternative Rejected: Direct DB access insufficient because tenants need schema-level isolation
```

## Using Spec Kit Workflow

When possible, prefer the official spec kit workflow:

1. **`/speckit.specify`**: Create feature specifications
2. **`/speckit.plan`**: Generate implementation plans
3. **`/speckit.implement`**: Execute implementation with built-in constitution checks

These commands have built-in constitutional compliance verification and reduce the risk of violations.

## Additional Resources

- **Constitution**: `.specify/memory/constitution.md`
- **Plan Template**: `.specify/templates/plan-template.md` (includes Constitution Check section)
- **Spec Template**: `.specify/templates/spec-template.md`
- **README**: `README.md` for project overview

## Summary

**Remember**: The constitution is not just guidelines—it's the law of the land for this project. Every change, no matter how small, must be reviewed against these principles. When in doubt, read the constitution, discuss with the team, and err on the side of simplicity and alignment.
