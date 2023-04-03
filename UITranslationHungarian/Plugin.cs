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

namespace UITranslationHungarian
{
    [BepInPlugin("akarnokd.planbterraformmods.uitranslationhungarian", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        static ManualLogSource logger;

        static ConfigEntry<bool> dumpLabels;

        static string languageId = "Hungarian";
        static string englishName = "Hungarian";
        static string localizedName = "Magyar";
        static string steamName = "hungarian";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            dumpLabels = Config.Bind("General", "DumpLabels", false, "Dump all labels of all supported languages?");

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
            DumpLabels(____dicoLoc);

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
            logger.LogInfo("  Language matrix updated.");
        }

        private static void DumpLabels(Dictionary<string, CSentence> ____dicoLoc)
        {
            if (dumpLabels.Value)
            {
                logger.LogInfo("Localization dump");
                Assembly me = Assembly.GetExecutingAssembly();
                string dir = Path.GetDirectoryName(me.Location);

                CSentence csentence = ____dicoLoc["String Identifier"];

                // logger.LogInfo(string.Join(";", csentence.words));
                for (int i = 1; i < csentence.words.Count; i++)
                {
                    // Categories titles;String Identifier;English;Description;Translator questions and answers;French (France);...
                    if (i == 0 || i == 1 || i == 3 || i == 4)
                    {
                        continue;
                    }

                    var langName = csentence.words[i];
                    List<string> properties = new();
                    foreach (var kv in ____dicoLoc)
                    {
                        properties.Add(kv.Key + "=" + kv.Value.words[i].Replace("\n", "\\n"));
                    }
                    string fileName = Path.Combine(dir, "labels-" + langName + ".txt");
                    logger.LogInfo("  Dumping " + langName + " into " + fileName);
                    File.WriteAllLines(fileName, properties, Encoding.UTF8);
                }
            }
        }
    }
}
