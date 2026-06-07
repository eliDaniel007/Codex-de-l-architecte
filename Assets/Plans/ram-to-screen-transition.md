# Project Overview
- Game Title: Logic Learning Game (RAM & CPU Simulation)
- High-Level Concept: An educational game where players interact with hardware concepts like RAM and Screen displays by moving physical "data boxes".
- Players: Single player
- Target Platform: PC
- Render Pipeline: URP (based on project settings)

# Game Mechanics
## Core Gameplay Loop
1. The player enters the RAM scene from the Main scene.
2. In the RAM scene (visualized as an array of boxes), the player selects a box (e.g., the one containing the number "25").
3. The game transitions back to the Main scene, and the player appears holding that box.
4. The player carries the box to the Screen object.
5. The player deposits the box at the Screen to display its value.

## Controls and Input Methods
- **Movement**: WASD / Keyboard (Starter Assets Third Person Controller).
- **Selection in RAM**: Mouse click on 3D boxes.
- **Interaction in Main**: [E] key to interact with the Screen.

# UI
- **Prompt UI**: Floating text or overlay showing instructions like "[E] Deposer sur l'ecran".
- **Screen Display**: The TV/Screen in the Main scene will display the code and the value (e.g., "int nombre = 25;").

# Key Asset & Context
- `Assets/Scripts/Mission/GameState.cs`: Manages state across scenes and handles spawning boxes.
- `Assets/Scripts/Mission/RAMBoxSelector.cs`: (New) Script for clicking boxes in the RAM scene.
- `Assets/Scripts/Mission/PlayerHolder.cs`: Component on the player to hold objects.
- `Assets/Scripts/Mission/ConsoleScreen.cs`: Component on the screen to handle deposits.
- `Assets/Scripts/Mission/DataBox.cs`: Component defining box values.

# Implementation Steps
## 1. Create the Selection Script
- Create `Assets/Scripts/Mission/RAMBoxSelector.cs`.
- This script will handle `OnMouseDown()` to update `GameState` with the box's variables and trigger the scene change.

## 2. Configure the RAM Scene
- Attach `RAMBoxSelector` to the 3D boxes in the `RAM` scene.
- Set the `variableName` and `variableValue` for each box (e.g., one box with value "25").
- Ensure the boxes have `Colliders`.
- Ensure the Camera in the RAM scene is set up for clicking (Main Camera tag).

## 3. Setup Main Scene Components
- **Player**: Add the `PlayerHolder` component to the `PlayerArmature`.
- **Screen**: Add the `ConsoleScreen` component to the `flat_screen_television` object.
- **Trigger**: Ensure the Screen object has a `BoxCollider` with `isTrigger = true`.

## 4. Finalize GameState Logic
- Verify `GameState.cs` correctly handles `spawnDansLaMain` in `OnSceneLoaded`. (Currently it searches for `PlayerHolder` on the object tagged "Player").

# Verification & Testing
- **RAM Selection**: Clicking a box in the RAM scene should immediately load the MainScene.
- **Spawning**: Upon loading MainScene, the player should be holding a cyan cube with the correct value.
- **Deposit**: Walking to the screen should show a prompt. Pressing [E] should remove the cube and update the screen display with the value "25".
