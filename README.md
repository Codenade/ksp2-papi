# ksp2-papi

Mod that adds [PAPI (Precision Approach Path Indicator)](https://en.wikipedia.org/wiki/Precision_approach_path_indicator) lighting to Kerbal Space Program 2's runways.

> [!IMPORTANT]
> __If you are experiencing performance issues__, you can set the config option `use_pixel_counting` to `false` (or _No_ in SpaceWarp).
>   
> You can modify the mod settings using:
> * [SpaceWarp](https://github.com/SpaceWarpDev/SpaceWarp)
> * [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager/blob/master/README.md)
> * Manually by editing `BepInEx/config/ksp2-papi.cfg`
>   
> This will disable the pixel counting and fall back to unity's physics raycasting. When disabled however the mod will not account KSP2's distant terrain as it does not have colliders so you will see the flares through the terrain if you're far away from it.

# Installation

### Use [CKAN](https://forum.kerbalspaceprogram.com/topic/197082-ckan-the-comprehensive-kerbal-archive-network-v1332-laplace-ksp-2-support/) _(recommended)_

* This is the recommended way of installing KSP2 mods as it automatically installs dependencies.

### Manual Installation

1. Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) or [SpaceWarp](https://github.com/SpaceWarpDev/SpaceWarp)

2. Extract the contents of ksp2-papi*.zip into KSP2's root directory or copy `BepInEx/plugins/ksp2-papi` into `Kerbal Space Program 2/BepInEx/plugins`

# Build from source

### Required software

* [.Net SDK](https://dotnet.microsoft.com/en-us/download)
* [Unity 2022.3.5](https://unity.com/releases/editor/archive)

### Build instructions

1. Open Windows PowerShell

2. Clone this repository (either use `git clone` or download as `.zip`)

3. Go to repository root: `cd ksp2-papi`

4. Run `dotnet tool restore`

5. Run `dotnet cake [--target {Clear|Build|Pack|Install|Uninstall|Start}] [--configuration {Release|Debug}] [--ksp2-root <path>] [--skip-unity]`  
   Arguments enclosed in `[]` are optional.  
   The defaults when not provided are `--target Pack` and  `--configuration Release`.

   * `--target` Select one of the build targets defined in `build.cake`.
   * `--configuration` Select build configuration. 'Debug' will copy debug symbols to the output.
   * `--ksp2-root` Tagets 'Install', 'Uninstall' and 'Start' need to know where your installation of KSP2 is located. If you don't want to set 'KSP2_PATH' in your environment to point to KSP2's installation directory you can specify it here.
   * `--skip-unity` You may specify this option if you want to skip building the assets and just copy the ones from a previous asset build.

   Note: Targets 'Install', 'Uninstall' and 'Start' will not install the dependencies.

# Bug Reports and Feature Requests

Found any bugs🦗? Have an idea to improve things💡? → [Open an issue on GitHub](https://github.com/Codenade/ksp2-papi/issues)
