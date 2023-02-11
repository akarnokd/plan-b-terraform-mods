// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Use this to name GameObjects with a mod-specific common prefix.
        /// </summary>
        /// <param name="subname"></param>
        /// <returns></returns>
        static string Naming(string subname)
        {
            return "FeatMultiplayer_" + subname;
        }
    }
}
