// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Represents the current multiplayer mode.
        /// </summary>
        public static volatile MultiplayerMode multiplayerMode = MultiplayerMode.None;

        
    }

    public enum MultiplayerMode
    {
        /// <summary>
        /// Game hasn't reached the main menu yet.
        /// </summary>
        None,
        /// <summary>
        /// Game is in the main menu.
        /// </summary>
        MainMenu,
        /// <summary>
        /// Game was entered as single-player.
        /// </summary>
        SinglePlayer,
        /// <summary>
        /// The cost is currently loading a world but hasn't enabled joining yet.
        /// </summary>
        HostLoading,
        /// <summary>
        /// Game is running, joining is possible
        /// </summary>
        Host,
        /// <summary>
        /// Client is in the login and initial sync phase.
        /// </summary>
        ClientJoin,
        /// <summary>
        /// Game is running.
        /// </summary>
        Client
    }
}
