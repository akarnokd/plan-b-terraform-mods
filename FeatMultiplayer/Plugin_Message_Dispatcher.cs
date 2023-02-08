using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static readonly Dictionary<string, BaseMessage> messageRegistry = new Dictionary<string, BaseMessage>();

        void InitMessageDispatcher()
        {

        }

    }
}
