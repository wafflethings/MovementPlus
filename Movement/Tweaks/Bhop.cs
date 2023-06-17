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
    // [TweakMetadata("Bhop", $"{Movement.GUID}.bhop", "Jumping activates when held.", $"{Movement.GUID}.movement_mod", 3, AllowCG: false)]
    public class Bhop : Tweak
    {
        private Harmony harmony = new($"{Movement.GUID}.bhop");

        public override void OnTweakEnabled()
        {
            base.OnTweakEnabled();
            harmony.PatchAll(typeof(BhopPatches));
        }

        public override void OnTweakDisabled()
        {
            base.OnTweakDisabled();
            harmony.UnpatchSelf();
        }

        public class BhopPatches
        {
            [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Update)), HarmonyPrefix]
            public static void CancelCancelling(NewMovement __instance)
            {
                if (__instance.activated)
                {
                    if (!GameStateManager.Instance.PlayerInputLocked && InputManager.Instance.InputSource.Jump.IsPressed && (!__instance.falling || __instance.gc.canJump || __instance.wc.CheckForEnemyCols()) && !__instance.jumpCooldown)
                    {
                        if (__instance.gc.canJump || __instance.wc.CheckForEnemyCols())
                        {
                            __instance.currentWallJumps = 0;
                            __instance.rocketJumps = 0;
                            __instance.clingFade = 0f;
                            __instance.rocketRides = 0;
                        }
                        __instance.Jump();
                    }
                }
            }
        }
    }
}
