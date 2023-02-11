using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LibCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Reflection;
using UnityEngine;
using static LibCommon.GUITools;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CLine), nameof(CLine.Inverse))]
        static bool Patch_CLine_Inverse_Pre(CLine __instance)
        {
            if (multiplayerMode == MultiplayerMode.Client)
            {
                var msg = new MessageActionReverseLine();
                msg.lineId = __instance.id;
                SendHost(msg);
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CLine), nameof(CLine.Inverse))]
        static void Patch_CLine_Inverse_Post(CLine __instance)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                var msg = new MessageUpdateLine();
                msg.GetSnapshot(__instance, false);
                SendAllClients(msg);
            }
        }

        static void ReceiveMessageActionReverseLine(MessageActionReverseLine msg)
        {
            if (multiplayerMode == MultiplayerMode.Host)
            {
                LogDebug("ReceiveMessageActionReverseLine: Handling " + msg.GetType());

                for (int i = 1; i < GWays.lines.Count; i++)
                {
                    CLine line = GWays.lines[i];
                    if (line.id == msg.lineId)
                    {
                        line.Inverse();

                        break;
                    }
                }
            }
            else
            {
                LogWarning("ReceiveMessageActionReverseLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }

        static void ReceiveMessageUpdateLine(MessageUpdateLine msg)
        {
            if (multiplayerMode == MultiplayerMode.ClientJoin)
            {
                LogDebug("ReceiveMessageActionReverseLine: Deferring " + msg.GetType());
                deferredMessages.Enqueue(msg);
            }
            else if (multiplayerMode == MultiplayerMode.Client)
            {
                LogDebug("ReceiveMessageActionReverseLine: Handling " + msg.GetType());

                for (int i = 1; i < GWays.lines.Count; i++)
                {
                    CLine cline = GWays.lines[i];
                    if (cline.id == msg.line.id)
                    {
                        msg.ApplySnapshot(cline);

                        cline.UpdateStopDataOrginEnd(true, false);
                        cline.ComputePath_Positions(msg.computePath);

                        return;
                    }
                }

                // create a new line
                var line = new CLine(msg.line.stops[0].coords);
                msg.ApplySnapshot(line);
                GWays.lines.Add(line);
            }
            else
            {
                LogWarning("ReceiveMessageActionReverseLine: wrong multiplayerMode: " + multiplayerMode);
            }
        }
    }
}
