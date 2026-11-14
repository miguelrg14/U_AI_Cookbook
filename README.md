# AI Cookbook

A collection of game AI techniques implemented in Unity (C#). All the demos live in a single Unity project and are launched from a main menu.

**Unity version:** 6000.6.8f1 · Built-in Render Pipeline

## Demos

| # | Technique | Demo | Scene |
|---|-----------|------|-------|
| 01 | Behaviour Trees | Zombie infection | `Assets/01_BehaviourTrees/_Scenes/BT_ZombieInfection.unity` |
| 02 | Decision Trees | Predator & prey | `Assets/02_DecisionTrees/_Scenes/DT_Hunting.unity` |
| 03 | Finite State Machines | Predator & prey | `Assets/03_FiniteStateMachines/_Scenes/FSM_Hunting.unity` |
| 04 | Procedural Generation | Perlin noise terrain | `Assets/04_ProceduralGeneration/_Scenes/PCG_ProceduralGeneration.unity` |
| 05 | Minimax · Negamax · Aspiration | Connect Four | `Assets/05_Minimax/IA Practice/Scenes/Minimax_ConnectFour.unity` |
| 06 | Genetic Algorithms | Launch to target | `Assets/06_GeneticAlgorithms/Scenes/GA_LaunchTarget.unity` |
| 07 | Reinforcement Learning (Q-Learning, SARSA) | Maze solver (C# console app) | `07_Reinforcement-Learning/` |

## How to run

1. Open the `Proyecto/` folder with Unity Hub.
2. Open `Assets/_Menu/Scenes/MainMenu.unity` and press Play.
3. Inside any demo, press **Esc** or the **Menu** button (bottom-left) to return.

## Tests

EditMode tests (Window → General → Test Runner → EditMode → Run All):

- `AICookbook.Shared.Tests`: target detection ranges and hysteresis with real colliders, agent movement.
- `AICookbook.GeneticAlgorithms.Tests`: population, selection, crossover (including a regression test for BLX-α), mutation ranges and a full evolution run that must improve the population.

## Repository layout

```
Proyecto/                Unity project
  Assets/
    _Menu/               Main menu, demo catalog and navigation
    _Shared/             Code and art reused by several demos (camera, decision system, agent base classes, materials, icons)
    01_BehaviourTrees/   One folder per demo with its own scenes, scripts, prefabs and art
    ...
    TextMesh Pro/        Shared TMP resources
Documentacion/           Original assignment reports (Spanish)
07_Reinforcement-Learning/  Standalone C# console project
```

Each module's scripts live in their own namespace (`AICookbook.BehaviourTrees`, `AICookbook.DecisionTrees`, ...), so modules never clash with each other. Code reused by several demos lives in `Assets/_Shared` (`AICookbook.Shared`): the free camera, the decision system (actions, conditions, decision trees and state machines) and the agent building blocks used by demos 01-03 (`EntityStats`, `TargetDetectorBase`, `AgentMotor`, `WanderRoutine`). `05_Minimax` keeps the global namespace because its scenes are serialized in binary and their UI events reference the type names directly. Each module is also its own assembly (`.asmdef`): demos only depend on `AICookbook.Shared`, never on each other, and `05_Minimax` stays in `Assembly-CSharp`. To add a new demo, create its folder with its own `.asmdef`, add its scene to Build Profiles and register it in `Assets/_Menu/Scripts/DemoCatalog.cs`.
