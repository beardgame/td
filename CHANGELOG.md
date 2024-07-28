# Changelog

## v0.28

* Added visual overlays to the game, projected to the game geometry
  * The game now shows the grid & buildable tiles while building towers
  * Added a button & shortcut (G) to permanently enable the grid or building overlay
  * Updated rendering of the tower ranges
  * Added highlight for towers when hovering over their card in the statistics screen
* Added the current tower statistics to their status overlay
* Added some small jitter to enemy spawn times
* Started enforcing tier requirements for technology unlocks
* Balance: Steam Hammer can now receive the fire explosion upgrade

## v0.27

* Added new screen to show damage statistics at the end of each wave
  * Shows towers with most damage and highest accuracy
  * Shows distribution of damage across elements
* Expanded functionality of building status overlay
  * Added support for building upgrades
  * Added information about veterancy level and progress
  * Added support for switching targeting and discharge modes
  * Added button for building deletion
  * Added building name
  * Added support for the Core and walls
  * Made a clearer distinction between the preview and opened version of the overlay
* Removed old building status screen and status overlay
* Removed notifications
* Improved the layout of the top HUD
* Added sound effects for several actions
  * Placing a building
  * Upgrading a building
  * Unlocking a technology
* Added a background blur effect to several UI elements
* Added a glow to the technology button if a new technology is available
* Improved UI colours
* Added tooltips to the action bar buttons
* Removed hover effects on non-interactive buttons
* Fixed text being misaligned in most UI
* Made improvements to damage numbers by grouping nearby numbers into one
* Reduced the brightness of player cursors
* Reduced the brightness of the enemy path indicators
* Reduced the overall brightness of the game
* Reduced size of all towers
* Replaced enemy debug shapes with rendered sprite
* Aligned crevice drops more closely with tile boundaries
* Removed timers between enemy waves
* Added more options for level topology
* Balance: enemies now deal a single chunk of damage and die instantly upon reaching the Core
* Balance: reduced how effective fire damage is
* Balance: made towers receive their veterancy level on wave end if they are within 5% of levelling
* Balance: stopped attributing damage to towers caused by exploding enemies
* Bug fix: button states are now correctly initialized rather than requiring a mouse hover
* Bug fix: fixed gaps in the level heightmap
* Bug fix: allowed multiplayer clients to fire EMPs and receive EMP events
* Bug fix: fixed enemy spawn locations not being placed correctly for multiplayer clients
* Bug fix: fixed kinetics towers jamming not being synchronised in multiplayer
* Bug fix: fixed multiplayer clients always interpreting each enemy as a unique form
* Bug fix: fixed a crash that happened for some building statuses with an expiry time

## v0.26

* Replaced main menu design
* Replaced technology screen design
* Added work in progress status overlay for buildings
  * Replaces status overlay
  * Shows current upgrades
* Replaced some square UI elements with hexagonal elements
* Made improvements to text styling and formatting
* Made improvements to tooltip styling
* Added UI animations
  * Menu slide-in
  * Button background
* Bug fix: correctly pass mouse input through the version number overlay

## v0.25

* Completely rewrote UI skin
  * New solid backgrounds & colours for UI elements
  * Rounded corners for UI elements
  * Revised theming for common UI controls
  * Added drop shadows to tooltips
* In-game UI layout completely changed:
  * Action bar and resource information have been moved to the bottom of the screen
  * Wave information has moved to the top of the screen, together with a new layout for the Core statistics
  * The player information (list of players + pings) has been removed entirely
* Changed EMP shortcut from E to ctrl+E
* Fixed vertical alignment of text labels
* Updated debug console font to monospace
* Building status is now always shown when hovering over a building with the cursor
* Bug fix: backslashes are drawn again in the debug console

## v0.24

* Replaced UI font rendering
* Replaced fonts
* Replaced checkbox icon
* Hid status of buildings and enemies by default, toggle using Z and hold alt to temporarily invert the visibility state
* Added loading of core UI assets to the game bootup
* Known issue: vertical alignment of text labels is off
* Known issue: icons in the UI are drawn upside down

## v0.23

* Changed the building and upgrading mechanics
  * Resources are no longer consumed over time, but immediately consumed
  * Queueing of buildings or upgrades has been disabled
* Replaced flamethrower sprite
* Improved visuals of fire explosion
* Improved visuals of both incendiary and charged projectiles
* Updated sound effects for tesla coil and steam hammer
* Changed level generation to be less likely to generate map with small bottlenecks dividing the map in two

## v0.22

* Improved fire rendering
* Improved rendering of bullets, bullet casings, and bullet trails
* Rebalanced fire damage: most damage now happens over time, rather than immediately on hit
* Added new upgrades:
  * Napalm (flamethrower & flame trap)
  * Fire explosions (grenade launcher)
  * Napalm explosion (grenade launcher with fire explosion)
* Introduced a preview of the path refuelling drones will take when building or selecting a tower with fuel tanks
* Renamed some towers and technologies:
  * Mortar -> Grenade launcher
  * Stomp -> Steam hammer
  * All technologies are now named after the upgrade they unlock
* Changed lightning explosions to no longer do kinetics damage and do more lightning damage instead
* Added a status icon for kinetics towers that get jammed
* Added an application and window icon
* Bug fix: temperature is now capped
* Bug fix: damage is no longer drawn if the damage is zero
* Bug fix: damage done through an enemy being on fire is now correctly attributed to the tower setting the enemy on fire

## v0.21

