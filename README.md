# Nightreign Relic Editor
This is a tool designed for speedrunners and challenge runners to quickly edit relics in Elden Ring: Nightreign. This is not a general purpose relic editor, it will not allow non-relic effects (such as Marais/Grafted) to be edited in game. Currently in prerelease testing.

Please ensure that EAC is disabled before using.

Includes relic verification to ensure that any relic injected into the game is legally obtainable through the in-game random roll system, and therefore valid to be used during individual lord speedruns.

## Features
- Quick search for relic effects.
- Pull relic data from in game.
- Save and load relic presets
- Verify relics are valid for speedruns

## How To Use
<img width="1055" height="427" alt="nre" src="https://github.com/user-attachments/assets/a10cd89c-87ea-417d-8264-63ca0f239fdb" />
Changing relics on the main window will not affect your game at all. Changes are only sent to the game once the *Set relics in game* button is pressed.

Relic effects are selected from the listbox on the left. The filter textbox can be used to help find specific effects. Click on one of the buttons at the bottom labelled *Add to Relic 1/2/3* to add the effect to one of your relics. If the relic does not already have three effects, the effect will be added, and all effects on that relic will be ordered to match the order Nightreign places them. Relic effects will turn red if there is a verification issues, and holding your mouse on the red text will show a tooltip detailing what is wrong. Relics with red errors cannot be injected into the game.

If the *Show unique relic effects* checkbox is marked, that will show effects that are on the special unique premade relics, such as *Critical Hits +1*, which is only available on *Dark Night of the Baron*. Orange errors are generally not allowed on relics for speedruns, but are allowed to inject in case you make a mistake and overwrite your existing unique relics.

To remove an effect from a relic, click the *X* button next to the effect you wish to remove.

Once you are happy with your relic setups, first make sure that Nightreign is running with EAC disabled. Click the "Connect" button to connect to the game process, and the status text should change to "Connected" if succesful. Push the "Set relics in game" button to attempt to edit your in game relics. A final verification will be attempted, and a messagebox will tell you if a verification error occurred.

If you wish to modify your existing relic setup that is already in game, you can click the "Import relic data from game" button to show your in game relics in the program.

The checkboxes next to the relic names can be unchecked if you want the program to ignore that specific relic when either putting your relics into the game, or pulling data from the game.

## How Does Relic Verification Work?
In the relic effect list, you can see that each effect has three columns associated with it labelled *Id*, *Category*, and *Order*.

Nightreign orders effects on relic primarily based on the *Order* property, going in ascending numeric order. If two effects are in the same order group, then they will be sorted by *Id*, once again going in ascending numeric order. This is why character specific effects are always in the first slot, as all of them have their *Order* property set to 1.

Each relic can only have one effect from each *Category* group. This mostly applies to effects that increase damage output, with most of those effects being under category 100. For example, the evergaol attack power relic, and *Physical Attack Up* both are both in category 100, and cannot appear on the same relic.
