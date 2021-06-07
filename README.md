# World-Builder-2D
A unity asset to easilly create interconnected 2D levels and worlds.

# Level Editor

# Manual Part 1 of Heuristic Evaluation
## First Level Template
Image here

## Keyboard shortcuts
- F11: Fullscreen Scene View
- F12: Open World Editor
- Space: Open Level Editor
- Q/W/E: Switch to Move/Scale/Rotate

## Step by step instructions
1. Create a level by switching to the World Editor (F12) and right clicking on an empty space
2. Then simply select Level->Create Level and put in a name (e.g. "Start Area")
3. Open the level by right clicking on the level title and clicking "Open Level"
4. A Window pops up telling you to initialize the level (Just press the "Initialize Level Scenes" button)
If it doesn't show up press SPACE to open up the Level Editor!
5. Press SPACE after the initialization to show all possible Level Objects, which can be placed inside the scene
6. Select "All Categories" to open up the category menu and then select the tree icon to limit the objects to pick to only forest objects
7. Click on the "Sky Icon" to select it and then on the level itself to place the background
8. Now select the "Forest Ground" tiles and the "Main Ground" layer.
9. A burger menu button should pop up on the top right (if not try clicking on the "All Categories" button to hide the other categories)
10. If you click on the burger menu button and then on the level, a tilemap will be generated.
11. This is the level object inspector: Now you can click on the "Auto Tiles" tab and select the first tile to draw the ground
12. Same thing applies to the tree trunks, tree leaves and other level objects which use a tilemap
13. After you placed the Ground, also place the tree trunks and leafs
14. Now you can also place some flowers (they are not tiles so you can just click to spawn them)
15. Now click on the background layer and place the background trees just like before by selecting the darker auto-tiles or normal tiles
16. Finally select foreground and place ground and tree on the foreground layer
17. After completing the level click on the "Level" Game Object in the Hierarchy
18. Here you can set the boundaries of the level by clicking the "Edit with gizmo" button
19. After changing the size of the green/blueish rectangle click on "Save Bounds"
20. Now click on "Level Exits" and add one by clicking the "+" button.
21. Click on the newly created level exit and here you can change its name the trigger for exiting the level and the entering position when entering this level
22. After you changed everything save the scene (Ctrl+S)
23. Now you can open the World Editor again (F12) and connect the level exit dot with the left dot of the other forest level
24. Lastly, drag the green forest world box over both forest levels
