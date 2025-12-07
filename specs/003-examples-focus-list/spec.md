# Feature Specification: Examples Reorganization and Focus List

**Feature Branch**: `003-examples-focus-list`  
**Created**: 2025-01-27  
**Status**: Draft  
**Scope change (2025-12-07)**: Focus list (User Story 3) was cut; current scope covers only relocation/rename of MyFocusTui.  
**Input**: User description: "Create a new top level `/examples` folder and move the Rndr.Samples.MyFocusTui app there. When doing so, rename it MyFocusTui. Also update the project so you can list what focus you're actively working on."

## Overview

Reorganize the project structure by moving sample applications to a dedicated examples directory and enhance the MyFocusTui sample application with the ability to list and view active focus items. This improves project organization by separating example code from core library code and enhances the sample application's functionality to better demonstrate focus tracking capabilities.

### Core Value Proposition

1. **Better project organization**: Sample applications are clearly separated from core library code, making the project structure more intuitive for contributors and users
2. **Improved discoverability**: Examples are easier to find in a dedicated top-level directory
3. **Enhanced sample functionality**: The MyFocusTui application demonstrates focus listing capabilities, providing a more complete example of state management and view navigation

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reorganize Sample Application Location (Priority: P1)

A developer exploring the Rndr project wants to find example applications. They look for sample code and find it in a dedicated `/examples` directory at the top level of the project, making it clear these are demonstration applications separate from the core library.

**Why this priority**: Project organization is foundational. Without clear separation, it's confusing to distinguish between library code and examples. This must be done before other enhancements.

**Independent Test**: Can be fully tested by verifying the MyFocusTui application exists in `/examples/MyFocusTui` and no longer exists in `src/Rndr.Samples.MyFocusTui`. The application should still compile and run correctly from its new location.

**Acceptance Scenarios**:

1. **Given** the project structure, **When** a developer looks for example applications, **Then** they find a top-level `/examples` directory containing sample projects
2. **Given** the MyFocusTui application has been moved, **When** the project is built, **Then** it compiles successfully from its new location
3. **Given** the application has been renamed from `Rndr.Samples.MyFocusTui` to `MyFocusTui`, **When** the project is built, **Then** the namespace and project references are updated correctly
4. **Given** the application has been moved and renamed, **When** it is run, **Then** it functions identically to before the reorganization

---

### User Story 2 - Rename Sample Application (Priority: P1)

A developer wants to reference or build the MyFocusTui sample application. They find it in the examples directory with a clean, simple name `MyFocusTui` without the `Rndr.Samples.` prefix, making it easier to reference and understand.

**Why this priority**: Naming consistency is important for clarity. The `Rndr.Samples.` prefix is redundant when the application is already in an examples directory.

**Independent Test**: Can be fully tested by verifying the project name, namespace, and assembly name are all `MyFocusTui` (or appropriate variant) without the `Rndr.Samples.` prefix.

**Acceptance Scenarios**:

1. **Given** the application has been renamed, **When** a developer opens the project file, **Then** the project name is `MyFocusTui` without the `Rndr.Samples.` prefix
2. **Given** the application has been renamed, **When** code files are examined, **Then** namespaces use `MyFocusTui` instead of `Rndr.Samples.MyFocusTui`
3. **Given** the application has been renamed, **When** the application is built, **Then** the output assembly is named `MyFocusTui` (or appropriate executable name)

---

### User Story 3 - List Active Focus Items (Priority: P1) — **Cut**

This user story was descoped on 2025-12-07. No focus list view or related functionality is planned in the current scope.

---

### Edge Cases

- What happens if project references break during the move? (References should be updated to point to the new location)
- How does the build system handle the new directory structure? (Solution file and build scripts should be updated)
- What if there are external references to the old project path? (Documentation and any scripts should be updated)
- How does the focus list handle rapid focus changes? (The list should update reactively to reflect the current state)
- What if the focus list is accessed while no focus is active? (Should display an appropriate empty state message)

---

## Requirements *(mandatory)*

### Functional Requirements

#### Project Reorganization

- **FR-001**: System MUST move the `Rndr.Samples.MyFocusTui` application from `src/Rndr.Samples.MyFocusTui` to `examples/MyFocusTui`
- **FR-002**: System MUST rename the project from `Rndr.Samples.MyFocusTui` to `MyFocusTui` in the project file
- **FR-003**: System MUST update all namespace declarations from `Rndr.Samples.MyFocusTui` to `MyFocusTui` throughout the codebase
- **FR-004**: System MUST update project references in solution files and build configurations to point to the new location
- **FR-005**: System MUST ensure the application compiles and runs correctly from its new location

#### Focus List Functionality

- (Removed) FR-006–FR-009 were cut with the Focus List user story on 2025-12-07.

---

### Key Entities

- **MyFocusTui Application**: The sample focus tracking application that demonstrates Rndr framework capabilities
- **Active Focus**: The current focus item that a user is working on, stored in the application state
- **Focus List View**: A view or display that shows the currently active focus item
- **Examples Directory**: A top-level directory containing sample applications separate from core library code

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The MyFocusTui application is located in `/examples/MyFocusTui` and no longer exists in `src/Rndr.Samples.MyFocusTui`
- **SC-002**: The application compiles successfully from its new location without errors
- **SC-003**: The application runs correctly and maintains all existing functionality after the move and rename
- **SC-004**: All namespace references are updated from `Rndr.Samples.MyFocusTui` to `MyFocusTui`
- **SC-005–SC-007**: Focus list success criteria removed (scope cut 2025-12-07)

---

## Assumptions

- The project uses standard .NET project structure and solution files
- Project references can be updated in solution files and build configurations
- The MyFocusTui application currently supports a single active focus item (not multiple simultaneous focuses)
- The focus list functionality will display the current active focus, which may be enhanced in the future to support multiple focuses
- Users understand that moving the project may require updating any external documentation or scripts that reference the old path

---

## Out of Scope

- Adding support for multiple simultaneous active focus items (current design supports one active focus)
- Creating additional sample applications in the examples directory
- Modifying the core Rndr framework library
- Adding persistence or data export features to MyFocusTui
- Creating documentation for the examples directory structure (may be done separately)

