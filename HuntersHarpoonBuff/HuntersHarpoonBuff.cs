using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using R2API;
using System.Collections.Generic;

namespace HuntersHarpoonBuff
{

    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class HuntersHarpoonRework : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "OakPrime";
        public const string PluginName = "HuntersHarpoonRework";
        public const string PluginVersion = "2.0.2";

        private readonly Dictionary<string, string> DefaultLanguage = new Dictionary<string, string>();


        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Log.Init(Logger);
            
            try
            {
                this.UpdateText();
                IL.RoR2.GlobalEventManager.OnCharacterDeath += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(
                        x => x.MatchLdloc(48),
                        x => x.MatchLdcI4(0),
                        x => x.MatchBle(out _)
                    );
                    c.Index += 3;
                    c.RemoveRange(38);
                    c.Emit(OpCodes.Ldloc, 48);
                    c.Emit(OpCodes.Ldloc, 15);
                    c.EmitDelegate<Action<int, RoR2.CharacterBody>>((itemCount, body) =>
                    {
                        body?.ClearTimedBuffs(DLC1Content.Buffs.KillMoveSpeed);
                        for (int i = 0; i < 4; i++)
                        {
                            body?.AddTimedBuff(DLC1Content.Buffs.KillMoveSpeed, ((2.0f * itemCount) * (float)Math.Pow(0.75, i)));
                            //body?.AddTimedBuff(DLC1Content.Buffs.KillMoveSpeed, (1.0f + 2.0f * itemCount) / i);
                            //Log.LogInfo("Adding timed buff for: " + ((3.0f * itemCount) * (float)Math.Pow(0.75, i)) + " seconds");
                        }
                    });
                };


            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }
        }
        private void UpdateText()
        {
            this.ReplaceString("ITEM_MOVESPEEDONKILL_DESC", "Killing an enemy increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>100%</style>" +
                " fading over <style=cIsUtility>2</style> <style=cStack>(+2 per stack)</style> seconds.");
        }

        private void ReplaceString(string token, string newText)
        {
            this.DefaultLanguage[token] = Language.GetString(token);
            LanguageAPI.Add(token, newText);
        }
    }
}
