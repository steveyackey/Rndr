# Research: Examples Reorganization and Focus List

**Feature**: 003-examples-focus-list  
**Date**: 2025-01-27  
**Status**: Complete

## Overview

This research document consolidates findings for reorganizing the MyFocusTui sample application and adding focus list functionality. All technical decisions are straightforward as this is a reorganization and enhancement task using existing framework capabilities.

---

## Research Areas

### 1. Project Reorganization Strategy

**Decision**: Move `Rndr.Samples.MyFocusTui` from `src/` to top-level `examples/` directory and rename to `MyFocusTui`.

**Rationale**:
- Common .NET convention: Examples and samples are typically kept separate from source libraries
- Improved discoverability: Developers expect examples in a dedicated directory
- Clear separation: Distinguishes demonstration code from production library code
- Aligns with many open-source .NET projects (e.g., ASP.NET Core samples, Entity Framework examples)

**Alternatives Considered**:
- Keeping in `src/` with different naming: Rejected - doesn't improve organization
- Creating `samples/` instead of `examples/`: Rejected - `examples` is more common and clearer
- Keeping `Rndr.Samples.` prefix: Rejected - redundant when already in examples directory

**Implementation Notes**:
- Solution file (`Rndr.sln`) must be updated to reference new path
- Project references in `.csproj` files use relative paths, so they should continue working
- Namespace changes require updates to all `.cs` and `.tui` files

---

### 2. Namespace Renaming Strategy

**Decision**: Change namespace from `Rndr.Samples.MyFocusTui` to `MyFocusTui` throughout the codebase.

**Rationale**:
- Simpler namespace: Removes redundant `Rndr.Samples.` prefix
- Cleaner code: Shorter, more readable namespace declarations
- Standard practice: Sample applications often use simple namespaces without framework prefixes
- Context is clear: Being in `examples/MyFocusTui` directory provides sufficient context

**Alternatives Considered**:
- Keeping full namespace: Rejected - unnecessarily verbose
- Using `Examples.MyFocusTui`: Rejected - adds unnecessary nesting without benefit

**Implementation Notes**:
- Update `RootNamespace` in `.csproj` file
- Update all `namespace` declarations in `.cs` files
- Update `@using` directives in `.tui` files
- Generated code from Razor will automatically use new namespace

---

### 3. Focus List View Design

**Decision**: Create a new `FocusList.tui` view that displays the currently active focus item using existing state management and navigation.

**Rationale**:
- Uses existing patterns: Leverages `StateGlobal()` for shared state and `NavigationContext` for routing
- Reactive updates: Framework's `Signal<T>` provides automatic reactivity when focus changes
- Consistent UX: Follows same design patterns as existing Home and Log views
- Simple implementation: No new framework features required

**Alternatives Considered**:
- Adding focus list to Home view: Rejected - violates single responsibility, Home already shows current focus
- Creating a modal/dialog: Rejected - overcomplicated for simple list display
- Using a separate state store: Rejected - existing `FocusState` already tracks active focus

**Implementation Notes**:
- Access same `FocusState` via `StateGlobal("focus", ...)` as other views
- Display `CurrentTodo` when `HasActiveFocus` is true
- Show empty state message when no active focus
- Add navigation route `/focus-list` or `/focus` in `Program.cs`
- Add keyboard shortcut (e.g., 'f' or 'F') for quick access

---

### 4. Solution File Update Strategy

**Decision**: Update `Rndr.sln` to reference the new project path and update project GUID if needed.

**Rationale**:
- Required for build: Solution file must point to correct project location
- Maintains build system integration: Ensures IDE and CLI tools can find the project
- Standard .NET practice: Solution files track project locations explicitly

**Alternatives Considered**:
- Creating new solution file: Rejected - unnecessary, existing solution works fine
- Using wildcards: Rejected - .NET solution files don't support wildcards

**Implementation Notes**:
- Update project path from `src\Rndr.Samples.MyFocusTui\Rndr.Samples.MyFocusTui.csproj` to `examples\MyFocusTui\MyFocusTui.csproj`
- Update project name if displayed in solution folders
- Verify project GUID remains consistent (or update if project is recreated)

---

### 5. Build and Runtime Verification

**Decision**: Verify application compiles and runs correctly after move/rename using standard .NET build commands.

**Rationale**:
- Critical requirement: Application must function identically after reorganization
- Standard verification: `dotnet build` and `dotnet run` are standard validation steps
- Framework compatibility: Ensures Razor source generation still works correctly

**Alternatives Considered**:
- No verification: Rejected - violates requirement FR-005
- Automated testing only: Rejected - manual verification also needed for UI functionality

**Implementation Notes**:
- Run `dotnet build` from solution root
- Run `dotnet run --project examples/MyFocusTui/MyFocusTui.csproj`
- Verify all views render correctly
- Verify navigation works (Home, Log, new FocusList)
- Verify state management works (focus tracking, log entries)

---

## Summary

All research areas are resolved with clear decisions based on:
1. Standard .NET project organization conventions
2. Existing framework capabilities (no new features needed)
3. Simplicity and maintainability principles
4. Alignment with constitution principles (especially Minimal API Ergonomics and Vue-like Component Model)

**No blocking unknowns remain.** All technical decisions are straightforward and can proceed to implementation.

