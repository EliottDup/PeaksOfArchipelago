# Peaks of Archipelago
Peaks of archipelago is a (VERY) work in progress Archipelago impelentation for Peaks of Yore
# What is Archipelago
Archipelago is a multi-game randomiser, allowing you to play randomised game with your friends. For more see [this link](https://archipelago.gg/faq/en/)

# WARNING
loading a vanilla save WILL (ALMOST DEFINITELY) BREAK YOUR GAME! (so try not to do that, report any bugs on the github)

# Info
if Peaks of yore is not yet on archipelago.gg, I am probably still getting it approved by the owners/community.


# One More Thing
This repo uses an environment variable to automatically copy the built compiled result where ever you want (Like the game files) (definitely going to save me some dragging files around).
the variable is `LocalModFolder`.

For Windows:
```powershell
$env:LocalModFolder="C:\path\to\mods\folder"
```

For Linux/MacOS:
```bash
export LocalModFolder="/path/to/mods/folder"
```

A better option is making a file called `Local.settings.props` with the following contents
```xml
<Project>
  <PropertyGroup>
    <LocalModFolder>C:/path/to/folder</LocalModFolder>
  </PropertyGroup>
</Project>
```