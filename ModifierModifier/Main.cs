using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using CobwebAPI.API;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ModifierModifier
{
    [BepInPlugin(ModName, ModGUID, ModVersion)]
    public class Main : BaseUnityPlugin
    {
        public static readonly GUIStyle backgroundStyle = new GUIStyle();
        public static readonly GUIStyle GUIStyle = new GUIStyle();
        
        //private Vector2 currentDrag = new Vector2();
        public Vector2 scrollPosition;
        public bool menuEnabled;
        private InputAction _inputAction = new InputAction(binding: "<Keyboard>/Insert");
        
        public const string ModName = "ModifierModifier";
        public const string ModAuthor = "Bazinga";
        public const string ModGUID = "com.bazinga.modifiermodifier";
        public const string ModVersion = "1.1.1";
        internal Harmony Harmony;

        internal void Awake()
        {
            Harmony = new Harmony(ModGUID);

            Harmony.PatchAll();
            Logger.LogInfo($"{ModName} successfully loaded! Made by {ModAuthor} \n Toggle Menu with the INS or INSERT key");


            Main.GUIStyle.alignment = TextAnchor.MiddleCenter;
            Main.GUIStyle.fontSize = 16;
            Main.GUIStyle.fontStyle = FontStyle.Bold;
            Main.GUIStyle.normal.background = Texture2D.grayTexture;
            backgroundStyle.alignment = TextAnchor.MiddleCenter;
            backgroundStyle.normal.textColor = Color.black;
            backgroundStyle.normal.background = Texture2D.grayTexture;
            menuEnabled = false;
            _inputAction.Enable();
        }

        private void Update()
        {
            if (_inputAction.triggered)
            {
                Logger.LogInfo($"Menu Toggled: {menuEnabled}");
                menuEnabled = !menuEnabled;
            }
        }
        
        [HarmonyPatch(typeof(ModifierManager), "GetNonMaxedSurvivalMods")]
        private void OnGUI()
        {
            var menuRect = new Rect(0, 25f, Screen.width / 2, Screen.height / 2);

            int valX = 0;
            int valY = 0;
            if (menuEnabled)
            {
                GUI.BeginGroup(menuRect, GUIStyle);

                foreach (var mods in GetTotalNonMaxedModifiers())
                {
                    if ((valY + 77) >= (Screen.height / 2))
                    {
                        valY = 0;
                        valX += 173;
                    }

                    bool flag = GUI.Button(new Rect(25f + valX, (float) (25 + valY), 150f, 40f), mods.data.name,
                        Main.backgroundStyle);
                    if (flag)
                    {
                        Logger.LogInfo(mods.data.name + "Has been clicked");
                        mods.levelInSurvival = mods.data.maxLevel;

                        Logger.LogInfo(mods.levelInSurvival);
                        Logger.LogInfo(mods.levelInVersus);
                    }

                    valY += 47;
                }
                GUI.EndGroup();
            }



        }
        
        public void MaxMod(Modifier mod)
        {
            
            //WaveModifiers.Give(mod.data.name, mod.data.maxLevel);
            Logger.LogInfo(mod.levelInSurvival);
            Logger.LogInfo(mod.levelInVersus);
        }
        public List<Modifier> GetTotalNonMaxedModifiers()
        {
            var list = new List<Modifier>();
            list.AddRange(ModifierManager.instance.GetVersusMods());
            list.AddRange(ModifierManager.instance.GetNonMaxedSurvivalMods());
            return list;
        }

    }

    [HarmonyPatch(typeof(ModifierManager), "GetNonMaxedSurvivalMods")]
    internal class ModifierManagerGetNonMaxedSurvivalModsPatch
    {
        internal static List<Modifier> Mods { get; private set; } = new();
    
        internal static bool Prefix(ModifierManager __instance, ref List<Modifier> __result)
        {
            var templist = (from m in Traverse.Create(__instance).Field<List<Modifier>>("_modifiers").Value
                where m.levelInSurvival < m.data.maxLevel && m.data.survival
                select m).ToList();
            if (Mods.Count > 0)
            {
                foreach (Modifier mod in Mods)
                {
                    if (mod == null || templist.Contains(mod))
                        continue;
    
                    templist.Add(mod);
                }
            }
    
            __result = templist;
            return false;
        }
    
        [HarmonyPatch(typeof(VersionNumberTextMesh), "Start")]
        public class VersionNumberTextMeshPatch
        {
            public static void Postfix(VersionNumberTextMesh __instance)
            {
                __instance.textMesh.text +=
                    $"\n<color=red>{Main.ModName} v{Main.ModVersion} by {Main.ModAuthor} \n use the INS or INSERT key to toggle the menu!!!</color>";
            }
        }
    }
    
    // BIN
    // ON GUI SHIT
    // GUI.BeginGroup(new Rect(25f, 25f,Screen.width/2,Screen.height/2), Main.backgroundStyle);
    // foreach (var mods in GetTotalNonMaxedModifiers())
    // {
    //
    // }
    // GUI.EndGroup();
}
