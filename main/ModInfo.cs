using System.IO;
using System.Reflection;

namespace ksp2_papi
{
    public static class ModInfo
    {
        public const string Id = "ksp2-papi";
        public const string Name = "KSP2 PAPI";
        public const string Abstract = "Adds runway PAPIs";
        public const string Catalog = "addressables/catalog.json";
        public static string ModRootPath => _modRootPath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string CatalogPath => Path.Combine(ModRootPath, Catalog);
        public static string PapisJsonPath => Path.Combine(ModRootPath, "papis.json");

        private static string _modRootPath;
    }
}
