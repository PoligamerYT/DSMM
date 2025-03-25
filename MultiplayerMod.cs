using BepInEx;
using DSMM.Network;
using DSMM.UI;
using HarmonyLib;
using UnityEngine;
using Patches = DSMM.Common.Patches;
using DiscordManager = DSMM.Discord.DiscordManager;
using ManualLogSource = BepInEx.Logging.ManualLogSource;

namespace DSMM
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class MultiplayerMod : BaseUnityPlugin
    {
        public const string PluginGuid = "org.poligamer.DSMM", PluginName = "Multiplayer Mod", PluginVersion = "1.0.1";

        public static MultiplayerMod Instance;
        
        public ManualLogSource Log;

        public Harmony Harmony;

        public void Start()
        {
            Log = Logger;

            InitHarmony();

            new GameObject("[UIManager]").AddComponent<UIManager>();
            new GameObject("[NetworkManager]").AddComponent<NetworkManager>();
            new GameObject("[DiscordManager]").AddComponent<DiscordManager>();

            Logger.LogMessage("Multiplayer Started!");
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void InitHarmony()
        {
            Harmony = new Harmony("org.poligamer.DSMMM");
            Harmony.PatchAll(typeof(Patches));
        }
    }
}
