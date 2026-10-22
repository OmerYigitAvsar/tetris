# Tetris

Tetris is a 2D tilemap-based implementation of the classic falling-block puzzle game, built with Unity 6. It supports keyboard input and on-screen controls for desktop and mobile play.

## Features

- Standard seven tetromino shapes on a configurable 10 × 20 board
- Seven-bag random piece generation
- Next-piece preview and ghost-piece projection
- Left/right movement, soft drop, hard drop, and clockwise/counter-clockwise rotation
- Wall-kick checks and configurable piece lock delay
- Single, double, triple, and four-line scoring
- Increasing level and fall speed after every ten cleared lines
- Persistent high score using Unity PlayerPrefs
- Main menu, pause, restart, and game-over interfaces
- Keyboard and on-screen arcade-style controls

## Keyboard Controls

| Action | Input |
| --- | --- |
| Move left | `Left Arrow` or `A` |
| Move right | `Right Arrow` or `D` |
| Soft drop | `Down Arrow` or `S` |
| Hard drop | `Space` |
| Rotate clockwise | `X` |
| Rotate counter-clockwise | `Z` |
| Pause or resume | `Escape` |

## Requirements

- Unity `6000.3.8f1`
- Universal Render Pipeline `17.3.0`
- Unity 2D Tilemap packages listed in `Packages/manifest.json`

The exact editor version is recorded in `ProjectSettings/ProjectVersion.txt`. Package versions are locked in `Packages/packages-lock.json`.

## Getting Started

1. Clone the repository.
2. Open Unity Hub and choose **Add project from disk**.
3. Select the repository root.
4. Open the project with Unity `6000.3.8f1` and allow Unity to restore packages and regenerate local cache files.
5. Open `Assets/Scenes/OYUNLAR/OMER/Tetris/Tetris.unity` and enter Play mode.

## Project Structure

```text
Assets/
  Scenes/OYUNLAR/OMER/Tetris/
    Animation/      Menu animation assets
    Scripts/        Board, piece, input, and tetromino logic
    Tile/           Tetromino tile assets and palette
    UI/             Interface graphics, fonts, music, and sound effects
  Settings/         2D renderer and Universal Render Pipeline settings
  TextMesh Pro/     TextMesh Pro resources
Packages/           Unity package manifest and lock file
ProjectSettings/    Shared Unity project configuration
```

Unity-generated caches, local editor preferences, IDE files, build outputs, and signing material are intentionally excluded from version control. Unity `.meta` files remain tracked because they preserve asset GUIDs and references.

## Build Settings Note

The playable Tetris scene is not currently enabled in `ProjectSettings/EditorBuildSettings.asset`; only `Assets/Scenes/SampleScene.unity` is enabled. Add the Tetris scene to the build configuration before creating a distributable build.

## Asset Licensing

Before distributing the game, verify that every included font, music track, sound effect, and visual asset has an appropriate license for the intended use.
