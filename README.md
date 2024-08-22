# ksp2-papi

Mod that adds [PAPI (Precision Approach Path Indicator)](https://en.wikipedia.org/wiki/Precision_approach_path_indicator) to Kerbal Space Program 2's runways.

# Installation

### Option 1 _~~(recommended)~~_

* ~~Install using [CKAN](https://forum.kerbalspaceprogram.com/topic/197082-ckan-the-comprehensive-kerbal-archive-network-v1332-laplace-ksp-2-support/)~~ _(not submitted to CKAN yet)_

### Option 2

1. Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

2. Extract the contents of ksp2-papi*.zip into KSP2's root directory or copy `BepInEx/plugins/ksp2-papi` into `Kerbal Space Program 2/BepInEx/plugins`

# Build from source

### Required software

* [.Net SDK](https://dotnet.microsoft.com/en-us/download)
* [Unity 2022.3.5](https://unity.com/releases/editor/archive)

### Build instructions

1. Open Developer PowerShell for Visual Studio

2. Clone this repository (either use `git clone` or download as `.zip`)

3. Go to repository root: `cd ksp2-papi`

4. Run `dotnet tool restore`

5. Run `dotnet cake [--target {Clear|Build|Pack|Install|Uninstall|Start}] [--configuration {Release|Debug}]`  
   Arguments enclosed in `[]` are optional.  
   The defaults when not provided are `--target Pack` and  `--configuration Release`.

## Bug Reports and Feature Requests

Found any bugs🦗? Have an idea to improve things💡? → [Open an issue on GitHub](https://github.com/Codenade/ksp2-papi/issues)
