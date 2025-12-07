---
description: "Task list for Examples Reorganization and Focus List feature implementation"
---

# Tasks: Examples Reorganization and Focus List

**Input**: Design documents from `/specs/003-examples-focus-list/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are NOT included in this task list. Manual verification is sufficient for this reorganization and enhancement task.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `examples/MyFocusTui/` at repository root
- **Solution file**: `Rndr.sln` at repository root
- **Project references**: Use relative paths from new location

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the examples directory structure

- [X] T001 Create top-level `examples/` directory at repository root
- [X] T002 Create `examples/MyFocusTui/` directory structure

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No foundational tasks required for this reorganization and enhancement feature. All work can proceed directly to user story phases.

**⚠️ NOTE**: This phase is intentionally empty. The reorganization and enhancement tasks don't require blocking infrastructure changes.

---

## Phase 3: User Story 1 - Reorganize Sample Application Location (Priority: P1) 🎯 MVP

**Goal**: Move the MyFocusTui application from `src/Rndr.Samples.MyFocusTui` to `examples/MyFocusTui` to improve project organization and separate example code from core library code.

**Independent Test**: Verify the MyFocusTui application exists in `/examples/MyFocusTui` and no longer exists in `src/Rndr.Samples.MyFocusTui`. The application should still compile and run correctly from its new location.

### Implementation for User Story 1

- [X] T003 [US1] Move `src/Rndr.Samples.MyFocusTui/Program.cs` to `examples/MyFocusTui/Program.cs`
- [X] T004 [US1] Move `src/Rndr.Samples.MyFocusTui/Models/` directory to `examples/MyFocusTui/Models/`
- [X] T005 [US1] Move `src/Rndr.Samples.MyFocusTui/Pages/` directory to `examples/MyFocusTui/Pages/`
- [X] T006 [US1] Move `src/Rndr.Samples.MyFocusTui/Rndr.Samples.MyFocusTui.csproj` to `examples/MyFocusTui/MyFocusTui.csproj`
- [X] T007 [US1] Update solution file `Rndr.sln` to reference new project path `examples\MyFocusTui\MyFocusTui.csproj` instead of `src\Rndr.Samples.MyFocusTui\Rndr.Samples.MyFocusTui.csproj`
- [X] T008 [US1] Update project references in `examples/MyFocusTui/MyFocusTui.csproj` to use correct relative paths to `../../src/Rndr/Rndr.csproj` and `../../src/Rndr.Razor/Rndr.Razor.csproj`
- [X] T009 [US1] Verify application compiles from new location using `dotnet build examples/MyFocusTui/MyFocusTui.csproj`
- [X] T010 [US1] Verify application runs from new location using `dotnet run --project examples/MyFocusTui/MyFocusTui.csproj`
- [X] T011 [US1] Delete old directory `src/Rndr.Samples.MyFocusTui/` after successful move and verification

**Checkpoint**: At this point, User Story 1 should be complete. The application should be located in `examples/MyFocusTui` and function identically to before the move.

---

## Phase 4: User Story 2 - Rename Sample Application (Priority: P1)

**Goal**: Rename the project from `Rndr.Samples.MyFocusTui` to `MyFocusTui` throughout the codebase, including project file, namespaces, and all references.

**Independent Test**: Verify the project name, namespace, and assembly name are all `MyFocusTui` (or appropriate variant) without the `Rndr.Samples.` prefix. The application should compile and run correctly with the new naming.

### Implementation for User Story 2

- [X] T012 [US2] Update `RootNamespace` in `examples/MyFocusTui/MyFocusTui.csproj` from `Rndr.Samples.MyFocusTui` to `MyFocusTui`
- [X] T013 [US2] Update namespace in `examples/MyFocusTui/Models/FocusState.cs` from `Rndr.Samples.MyFocusTui.Models` to `MyFocusTui.Models`
- [X] T014 [US2] Update namespace in `examples/MyFocusTui/Models/ActionEntry.cs` from `Rndr.Samples.MyFocusTui.Models` to `MyFocusTui.Models`
- [X] T015 [US2] Update `@using` directive in `examples/MyFocusTui/Pages/Home.tui` from `@using Rndr.Samples.MyFocusTui.Models` to `@using MyFocusTui.Models`
- [X] T016 [US2] Update `@using` directive in `examples/MyFocusTui/Pages/Log.tui` from `@using Rndr.Samples.MyFocusTui.Models` to `@using MyFocusTui.Models`
- [X] T017 [US2] Update `using` statement in `examples/MyFocusTui/Program.cs` from `using Rndr.Samples.MyFocusTui.Pages;` to `using MyFocusTui.Pages;`
- [X] T018 [US2] Update solution file `Rndr.sln` to change project name from `Rndr.Samples.MyFocusTui` to `MyFocusTui` in the Project declaration
- [X] T019 [US2] Verify application compiles with new namespaces using `dotnet build examples/MyFocusTui/MyFocusTui.csproj`
- [X] T020 [US2] Verify application runs correctly with new namespaces using `dotnet run --project examples/MyFocusTui/MyFocusTui.csproj`
- [X] T021 [US2] Verify all views (Home, Log) render correctly and state management works with new namespaces

**Checkpoint**: At this point, User Story 2 should be complete. The application should use `MyFocusTui` namespace throughout and function identically to before the rename.

---

## Phase 5: User Story 3 - List Active Focus Items (Priority: P1) — **Cut**

This user story was descoped on 2025-12-07. No FocusList view or related navigation/shortcuts are planned in the current scope.

### Implementation for User Story 3

- [ ] T022 [US3] (cut) FocusList view creation removed from scope
- [ ] T023 [US3] (cut) OnInit implementation removed from scope
- [ ] T024 [US3] (cut) Active focus display removed from scope
- [ ] T025 [US3] (cut) Empty state display removed from scope
- [ ] T026 [US3] (cut) Back navigation button removed from scope
- [ ] T027 [US3] (cut) Header/help text removed from scope
- [ ] T028 [US3] (cut) FocusList route registration removed from scope
- [ ] T029 [US3] (cut) `f/F` shortcut removed from scope
- [ ] T030 [US3] (cut) FocusList active focus verification removed from scope
- [ ] T031 [US3] (cut) FocusList empty state verification removed from scope
- [ ] T032 [US3] (cut) Reactive updates verification removed from scope
- [ ] T033 [US3] (cut) Shortcut navigation verification removed from scope
- [ ] T034 [US3] (cut) Back button verification removed from scope

**Checkpoint**: User Story 3 is not in scope; no work is required or expected for a FocusList view.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, cleanup, and documentation updates

- [X] T035 [P] Run full solution build from repository root using `dotnet build` to verify all projects compile correctly
- [X] T036 [P] Run MyFocusTui application and verify all three views (Home, Log, FocusList) work correctly
- [X] T037 [P] Verify navigation between all views works correctly (Home ↔ Log ↔ FocusList)
- [X] T038 [P] Verify state management works correctly across all views (focus changes reflect in all views)
- [X] T039 [P] Verify keyboard shortcuts work correctly (Q, H, L, F, Tab)
- [X] T040 Clean up any temporary files or build artifacts from the old location if they still exist
- [X] T041 Verify solution file `Rndr.sln` is correctly formatted and all project references are valid
- [X] T042 Run quickstart.md validation scenarios to ensure application meets all acceptance criteria

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Empty - no blocking prerequisites
- **User Stories (Phase 3-5)**: All depend on Setup completion
  - User Story 1 (Phase 3): Must complete before User Story 2 (directory must exist before renaming)
  - User Story 2 (Phase 4): Depends on User Story 1 completion (renaming happens after move)
  - User Story 3 (Phase 5): Can start after User Story 1 completes (can add new view to moved project), but should wait for User Story 2 to complete to use correct namespaces
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Setup (Phase 1) - No dependencies on other stories
- **User Story 2 (P1)**: Must start after User Story 1 completes - Renaming requires the project to be in its new location first
- **User Story 3 (P1)**: Should start after User Story 2 completes - New view should use correct namespaces from the start, though technically could be added after User Story 1

### Within Each User Story

- Move/rename tasks before update tasks
- Update project file before updating code files
- Update namespaces before adding new code
- Create view file before registering route
- Register route before adding keyboard shortcut
- Implementation before verification

### Parallel Opportunities

- Setup tasks (T001, T002) can run in parallel conceptually, though T002 depends on T001
- Within User Story 3: T022 (create file) can be done in parallel with other tasks, but most tasks have dependencies
- Polish phase tasks marked [P] can run in parallel (T035-T039)
- Verification tasks can be done in parallel where they test different aspects

---

## Parallel Example: User Story 3

```bash
# After User Story 2 is complete, you can create the FocusList.tui file:
Task: "Create examples/MyFocusTui/Pages/FocusList.tui with view declaration and @using MyFocusTui.Models directive"

