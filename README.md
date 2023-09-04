# ksp2-papi

Mod that adds [PAPI (Precision Approach Path Indicator)](https://en.wikipedia.org/wiki/Precision_approach_path_indicator) to Kerbal Space Program 2's runways.

# Installation

### Option 1

* ~~Install using [CKAN](https://forum.kerbalspaceprogram.com/topic/197082-ckan-the-comprehensive-kerbal-archive-network-v1332-laplace-ksp-2-support/)~~ (not submitted to CKAN yet)

### Option 2

1. Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

2. Extract the contents of ksp2-papi*.zip into KSP2's root directory or copy `BepInEx/plugins/ksp2-papi` into `Kerbal Space Program 2/BepInEx/plugins`

# bug reports, feature requests and questions/difficulties

Please [open an issue on github](https://github.com/Codenade/ksp2-papi/issues/new/choose)

# Build from source

### Requirements

* [Python 3.5 or newer](https://www.python.org/downloads/)
* [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)
* [Unity 2020.3.33](https://unity.com/releases/editor/archive)
* [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) installed into Kerbal Space Program 2

### Build instructions

1. Clone this repository

2. Add a new environment variable named `KSP2_PATH` with the value set to the path to your installation of KSP 2 (eg. `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2`)

3. Run `./build.bat [--unity_executable "path to your Unity.exe"] [--install] [--start] [--skip_addressables]` (arguments enclosed with `[]` are optional)