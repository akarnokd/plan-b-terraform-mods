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
        static void FullSync(ClientSession session)
        {
            FullSync<MessageSyncAllFlags>(session);

            FullSync<MessageSyncAllAltitude>(session);

            FullSync<MessageSyncAllWater>(session);

            FullSync<MessageSyncAllContentId>(session);

            FullSync<MessageSyncAllContentData>(session);

            FullSync<MessageSyncAllGroundId>(session);

            FullSync<MessageSyncAllGroundData>(session);

            FullSync<MessageSyncAllMain>(session);

            FullSync<MessageSyncAllGame>(session);

            FullSync<MessageSyncAllPlanet>(session);

            FullSync<MessageSyncAllItems>(session);

            FullSync<MessageSyncAllWaterInfo>(session);

            FullSync<MessageSyncAllDrones>(session);

            FullSync<MessageSyncAllWays>(session);

            FullSync<MessageSyncAllCamera>(session);
        }

        static void FullSync<T>(ClientSession sess) where T : MessageSync, new()
        {
            var msg = new T();
            msg.GetSnapshot();
            sess.Send(msg);
        }
    }
}
