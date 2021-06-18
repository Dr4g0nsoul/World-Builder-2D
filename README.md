# World Builder 2D
A unity asset to easilly create interconnected 2D levels and worlds.

## Keyboard shortcuts
- F11: Fullscreen Scene View
- F12: Open World Editor
- Space: Open Level Editor
- Esc: Deselect currently selected level object
- Q/W/E: Switch to Move/Scale/Rotate
- Ctrl+S: Save

## World Editor
![World Editor Parts](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/EditorHelp3.png)
- Right click on an empty space to create levels or worlds
- Right click on the title section of a level to open/rename/delete the level
- Connect levels by clicking on a level exit dot and dragging it to another dot
- Use the bottom right corner of a world to resize the world by clicking and dragging the mouse
### The World Editor Inspector
![World Editor Inspector](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/EditorHelp4.png)
- When clicking on a level or world you can change its properties.
- Under the tab "Level Favorites" or "World Favorites", you can set categories and individual level objects, which are filtered by when activating the filters in the level editor

## Level Editor
![Level Editor Parts](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/EditorHelp1.png)
- Press SPACE to open up the Level Editor
- Select a layers on which you want to put your level objects
  - If a layer has parallax scrolling enabled, it will automatically move the layer with the camera
    - What you see in the scene is what you get when playing the game
  - Only the layers available to the currently selected level object are shown
    - Press ESCAPE to deselect the current level object to show all layers
- Select one or multiple categories to filter out unneeded level objects
- You can also search for individual level objects using the search function or by using the Level and World filter function, which can be set up in the World Editor inspector of the current World / Level.
- ***Filters only work on the level objects under "All items" and not within the recently used items bar***
- If the filter/layer button stays colored, it meanst that it is currently active
### The Level Editor Inspector
![Level Editor Inspector](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/EditorHelp2.png)
- Here you get additional options and information about level objects
- Some level object types will also give you additional controlls, like for example Tile Maps, where you can select the tile to place on the selected tilemap

## Level Properties
![Level Properties](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/EditorHelp5.png)
- To access the level properties in the Inspector, you need to click on the GameObject called "Level" at the root of your Level Scene.
- Here you can adjust different properties about the current level, like Name, Description, its boundaries/size and level exits
- To change the levels size just press the "Edit with gizmo" button under "Level Bounds" to make the handles appear on the boundary indicator.
  - Just drag the handles to resize the box and click the "Save Bounds" button to apply the changes
### Level Exits
![Level Exit Properties](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/EditorHelp6.png)
- When clicking on the "Level Exits" tab you can add level exits which bring you to other levels
- When selecting a level exit, a box and a dot appear.
  - The dot can be dragged around, which sets the entry point of the level when you enter the current level from another level
  - The box can be adjusted exactly like the level bounds and represents the trigger which triggers the level transition and brings the player to another level

## Tilemap Editor
![Tilemap Editor](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/EditorHelp7.png)
- Click on the Burger icon on the top right to access the tilemap properties
  - If another message appears instead of the tiles try clicking on the level
  - Tiles from a different Level Object or Layer will be drawn on a separate tilemap
- To draw tiles just select them from the tile list
- Use the "fill rectangle" tool to fill a square with the selected tile
  - Just press on the "Fill rectangle" button after selecting the tile and it should stay blue
  - Now just click on one part of the map and then on another and it should fill that area with the selected tile
    - Two clicks no mouse drag
- Use the "Enter Deletion Mode" button to delete tiles
  - If the button stays red you are in deletion mode
### Auto-tiles
![Auto Tiles](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/EditorHelp8.png)
- Auto tiles select the correct tile depending on its neighbours
- Use the dropdown to select the auto tile mode, this mode changes the behavior on how neighbouring tiles are checked and updated
  - **All Tiles**: Every tile on the same tilemap are considered valid
  - **Auto Tiles only**: Only auto tiles on the same tilemap are considered valid
  - **Same group auto tiles**: Only auto tiles of the same group, on the same tilemap, are considered valid
- You can check the tilemap group by hovering over a placed tile on the tilemap.
  - If a number appears it is an auto-tile, the number corresponds to the group index
### Level background
- To place the level background, just select the background level object and then click on the level to place it
- No further action is needen, the background will move automatically with the game or scene camera, depending if you are currently in game mode or not

## Manual for Part 1 of the Heuristic Evaluation
### First Level Template
![First Level Template](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/level.png)

### Step by step instructions
1. Create a level by switching to the World Editor (F12) and right clicking on an empty space
2. Then simply select Level->Create Level and put in a name (e.g. "Start Area")
3. Open the level by right clicking on the level title and clicking "Open Level"
4. A Window pops up telling you to initialize the level (Just press the "Initialize Level Scenes" button)
If it doesn't show up press SPACE to open up the Level Editor!
5. Press SPACE after the initialization to show all possible Level Objects, which can be placed inside the scene
6. Select "All Categories" to open up the category menu and then select the tree icon to limit the objects to pick to only forest objects
7. Click on the Level Object with the "Sky Image" to select it and then on the level itself to place the background
8. Now select the "Forest Ground" tiles and the "Main Ground" layer. 
(You can hover over buttons to reveal a tooltip, which tells you what element you are currently hovering)
9. A burger menu button should pop up on the top right (if not try clicking on the "All Categories" button to hide the other categories)
10. If you click on the burger menu button and then on the level, a tilemap will be generated.
11. This is the level object inspector: Now you can click on the "Auto Tiles" tab and select the first tile to draw the ground
12. Same thing applies to the tree trunks, tree leaves and other level objects which use a tilemap
13. After you placed the Ground, also place the tree trunks and leafs
14. Now you can also place some flowers (they are not tiles so you can just click to spawn them)
15. Now click on the background layer and place the background trees just like before by selecting the darker auto-tiles or normal tiles
16. Finally select foreground and place ground and tree on the foreground layer
17. After completing the level, click on the "Level" Game Object in the Hierarchy
18. In the normal inspector window, you can set the boundaries of the level by clicking the "Edit with gizmo" button
19. After changing the size of the green/blueish rectangle, click on "Save Bounds" to save your changes
20. Now click on the "Level Exits" tab and add a new level exit by clicking the "+" button.
21. Click on the newly created level exit. Below the list, new options appear where you can change the level exits name, the trigger for exiting the level, and the entering position when entering this level
22. After you are done save the scene (Ctrl+S)
23. Now you can open the World Editor again (F12) and connect the level exit dot with the left dot of the other forest level
24. Lastly, drag the green forest world box over both forest levels by first dragging the world box to the left and then expanding it using the bottom right corner

## Requirements for Part 2 of the Heuristic Evaluation
-	Create a volcano level with two level exits.
-	The size of the level should be approximatly 2 screens wide and 1 screen tall.
-	Add a background image to the level
-	Foreground and background layers are optional.
-	Obstacles and/or enemies should be added
-	Obstacles/Enemies contain only volcano themed objects.
  -	Try to avoid objects that are not mountain or lava themed.
  -	Use the “volcano” category to filter out the right objects.
-	Adjust the level boundaries and connect the level to other adjacent levels.

### A reference for level ideas (The actual level should be smaller)
![Second Level Reference](https://github.com/Dr4g0nsoul/World-Builder-2D/blob/release/images/level2.png)
