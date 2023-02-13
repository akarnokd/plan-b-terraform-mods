using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace MiscDebugOffline
{
    [BepInPlugin("akarnokd.planbterraformmods.miscdebugoffline", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ManualLogSource logger;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            logger = Logger;

            Hotfix();

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SSteam), "Init")]
        static void SSteam_Init(ref bool ____initiliazed, ref bool ____ownsFullGame, ref bool ____ownsDemo, ref bool ____ownsApp)
        {
            if (!____initiliazed)
            {
                ____initiliazed = true;
                ____ownsFullGame = true;
                ____ownsDemo = true;
                ____ownsApp = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSteam), nameof(SSteam.GetLangage))]
        static bool SSteam_GetLangage(bool ____initiliazed, ref GLoc.LanguageName __result)
        {
            if (!____initiliazed)
            {
                __result = GLoc.langages.Find((GLoc.LanguageName x) => x.steamAPI == "english");
                return false;
            }
            return true;
        } 

        static void Hotfix()
        {
            Assembly me = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(me.Location);

            string f = dir + "\\..\\..\\..\\Plan B Terraform_Data\\Plugins\\x86_64\\P" + "l" + "a" + "n" + "B" + "_" + "D" + "l" + "l" + "." + "d" + "l" + "l";
            string f2 = f + ".bak";

            string h0 = "48f868ddfdbebde27410875994af51b0";
            string h1 = "cc516da4d1ea9c876f589c22a87a1a0a";

            if (File.Exists(f))
            {
                logger.LogInfo("Found");
                byte[] bytes = File.ReadAllBytes(f);

                var md5 = MD5.Create();
                byte[] hash = md5.ComputeHash(bytes);
                string h = string.Join("", hash.Select(x => $"{x:X2}")).ToLower();

                if (h == h1)
                {
                    logger.LogInfo("    already patched");
                }
                else if (h == h0)
                {
                    logger.LogInfo("    needs patching");
                    byte[] old =
                    {
                        0x75, 0x12, 0x48, 0xC7,
                        0xC0, 0xFF, 0xFF, 0xFF,
                        0xFF, 0x2B, 0x05, 0xB2,
                        0x12, 0x01, 0x00, 0xE9,
                        0x0A, 0x09, 0x00, 0x00
                    };

                    var nmax = bytes.Length - old.Length;
                    for (int i = 0; i < nmax; i++)
                    {
                        bool notfound = false;
                        for (int j = 0; j < old.Length; j++)
                        {
                            byte a = bytes[i + j];
                            byte b = old[j];

                            if (a != b)
                            {
                                notfound = true;
                                break;
                            }
                        }
                        if (!notfound)
                        {
                            logger.LogInfo("    at 0x" + $"{i:X4}");

                            if (!File.Exists(f2))
                            {
                                logger.LogInfo("    backed up");
                                File.WriteAllBytes(f2, bytes);
                            }

                            for (int j = 0; j < old.Length; j++)
                            {
                                bytes[i + j] = 0x90;
                            }
                            logger.LogInfo("    patching");
                            File.WriteAllBytes(f, bytes);
                            break;
                        }
                    }
                }
                else
                {
                    logger.LogWarning("    original changed");
                }

                logger.LogInfo("    Done.");
            }
            else
            {
                logger.LogInfo("Not found");
            }
        }
    }
}
