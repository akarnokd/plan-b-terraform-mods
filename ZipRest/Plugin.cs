using BepInEx;

namespace ZipRest
{
    [BepInPlugin("akarnokd.planbterraformmods.ziprest", "Zip the other mods", "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            // Harmony.CreateAndPatchAll(typeof(Plugin));
        }
    }
}
