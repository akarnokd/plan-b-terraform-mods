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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void SLoc_Load()
        {
            Translation.UpdateTranslations("English", new()
            {
                // Main Menu Panel
                { "FeatMultiplayer.HostConfig", "<b>==== Host configuration ====</b>" },
                { "FeatMultiplayer.ClientConfig", "<b>==== Client configuration ====</b>" },
                { "FeatMultiplayer.HostMode", "[{0}] Host a game?" },
                { "FeatMultiplayer.UseUPnP", "[{0}] Use UPnP port forwarding?" },
                { "FeatMultiplayer.HostIP", "    Host IP = {0}:{1}" },
                { "FeatMultiplayer.UPnPStatus", "    Status = {0}" },
                { "FeatMultiplayer.UPnPAddress", "    Public IP = {0}" },
                { "FeatMultiplayer.ClientIP", "    Target IP = {0}:{1}" },
                { "FeatMultiplayer.ClientAs", "Join as <i>< {0} ></i>" },
                { "FeatMultiplayer.Error_AlreadyLoggedIn", "User already logged in!" },
                { "FeatMultiplayer.Error_MaxClients", "Maximum number of clients reached. Sorry." },
                { "FeatMultiplayer.Error_InvalidUserOrPassword", "Invalid username or password." },
            });

            Translation.UpdateTranslations("Hungarian", new()
            {
                // Main Menu Panel
                { "FeatMultiplayer.HostConfig", "<b>==== Gazda beállítások ====</b>" },
                { "FeatMultiplayer.ClientConfig", "<b>==== Vendég beállítások ====</b>" },
                { "FeatMultiplayer.HostMode", "[{0}] Játék házigazdaként?" },
                { "FeatMultiplayer.UseUPnP", "[{0}] UPnP port továbbítás?" },
                { "FeatMultiplayer.HostIP", "    Gazda IP = {0}:{1}" },
                { "FeatMultiplayer.UPnPStatus", "    Állapot = {0}" },
                { "FeatMultiplayer.UPnPAddress", "    Nyilvános IP = {0}" },
                { "FeatMultiplayer.ClientIP", "    Cél IP = {0}:{1}" },
                { "FeatMultiplayer.ClientAs", "Csatlakozás, mint <i>< {0} ></i>" },
                { "FeatMultiplayer.Error_AlreadyLoggedIn", "A felhasználó már egyszer bejelentkezett!" },
                { "FeatMultiplayer.Error_MaxClients", "Megtelt a szerver. Sajnálom." },
                { "FeatMultiplayer.Error_InvalidUserOrPassword", "Érvénytelen felhasználó vagy jelszó." },
            });
        }
    }
}
