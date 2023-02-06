# Enemy class structure

Enemy classes can use `GameObjectBlueprint`.

## Components:

* [0..n] `Socket : ISocket`
  * Parameters: socket shape
* [1..1] `Threat : IThreat`
  * Parameters: threat value
  * Note: currently a float, should be strongly typed!
* [1..1] `ArchetypeProperty : IProperty<Archetype>`
  * Parameters: archetype

## Directory structure in mod:

* /
  * defs/
    * blueprints/
      * enemy-classes/
        * minions/
          * simple-minion.json5
          * complex-minion.json5
          * ...
        * ...
    * enemy-modules/
      * on-hit/
        * on-hit-explode.json5
        * on-hit-burrow.json5
        * ...
      * complex-minion/
        * very-specific-module-fire.json5
        * very-specific-module-lightning.json5
      * ...
