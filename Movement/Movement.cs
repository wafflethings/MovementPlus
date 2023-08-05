using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UltraTweaker.Tweaks;
using UltraTweaker.Handlers;
using UltraTweaker;
using System.IO;

namespace Movement
{
    [BepInPlugin(GUID, Name, Version)]
    public class Movement : BaseUnityPlugin
    {
        public const string GUID = "waffle.ultrakill.movement";
        public const string Name = "MovementPlus";
        public const string Version = "1.0.0";

        public static readonly string AssetsPath = Path.Combine(ModPath(), "Assets");
        public static readonly string BundlePath = Path.Combine(AssetsPath, "movementplus.bundle");
        public static AssetBundle Bundle;

        public void Start()
        {
            Bundle = AssetBundle.LoadFromFile(BundlePath);
            SettingUIHandler.Pages.Add($"{GUID}.movement_mod", new SettingUIHandler.Page("MOVEMENT+"));
            UltraTweaker.UltraTweaker.AddAssembly(Assembly.GetExecutingAssembly());
        }

        public static string ModPath(Assembly asm = null)
        {
            if (asm == null)
            {
                asm = Assembly.GetExecutingAssembly();
            }

            return asm.Location.Substring(0, asm.Location.LastIndexOf(Path.DirectorySeparatorChar));
        }
    }
}
