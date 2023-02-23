// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;

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

            session.planetDataSync = GPlanet.dailyTemperature.Count;
        }

        static void FullSync<T>(ClientSession sess) where T : MessageSync, new()
        {
            LogDebug("FullSync: " + typeof(T) + " to " + sess.clientName);
            var msg = new T();
            msg.GetSnapshot();
            sess.Send(msg);
        }
    }
}
