# AddonWeapons
Modders create a lot of cool weapons for GTA 5, but they have to replace the standard models. AddonWeapons allows you to add weapons to the game without replacement and buy them in Ammu-nation in a separate menu. There are also weapons available exclusively in GTA Online.

The box of weapons is located in the two Ammu-nations with a shooting range in Los Santos.

The script finds new weapons automatically, just install them like any addon mod (eg. cars).

**IMPORTANT: This mod by itself doesn't bring any new weapons to the game.**
# Installation
1. Install [ScriptHookV](http://dev-c.com/gtav/scripthookv/)
2. Install [ScriptHookVDotNet](https://github.com/scripthookvdotnet/scripthookvdotnet/releases/latest)
3. Install [LemonUI (SHVDN3)](https://github.com/LemonUIbyLemon/LemonUI/releases/latest)
4. Move the `scripts` folder into your main GTAV folder (press _Replace the files in the destination_ if Windows asks you to).

# Trainer Mode (BETA)
You can customize AddonWeapons into trainer mode. In this case, the weapon purchase menu can be opened anywhere.
Set the menu activation key in the AddonWeapons\settings.ini file

# Weapon components (for mod authors)
To display weapon components in the menu, fill in the weapon_shop.meta file correctly.

# Weapon tints (for mod authors)
In order for the tints names to be displayed correctly in the purchase menu, you need to adhere to Rockstar standards: either use a set of 8 tints from standard weapons, or a set of 32 tints from MKII weapons.
If you are not adding all the tints in your mod, or combining them from different types of weapon, you will need to create a text file in the AddonWeapons/tints folder with the name of the weapon from weapon_shop.meta, and write all the tints names line by line. Otherwise, the menu will display tints without names (Tint 1, Tint 2, etc.), or the names will not correspond to the actual weapon tint.
