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
    // [TweakMetadata("Wall Run", $"{Movement.GUID}.wall_run", "Run on walls.", $"{Movement.GUID}.movement_mod", 3, AllowCG: false)]
    public class WallRun : Tweak
    {
        private Harmony harmony = new($"{Movement.GUID}.wall_run");

        public override void OnTweakEnabled()
        {
            base.OnTweakEnabled();
            harmony.PatchAll(typeof(WallPatches));
        }

        public override void OnTweakDisabled()
        {
            base.OnTweakDisabled();
            harmony.UnpatchSelf();
        }

        public class WallPatches
        {
            private static float startWj;
            private static float startSpeed;
            private static bool isRunning;
            private static float downPull;
            private static float time;

            [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Start)), HarmonyPrefix]
            public static void SetWj(NewMovement __instance)
            {
                startSpeed = __instance.walkSpeed;
                startWj = __instance.wallJumpPower;
            }

            [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Update)), HarmonyPrefix]
            public static void AddUpForce(NewMovement __instance)
            {
                isRunning = __instance.wc.CheckForCols() && !__instance.gc.onGround;

                if (isRunning)
                {
                    __instance.walkSpeed = 0;
                    __instance.movementDirection = Vector3.zero;
                    __instance.wallJumpPower = startWj * 1.125f;
                    __instance.rb.velocity = (__instance.transform.forward * 25 * Time.deltaTime) + new Vector3(__instance.rb.velocity.x, Mathf.Clamp(__instance.rb.velocity.y, downPull, 100), __instance.rb.velocity.z);
                    time += Time.deltaTime;             
                    if (time > 2)
                    {
                        downPull -= Time.deltaTime * 5;
                    }
                }
                else
                {
                    __instance.walkSpeed = startSpeed;
                    __instance.wallJumpPower = startWj;
                    downPull = 0;
                    time = 0;
                }
            }
        }
    }
}
