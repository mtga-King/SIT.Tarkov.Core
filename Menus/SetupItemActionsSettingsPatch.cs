﻿using HarmonyLib;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SIT.A.Tarkov.Core.Menus
{
    internal class SetupItemActionsSettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = PatchConstants.EftTypes.Single(x => PatchConstants.GetMethodForType(x, "TrySendCommands") != null);
            return PatchConstants.GetMethodForType(t, "TrySendCommands");
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var listCountIndex = -1;
            var timeIndex = -1;

            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                // We are looking for the List Count that TrySendCommands checks before sending the next command
                if(listCountIndex == -1 && codes[i].opcode == OpCodes.Ldc_I4_S)
                {
                    listCountIndex = i;
                }
                if(timeIndex == -1 && codes[i].opcode == OpCodes.Ldc_R4)
                {
                    timeIndex = i;
                }
            }

            if (listCountIndex != -1)
            {
                codes.RemoveAt(listCountIndex);
                codes.Insert(listCountIndex, new CodeInstruction(OpCodes.Ldc_I4_S, 0));
            }

            return codes;
        }
    }
}
