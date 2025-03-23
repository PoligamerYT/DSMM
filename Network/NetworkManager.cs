using DSMM.Common;
using DSMM.Network.Enums;
using DSMM.Network.Packets;
using DSMM.UI;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = DSMM.Math.Vector3;

namespace DSMM.Network
{
    public class NetworkManager : MonoBehaviour
    {
        public bool HaveRecievePrimaryInfo = false;

        public int MaxPlayers = 5;
        public static NetworkManager Instance;

        public SteamLobby SteamLobby;

        public List<Player> Players = new List<Player>();

        public bool IsCLient = false;
        public bool IsServer = false;

        public GameMode CurrentGameMode = GameMode.Vanilla;

        public ControlType CurrentControlType = ControlType.Player;

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
            SteamLobby = gameObject.AddComponent<SteamLobby>();

            RegisterPackets();
        }

        private void OnApplicationQuit()
        {
            if (!IsConnected())
                return;

            SteamLobby.LeaveLobby(LeaveType.Quit);
        }

        private void Update()
        {
            PacketHandler.ProcessBufferedPackets();

            UpdatePlayersPosition();
        }

        private void FixedUpdate()
        {
            if (!IsConnected())
                return;

            SendPlayerMovement();
        }

        public void SendPlayerMovement()
        {
            var field = typeof(BaseActor).GetField("_moveDir", BindingFlags.Instance | BindingFlags.NonPublic);

            switch (CurrentGameMode)
            {
                case GameMode.Vanilla:
                    PlayerPositionPacket packet_ = new PlayerPositionPacket
                    {
                        PlayerPosition = new Vector3(PlayerController.Instance._playerActor.gameObject.transform.position),
                        SwordPosition = new Vector3(PlayerController.Instance._sword.gameObject.transform.position),
                        SwordRotation = PlayerController.Instance._sword.gameObject.transform.rotation.eulerAngles.z,
                        MoveDirection = (float)field.GetValue(PlayerController.Instance._playerActor),
                        VelocityMagnitude = PlayerController.Instance._playerActor._rigidBody.velocity.magnitude
                    };

                    SendPacketToAll(packet_, sendMode: EP2PSend.k_EP2PSendUnreliableNoDelay);
                    break;
                case GameMode.CoOpChaos:
                    PlayerPositionPacket packet__ = new PlayerPositionPacket();

                    if (CurrentControlType == ControlType.Player)
                    {
                        packet__.PlayerPosition = new Vector3(PlayerController.Instance._playerActor.gameObject.transform.position);
                        packet__.MoveDirection = (float)field.GetValue(PlayerController.Instance._playerActor);
                        packet__.VelocityMagnitude = PlayerController.Instance._playerActor._rigidBody.velocity.magnitude;
                    }
                    else
                    {
                        packet__.SwordPosition = new Vector3(PlayerController.Instance._sword.gameObject.transform.position);
                        packet__.SwordRotation = PlayerController.Instance._sword.gameObject.transform.rotation.eulerAngles.z;
                    }

                    SendPacketToAll(packet__, sendMode: EP2PSend.k_EP2PSendUnreliableNoDelay);
                    break;
            }
        }

        public void RegisterPackets()
        {
            PacketHandler.Packets.Add(typeof(HostLeavePacket), OnHostLeave);
            PacketHandler.Packets.Add(typeof(PrimaryInfoPacket), OnPrimaryInfo);
            PacketHandler.Packets.Add(typeof(PlayerPositionPacket), OnPlayerPosition);
            PacketHandler.Packets.Add(typeof(StageChangePacket), OnStageChangePacket);
            PacketHandler.Packets.Add(typeof(PlayerTeleportPacket), OnPlayerTeleport);
            PacketHandler.Packets.Add(typeof(SwordChangePacket), OnSwordChange);
            PacketHandler.Packets.Add(typeof(RestartGamePacket), OnRestartGame);
        }

