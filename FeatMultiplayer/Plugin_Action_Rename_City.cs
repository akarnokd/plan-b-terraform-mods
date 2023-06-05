// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using HarmonyLib;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SSceneHud_Selection), "OnClick_Button")]
        static bool Patch_SSceneHud_Selection_OnClick_Button()
        {
            if (multiplayerMode != MultiplayerMode.SinglePlayer)
            {
                CCity city = SSingleton<SGame>.Inst.GetCity(GScene3D.selectionCoords);
                if (city != null)
                {
                    SSceneSingleton<SSceneUIOverlay>.Inst.popup.Show(SLoc.Get("Selection_City_RenamePopup",
                        new object[] { city.name }), "COMMON_CONFIRM", "COMMON_CANCEL", delegate
                    {
                        city.name = SSceneSingleton<SSceneUIOverlay>.Inst.popup.inputField.text;
                        SignalCityRenamed(city);

                    }, city.name, false);
                }
                return false;
            }
            return true;
        }

        static void SignalCityRenamed(CCity city)
        {
            LogDebug("City " + city.cityId + " renamed to " + city.name);
            var msg = new MessageRenameCity();
            msg.id = city.cityId;
            msg.name = city.name;

            if (multiplayerMode == MultiplayerMode.Host)
            {
                SendAllClients(msg);
            }
            else
            {
                SendHost(msg);
            }
        }

        static void ReceiveMessageRenameCity(MessageRenameCity msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageRenameCity: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else
            {
                LogDebug("ReceiveMessageRenameCity: Handling " + msg.GetType());
                var city = GGame.cities.Find(v => v != null && v.cityId == msg.id);
                if (city != null)
                {
                    city.name = msg.name;
                    if (multiplayerMode == MultiplayerMode.Host)
                    {
                        SendAllClients(msg);
                    }
                }
                else
                {
                    LogWarning("ReceiveMessageUpdateCity: City not found. id = " + msg.id);
                }
            }
        }
    }
}
