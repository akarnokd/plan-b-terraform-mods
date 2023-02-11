// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using System.Collections.Generic;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// The client name joining a game. Used for logging into dedicated files.
        /// </summary>
        static volatile string clientName = "";

        static string clientPassword = "";

        static readonly Dictionary<string, ClientSession> loggedInClients = new();

        static void SessionTerminate(ClientSession sess)
        {
            sess.disconnected = true;
            if (sess.clientName != null)
            {
                if (loggedInClients.Remove(sess.clientName))
                {
                    // notify
                }
            }
            sessions.Remove(sess.id);
        }

        static void ReceiveMessageLogin(MessageLogin ml)
        {
            if (multiplayerMode != MultiplayerMode.Host)
            {
                return;
            }

            if (loggedInClients.Count == maxClients.Value)
            {
                LogInfo("User " + ml.userName + " beyond capacity");

                var response = new MessageLoginResponse();
                response.reason = "Error_MaxClients";
                ml.sender.Send(response);
                ml.sender.Send(MessageDisconnect.Instance);
                return;
            }

            if (loggedInClients.ContainsKey(ml.userName))
            {
                LogInfo("User " + ml.userName + " already logged in");

                var response = new MessageLoginResponse();
                response.reason = "Error_AlreadyLoggedIn";
                ml.sender.Send(response);
                ml.sender.Send(MessageDisconnect.Instance);
                return;
            }

            if (hostUsers.TryGetValue(ml.userName, out var pass))
            {
                if (pass == ml.password)
                {
                    LogInfo("User " + ml.userName + " logged in successfully");
                    ml.sender.loginSuccess = true;
                    loggedInClients.Add(ml.userName, ml.sender);
                    ml.sender.clientName = ml.userName;

                    var response = new MessageLoginResponse();
                    response.reason = "Welcome";
                    ml.sender.Send(response);

                    FullSync(ml.sender);
                }
                else
                {
                    LogInfo("User " + ml.userName + " provided invalid password");

                    var response = new MessageLoginResponse();
                    response.reason = "Error_InvalidUserOrPassword";
                    ml.sender.Send(response);
                    ml.sender.Send(MessageDisconnect.Instance);
                }
            }
            else
            {
                LogInfo("User " + ml.userName + " not found");

                var response = new MessageLoginResponse();
                response.reason = "Error_InvalidUserOrPassword";
                ml.sender.Send(response);
                ml.sender.Send(MessageDisconnect.Instance);
            }
        }
    }
}