* Rewrote arc targeting to include bounces, branches, and be able to shoot around corner
* Introduced damage scaling for hitting multiple targets with lightning
* Added rendering for lightning arcs
* Added capacitor charge to Tesla Coil
  * Damage is now tied to the amount of charge in the capacitor when fired
  * Allowed choosing between fully or partially charged capacitor for shooting
  * Add animations to the Tesla Coil informing the charge level of the capacitor
* Introduced lightning upgrades:
  * Improved capacitors (tesla coil)
  * Lightning explosion (mortar & steam hammer)
  * Branching (tesla coil & weapons with charged projectiles)
* Made each lightning upgrade add one potential bounce to lightning arcs
* Replaced on hit effect of charged projectiles to spawn a lightning arc rather than do lightning damage directly
* Remove charged projectiles as potential upgrade from mortar
* Added smoke trails to projectiles
* Lowered the chance that lightning towers only hit the level
* Added FPS cap

## v0.20

* Added armour and shields for enemies
* Added status icons for most statuses
  * Tower temperature is now a status, separated into hot or cold
  * Tower fuel is now tracked as a status
* Added consistent health bar rendering, including armour and shields
* Added particle effects
  * Smoke when a tower is disabled
  * Bullet casings
  * Projectile trails
  * Exhaust gases for Kinetics towers
* Made the Cannon have two barrels that shoot alternatingly
* Replaced sprite for Cannon
* Changed spawn patterns of swarming enemies to be closer together
* Made the sniper tower use the "highest health" target mode by default
* Consistently refer to the Kinetics element as "Kinetics" and no longer use "Force"

## v0.19

* Added multiple biomes
* Introduced new biomes: coral, basalt
* Replaced rubble blockers with fungi
* Improved level texture rendering
* Added level clutter: glowing fungi, glowing moss
* Bug fix: buildings cost resources again

## v0.18

* Replaced level textures
* Added Core sprite
* Added sprites for Stomp and refueling drone
* Replaced existing tower sprites: Wall, Tesla Coil, Gatling
* Added tower foundations
* New enemy generation algorithm
  * Waves may now spawn multiple types of enemies, mixing them in various ways
  * Added swarm enemies
  * Added accent elements to chapters

## v0.17

* Tesla tower now shoots arcs that may bounce or split
* Make changes to Force towers
  * Force towers start with a free upgrade slot
  * Force towers may jam every now and then, disabling the tower for a short amount of time

## v0.16

* Added background sound that changes dynamically based on the phase of the game
* Buildings now keep track of their temperature
  * Buildings in overdrive slowly accumulate temperature
  * Fire explosions cause nearby buildings to heat up
  * Buildings slowly cool off to the equilibrium over time
  * Buildings that reach a temperature that is too high are disabled until they cool off enough
  * Removed the damage over time from the overdrive
* New options:
  * Always show health bar
  * Always show temperature gauge
* Changed enemy colliders to improve the accuracy of towers
* Show the icons of the enemies that will spawn each wave
* Added rubble to level generation
* Added metal ore veins to level generation
* Rebalanced the volume of some sound effects
* Bug fix: enemy generation now deterministically depends on the seed

## v0.15

* Added a new tower: flame trap
* Added explosive gas clouds that ignite when hit by fire damage
* Towers can now be built in specific orientations
* The current health of the Core is now always shown in the UI
* There is now an always visible button to trigger the Core's EMP
* The Core now shows its damage statistics
* The escape key now more consistently closes open windows
* Added a global 'E' shortcut to activate the EMP
* Allowed selecting buildings from the action bar using number keys on the keyboard
* Added a select box to choose a game mode in the lobby screen
* Elements have been renamed to the more colloquial terms
* The activation of spawn locations is now seeded
* The revealing of new map nodes is now seeded
* Bug fix: enemies are now once again slowed through varying lightning effects
* Bug fix: progress bars are now rendered correctly when opening a window after the progress has already started
* Bug fix: the game no longer crashes when attempting to batch enemies in later waves
* Bug fix: it is no longer possible to interact with the game while the pause menu is open

## v0.14

* Added a variety of enemies with different behaviours:
  * Enemies that sprint when moving in straight lines;
  * Enemies that explode when dying;
  * Enemies that kill themselves when arriving at the Core, doing a large chunk of damage;
  * Enemies that attempt to dodge out of the way of incoming projectiles.
* The Core is now outfitted with weapons to defend itself
* The Core is now outfitted with an emergency EMP that deals massive damage to enemies, but disables all towers temporarily
* Added overdrive mode for towers, increasing their damage output but slowly damaging them
* Wider corridors between caves can now be generated
* Cave layouts have been improved for better gameplay and mazing
* Changing the targeting mode of a tower immediately causes the target to seek a new target
* Balance: difficulty of the generated enemies now scales less fast
* Balance: enemies typically spawn in tighter groups now
* Balance: tower upgrades are now applied faster, but disable the tower while being upgraded
* UI: changed the colour for electric damage and conductivity research to a lighter purple
* Bug fix: targeting mode is now synchronized over the network
* Bug fix: enemy position synchronization over the network works again
* Bug fix: building a tower on top of a wall will now take into account the reduced cost for deciding when to start building

## v0.13

* Show the price of building a building on top of the ghost, keeping in mind refunds
* All damage output now more accurately follows the intended balance
* Wave generation is now seeded
* Bug fix: walls are now retained until tower replacing it actually starts building
* Colours of elements are now consistent between technology and damage numbers

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
