using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DSMM.UI;
using DSMM.Network.Enums;
using DSMM.Network.Packets;
using System.Collections;
using DSMM.Common;
using DSMM.Discord;
using System.Reflection;

namespace DSMM.Network
{
    public class SteamLobby : MonoBehaviour
    {
        protected Callback<LobbyCreated_t> LobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> JoinRequest;
        protected Callback<LobbyEnter_t> LobbyEntered;
        protected Callback<P2PSessionRequest_t> P2PSessionRequest;

        private List<CSteamID> PreviousMembers = new List<CSteamID>();

        public CSteamID CurrentLobbyID;
        public CSteamID CurrentRemoteSteamId;
        public string LobbyName;

        public Coroutine PacketCoroutine;

        public void Start()
        {
            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);

            PacketCoroutine = StartCoroutine(PacketCheckCoroutine());
        }

        public void HostLobby()
        {
            if (NetworkManager.Instance.IsConnected())
                return;

            SteamMatchmaking.CreateLobby(UIManager.Instance.GetLobbyType(), NetworkManager.Instance.MaxPlayers);
        }

        public void LeaveLobby(LeaveType leaveType = LeaveType.Unknown)
        {
            if (NetworkManager.Instance.IsConnected())
            {
                if (NetworkManager.Instance.IsLobbyOwner())
                {
                    HostLeavePacket Packet = new HostLeavePacket
                    {
                        HostSteamID = SteamUser.GetSteamID()
                    };

                    NetworkManager.Instance.SendPacketToAll(Packet);
                }

                SteamMatchmaking.LeaveLobby(CurrentLobbyID);
            }

            MultiplayerMod.Instance.Log.LogMessage("Leaving Lobby! Reason: " + leaveType);

            NetworkManager.Instance.IsCLient = false;
            NetworkManager.Instance.IsServer = false;

            NetworkManager.Instance.HaveRecievePrimaryInfo = false;

            if (leaveType == LeaveType.Quit)
                return;

            Utils.DestroyAllPlayers();

            DiscordManager.Instance.BackToDefault();

            PlayerController.Instance.RespawnPlayer();

            UIManager.Instance.OnLeaveLobby();

            NetworkManager.Instance.SteamLobby = gameObject.AddComponent<SteamLobby>();

            StartCoroutine(DestroyLobbyAfterFrame());
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
                return;

            NetworkManager.Instance.CurrentGameMode = UIManager.Instance.GetGameMode();

            if(NetworkManager.Instance.CurrentGameMode == GameMode.CoOpChaos)
                NetworkManager.Instance.CurrentControlType = Utils.GetRandomEnumValue<ControlType>(); 

            MultiplayerMod.Instance.Log.LogMessage("Lobby Created Succesfully");

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "´s LOBBY");

            NetworkManager.Instance.IsServer = true;

            if (Main.Instance._state == Main.GameState.Title)
            {
                Main.Instance.StartGame();
            }

