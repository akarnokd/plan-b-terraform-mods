using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using static GLoc;

namespace UITranslationTChinese
{
    [BepInPlugin("akarnokd.planbterraformmods.uitranslationtchinese", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        static ManualLogSource logger;

        static string languageId = "TChinese";
        static string englishName = "Chinese (Traditional)";
        static string localizedName = "繁體中文";
        static string steamName = "tchinese";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            // Patch in the new language option

            var languageIndex = -1;
            for (int i = 0; i < GLoc.langages.Count; i++)
            {
                GLoc.LanguageName e = GLoc.langages[i];
                if (e.localizeDirect == languageId)
                {
                    languageIndex = i;
                    break;
                }
            }

            if (languageIndex == -1)
            {
                GLoc.langages.Add(new GLoc.LanguageName(englishName, localizedName, steamName, languageId));
            }


            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void SLoc_Load(Dictionary<string, CSentence> ____dicoLoc)
        {
            logger.LogInfo("Applying language " + languageId);

            logger.LogInfo("  Checking the translation matrix");
            CSentence csentence = ____dicoLoc["String Identifier"];
            int languageIndex = csentence.words.IndexOf(languageId);

            if (languageIndex == -1)
            {
                languageIndex = csentence.words.Count;

                logger.LogInfo("    Expanding the language matrix with column " + languageIndex);
                foreach (var cs in ____dicoLoc.Values)
                {
                    while (cs.words.Count <= languageIndex)
                    {
                        cs.words.Add("");
                    }
                }
            }

            // Get the translation file
            Assembly me = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(me.Location);
            string file = Path.Combine(dir, "labels-" + languageId + ".txt");

            logger.LogInfo("  Loading translation file " + file);

            var lines = File.ReadAllLines(file, Encoding.UTF8);

            foreach (var line in lines)
            {
                // skip comments and lines without equals sign
                if (line.StartsWith("#") || !line.Contains("="))
                {
                    continue;
                }

                var first = line.IndexOf("=");

                var lkey = line.Substring(0, first);
                var lvalue = line.Substring(first + 1);

                if (____dicoLoc.TryGetValue(lkey, out var cs))
                {
                    cs.words[languageIndex] = lvalue.Replace("\\n", "\n").Replace("\\t", "\t");
                    cs.CheckValidity();
                }
            }
            SLoc.Localize();
            logger.LogInfo("  Language matrix updated.");
        }
    }
}