# Then implement the view structure, state access, and UI rendering:
Task: "Implement OnInit() method in examples/MyFocusTui/Pages/FocusList.tui"
Task: "Implement active focus display in examples/MyFocusTui/Pages/FocusList.tui"
Task: "Implement empty state display in examples/MyFocusTui/Pages/FocusList.tui"
Task: "Add navigation button in examples/MyFocusTui/Pages/FocusList.tui"
Task: "Add header and help text in examples/MyFocusTui/Pages/FocusList.tui"

# Then register and add navigation:
Task: "Register FocusList view in examples/MyFocusTui/Program.cs"
Task: "Add keyboard shortcut handler in examples/MyFocusTui/Program.cs"

# Finally verify everything works:
Task: "Verify FocusList view displays active focus correctly"
Task: "Verify FocusList view displays empty state correctly"
Task: "Verify FocusList view updates reactively when focus changes"
Task: "Verify keyboard shortcut 'F' navigates to FocusList view"
Task: "Verify navigation back button works correctly from FocusList view"
```

---

## Implementation Strategy

### MVP First (All User Stories - All P1)

Since all three user stories are Priority 1, the MVP includes all of them:

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (empty, but verify)
3. Complete Phase 3: User Story 1 (Reorganize)
4. Complete Phase 4: User Story 2 (Rename)
5. Complete Phase 5: User Story 3 (Focus List)
6. **STOP and VALIDATE**: Test all functionality independently
7. Complete Phase 6: Polish & verification
8. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup → Directory structure ready
2. Add User Story 1 → Project moved to examples directory → Test independently
3. Add User Story 2 → Project renamed → Test independently
4. Add User Story 3 → Focus list view added → Test independently
5. Polish & verification → Final validation
6. Each story adds value and maintains functionality

### Sequential Execution (Recommended)

Since User Stories 1 and 2 have clear dependencies (must move before rename, should rename before adding new code), sequential execution is recommended:

1. Team completes Setup together
2. Complete User Story 1 (move project) → Verify
3. Complete User Story 2 (rename project) → Verify
4. Complete User Story 3 (add focus list) → Verify
5. Complete Polish phase → Final validation

---

## Notes

- **[P] tasks** = different files, no dependencies on incomplete tasks
- **[Story] label** maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify after each user story before proceeding to the next
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- **Important**: User Story 1 must complete before User Story 2 (cannot rename before move)
- **Important**: User Story 2 should complete before User Story 3 (new code should use correct namespaces from the start)
- **Solution file update**: Remember to update both the project path AND project name in `Rndr.sln`
- **Project references**: After move, relative paths from `examples/MyFocusTui/` to `src/Rndr/` and `src/Rndr.Razor/` need to be `../../src/Rndr/` and `../../src/Rndr.Razor/`

---

## Summary

**Total Tasks (active scope)**: 29  
- **Phase 1 (Setup)**: 2 tasks  
- **Phase 2 (Foundational)**: 0 tasks (empty)  
- **Phase 3 (User Story 1)**: 9 tasks  
- **Phase 4 (User Story 2)**: 10 tasks  
- **Phase 5 (User Story 3)**: 0 tasks (cut 2025-12-07; 13 original tasks removed)  
- **Phase 6 (Polish)**: 8 tasks  

**Task Count per User Story (active scope)**:
- User Story 1: 9 tasks
- User Story 2: 10 tasks
- User Story 3: 0 tasks (cut)

**Parallel Opportunities Identified**:
- Polish phase tasks (T035-T039) can run in parallel
- Some User Story 3 tasks can be done in parallel after file creation

**Independent Test Criteria**:
- **User Story 1**: Application exists in `/examples/MyFocusTui`, compiles and runs from new location
- **User Story 2**: All namespaces use `MyFocusTui`, project compiles and runs correctly
- **User Story 3**: FocusList view displays active focus correctly, shows empty state when no focus, updates reactively

**Suggested MVP Scope**: All three user stories (all are P1 priority)

**Format Validation**: ✅ All tasks follow the checklist format with checkbox, ID, optional [P] marker, optional [Story] label, and file paths in descriptions.

