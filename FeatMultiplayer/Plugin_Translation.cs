﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;
using LibCommon;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SLoc), nameof(SLoc.Load))]
        static void Patch_SLoc_Load()
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
                { "FeatMultiplayer.Error_ConnectionRefused", "Connection to the host refused!" },
                { "FeatMultiplayer.Error_AlreadyLoggedIn", "User already logged in!" },
                { "FeatMultiplayer.Error_MaxClients", "Maximum number of clients reached. Sorry." },
                { "FeatMultiplayer.Error_InvalidUserOrPassword", "Invalid username or password." },
                { "FeatMultiplayer.Error_LoginTimeout", "Login attempt timed out." },
                { "FeatMultiplayer.NetworkButton.Host.Title", "Hosting the game" },
                { "FeatMultiplayer.NetworkButton.Host.Desc", "You are hosting this game. There are {0} clients." },
                { "FeatMultiplayer.NetworkButton.Client.Title", "You have joined a game" },
                { "FeatMultiplayer.NetworkButton.Client.Desc", "Speed controls and saves are disabled." },
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
                { "FeatMultiplayer.Error_ConnectionRefused", "Nem lehet csatlakozni a házigazdához!" },
                { "FeatMultiplayer.Error_AlreadyLoggedIn", "A felhasználó már egyszer bejelentkezett!" },
                { "FeatMultiplayer.Error_MaxClients", "Megtelt a szerver. Sajnálom." },
                { "FeatMultiplayer.Error_InvalidUserOrPassword", "Érvénytelen felhasználó vagy jelszó." },
                { "FeatMultiplayer.Error_LoginTimeout", "A bejelentkezési kísérlet időtúllépési hibát eredményezett." },
                { "FeatMultiplayer.NetworkButton.Host.Title", "Házigazda" },
                { "FeatMultiplayer.NetworkButton.Host.Desc", "Megosztottad ezt a játékot. Vendégek száma: {0}." },
                { "FeatMultiplayer.NetworkButton.Client.Title", "Csatlakoztál egy játékhoz" },
                { "FeatMultiplayer.NetworkButton.Client.Desc", "A sebesség-állítás és mentés le van tiltva." },
            });
        }
    }
}