        private void OnRestartGame(Player sender, object obj)
        {
            RestartGamePacket packet = (RestartGamePacket)obj;

            Main.Instance._state = Main.GameState.Restarting;

            Fader.Instance.FadeOut(1f, 0f, delegate
            {
                StartCoroutine(Utils.AdjustSword(0));
                StageManager.Instance.SetStage(StageManager.s_resetStage);
                PlayerController.Instance.TeleportPlayer(new UnityEngine.Vector3(-3, 1, 0));
                PlayerController.Instance._allowControl = true;
                Main.Instance._state = Main.GameState.Game;
                Main.Instance._playtime = (float)(Utils.GetUnixTime() - packet.Timestamp);
                Fader.Instance._isFading = false;
                Main._lapCount = 0;
                Main._totalLapTime = (float)(Utils.GetUnixTime() - packet.Timestamp);
                Fader.Instance.gameObject.SetActive(false);
                Main.Instance._winScreen.SetActive(false);
                Main.Instance._gameHUD.Show(true);
            });
        }

        private void OnSwordChange(Player sender, object obj)
        {
            SwordChangePacket packet = (SwordChangePacket)obj;

            switch (packet.SwordChangeType)
            {
                case SwordChangeType.Grow:
                    sender.GetPlayerController()._sword.Grow();
                    break;
                case SwordChangeType.Shrink:
                    sender.GetPlayerController()._sword.Shrink();
                    break;
            }
        }

        private void OnPlayerTeleport(Player sender, object obj)
        {
            PlayerTeleportPacket packet = (PlayerTeleportPacket)obj;

            sender.GetPlayerController()._playerActor._rigidBody.bodyType = RigidbodyType2D.Dynamic;
            sender.GetPlayerController()._playerActor.transform.localScale = UnityEngine.Vector3.one;
            sender.GetPlayerController()._playerActor.transform.position = packet.Position.GetVector3();
            sender.GetPlayerController()._sword.transform.rotation = Quaternion.identity;
        }

        private void OnStageChangePacket(Player sender, object obj)
        {
            StageChangePacket packet = (StageChangePacket)obj;

            SwordTrigger swordTrigger = GameObject.FindObjectsOfType<SwordTrigger>().FirstOrDefault(x => x._achievementId == (AchievementId)packet.StageID);

            if (!swordTrigger._activated && !StageManager.Instance._stageComplete)
            {
                swordTrigger._tutorial?.Complete();
                AchievementManager.Instance.UnlockAchievement(swordTrigger._achievementId);
                AudioManager.Instance.PlayOneShot(swordTrigger._sfx, swordTrigger._sfxVolume);
                swordTrigger._activated = true;
                StageManager.Instance._stageComplete = true;
                StartCoroutine(swordTrigger.DoEndSequence());
            }
        }

        public void SendPacketTo<T>(T packet, Player sendTo, EP2PSend sendMode = EP2PSend.k_EP2PSendReliable)
        {
            byte[] packetData = PacketHandler.SerializePacket(packet);

            SteamNetworking.SendP2PPacket(new CSteamID(sendTo.SteamID), packetData, (uint)packetData.Length, sendMode);
        }

        public void SendPacketToAll<T>(T packet, bool excludingMe = true, EP2PSend sendMode = EP2PSend.k_EP2PSendReliable)
        {
            byte[] packetData = PacketHandler.SerializePacket(packet);

            foreach (CSteamID id in SteamLobby.GetLobbyMembers())
            {
                if (excludingMe)
                {
                    if (id != SteamUser.GetSteamID())
                    {
                        SteamNetworking.SendP2PPacket(id, packetData, (uint)packetData.Length, sendMode);
                    }
                }
                else
                {
                    SteamNetworking.SendP2PPacket(id, packetData, (uint)packetData.Length, sendMode);
                }
            }
        }

        private void OnHostLeave(Player sender, object obj)
        {
            SteamLobby.LeaveLobby(LeaveType.Packet);
        }

