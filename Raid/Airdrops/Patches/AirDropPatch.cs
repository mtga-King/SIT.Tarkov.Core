﻿using Comfort.Common;
using EFT;
using EFT.Airdrop;
using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;
/***
 * Full Credit for this patch goes to SPT-Aki team. Specifically CWX!
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules. 
*/
namespace Aki.Custom.Airdrops.Patches
{
    public class AirdropPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod("OnGameStarted", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var points = LocationScene.GetAll<AirdropPoint>().Any();

            if (gameWorld != null && points)
            {
                gameWorld.gameObject.AddComponent<AirdropsManager>();
            }
        }
    }
}