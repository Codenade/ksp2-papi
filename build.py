import os, subprocess, os.path, shutil, argparse

class h_styles:
    CYAN = '\033[96m'
    GREEN = '\033[92m'
    RED = '\033[91m'
    ENDCOLOR = '\033[0m'

def execute_build():
  parser = argparse.ArgumentParser()
  parser.add_argument("--unity_executable", required=False, type=str, default="C:/Program Files/Unity/Hub/Editor/2020.3.33f1/Editor/Unity.exe")
  parser.add_argument("--install", required=False, default=False, action="store_true")
  parser.add_argument("--start", required=False, default=False, action="store_true")
  parser.add_argument("--skip_addressables", required=False, default=False, action="store_true")
  args = parser.parse_args()
  shutil.rmtree(os.path.join(os.getcwd(), "build"), True)
  print("Starting assembly build")
  try:
    subprocess.run("dotnet build", stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT).check_returncode()
    print(h_styles.CYAN + "Assembly build finished" + h_styles.ENDCOLOR)
  except:
    error("Assembly build failed")
  if (not args.skip_addressables):
    print("Building assets")
    try:
      subprocess.run(executable=args.unity_executable, args="-projectPath \"" + os.getcwd() + "/ksp2-papi-assets/\" -quit -batchmode -executeMethod BuildAssets.PerformBuild", stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT).check_returncode()
      shutil.copytree("ksp2-papi-assets/Library/com.unity.addressables/aa/windows", "build/BepInEx/plugins/ksp2-papi/addressables", dirs_exist_ok=True)
      print(h_styles.CYAN + "Building assets finished" + h_styles.ENDCOLOR)
    except Exception as e:
      error("Building assets failed: " + e.__str__())
  print("Copying README.md and LICENSE.txt")
  shutil.copy("README.md", "build/BepInEx/plugins/ksp2-papi/README.md")
  shutil.copy("LICENSE.txt", "build/BepInEx/plugins/ksp2-papi/LICENSE.txt")
  print("Creating build.zip")
  try:
    shutil.make_archive("build/build", "zip", "build", "BepInEx")
  except:
    error("Could not create build.zip")
  print(h_styles.GREEN + "SUCCESS: Build finished" + h_styles.ENDCOLOR)
  if args.install or args.start:
    print("Installing to \'" + os.getenv("KSP2_PATH") + "\'")
    try:
      shutil.copytree(src="build", dst=os.getenv("KSP2_PATH"), ignore=shutil.ignore_patterns("*.zip"), dirs_exist_ok=True)
    except:
      error("Failed to install")
  if args.start:
    print("Starting KSP2")
    os.system("\"" + shutil.which(os.path.join(os.getenv("KSP2_PATH"), "KSP2_x64.exe")) + "\"")
  exit(0)

def error(msg: str):
  print(h_styles.RED + "FAILED: " + msg + h_styles.ENDCOLOR)
  exit(1)

if __name__ == "__main__":
  execute_build()