/*
 * Procedural Dungeon Generation System uses Binary Space Partitioning (BSP) to create random dungeons.
 * How it works in this game-
 * Define a root space representing the entire dungeon area.
 * Divide the space using vertical or horizontal line in a random point and add 
 * them to a tree.
 * Check if the new spaces are larger than the minimum size, if so, repeat the division process.
 * Repeat step 2
 * Repeat step 3 until all spaces are smaller than the minimum size.
 * Stop when no more divisions can be made.
 * For every created space, create a room within it by randomly selecting a corner points.
 * Start from the youngest tree branch and draw corridors between sibling rooms.
 * Go up a layer of our tree structure and repeat step 8 until we reach the root.
 */