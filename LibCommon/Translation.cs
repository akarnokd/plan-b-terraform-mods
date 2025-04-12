// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using HarmonyLib;
using System;
using System.Collections.Generic;

namespace LibCommon
{
    /// <summary>
    /// Handle manipulating the game's translation data structures
    /// </summary>
    public static class Translation
    {

        static Dictionary<string, CSentence> ____dicoLoc;

        /// <summary>
        /// Returns a list of currently known language identifiers (GLoc.LanguageName.localizeDirect).
        /// </summary>
        /// <returns>The list of language identifiers.</returns>
        public static List<string> GetLanguageIds()
        {
            List<string> list = new();

            foreach (var ln in GLoc.langages)
            {
                list.Add(ln.localizeDirect);
            }
            return list;
        }

        /// <summary>
        /// Updates the given language's translations with the given dictionary. Does not create a new language translation by itself.
        /// Translations for non-existent keys are ignored. Make sure you call this method after new languages have been patched in.
        /// </summary>
        /// <param name="languageId">The target language id. See GLoc.langages; the id is the GLoc.LanguageName.localizeDirect field values.</param>
        /// <param name="translations">The dictionary from key codes to translations</param>
        /// <returns>True if successful, false if the particular language is not supported by the current install.</returns>
        public static bool UpdateTranslations(string languageId, Dictionary<string, string> translations)
        {
            if (____dicoLoc == null)
            {
                var field = AccessTools.Field(typeof(SLoc), "_dicoLoc");
                if (field == null)
                {
                    UnityEngine.Debug.LogError("Unable to locate SLoc._dicoLoc\n" + Environment.StackTrace);
                    return false;
                }
                ____dicoLoc = (Dictionary<string, CSentence>)field.GetValue(SSingleton<SLoc>.Inst);
                if (____dicoLoc == null)
                {
                    UnityEngine.Debug.LogError("SLoc._dicoLoc is null!\n" + Environment.StackTrace);
                    return false;
                }
            }

            CSentence languageIdString = ____dicoLoc["String Identifier"];
            int languageIndex = languageIdString.words.IndexOf(languageId);

            if (languageIndex == -1)
            {
                return false;
            }

            foreach (var kv in translations)
            {
                if (!____dicoLoc.TryGetValue(kv.Key, out var cs))
                {
                    cs = new();
                    cs.id = kv.Key;
                    ____dicoLoc.Add(kv.Key, cs);
                }
                // Pad out rows just in case not all columns are defined
                while (cs.words.Count < languageIdString.words.Count)
                {
                    cs.words.Add("");
                }
                cs.words[languageIndex] = kv.Value.Replace("\\n", "\n").Replace("\\t", "\t");
            }
            SLoc.Localize();

            return true;
        }
    }
}
