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
    [TweakMetadata("Double Jump", $"{Movement.GUID}.double_jump", "Jump, but double.", $"{Movement.GUID}.movement_mod", 2, AllowCG: false)]
    public class DoubleJump : Tweak
    {
        private Harmony harmony = new($"{Movement.GUID}.double_jump");

        public DoubleJump()
        {
            Subsettings = new()
            {
                { "jumps", new IntSubsetting(this, new Metadata("Jumps", "jumps", "How many jumps you get."),
                    new SliderIntSubsettingElement("{0}"), 2, 5, 1) },
            };
        }

        public static float Jumps;
        public static float JumpsLeft;

        public override void OnTweakEnabled()
        {
            base.OnTweakEnabled();
            SetValues();
            harmony.PatchAll(typeof(JumpPatches));
        }

        public override void OnTweakDisabled()
        {
            base.OnTweakDisabled();
            harmony.UnpatchSelf();
        }

        public override void OnSubsettingUpdate()
        {
            SetValues();
        }

        public void SetValues()
        {
            Jumps = Subsettings["jumps"].GetValue<int>();
        }

        public class JumpPatches
        {
            [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Update)), HarmonyPrefix]
            public static void DoubleJump(NewMovement __instance)
            {
                if (__instance.gc.onGround)
                {
                    JumpsLeft = Jumps;
                }

                if (InputManager.Instance.InputSource.Jump.WasPerformedThisFrame && !__instance.gc.onGround && !__instance.wc.onWall && JumpsLeft > 0)
                {
                    __instance.Jump();
                    JumpsLeft--;
                }
            }
        }
    }
}
