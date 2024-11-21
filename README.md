# Orbit Oasis 🚀
Orbit Oasis is a casual, creative, exploratory game where players can build up and modify their environment. Set from the perspective of a recently freed robot who has been granted their own plot of land, players can plant trees, build structures, raise animals, and create an oasis in any way they choose.

The game was created as a portfolio project with the intent of showcasing programming skills, and only free and available-for-use assets were used.

## Game Trailer 🎥
Check out the [**game trailer**](https://youtu.be/Y6ZFQQC7rkk) on YouTube!

## Play Now! 🎮
Visit the [**game page**](https://grandersson.itch.io/orbit-oasis) on itch.io to download and start playing!

## Key Features 🌟
- Dynamic Building System
- Interactive Farming Mechanics
- Flexible and Robust Inventory System
- Customizable Environment
- Space Race Minigame
- Introductory Quest System
- Performant Autosave Functionality
- Scalable Systems

[More Information On Key Features](https://github.com/code-greg-42/Orbit-Oasis/blob/main/KeyFeaturesDetails.md)

## Codebase Overview 💻
The scripts folder is located at:
<br>
[/Assets/Scripts](https://github.com/code-greg-42/Orbit-Oasis/tree/main/Assets/Scripts)

#### Data Manager
The DataManager script is the highest-level script in the project. It is active across all scenes and is responsible for maintaining, saving, and loading the game state. You can find it here:
<br>
[/Assets/Scripts/DataClasses/DataManager.cs](https://github.com/code-greg-42/Orbit-Oasis/blob/main/Assets/Scripts/DataClasses/DataManager.cs)

#### Organized By Scene
The codebase is structured with scene-specific folders. Each scene has its own dedicated folder containing the scripts and logic relevant to that part of the game:
<br>
[/Assets/Scripts/MainScene/]()
<br>
[/Assets/Scripts/SpaceRace/]()
<br>
[/Assets/Scripts/Menu/]()
<br>
[/Assets/Scripts/Story/]()

#### Managers Folder
The bulk of the game’s logic is in the main scene, with manager scripts handling specific features such as building and inventory. These scripts are the highest level scripts in the main scene and are located in the following folder:
<br>
[/Assets/Scripts/MainScene/Managers](https://github.com/code-greg-42/Orbit-Oasis/tree/main/Assets/Scripts/MainScene/Managers)

#### Key Scripts By Feature
Additionally, here is a breakdown of important scripts grouped by their respective features.

##### Player Mechanics
- [PlayerAnimation.cs]() - player animation states.
- [PlayerControls.cs]() - input and action availability.
- [PlayerMovement.cs]() - movement logic and physics.
##### Building
- [BuildManager.cs]() - building placement and validation.
- [BuildableObject.cs]() - attachment point and collision checking.
##### Farming
- [FarmableObject.cs]() - farmable resource mechanics and generation.
- [FarmingTool.cs]() - tool collision tracking and interaction.
##### Inventory
- [InventoryManager.cs]() - add, sell, drop items.
- [InventorySlot.cs]() - display component for inventory.
- [Item.cs]() - base class for item properties.
##### Customizable Environment
- [ItemPlacementManager.cs]() - activates placement mode and maintains preview.
- [PlaceableItem.cs]() - valid placement determination.
- [Projectile.cs]() - captures objects into inventory.
##### Space Race Minigame
- [SpaceRaceGameManager.cs]() - overall minigame logic.
- [SpaceRacePlayerMovement.cs]() - spaceship movement controller.
- [SpaceRaceAsteroid.cs]() - obstacle behavior and interactions.
##### Introductory Quests/Dialogue
- [QuestManager.cs]() - tracks quest progress and gives rewards.
- [DialogueManager.cs]() - fetches and displays in-game dialogue.
##### Sound/UI
- [MainSoundManager.cs]() - manages sound effects in the main scene.
- [MainUIManager.cs]() - manages ui elements in the main scene.

## Developer Notes 📝
Developing Orbit Oasis was both a rewarding and challenging experience. The scope was much larger than anything I'd tackled before, providing the opportunity to build complex systems, learn new skills, and grow as a programmer. Each feature brought unique challenges that required a focus on both performance and gameplay.

---

#### Development Anecdote:
One challenge I encountered involved the building mechanics. The foundation for these mechanics had already been implemented, but one hard-to-solve issue remained: how to disallow the placement of a new build when the preview object was clipping through an already placed build.

At first glance, this might seem simple — you could just disallow placement whenever a collision occurs. However, the building mechanics relied on slight overlaps between builds to create visually seamless connections, so this wasn’t a viable option. What I really needed was a way to determine *how much* the preview was colliding with existing builds. Unfortunately, collision volume information wasn’t readily available. 

Performance was a key consideration, so constant calculations weren’t ideal, and manually measuring the overlap proved inaccurate. After exploring several approaches, I eventually settled on a solution that achieved the goal with minimal overhead. You can view the implementation in [BuildableObject.cs](https://github.com/code-greg-42/Orbit-Oasis/blob/main/Assets/Scripts/MainScene/Building/BuildableObject.cs) (line 205).

This challenge taught me a valuable lesson in problem-solving: sometimes, you need to come up with creative solutions due to constraints from existing systems. It was frustrating to know how simple the solution could have been if any collision disallowed building, but this approach ultimately resulted in better overall gameplay, and prevented the need for a rework of the building mechanics I was already happy with.

---

Overall, developing Orbit Oasis allowed me to refine my skills in problem solving, performance optimization, debugging, and designing scalable systems — key strengths I’m eager to bring to future projects.

#### Other Skills I Strengthened:
- Code Organization
- Unity's Physics System
- Data Persistence
- Inventory Systems
- Quest Design and Implementation
- Game State Management
- Object-Oriented Programming
- Cinemachine Integration
- Audio Management

## Additional Documentation 📂
- [**Game Design Document**](https://github.com/code-greg-42/Orbit-Oasis/blob/main/GameDesignDocument.md)

## Additional Gameplay Videos 🎥
- [**Space Race Minigame - Insane Difficulty**](https://youtu.be/8dULJcSHKwY)
- [**Full Tutorial Playthrough**](https://youtu.be/dJkheYfaU6U)

## Programs Used 🛠️
[![Unity Badge](https://img.shields.io/badge/Unity-2022.3.20f1-ffcc00?logo=unity&logoColor=white)](https://unity.com/releases/editor/whats-new/2022.3.20)
<br>
[![Audacity Badge](https://img.shields.io/badge/Audacity-Audio_Editing-blue?logo=audacity&logoColor=white)](https://www.audacityteam.org/)
