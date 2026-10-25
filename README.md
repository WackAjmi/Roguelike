Option 1:
Roguelike
A turn-based 2D roguelike made with Unity. Move across procedurally populated boards, collect food to stay alive, fight enemies, and progress through increasingly challenging levels.

Features:

Progressive levels (Bonus): Board dimensions grow as you advance, and later levels contain more enemies and walls.

Turn-based enemies: Enemies move toward the player after each turn and attack when adjacent.

Food and survival: Collect food to restore your supply. Turns and enemy attacks consume food; the game ends when it reaches zero.

Smooth movement: The player moves between cells with a short animation.

Menus (Bonus): Start from the title screen, pause and resume with Escape, or restart and quit from the menus.

Audio and visual effects: Sound effects and editable particle prefabs play for events such as movement, wall attacks, food collection, and enemy deaths.

Object pooling: Board objects are reused between levels.

Controls
Input	Action
Arrow keys	Move
Escape	Pause or resume
Enter	Restart after game over
Running the Game
Open the project in Unity 6.
Open Assets/Scenes/Main.unity.
Press Play in the Unity Editor, then select Play on the title screen.

Enemy Behavior
Enemies pursue the player, taking available open cells to get closer. When adjacent, they attack and reduce the player’s food supply. Moving into an enemy damages it; defeating one triggers its death sound and particle effect.
