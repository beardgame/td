# Changelog

## v0.12

* Rebalanced the early game by providing 245 resources instead of 150
* Reduced walls in cost from 10 resources to 7
* Damage numbers are now shown as pop-up over enemies
* Each veterancy level for buildings now increases a building's effectiveness by 15%

## v0.11

* Added targeting mode selection for towers
* Added pinging for multiplayer
* Resources are now awarded on wave completion rather than throughout
* The first wave now no longer automatically spawns
* Bug fix: towers actually hit most of the time now
* Bug fix: resources are correctly refunded when cancelling a building
* Added quality of life improvements for development

## v0.10

* Added sound effects to towers, wave start, and wave end
* Added screenshake
* Added particle effects to enemies getting hit and dying
* Added particle effects for splash damage
* Added particle effects to damage getting hit
* Added effects for building placement, construction, and level-up
* Added visual indication of building levels and upgrades
* Added notification when building levels up
* Enemies now have much less health, and more enemies spawn
* Improved enemy collision and crowding; they just want a hug
* Polished all existing particle effects
* Removed weapon recoil
* Hide building status indicators when building built
* Moved EnemySpawnDefinition into mod file
* Bug fix: building veterancy is synchronized across network
* Bug fix: applying slowing to enemies works again
* Linux binaries are now included in releases
* Master server binaries are now included in releases

## v0.9

* New upgrades: homing
* Rebuilt enemy movement to be more natural and physics based
* Exploration is now automated
* Enemies now always spawn at the edge of the visible area
* Changed generation of enemy spawners: they now spawn on dead ends as small nodes
* Added skull icons for revealed spawner locations and locations where enemies enter the visible area
* Building ghosts now look more like architectural drawings
* Made flames collide and deflect more realistically
* Improved flame thrower visuals
* Added fuel gauge indicator to flame throwers
* Bug fix: notifications now disappear even if the game is paused

## v0.8

* New towers: flamethrower, tesla coil
* New upgrades: charged projectiles
* New status effect: slowed, slowing down enemy movement
* New mechanic: refueling with drones, hail robots
* Refactored projectile movement to support larger variety of projectile behaviour
* Added animation system for graphical effects
* Migrated away from System.Drawing, bringing macOS support ever closer

## v0.7

* Added sprites for all existing towers
* Started generation of normal maps for tower sprites
* Added upgrades: incendiary bullets, shrapnel, burst
* Workers are no more, death to robots
* Made veterancy levels easier to obtain
* Towers now predict where enemies will be when they move
* Removed debug upgrades
