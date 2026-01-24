# Project Groups

A **project group** is a collection of related Dassie projects that are built and deployed together. Project groups are useful for organizing larger applications consisting of multiple components, such as a main executable and supporting libraries.

## Overview

Project groups provide:
- **Multi-project builds**: Compile multiple projects in the correct dependency order
- **Unified deployment**: Deploy all components together using deployment targets
- **Shared configuration**: Common settings across all component projects
- **Executable selection**: Specify which project produces the runnable output

## Defining a Project Group

Project groups are defined in a `dsconfig.xml` file using the `<ProjectGroup>` element:

```xml
<?xml version="1.0" encoding="utf-8"?>
<DassieConfig FormatVersion="1.0">
  <ProjectGroup>
    <Components>
      <Project Path="./Core/dsconfig.xml"/>
      <Project Path="./UI/dsconfig.xml"/>
      <Project Path="./App/dsconfig.xml"/>
    </Components>
    <Executable>./App/dsconfig.xml</Executable>
    <Targets>
      <!-- Deployment targets -->
    </Targets>
  </ProjectGroup>
</DassieConfig>
```

## Project Group Structure

### Components

The `<Components>` element contains a list of projects that belong to the group:

```xml
<Components>
  <Project Path="./Library1/dsconfig.xml"/>
  <Project Path="./Library2/dsconfig.xml"/>
  <Project Path="./MainApp/dsconfig.xml"/>
</Components>
```

| Element | Description |
|---------|-------------|
| `<Project>` | A single Dassie project |
| `Path` | Path to the project's configuration file (relative or absolute) |

### Nested Project Groups

Project groups can contain other project groups for hierarchical organization:

```xml
<Components>
  <Project Path="./Core/dsconfig.xml"/>
  <ProjectGroupComponent Path="./Plugins/dsconfig.xml"/>
  <Project Path="./App/dsconfig.xml"/>
</Components>
```

### Executable Component

The `<Executable>` element specifies which project produces the main executable. This is used by the `dc run` command:

```xml
<Executable>./App/dsconfig.xml</Executable>
```

If not specified, you cannot use `dc run` on the project group.

## Building Project Groups

### Build All Components

To build all projects in a group:

```bash
dc build
```

The compiler automatically determines the correct build order based on project references.

### Deploy

To build and deploy a project group:

```bash
dc deploy
```

This command:
1. Builds all component projects
2. Executes all defined deployment targets
3. Cleans up temporary files

#### Deploy Options

| Option | Description |
|--------|-------------|
| `--ignore-missing` | Continue deployment even if some targets are not found |
| `--fail-fast` | Stop immediately if any target fails |

```bash
dc deploy --ignore-missing    # Ignore missing targets
dc deploy --fail-fast         # Stop on first failure
```

### Run

To build and run the executable component:

```bash
dc run
```

### Clean

To clean build artifacts from all projects:

```bash
dc clean
```

## Deployment Targets

Deployment targets define actions to perform after building, such as copying files, creating installers, or publishing to a server.

### Built-in Targets

The Dassie compiler includes some built-in deployment targets. Additional targets can be added through [compiler extensions](./extensions.md).

### Defining Targets

Targets are defined in the `<Targets>` element:

```xml
<Targets>
  <CopyFiles Destination="./deploy">
    <Include>*.dll</Include>
    <Include>*.exe</Include>
    <Exclude>*.pdb</Exclude>
  </CopyFiles>
</Targets>
```

Each target can have:
- **Attributes**: Configuration options as XML attributes
- **Child elements**: Additional target-specific configuration

### Custom Targets

You can create custom deployment targets through [compiler extensions](./extensions.md) by implementing the `IDeploymentTarget` interface.

## Example: Web Application

A typical web application project group:

```xml
<?xml version="1.0" encoding="utf-8"?>
<DassieConfig FormatVersion="1.0">
  <ProjectGroup>
    <Components>
      <!-- Shared data models -->
      <Project Path="./Shared/dsconfig.xml"/>
      
      <!-- Backend API -->
      <Project Path="./API/dsconfig.xml"/>
      
      <!-- Frontend (if using Dassie for frontend) -->
      <Project Path="./Web/dsconfig.xml"/>
    </Components>
    
    <Executable>./API/dsconfig.xml</Executable>
    
    <Targets>
      <!-- Copy API files -->
      <CopyFiles Destination="./publish/api">
        <Include>./API/bin/**/*</Include>
      </CopyFiles>
      
      <!-- Copy web files -->
      <CopyFiles Destination="./publish/wwwroot">
        <Include>./Web/bin/**/*</Include>
      </CopyFiles>
    </Targets>
  </ProjectGroup>
</DassieConfig>
```

## Example: Desktop Application with Plugins

```xml
<?xml version="1.0" encoding="utf-8"?>
<DassieConfig FormatVersion="1.0">
  <ProjectGroup>
    <Components>
      <!-- Core library -->
      <Project Path="./Core/dsconfig.xml"/>
      
      <!-- Plugin interface -->
      <Project Path="./PluginInterface/dsconfig.xml"/>
      
      <!-- Main application -->
      <Project Path="./App/dsconfig.xml"/>
      
      <!-- Plugins (as nested group) -->
      <ProjectGroupComponent Path="./Plugins/dsconfig.xml"/>
    </Components>
    
    <Executable>./App/dsconfig.xml</Executable>
  </ProjectGroup>
</DassieConfig>
```

## Directory Structure

A typical project group might have this structure:

```
MySolution/
??? dsconfig.xml          # Project group definition
??? Core/
?   ??? dsconfig.xml      # Core library project
?   ??? src/
?       ??? *.ds
??? UI/
?   ??? dsconfig.xml      # UI library project
?   ??? src/
?       ??? *.ds
??? App/
?   ??? dsconfig.xml      # Main application project
?   ??? src/
?       ??? *.ds
??? bin/                  # Combined build output
```

## Best Practices

1. **Keep the group file at the root**: Place the project group's `dsconfig.xml` at the solution root directory.

2. **Use relative paths**: Reference component projects using relative paths for portability.

3. **Define dependencies explicitly**: Use `<ProjectReference>` in individual project files to define cross-project dependencies.

4. **Separate concerns**: Split functionality into logical projects (core, UI, plugins, etc.).

5. **Single executable**: Generally, only one project should produce an executable; others should be libraries.

## See Also

- [Project Files](./projects.md) - Individual project configuration
- [Compiler Extensions](./extensions.md) - Creating custom deployment targets
- [Command-Line Reference](./cli.md) - `dc deploy` command reference
