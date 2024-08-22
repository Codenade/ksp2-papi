using System.IO;
using System.Reflection;

namespace ksp2_papi
{
    public class ModInfo
    {
        public readonly static string Id = "ksp2-papi";
        public readonly static string Name = "KSP2 PAPI";
        public readonly static string Abstract = "Adds runway PAPIs";
        public readonly static string Catalog = "addressables/catalog.json";
        public static string ModRootPath => _modRootPath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string CatalogPath => Path.Combine(ModRootPath, Catalog);
        public static string PapisJsonPath => Path.Combine(ModRootPath, "papis.json");

        private static string _modRootPath;
    }
}
