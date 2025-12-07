# Missing Core Components Analysis

This document analyzes missing core components for the Rndr TUI framework and provides recommendations for future implementation.

## Current Component Inventory

### Implemented Components
1. **Column** - Vertical stack container
2. **Row** - Horizontal arrangement container
3. **Panel** - Bordered container with title
4. **Text** - Text display with styling
5. **Button** - Clickable button
6. **TextInput** - Text entry field
7. **Spacer** - Flexible space element
8. **Centered** - Centers content
9. **Modal** - Overlay dialog ✓ NEW

## Missing Components by Priority

### High Priority (Core UI Elements)

#### 1. Tabs/TabView
**Purpose**: Switch between different content views without navigation
**Use Case**: Settings pages, multi-panel dashboards
**API Design**:
```csharp
<Tabs>
    <Tab Title="Overview">
        <Column><!-- content --></Column>
    </Tab>
    <Tab Title="Settings">
        <Column><!-- content --></Column>
    </Tab>
</Tabs>
```

#### 2. List/SelectList
**Purpose**: Scrollable list with item selection
**Use Case**: File browsers, option selection, log viewers
**API Design**:
```csharp
<SelectList Items="@items" OnSelect="@HandleSelect">
    <ItemTemplate Context="item">
        <Text>@item.Name</Text>
    </ItemTemplate>
</SelectList>
```

#### 3. Checkbox
**Purpose**: Boolean selection control
**Use Case**: Todo lists, feature toggles, multi-select
**API Design**:
```csharp
<Checkbox Checked="@isChecked" OnChanged="@OnToggle" Label="Enable feature" />
```

#### 4. ProgressBar
**Purpose**: Visual progress indication
**Use Case**: File uploads, long-running operations, task completion
**API Design**:
```csharp
<ProgressBar Value="@progress" Max="100" ShowPercentage="true" />
```

### Medium Priority (Enhanced UX)

#### 5. RadioButton/RadioGroup
**Purpose**: Single selection from multiple options
**Use Case**: Settings, form inputs
**API Design**:
```csharp
<RadioGroup Value="@selected" OnChanged="@OnSelect">
    <Radio Value="option1" Label="Option 1" />
    <Radio Value="option2" Label="Option 2" />
</RadioGroup>
```

#### 6. Table/DataGrid
**Purpose**: Display tabular data with columns
**Use Case**: Data viewing, comparison tables, reports
**API Design**:
```csharp
<Table Items="@data">
    <Column Header="Name" Field="@(x => x.Name)" />
    <Column Header="Status" Field="@(x => x.Status)" />
</Table>
```

#### 7. ScrollView
**Purpose**: Scrollable content container
**Use Case**: Long content, logs, documentation viewers
**API Design**:
```csharp
<ScrollView Height="20">
    <Column>
        <!-- long content -->
    </Column>
</ScrollView>
```

#### 8. Dropdown/Select
**Purpose**: Compact selection from many options
**Use Case**: Forms, filters, settings
**API Design**:
```csharp
<Select Value="@selected" Options="@options" OnChanged="@OnSelect" />
```

### Lower Priority (Nice to Have)

#### 9. Menu/MenuBar
**Purpose**: Hierarchical menu navigation
**Use Case**: Application menus, context menus

#### 10. Tooltip
**Purpose**: Contextual help text on hover
**Use Case**: Inline documentation, hints

#### 11. TreeView
**Purpose**: Hierarchical data display
**Use Case**: File systems, organizational structures

#### 12. Spinner/LoadingIndicator
**Purpose**: Animated loading state
**Use Case**: Async operations, data fetching

#### 13. StatusBar
**Purpose**: Persistent status information
**Use Case**: Application state, notifications

## Implementation Recommendations

### Phase 1: Essential Inputs (2-3 components)
Focus on components that complete the basic input story:
1. **Checkbox** - Simplest, most common input after Button
2. **List/SelectList** - Essential for many applications
3. **ProgressBar** - Important for user feedback

### Phase 2: Enhanced Navigation (2 components)
1. **Tabs** - Common pattern for organizing content
2. **ScrollView** - Necessary for handling long content

### Phase 3: Advanced Inputs (2-3 components)
1. **RadioButton/RadioGroup** - Complete the input controls
2. **Dropdown/Select** - Space-efficient selection
3. **Table** - Data display

## Implementation Guidelines

### Consistency Principles
1. **Builder Pattern**: All components follow the builder API pattern
2. **Razor Support**: All components supported in .tui files
3. **Theming**: Components respect the theme colors and styles
4. **Testing**: Comprehensive unit tests and test helpers
5. **Documentation**: README updates and usage examples

### Code Structure
Each component requires:
- `*Node.cs` class in `src/Rndr/Layout/`
- `NodeKind` enum entry
- Builder methods in `LayoutBuilder`, `ColumnBuilder`, `RowBuilder`
- `Render*()` method in `ConsoleRenderer`
- `Emit*Tag()` method in `TuiCodeEmitter`
- Parser updates in `TuiSyntaxParser`
- Test helper methods in `NodeExtensions`
- Unit tests in `LayoutBuilderTests`
- Example usage in sample apps

### Estimated Effort
- Simple component (Checkbox, Spacer): ~2-3 hours
- Medium component (ProgressBar, Modal): ~4-6 hours
- Complex component (Table, Tabs): ~8-12 hours

## Prioritization Criteria

Components were prioritized based on:
1. **Frequency of use** in typical TUI applications
2. **Complexity** of implementation
3. **Dependencies** on other components
4. **Value to developers** building apps
5. **Completeness** of the component library

## Next Steps

### Immediate (This Session)
- ✅ Modal component implemented and documented

### Short-term (Next Session)
- Implement Checkbox (simplest remaining input)
- Implement List/SelectList (high value)
- Implement ProgressBar (good user feedback)

### Medium-term
- Implement Tabs and ScrollView
- Implement RadioButton and Dropdown
- Implement Table component

### Long-term
- Advanced components (TreeView, Menu, etc.)
- Animation and transitions
- Enhanced theming system
