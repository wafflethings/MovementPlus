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
    [TweakMetadata("Recoil", $"{Movement.GUID}.recoil", "Every weapon gives recoil.", $"{Movement.GUID}.movement_mod", 0, AllowCG: false)]
    public class Recoil : Tweak
    {
        private Harmony harmony = new($"{Movement.GUID}.recoil");

        public Recoil()
        {
            Subsettings = new()
            {
                { "rev_recoil", new FloatSubsetting(this, new Metadata("Revolver", "rev_recoil", "Revolver recoil amount."),
                    new SliderFloatSubsettingElement("{0}x", 1), 1, 2, 0) },
                { "sho_recoil", new FloatSubsetting(this, new Metadata("Shotgun", "sho_recoil", "Shotgun recoil amount."),
                    new SliderFloatSubsettingElement("{0}x", 1), 1, 2, 0) },
                { "nai_recoil", new FloatSubsetting(this, new Metadata("Nail", "nai_recoil", "Nailgun recoil amount."),
                    new SliderFloatSubsettingElement("{0}x", 1), 1, 2, 0) },
                { "rai_recoil", new FloatSubsetting(this, new Metadata("Railcannon", "rai_recoil", "Railcannon recoil amount."),
                    new SliderFloatSubsettingElement("{0}x", 1), 1, 2, 0) },
                { "rock_recoil", new FloatSubsetting(this, new Metadata("Rocket", "rock_recoil", "Rocket Launcher recoil amount."),
                    new SliderFloatSubsettingElement("{0}x", 1), 1, 2, 0) },
                { "floor_mult", new FloatSubsetting(this, new Metadata("On Floor", "floor_mult", "On floor recoil multiplier."),
                    new SliderFloatSubsettingElement("{0}x", 1), 1, 2, 0) }
            };
        }

        public static float FlooredMultiplier => NewMovement.Instance.gc.touchingGround ? FloorMult : 1;

        public static float RevolverRecoil;
        public static float ShotgunRecoil;
        public static float NailgunRecoil;
        public static float RailcannonRecoil;
        public static float RocketRecoil;
        public static float FloorMult;

        public static void AddRecoil(float strength)
        {
            NewMovement.Instance.rb.AddForce(strength * -CameraController.Instance.transform.forward * FlooredMultiplier, ForceMode.VelocityChange);
        }

        public override void OnTweakEnabled()
        {
            base.OnTweakEnabled();
            SetValues();
            harmony.PatchAll(typeof(RecoilPatches));
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
            RevolverRecoil = Subsettings["rev_recoil"].GetValue<float>();
            ShotgunRecoil = Subsettings["sho_recoil"].GetValue<float>();
            NailgunRecoil = Subsettings["nai_recoil"].GetValue<float>();
            RailcannonRecoil = Subsettings["rai_recoil"].GetValue<float>();
            RocketRecoil = Subsettings["rock_recoil"].GetValue<float>();
            FloorMult = Subsettings["floor_mult"].GetValue<float>();
        }

        public class RecoilPatches
        {
            [HarmonyPatch(typeof(Revolver), nameof(Revolver.Shoot)), HarmonyPrefix]
            private static void AddRevolverRecoil(Revolver __instance, int shotType)
            {
                if (shotType == 2)
                {
                    float multiplier = __instance.pierceShotCharge / 100;
                    AddRecoil(20 * RevolverRecoil * multiplier);
                }
                else
                {
                    AddRecoil(10 * RevolverRecoil);
                }
            }

            [HarmonyPatch(typeof(Shotgun), nameof(Shotgun.Shoot)), HarmonyPrefix]
            private static void AddShotgunRecoil(Shotgun __instance)
            {
                float multiplier = 1 + (__instance.variation == 1 ? (__instance.primaryCharge + 1) / 3 : 0);
                AddRecoil(15 * multiplier * ShotgunRecoil);
            }

            [HarmonyPatch(typeof(Shotgun), nameof(Shotgun.ShootSinks)), HarmonyPrefix]
            private static void AddShotgunCoreRecoil(Shotgun __instance)
            {
                AddRecoil(10 * (__instance.grenadeForce / 30 * 2) * ShotgunRecoil);
            }

            [HarmonyPatch(typeof(Nailgun), nameof(Nailgun.Shoot)), HarmonyPrefix]
            private static void AddNailRecoil()
            {
                AddRecoil(0.75f * NailgunRecoil);
            }

            [HarmonyPatch(typeof(Nailgun), nameof(Nailgun.ShootMagnet)), HarmonyPrefix]
            private static void AddNailMagnetRecoil()
            {
                AddRecoil(10 * NailgunRecoil);
            }

            [HarmonyPatch(typeof(Railcannon), nameof(Railcannon.Shoot)), HarmonyPrefix]
            private static void AddRailRecoil()
            {
                AddRecoil(50 * RailcannonRecoil);
            }

            [HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.Shoot)), HarmonyPrefix]
            private static void AddRocketRecoil()
            {
                AddRecoil(15 * RocketRecoil);
            }


            [HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.ShootCannonball)), HarmonyPrefix]
            private static void AddRocketCannonballRecoil(RocketLauncher __instance)
            {
                AddRecoil(30 * __instance.cbCharge * RocketRecoil);
            }
        }
    }
}
