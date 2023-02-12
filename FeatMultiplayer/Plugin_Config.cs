// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static ConfigEntry<bool> modEnabled;

        static ConfigEntry<bool> hostMode;
        static ConfigEntry<bool> useUPnP;

        static ConfigEntry<string> hostServiceAddress;
        static ConfigEntry<string> clientConnectAddress;

        static ConfigEntry<int> hostPort;
        static ConfigEntry<int> clientPort;

        static ConfigEntry<string> hostUserAndPasswords;
        static ConfigEntry<string> clientUserAndPasswords;

        static ConfigEntry<int> hostLogLevel;
        static ConfigEntry<int> clientLogLevel;

        static ConfigEntry<int> maxClients;

        static ConfigEntry<int> fontSize;

        static readonly Dictionary<string, string> hostUsers = new();
        
        static readonly Dictionary<string, string> clientUsers = new();

        static ConfigEntry<int> networkButtonLeft;
        static ConfigEntry<int> networkButtonSize;

        static ConfigEntry<bool> autoScale;

        void InitConfig()
        {
            modEnabled = Cfg("General", "Enabled", true, "Is the mod enabled?");
            fontSize = Cfg("General", "FontSize", 30, "The font size used in menus and dialogs of this mod.");

            hostMode = Cfg("Host", "Active", false, "Should loading a game host it for multiplayer as well?");
            useUPnP = Cfg("Host", "UseUPnP", false, "Use UPnP to automatically setup port forwarding on compatible networks/devices?");

            hostServiceAddress = Cfg("Host", "ServiceAddress", "", "The IP address of the adapter to use when hosting, in case of a multi-adapter system. Empty = auto");
            clientConnectAddress = Cfg("Client", "ConnectAddress", "", "The IP address of the host to connect to.");

            hostPort = Cfg("Host", "Port", 23208, "The port number where the hosting will happen.");
            clientPort = Cfg("Client", "Port", 23208, "The port number where the hosting is.");

            hostUserAndPasswords = Cfg("Host", "Users", "buddy:buddysPassword,dude:dudesPassword", "The comma separated list of user:password pairs the host will let in a multiplayer game. Spaces ignored.");
            clientUserAndPasswords = Cfg("Client", "Users", "buddy:buddysPassword,dude:dudesPassword", "The comma separated list of user:password pairs to connect to a game as a client. Spaces ignored.");

            hostLogLevel = Cfg("Host", "LogLevel", 1, "0 = Debug+, 1 = Info+, 2 = Warning+, 3 = Error+, 4 = Fatal");
            clientLogLevel = Cfg("Client", "LogLevel", 1, "0 = Debug+, 1 = Info+, 2 = Warning+, 3 = Error+, 4 = Fatal");

            maxClients = Cfg("Host", "MaxClients", 3, "Number of distinct clients that can join a session");

            ParseUsers(hostUserAndPasswords.Value, hostUsers);
            ParseUsers(clientUserAndPasswords.Value, clientUsers);

            networkButtonLeft = Cfg("GUI", "NetworkButtonLeft", 250, "Position of the network button on the top-left");
            networkButtonSize = Cfg("GUI", "NetworkButtonSize", 50, "Size of the network button");
            autoScale = Cfg("GUI", "AutoScale", true, "Automatically scale the GUIs?");
        }

        private ConfigEntry<T> Cfg<T>(string group, string name, T value, string desc)
        {
            return Config.Bind(group, name, value, desc);
        }

        private void ParseUsers(string text, Dictionary<string, string> dict)
        {
            text = text.Trim();
            var entries = text.Split(',');
            foreach (var entry in entries)
            {
                var entryTrimmed = entry.Trim();

                var userPass = entryTrimmed.Split(':');

                if (userPass.Length == 2)
                {
                    dict[userPass[0].Trim()] = userPass[1].Trim();
                }
                else
                {
                    Logger.LogError("Invalid user:password format: " + entryTrimmed);
                }
            }
        }
    }
}
