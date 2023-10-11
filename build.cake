#addin nuget:?package=Cake.Unity&version=0.9.0

var target = Argument("target", "Pack");
var configuration = Argument("configuration", "Release");
var ksp2Root = Argument("ksp2-root", string.Empty);

Task("Clean")
	.Does(() =>
	{
		CleanDirectory($"./build");
		Information("Cleaned ./build");
		DotNetClean("./ksp2-papi.csproj", new DotNetCleanSettings
		{
			Configuration = configuration,
			NoLogo = true,
			Verbosity = (DotNetVerbosity)1
		});
	});

Task("Build")
	.Does(() =>
	{
		Information("Create output directory");
		CreateDirectory("./build/BepInEx/plugins/ksp2-papi/");

		DotNetBuild("./ksp2-papi.csproj", new DotNetBuildSettings
		{
			Configuration = configuration,
			NoLogo = true
		});

		Information("Copying assembly build productis to output directory");
		CopyFileToDirectory($"./bin/{configuration}/netstandard2.0/ksp2-papi.dll", Directory("./build/BepInEx/plugins/ksp2-papi/"));
		if (configuration == "Debug")
			CopyFileToDirectory($"./bin/{configuration}/netstandard2.0/ksp2-papi.pdb", Directory("./build/BepInEx/plugins/ksp2-papi/"));

		// Find unity editor 2020.3.33f1
		Information("Trying to find Unity Editor version 2020.3.33f1");
		var unityEditor = FindUnityEditor(2020, 3, 33, 'f');
		if (unityEditor is null && !BuildSystem.GitHubActions.IsRunningOnGitHubActions)
		{
			throw new CakeException("Could not find Unity Editor 2020.3.33f1");
		}
		else if (BuildSystem.GitHubActions.IsRunningOnGitHubActions)
		{
			Information("Unity asset build already done by GitHub Actions workflow");
		}
		else
		{
			// Set unity log file location according to: https://docs.unity3d.com/Manual/LogFiles.html
			FilePath unityLogLocation = File("");
			if (IsRunningOnLinux()) unityLogLocation = File(@"~/.config/unity3d/Editor.log");
			if (IsRunningOnMacOs()) unityLogLocation = File(@"~/Library/Logs/Unity/Editor.log");
			if (IsRunningOnWindows()) unityLogLocation = ExpandEnvironmentVariables(File(@"%LOCALAPPDATA%\Unity\Editor\Editor.log"));
			
			Information("Building assets");
			UnityEditor(unityEditor, new UnityEditorArguments
			{
				BatchMode = true,
				NoGraphics = true,
				Quit = true,
				ProjectPath = MakeAbsolute(Directory("./ksp2-papi-assets")),
				ExecuteMethod = "BuildAssets.PerformBuild",
				LogFile = unityLogLocation
			}, new UnityEditorSettings
			{
				RealTimeLog = false
			});
		}
		Information("Copying assets to output directory");
		CopyDirectory("./ksp2-papi-assets/Library/com.unity.addressables/aa/windows", "./build/BepInEx/plugins/ksp2-papi/addressables");
	});

Task("Pack")
	.IsDependentOn("Build")
	.Does(() =>
	{
		CopyFile("./README.md", "./build/BepInEx/plugins/ksp2-papi/README.md");
		CopyFile("./LICENSE.txt", "./build/BepInEx/plugins/ksp2-papi/LICENSE.txt");
		Zip("./build", "build/ksp2-papi.zip", "./build/**/*");
	});
	

Task("Install")
	.IsDependentOn("Build")
	.Does(() =>
	{
		var gameDir = ksp2Root != string.Empty ? ksp2Root : EnvironmentVariable("KSP2_PATH") ?? throw new ArgumentNullException("ksp2-root");
		CopyDirectory("./build/", Directory(gameDir));
	});

Task("Uninstall")
	.Does(() =>
	{
		var gameDir = ksp2Root != string.Empty ? ksp2Root : EnvironmentVariable("KSP2_PATH") ?? throw new ArgumentNullException("ksp2-root");
		DeleteDirectory(Directory(gameDir) + Directory("BepInEx/plugins/ksp2-papi"), new DeleteDirectorySettings
		{
			Recursive = true
		});
	});

Task("Start")
	.IsDependentOn("Install")
	.Does(() =>
	{
		var gameDir = ksp2Root != string.Empty ? ksp2Root : EnvironmentVariable("KSP2_PATH") ?? throw new ArgumentNullException("ksp2-root");
		StartAndReturnProcess(Directory(gameDir) + File("KSP2_x64.exe"), new ProcessSettings
		{
			Arguments = ProcessArgumentBuilder.FromString("-single-instance"),
			WorkingDirectory = Directory(gameDir)
		});
	});

RunTarget(target);