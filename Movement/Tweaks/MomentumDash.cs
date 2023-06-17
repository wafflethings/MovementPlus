using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UltraTweaker.Handlers;
using UltraTweaker.Tweaks;
using UnityEngine;

namespace Movement.Tweaks
{
    [TweakMetadata("Momentum Dash", $"{Movement.GUID}.momentum_dash", "Dashes don't decrease speed at the end.", $"{Movement.GUID}.movement_mod", 3, AllowCG: false)]
    public class MomentumDash : Tweak
    {
        private Harmony harmony = new($"{Movement.GUID}.momentum_dash");

        public override void OnTweakEnabled()
        {
            base.OnTweakEnabled();
            harmony.PatchAll(typeof(DashPatches));
        }

        public override void OnTweakDisabled()
        {
            base.OnTweakDisabled();
            harmony.UnpatchSelf();
        }

        public class DashPatches
        {
            [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Update)), HarmonyPrefix]
            public static bool CancelCancelling(NewMovement __instance)
            {
                if (__instance.boostLeft - 4 >= 0 && !__instance.gc.touchingGround)
                {
                    __instance.boostLeft = 0;
                    __instance.boost = false;
                    return false;
                }

                return true;
            }
        }
    }
}
