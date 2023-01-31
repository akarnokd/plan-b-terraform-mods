using BepInEx;

namespace LibCommon
{
    [BepInPlugin("akarnokd.planbterraformmods.libcommon", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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
