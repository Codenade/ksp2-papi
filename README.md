# ksp2-papi

Mod that adds [PAPI (Precision Approach Path Indicator)](https://en.wikipedia.org/wiki/Precision_approach_path_indicator) to Kerbal Space Program 2's runways.

# Installation

### Option 1 (recommended)

* ~~Install using [CKAN](https://forum.kerbalspaceprogram.com/topic/197082-ckan-the-comprehensive-kerbal-archive-network-v1332-laplace-ksp-2-support/)~~ (not submitted to CKAN yet)

### Option 2

1. Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

2. Extract the contents of ksp2-papi*.zip into KSP2's root directory or copy `BepInEx/plugins/ksp2-papi` into `Kerbal Space Program 2/BepInEx/plugins`

# bug reports, feature requests and questions/difficulties

Please [open an issue on github](https://github.com/Codenade/ksp2-papi/issues/new/choose)

# Build from source

### Required software

* [.Net SDK](https://dotnet.microsoft.com/en-us/download)
* [Unity 2022.3.5](https://unity.com/releases/editor/archive)

### Build instructions

1. Clone this repository

2. Run `dotnet tool restore`

3. Run `dotnet cake [--target {Clear|Build|Pack|Install|Uninstall|Start}] [--configuration {Release|Debug}]`  
   Arguments enclosed in `[]` are optional.  
   The defaults when not provided are `--target Pack` and  `--configuration Release`.