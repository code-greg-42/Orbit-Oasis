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
- Performant Autosave Functionality
- Easily Exentensible Systems

[Additional descriptions on features](https://github.com/code-greg-42/Orbit-Oasis/blob/main/KeyFeaturesDetails.md)

## Codebase Overview 💻

## Developer Notes 📝
Developing Orbit Oasis was both a rewarding and challenging experience. The scope was much larger than anything I'd tackled before, providing the opportunity to build complex systems, learn new skills, and grow as a programmer. Each feature brought unique challenges that required a focus on both performance and gameplay.

---

#### Development Anecdote:
One challenge I encountered involved the building mechanics. The foundation for these mechanics had already been implemented, but one hard-to-solve issue remained: how to disallow the placement of a new build when the preview object was clipping through an already placed build.

At first glance, this might seem simple — you could just disallow placement whenever a collision occurs. However, the building mechanics relied on slight overlaps between builds to create visually seamless connections, so blocking all collisions wasn’t a viable option. What I really needed was a way to determine *how much* the preview was colliding with existing builds. Unfortunately, collision volume information wasn’t readily available. 

Performance was a key consideration, so constant calculations weren’t ideal, and manually measuring the overlap proved inaccurate. After exploring several approaches, I eventually settled on a solution that achieved the goal with minimal overhead. You can view the implementation in [BuildableObject.cs](https://github.com/code-greg-42/Orbit-Oasis/blob/main/Assets/Scripts/MainScene/Building/BuildableObject.cs)

This challenge taught me a valuable lesson in problem-solving: sometimes, you need to come up with creative solutions due to constraints from existing systems. It was frustrating to know how simple the solution could have been if any collision disallowed building, but this approach ultimately resulted in better overall gameplay, and prevented the need for a rework of the building mechanics I was already happy with.

---

Overall, developing Orbit Oasis allowed me to refine my skills in problem solving, performance optimization, debugging, and designing scalable systems — key strengths I’m eager to bring to future projects.

#### Other Skills I Strengthened:
- Code Organization
- Unity's Physics System
- Data Persistence
- Inventory Systems
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
