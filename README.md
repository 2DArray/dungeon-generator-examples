
# Dungeon Generator Examples
Some examples of dungeon generators in Unity, including an example of a **"Fitness Function"** which can turn a shitty, broken generator into a usable one.

![16x generator gif](https://i.imgur.com/cPBFyCC.gif)

## Running the demos
There's one scene in the project, which contains a few game objects.  One of them is called "Map Curator" - this object spawns instances of our various dungeon generators.  All generators will start when you hit play, and you can quickly restart them by hitting the spacebar during Play Mode.  To switch between them, stop the game and switch the Template field in the MapCurator inspector to a different generator (they're all attached to inactive game objects in the same scene).

Enable the "Fast Forward" checkbox on Map Curator if you don't want to watch the generators while they're working - then you can jam the spacebar to see a bunch of results quickly.

## A general-purpose procgen method
The demos in this repo are only showing how to generate dungeons, but the following routine can be applied to any form of content that you want to create procedurally.

Decisions about how a procedural generator should work often end up involving a difficult balancing-act between two important needs:

* You want the generator to produce as much variety as possible, but...
* You also want the generator's creations to be of consistently-high quality

A **Fitness Function** allows you to achieve both, and it lets you focus on the two problems independently.

Okay, here we go!

### 1.  Make some examples of your content by hand  
  * *Sketch them on paper, draw them in MS Paint, etc.*

### 2.  Keep going until you can mentally describe a rough approximation of your process  
  * *Consider what you always do, and what you never do.*
  * *You don't need a perfect approximation.  Expect the generator to be weaker on average than your original examples.*
  * *(Just try to capture the most important steps)*
  * *("I start by picking some number of room locations, then I see how big I can make the rooms without overlapping them, then I add a bunch of halls and doorways to connect the rooms.")*

### 3.  Convert your approximated-process into code
  * *Try not to assume that your generator is "locked in" at any point - you can change anything about its behavior later on.*
### 4.  Explore lots of examples made by your new generator

  * *Identify which examples look good, and which examples are unusable*

### 5.  Write a function that measures the good/bad qualities of a generated asset - this is your generator's Fitness Function.  
  * *The only fitness function in this repo is "how long is the shortest path from the spawn tile to the exit?" (it's a standard pathfinding routine). It returns 0 if there's no path for a given map, or it returns the length of the shortest path if it can find one.*  
  * *For situations where your generator faces difficult or nuanced constraints (like a stealth game with randomly generated layouts), don't worry - you can combine two or more fitness functions very easily!  Their output is just a number, so you can add them together, maybe multiplying them by weights to show it different levels of importance, and so on.*
  * *A slightly more considerate fitness function might reward good qualities and penalize bad qualities:*  
  `Fitness(A) = MeasureDistance(A) - MeasureDeadEnds(A)*0.2`

### 6.  When you generate content for a player, generate a bunch of examples, measure their fitness values, and pick one of the highest-scoring results.  This selection is the only asset out of the set that you actually show to the player.  Discard all the others!

To see the true power of a Fitness Function, we can try plugging "Dungeon Generator (Noise)" into the MapCurator's Template slot, and setting the number of generators to 64.  If you check the results here, you'll see some abject horseshit - each map is illegible white noise, because this is about as low-quality as a dungeon-generating procedure could be.  There's no guarantee that any particular map will have a clear path to the exit.

![Shitty noise example](https://puu.sh/A2pCz/9335ec3890.png)

But! If you enable the "Check Fitness" tickbox, and also enable "Fill Inactive Tiles," you start getting maps that actually seem like they could be usable.  This last setting tells that generator that after the fitness function has found a path from the start to the finish, it should delete all of the open tiles that it wasn't able to reach.

By enabling the fitness function and taking the top score out of 64 examples, even the dumbest dungeon-maker possible can give us a usable result.  A more elaborate fitness function could find more well-targeted layouts for some specific set of game mechanics.

![Fitness-tested/cleared noise example](https://puu.sh/A2pHg/a76a13213d.png)

I hope that this has been useful to someone!  I'm really enamored with the fitness function strategy, so let me know if you've seen any whitepapers about it, or if you make some cool stuff with any of these ideas.

Twitter: [@2DArray](http://www.twitter.com/2DArray)