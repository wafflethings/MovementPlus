using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UltraTweaker.Handlers;
using UltraTweaker.Tweaks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Movement.Tweaks
{
    [TweakMetadata("Hook Swing", $"{Movement.GUID}.real_hook", "Swing with the whiplash.", $"{Movement.GUID}.movement_mod",0, AllowCG: false)]
    public class RealHook : Tweak
    {
        private Harmony harmony = new($"{Movement.GUID}.real_hook");

        public RealHook()
        {
            Subsettings = new()
            {
                { "any_surf", new BoolSubsetting(this, new Metadata("Any Surface", "any_surf", "If you should hook on every surface."),
                    new BoolSubsettingElement(), true) },
                { "space_pull", new FloatSubsetting(this, new Metadata("Space To Pull", "space_pull", "Amount to pull yourself to the center with Space."),
                    new SliderFloatSubsettingElement("{0}"), 50, 100, 0) },
                { "space_short", new FloatSubsetting(this, new Metadata("Space To Shorten", "space_short", "Amount to shorten the hook with Space."),
                    new SliderFloatSubsettingElement("{0}"), 0, 100, 0) },
            };
        }

        public override void OnTweakEnabled()
        {
            base.OnTweakEnabled();
            harmony.PatchAll(typeof(RealHookPatches));
        }

        public override void OnTweakDisabled()
        {
            base.OnTweakDisabled();
            harmony.UnpatchSelf();
        }

        private static GameObject InvisHook;
        private static float pullSpeed;
        private static float shortenSpeed;

        public override void OnSubsettingUpdate()
        {
            SetValues();
        }

        public override void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            SetValues();
        }

        public void SetValues()
        {
            Destroy(InvisHook);
            pullSpeed = Subsettings["space_pull"].GetValue<float>();
            shortenSpeed = Subsettings["space_short"].GetValue<float>();
            TryMakeHookPoint();
        }

        public void TryMakeHookPoint()
        {
            if (IsGameplayScene() && Subsettings["any_surf"].GetValue<bool>())
            {
                InvisHook = Instantiate(Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Levels/Interactive/GrapplePoint.prefab").WaitForCompletion());
                InvisHook.transform.GetChild(0).gameObject.SetActive(false);
                InvisHook.transform.GetChild(1).gameObject.SetActive(false);
                InvisHook.GetComponent<Light>().enabled = false;
                InvisHook.GetComponent<AudioSource>().enabled = false;
                InvisHook.GetComponent<HookPoint>().grabParticle = new GameObject();
                InvisHook.GetComponent<HookPoint>().grabParticle.AddComponent<DestroyOnCheckpointRestart>();
                InvisHook.transform.localScale = Vector3.one * 2;
            }
        }

        public class RealHookPatches
        {
            private static ConfigurableJoint gj;

            [HarmonyPatch(typeof(HookArm), nameof(HookArm.Update)), HarmonyPostfix]
            public static void MakeSwing(HookArm __instance)
            {
                if (!__instance.lightTarget && __instance.state == HookState.Pulling)
                {
                    if (__instance.caughtTransform != null)
                    {
                        __instance.hookPoint = __instance.caughtTransform.position + __instance.caughtPoint;

                        if (!NewMovement.Instance.GetComponent<ConfigurableJoint>())
                        {
                            gj = NewMovement.Instance.gameObject.AddComponent<ConfigurableJoint>();
                            gj.xMotion = ConfigurableJointMotion.Limited;
                            gj.yMotion = ConfigurableJointMotion.Limited;
                            gj.zMotion = ConfigurableJointMotion.Limited;
                            gj.linearLimit = new SoftJointLimit()
                            {
                                limit = Vector3.Distance(gj.transform.position, __instance.caughtTransform.position + __instance.caughtPoint)
                            };
                            gj.autoConfigureConnectedAnchor = false;
                            NewMovement.Instance.rb.AddForce((Vector3.up * 15f) * -1, ForceMode.VelocityChange);
                        }

                        gj.connectedAnchor = __instance.caughtTransform.position + __instance.caughtPoint;
                    }

                    if (__instance.caughtEid != null && __instance.caughtEid.dead)
                    {
                        Destroy(gj);
                        __instance.StopThrow(0);
                    }
                }
                else
                {
                    Destroy(gj);
                }
            }

            [HarmonyPatch(typeof(HookArm), nameof(HookArm.Update)), HarmonyPrefix]
            public static void CollideWithWalls(HookArm __instance)
            {
                if (InvisHook != null && __instance.state == HookState.Ready)
                {
                    Physics.Raycast(CameraController.Instance.transform.position, CameraController.Instance.transform.forward, out RaycastHit raycastHit, 1000000,
                       __instance.enviroMask, QueryTriggerInteraction.Ignore);

                    InvisHook.transform.position = raycastHit.point - CameraController.Instance.transform.forward;
                }
            }

            [HarmonyPatch(typeof(HookArm), nameof(HookArm.FixedUpdate)), HarmonyPrefix]
            public static bool StopPull(HookArm __instance, ref Vector3 __state)
            {
                __state = NewMovement.Instance.rb.velocity;

                if (!__instance.lightTarget && __instance.state == HookState.Pulling)
                {
                    return false;
                }

                return true;
            }

            [HarmonyPatch(typeof(HookArm), nameof(HookArm.FixedUpdate)), HarmonyPostfix]
            public static void SetVelo(HookArm __instance, Vector3 __state)
            {
                if (!__instance.lightTarget)
                {
                    NewMovement.Instance.rb.velocity = __state;
                }
            }

            [HarmonyPatch(typeof(HookArm), nameof(HookArm.StopThrow)), HarmonyPrefix]
            public static bool StopJumpDisablingTheHook(HookArm __instance, float animationTime)
            {
                if (__instance.state == HookState.Pulling && !__instance.lightTarget && InputManager.Instance.InputSource.Jump.WasPerformedThisFrame && animationTime == 1)
                {
                    return false;
                }

                return true;
            }


            [HarmonyPatch(typeof(HookArm), nameof(HookArm.Update)), HarmonyPostfix]
            public static void Pull(HookArm __instance)
            {
                if (__instance.state == HookState.Pulling && !__instance.lightTarget && InputManager.Instance.InputSource.Jump.IsPressed) 
                {
                    gj.linearLimit = new()
                    {
                        limit = gj.linearLimit.limit - (Time.deltaTime * shortenSpeed)
                    };

                    NewMovement.Instance.rb.velocity = pullSpeed * (HookArm.Instance.caughtTransform.transform.position + HookArm.Instance.caughtPoint - __instance.transform.position).normalized;
                }
            }
        }
    }
}