            UIManager.Instance.OnEnterLobby();
        }

        private void OnJoinRequest(GameLobbyJoinRequested_t callback)
        {
            UIManager.Instance.Loading();

            UIManager.Instance.GetPauseScreenUI().ShowSubmenu(4);

            MultiplayerMod.Instance.Log.LogMessage("Request To Join Lobby");
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        private void OnPlayerJoin(CSteamID Player)
        {
            MultiplayerMod.Instance.Log.LogMessage("New player joined: " + SteamFriends.GetFriendPersonaName(Player));

            if (NetworkManager.Instance.IsCLient && !NetworkManager.Instance.HaveRecievePrimaryInfo)
                return;

            if(NetworkManager.Instance.CurrentGameMode == GameMode.Vanilla)
                Utils.CreatePlayer(Player.m_SteamID);

            if (NetworkManager.Instance.IsServer && SteamUser.GetSteamID() != Player)
            {
                StartCoroutine(SendPrimaryInfo(Player.m_SteamID));
            }

            DiscordManager.Instance.UpdateDiscordRichPresenceWithSecret(CurrentLobbyID.ToString());
        }

        private void OnPlayerLeft(CSteamID Player)
        {
            if (NetworkManager.Instance.IsLobbyOwner(Player.m_SteamID))
            {
                LeaveLobby();
            }

            MultiplayerMod.Instance.Log.LogMessage("Player left: " + SteamFriends.GetFriendPersonaName(Player));

            SteamNetworking.CloseP2PSessionWithUser(Player);

            if (NetworkManager.Instance.CurrentGameMode == GameMode.Vanilla)
                Utils.DestroyPlayer(Player.m_SteamID);

            DiscordManager.Instance.UpdateDiscordRichPresenceWithSecret(CurrentLobbyID.ToString());
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            LobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

            if (!NetworkManager.Instance.IsServer)
                NetworkManager.Instance.IsCLient = true;

            if(NetworkManager.Instance.IsCLient)
                PlayerController.Instance._allowControl = false;

            PlayerController.Instance.RespawnPlayer();

            Utils.CreatePlayer(PlayerController.Instance);
        }

        private void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
        {
            var lobbyMembers = GetLobbyMembers();

            SteamNetworking.AcceptP2PSessionWithUser(pCallback.m_steamIDRemote);
            MultiplayerMod.Instance.Log.LogMessage($"Accepted P2P request from: {pCallback.m_steamIDRemote}");
        }

        public List<CSteamID> GetLobbyMembers()
        {
            List<CSteamID> members = new List<CSteamID>();

            int memberCount = SteamMatchmaking.GetNumLobbyMembers(CurrentLobbyID);
            for (int i = 0; i < memberCount; i++)
            {
                members.Add(SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobbyID, i));
            }

            return members;
        }

        public void DetectPLayerJoinOrLeave()
        {
            List<CSteamID> CurrentMembers = GetLobbyMembers();

            List<CSteamID> NewMembers = CurrentMembers.Except(PreviousMembers).ToList();
            List<CSteamID> LeftMembers = PreviousMembers.Except(CurrentMembers).ToList();

            PreviousMembers = CurrentMembers;

            foreach (CSteamID NewMember in NewMembers)
            {
                OnPlayerJoin(NewMember);
            }

            foreach (CSteamID LeftMember in LeftMembers)
            {
                OnPlayerLeft(LeftMember);
            }
        }

        private void Update()
        {
            if (!NetworkManager.Instance.IsConnected())
                return;

            DetectPLayerJoinOrLeave();
        }

        IEnumerator SendPrimaryInfo(ulong steamId)
        {
            MultiplayerMod.Instance.Log.LogMessage($"Primary Info Send to: {steamId}!");

            for (int i = 0; i < 5; i++)
            {
                var field = typeof(Sword).GetField("_length", BindingFlags.Instance | BindingFlags.NonPublic);
                PrimaryInfoPacket packet = new PrimaryInfoPacket
                {
                    Players = NetworkManager.Instance.Players,
                    Stage = StageManager.Instance._activeStage,
                    Sword = (int)field.GetValue(PlayerController.Instance._sword),
                    Timestamp = Utils.GetUnixTime(),
                    PlayTime = Main.Instance._playtime,
                    TotalLapTime = Main._totalLapTime,
                    LapCount = Main._lapCount,
                    GameMode = NetworkManager.Instance.CurrentGameMode,
                    ControlType = NetworkManager.Instance.CurrentControlType == ControlType.Sword ? ControlType.Player : ControlType.Sword
                };

                NetworkManager.Instance.SendPacketTo(packet, new Player(steamId));

                yield return new WaitForSeconds(3f);
            }
        }

        IEnumerator PacketCheckCoroutine()
        {
            uint packetSize;
            CSteamID remoteSteamID = new CSteamID();

            while (true)
            {
                while (SteamNetworking.IsP2PPacketAvailable(out packetSize))
                {
                    byte[] packetData = new byte[packetSize];
                    uint bytesRead;

                    if (SteamNetworking.ReadP2PPacket(packetData, packetSize,
                        out bytesRead, out remoteSteamID))
                    {
                        PacketHandler.DeserializePacket(remoteSteamID, packetData, bytesRead);
                    }
                }

                yield return null;
            }
        }

        private IEnumerator DestroyLobbyAfterFrame()
        {
            yield return null;
            Destroy(this);
        }

        private void OnDestroy()
        {
            LobbyCreated?.Dispose();
            JoinRequest?.Dispose();
            LobbyEntered?.Dispose();
            P2PSessionRequest?.Dispose();

            if (PacketCoroutine != null)
            {
                StopCoroutine(PacketCoroutine);
                PacketCoroutine = null;
            }
        }
    }
}
