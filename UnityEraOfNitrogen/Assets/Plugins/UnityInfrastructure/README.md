# UnityInfrastructure

## Build

### Install Dependency

Several types depends on external library.  
Need to install additional packages before use them.

#### a. Install from UPM with Name

1. Open **Unity Package Manager**_(UPM)_ from Unity top-menu `Window > Package Management > Package Manager`.
1. Select top `+` button and select `Install package by name...`.
1. Input specific **package name** to `Name` field and select `Install` button.

* Specific **package names** are listed in corresponding `README` file.

### Conditional Compile

Several types are optional to avoid identifier collision or complexity.  
Need to add compiler symbols to enable them.

1. Open **Project Settings** from Unity top-menu `Edit > Project Settings...`
1. Go to `Player` tab and expand `Other Settings` section.
1. Add specific **symbol** to `Scripting Define Symbols` list and select `Apply` button.

* Specific **symbols** are listed in corresponding `README` file.
