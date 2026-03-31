---
name: unity-cli
description: Control Unity Editor from command line. Execute C# code, manage play mode, read console logs, run menu items, reserialize assets, and profile performance. Requires Unity with unity-cli-connector package installed.
license: MIT
compatibility: opencode
metadata:
  binary_path: C:\git\unity-cli\unity-cli.exe
  unity_package: https://github.com/youngwoocho02/unity-cli.git?path=unity-connector
---

## Unity CLI Skill

Control Unity Editor from the command line. No MCP, no Python, no dependencies - just a single binary.

## Prerequisites

1. Unity CLI binary must be built and available at: `C:\git\unity-cli\unity-cli.exe`
2. Unity project must have the unity-cli-connector package installed:
   - Package Manager → Add package from git URL: `https://github.com/youngwoocho02/unity-cli.git?path=unity-connector`
3. Unity Editor must be open and running
4. Recommended: Edit → Preferences → General → Interaction Mode = "No Throttling"

## Binary Path

```
C:\git\unity-cli\unity-cli.exe
```

## Available Commands

### Editor Control

```bash
# Enter play mode
unity-cli editor play

# Enter play mode and wait until fully loaded
unity-cli editor play --wait

# Stop play mode
unity-cli editor stop

# Toggle pause (only works during play mode)
unity-cli editor pause

# Refresh assets
unity-cli editor refresh

# Refresh and recompile scripts
unity-cli editor refresh --compile
```

### Console Logs

```bash
# Read error and warning logs (default)
unity-cli console

# Read last N log entries of all types
unity-cli console --lines 20 --filter all

# Read only errors
unity-cli console --filter error

# Clear console
unity-cli console --clear
```

### Execute C# Code

```bash
# Simple expressions
unity-cli exec "Time.time"
unity-cli exec "Application.dataPath"
unity-cli exec "EditorSceneManager.GetActiveScene().name" --usings UnityEditor.SceneManagement

# Query game objects
unity-cli exec "GameObject.FindObjectsOfType<Camera>().Length"
unity-cli exec "Selection.activeGameObject?.name ?? \"nothing selected\""

# Multi-statement (explicit return)
unity-cli exec "var go = new GameObject(\"Marker\"); go.tag = \"EditorOnly\"; return go.name;"

# ECS world inspection
unity-cli exec "World.All.Count" --usings Unity.Entities

# Modify project settings
unity-cli exec "PlayerSettings.bundleVersion = \"1.2.3\"; return PlayerSettings.bundleVersion;"
```

### Menu Items

```bash
# Execute any Unity menu item by path
unity-cli menu "File/Save Project"
unity-cli menu "Assets/Refresh"
unity-cli menu "Window/General/Console"
```

### Asset Reserialize

```bash
# Reserialize entire project
unity-cli reserialize

# Reserialize specific assets (useful after AI edits to .prefab, .unity, .asset files)
unity-cli reserialize Assets/Prefabs/Player.prefab
unity-cli reserialize Assets/Scenes/Main.unity Assets/Scenes/Lobby.unity
```

### Profiler

```bash
# Read profiler hierarchy
unity-cli profiler hierarchy
unity-cli profiler hierarchy --depth 3
unity-cli profiler hierarchy --root SimulationSystem --depth 3

# Filter and sort
unity-cli profiler hierarchy --min 0.5 --sort self --max 10

# Enable/disable profiler
unity-cli profiler enable
unity-cli profiler disable
```

### Status

```bash
# Show Unity Editor state
unity-cli status
```

### List Tools

```bash
# Show all available tools (built-in + custom)
unity-cli list
```

### Custom Tools

```bash
# Call a custom tool
unity-cli my_custom_tool --params '{"key": "value"}'
```

## Global Options

```bash
--port <N>       Override Unity instance port
--project <path> Select Unity instance by project path
--timeout <ms>   HTTP request timeout (default: 120000)
```

## Multiple Unity Instances

When multiple Unity Editors are open, each registers on a different port:

```bash
# Select by project path
unity-cli --project MyGame editor play

# Select by port
unity-cli --port 8091 editor play
```

## Writing Custom Tools

Create a static class with `[UnityCliTool]` attribute in any Editor assembly:

```csharp
using UnityCliConnector;
using Newtonsoft.Json.Linq;

[UnityCliTool(Description = "Spawn an enemy at a position")]
public static class SpawnEnemy
{
    public class Parameters
    {
        [ToolParameter("X world position", Required = true)]
        public float X { get; set; }

        [ToolParameter("Y world position", Required = true)]
        public float Y { get; set; }

        [ToolParameter("Z world position", Required = true)]
        public float Z { get; set; }

        [ToolParameter("Prefab name in Resources folder")]
        public string Prefab { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        float x = parameters["x"]?.Value<float>() ?? 0;
        float y = parameters["y"]?.Value<float>() ?? 0;
        float z = parameters["z"]?.Value<float>() ?? 0;
        string prefabName = parameters["prefab"]?.Value<string>() ?? "Enemy";

        var prefab = Resources.Load<GameObject>(prefabName);
        var instance = Object.Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);

        return new SuccessResponse("Enemy spawned", new
        {
            name = instance.name,
            position = new { x, y, z }
        });
    }
}
```

## When to Use This Skill

Use this skill when:
- User asks to control Unity Editor from command line
- User needs to run C# code in Unity without writing scripts
- User wants to check Unity console logs
- User needs to enter/exit play mode programmatically
- User wants to execute menu items remotely
- User needs to reserialize assets after text-based edits
- User wants to profile Unity performance

## Troubleshooting

If commands fail:
1. Check Unity is running: `unity-cli status`
2. Verify connector package is installed
3. Check port conflicts (default: 8090)
4. Ensure "No Throttling" is enabled in Unity preferences