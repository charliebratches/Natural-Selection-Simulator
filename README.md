Natural Selection Simulator
Inspired by a video on Primer's youtube channel: https://www.youtube.com/watch?v=0ZGbIKd0XrM

![Cool gif](./naturalSelectionGif.gif)

Overview:

This is a basic simulation of a population with 3 changing traits: size, speed, and sense. 
In this world, distinct groups of creatures will evolve from a common ancestor. Each group will look different, and have slightly different genetic abilities.
Over time, the population will become genetically diverse enough that groups will be unable to interbreed. When genetically distinct groups cannot interbreed, speciation has occurred.

Game Rules:

Each day, the population spawns in the center of a field of green pellets. A pellet is a unit of food that is essential for survival, reproduction, and ultimately speciation.
Each creature is randomly assigned to be male or female. Atleast one male and one female must eat something in a day in order to produce offspring. For a male and female to
produce offspring, they must be within a genetic range called a "speciation amount". This just means that their 3 genetic traits must be within range of each other.
Each time an offspring is produced, a random mutation occurs to one of their 3 traits - size, speed, & sense.

Over time, speciation occurs in this simulation - meaning that visually & genetically distinct groups form that cannot inter-breed.

This was just a fun weekend project - some day I would love to come back to it and expand the complexity of the simulation!

Traits:

* Increased size means creatures are able to eat more food. Larger creatures are more red.
* Increased speed means creatures are able to get food faster. Faster creatures are more green.
* Increased sense means creatures are able to see food in a larger radius. Sensitive creatures are more blue.

All three traits also come with an energy cost. For instance, very large, fast creatures will expend all their calories very quickly. 
A creature that runs out of energy cannot search for food for the rest of the day.

Sometimes, the simulation is a "dud". There is a good amount of randomness here, and it takes some luck for a stable population to occur, let alone speciation. Letting it run for an hour will either result in a diverse population or complete extinction.