        private void OnPrimaryInfo(Player sender, object obj)
        {
            PrimaryInfoPacket Packet = (PrimaryInfoPacket)obj;

            if (HaveRecievePrimaryInfo)
                return;

            MultiplayerMod.Instance.Log.LogMessage("Primary Info Recieve!");

            StageManager.Instance.SetStage(Packet.Stage);

            StartCoroutine(Utils.AdjustSword(Packet.Sword));

            CurrentGameMode = Packet.GameMode;

            foreach (Player p in Packet.Players)
            {
                if (CurrentGameMode == GameMode.Vanilla)
                    Utils.CreatePlayer(p);
            }

            if (Main.Instance._state == Main.GameState.Title)
            {
                Main.Instance.StartGame();
            }

            UIManager.Instance.OnEnterLobby();

            HaveRecievePrimaryInfo = true;

            PlayerController.Instance._allowControl = true;

            if (CurrentGameMode == GameMode.CoOpChaos)
            {
                CurrentControlType = Packet.ControlType;
            }

            Main.Instance._playtime = Packet.PlayTime + (float)(Utils.GetUnixTime() - Packet.Timestamp);
            Main._totalLapTime = Packet.TotalLapTime + (float)(Utils.GetUnixTime() - Packet.Timestamp);
            Main._lapCount = Packet.LapCount;
        }

        private void OnPlayerPosition(Player sender, object obj)
        {
            try
            {
                PlayerPositionPacket packet = (PlayerPositionPacket)obj;

                switch (CurrentGameMode)
                {
                    case GameMode.Vanilla:
                        PlayerController controller = sender.GetPlayerController();

                        StartCoroutine(Utils.LerpPosition(controller._playerActor.gameObject.transform, packet.PlayerPosition, 100f));
                        StartCoroutine(Utils.LerpPosition(controller._sword.gameObject.transform, packet.SwordPosition, 100f));
                        StartCoroutine(Utils.LerpRotation(controller._sword.gameObject.transform, new Vector3(0, 0, packet.SwordRotation), 100f));

                        controller._playerActor.Move(packet.MoveDirection);
                        sender.VelocityMagnitude = packet.VelocityMagnitude;
                        break;
                    case GameMode.CoOpChaos:
                        if (CurrentControlType == ControlType.Player)
                        {
                            StartCoroutine(Utils.LerpPosition(PlayerController.Instance._sword.gameObject.transform, packet.SwordPosition, 100f));
                            StartCoroutine(Utils.LerpRotation(PlayerController.Instance._sword.gameObject.transform, new Vector3(0, 0, packet.SwordRotation), 100f));
                        }
                        else
                        {
                            StartCoroutine(Utils.LerpPosition(PlayerController.Instance._playerActor.gameObject.transform, packet.PlayerPosition, 100f));
                        }
                        break;
                }
            }
            catch { }
        }

        public void HostLobby()
        {
            SteamLobby.HostLobby();
        }

        public void CancelMultiplayer()
        {
            SteamLobby.LeaveLobby(LeaveType.Manual);
        }

        public bool IsConnected()
        {
            if (!IsCLient && !IsServer)
                return false;

            return true;
        }

        public bool IsLobbyOwner()
        {
            if (!IsConnected())
                return false;

            CSteamID Owner = SteamMatchmaking.GetLobbyOwner(SteamLobby.CurrentLobbyID);

            if (Owner == SteamUser.GetSteamID())
            {
                return true;
            }

            return false;
        }

        public bool IsLobbyOwner(ulong Player)
        {
            if (!IsConnected())
                return false;

            CSteamID Owner = SteamMatchmaking.GetLobbyOwner(SteamLobby.CurrentLobbyID);

            if (Owner == new CSteamID(Player))
            {
                return true;
            }

            return false;
        }

        public Player GetPlayer(ulong SteamID)
        {
            return Players.FirstOrDefault(p => p.SteamID == SteamID);
        }

        public bool IsPlayer(ulong SteamID)
        {
            return Players.Any(p => p.SteamID == SteamID);
        }

        public Player GetLocalPlayer()
        {
            return Players.FirstOrDefault(p => p.SteamID == SteamUser.GetSteamID().m_SteamID);
        }

        public ulong GetLobbyOwner()
        {
            return SteamMatchmaking.GetLobbyOwner(SteamLobby.CurrentLobbyID).m_SteamID;
        }

        private void UpdatePlayersPosition()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].PlayerPosition = new Vector3(Players[i].GetPlayerController()._playerActor.gameObject.transform.position);
                Players[i].SwordPosition = new Vector3(Players[i].GetPlayerController()._sword.gameObject.transform.position);
                Players[i].SwordRotation = Players[i].GetPlayerController()._sword.gameObject.transform.rotation.eulerAngles.z;
            }
        }
    }
}
