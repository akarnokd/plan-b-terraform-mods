using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.REPL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FeatLuaModManager
{
    [BepInPlugin("akarnokd.planbterraformmods.featluamodmanager", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ConfigEntry<bool> modEnabled;

        static ManualLogSource logger;

        static Script _script;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");

            logger = Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLua), nameof(SLua.Init))]
        static void SLua_Init(Script ____script)
        {
            _script = ____script;

            SetupFunctions();
            RunLua();
        }

        static void SetupFunctions()
        {
            UserData.RegisterType(typeof(CItem), InteropAccessMode.Default, null);

            _script.Globals["AddCItem"] = new Func<DynValue, object>(AddCItem);
            _script.Globals["GetCItem"] = new Func<string, object>(GetCItem);
        }

        static void RunLua()
        {
            var previousScriptLoader = _script.Options.ScriptLoader;
            try
            {
                var path = Application.persistentDataPath + "\\mods\\local";
                logger.LogInfo("Looking for local mods in " + path);
                if (!Directory.Exists(path))
                {
                    logger.LogInfo("   Creating mod dir the first time");
                    Directory.CreateDirectory(path);
                }
                logger.LogInfo("Enumerating mods...");
                int count = 0;
                foreach (var dir in Directory.GetDirectories(path))
                {
                    string metafile = Path.Combine(dir, "metadata.json");

                    if (File.Exists(metafile))
                    {
                        count++;
                        // TODO analyse metafile
                        logger.LogInfo("   Found " + Path.GetDirectoryName(metafile));

                        var repl = new ReplInterpreterScriptLoader();
                        repl.ModulePaths = new string[] { dir + "/?.lua" };
                        repl.IgnoreLuaPathGlobal = true;
                        _script.Options.ScriptLoader = repl;

                        string mainFile = Path.Combine(dir, "main.lua");
                        logger.LogInfo("      Executing " + Path.GetFileName(mainFile));
                        try
                        {
                            _script.DoFile(mainFile);
                            logger.LogInfo("      Success " + Path.GetFileName(mainFile));
                        }
                        catch (ScriptRuntimeException ex)
                        {
                            logger.LogError("Error running " + mainFile + "\r\n" + ex);
                        }
                    }
                    else
                    {
                        logger.LogInfo("Not found: " + metafile);
                    }
                }
                if (count == 0)
                {
                    logger.LogInfo("No mod files found.");
                }
            }
            finally
            {
                _script.Options.ScriptLoader = previousScriptLoader;
            }
        }

        static object AddCItem(DynValue luaTable)
        {
            string @string = _script.Globals.Pairs.First((TablePair x) => x.Value.Equals(luaTable)).Key.String;

            CItem cluaEntity = (CItem)luaTable.ToObject();
            cluaEntity.id = (byte)GItems.items.Count;
            cluaEntity.codeListName = @string;

            GItems.items.Add(cluaEntity);

            return cluaEntity;
        }

        static object GetCItem(string codeName)
        {
            return GItems.items.Find(v => v != null && v.codeName == codeName);
        }

        static object AddCLevel(DynValue luaTable)
        {
            string @string = _script.Globals.Pairs.First((TablePair x) => x.Value.Equals(luaTable)).Key.String;

            CLevel cluaEntity = (CLevel)luaTable.ToObject();
            cluaEntity.id = (byte)GItems.items.Count;
            cluaEntity.codeListName = @string;

            GGame.levels.Add(cluaEntity);

            return cluaEntity;
        }
    }
}