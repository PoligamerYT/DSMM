using System;
using System.Text;
using UnityEngine;
using DSMM.Network;
using DSMM.Network.Enums;
using Steamworks;
using System.IO.Pipes;
using DSMM.UI;

namespace DSMM.Discord
{
    public class DiscordManager : MonoBehaviour
    {
        public static DiscordManager Instance;

        public Discord discord;

        private bool dllReady = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (!IsDiscordRunning())
                return;

            try
            {      
                Default();
            }
            catch (DllNotFoundException e)
            {
                Debug.LogWarning("No Discord DLL found, plase install to be able to use the discord functionality");
            }
        }

        private void Default()
        {
            discord = new Discord(1286381942769455105, (UInt64)CreateFlags.Default);

            var activityManager = discord.GetActivityManager();
            var activity = new Activity
            {
                State = "Playing",
                Timestamps = new ActivityTimestamps
                {
                    Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            };

            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res == Result.Ok)
                {
                    MultiplayerMod.Instance.Log.LogMessage("Discord Rich Presence Updated!");
                }
            });

            activityManager.OnActivityJoin += (string secret) =>
            {
                MultiplayerMod.Instance.Log.LogMessage($"Received Join Request with Steam lobby ID: {secret}");

                UIManager.Instance.Loading();

                UIManager.Instance.GetPauseScreenUI().ShowSubmenu(4);

                if (Network.NetworkManager.Instance.IsConnected())
                    Network.NetworkManager.Instance.SteamLobby.LeaveLobby(LeaveType.Unknown);

                CSteamID lobbyId = new CSteamID(ulong.Parse(secret));

                SteamMatchmaking.JoinLobby(lobbyId);
            };

            dllReady = true;
        }

        void Update()
        {
            if (!dllReady)
                return;

            discord.RunCallbacks();
        }

        public void BackToDefault()
        {
            if (!dllReady)
                return;

            var activity = new Activity
            {
                State = "Playing",
                Timestamps = new ActivityTimestamps
                {
                    Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            };

            var activityManager = discord.GetActivityManager();

            activityManager.RegisterCommand("discord");

            activityManager.UpdateActivity(activity, (result) =>
            {
                if (result == Result.Ok)
                {
                    MultiplayerMod.Instance.Log.LogMessage("Discord Rich Presence Is Back To Deafult!");
                }
            });
        }

        public void UpdateDiscordRichPresenceWithSecret(string joinSecret)
        {
            if (!dllReady)
                return;

            var activity = new Activity
            {
                State = "Playing",
                Party = new ActivityParty
                {
                    Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(joinSecret)),
                    Size = new PartySize
                    {
                        CurrentSize = Network.NetworkManager.Instance.SteamLobby.GetLobbyMembers().Count,
                        MaxSize = Network.NetworkManager.Instance.MaxPlayers
                    }
                },
                Secrets = new ActivitySecrets
                {
                    Join = joinSecret  
                },
                Timestamps = new ActivityTimestamps
                {
                    Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            };

            var activityManager = discord.GetActivityManager();
            activityManager.UpdateActivity(activity, (result) =>
            {
                if (result == Result.Ok)
                {
                    MultiplayerMod.Instance.Log.LogMessage("Discord Rich Presence Updated with Steam Lobby ID.");
                }
            });
        }

        public static bool IsDiscordRunning()
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", "discord-ipc-0", PipeDirection.Out))
                {
                    pipeClient.Connect(100);
                    return true;
                }
            }
            catch (Exception)
            {
                return false; 
            }
        }
    }
}
