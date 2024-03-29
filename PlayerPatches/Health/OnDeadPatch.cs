﻿using EFT;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.PlayerPatches.Health
{
    public class OnDeadPatch : ModulePatch
    {
        public static event Action<EFT.Player, EDamageType> OnPersonKilled;
        public static bool DisplayDeathMessage = true;

        public OnDeadPatch(BepInEx.Configuration.ConfigFile config)
        {
            var enableDeathMessage = config.Bind("Bundles", "Enable", true);
            if (enableDeathMessage != null && enableDeathMessage.Value == true)
            {
                DisplayDeathMessage = enableDeathMessage.Value;
                
                //DisplayDeathMessage = JsonConvert.DeserializeObject<bool>();
            }

            if(bool.TryParse(new Request().PostJson("/client/raid/person/killed/showMessage", null, true), out bool serverDecision))
            {
                Logger.LogInfo("OnDeadPatch:Server Decision:" + serverDecision);
                DisplayDeathMessage = serverDecision;
            }
        }

        protected override MethodBase GetTargetMethod() => PatchConstants.GetMethodForType(typeof(Player), "OnDead");

        [PatchPostfix]
        public static void PatchPostfix(EFT.Player __instance, EDamageType damageType)
        {
            Player deadPlayer = __instance;
            if (deadPlayer == null)
                return;

            if(OnPersonKilled != null)
            {
                OnPersonKilled(__instance, damageType);
            }

            var killedBy = PatchConstants.GetFieldOrPropertyFromInstance<Player>(deadPlayer, "LastAggressor", false);
            if (killedBy == null)
                return;

            var killedByLastAggressor = PatchConstants.GetFieldOrPropertyFromInstance<Player>(killedBy, "LastAggressor", false);
            if (killedByLastAggressor == null)
                return;

            if (DisplayDeathMessage)
            {
                if (killedBy != null)
                    PatchConstants.DisplayMessageNotification($"{killedBy.Profile.Info.Nickname} killed {deadPlayer.Profile.Nickname}");
                else
                    PatchConstants.DisplayMessageNotification($"{deadPlayer.Profile.Nickname} has died by {damageType}");
            }

            Dictionary<string, object> map = new Dictionary<string, object>();
            map.Add("diedAID", __instance.Profile.AccountId);
            if (__instance.Profile.Info != null)
            {
                map.Add("diedFaction", __instance.Side);
                if(__instance.Profile.Info.Settings != null)
                    map.Add("diedWST", __instance.Profile.Info.Settings.Role);
            }
            if (killedBy != null) 
            {
                map.Add("killedByAID", killedBy.Profile.AccountId);
                map.Add("killerFaction", killedBy.Side);
            }
            if(killedByLastAggressor != null)
            {
                map.Add("killedByLastAggressorAID", killedByLastAggressor.Profile.AccountId);
            }
            _ = new Request().PostJsonAsync("/client/raid/person/killed", JsonConvert.SerializeObject(map));
        }
    }
}
