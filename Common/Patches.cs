using HarmonyLib;
using NetworkManager = DSMM.Network.NetworkManager;
using UnityEngine;
using DSMM.Network.Packets;
using Steamworks;
using DSMM.Network.Enums;
using System.Reflection;
using System.Collections.Generic;
using static Main;
using DSMM.UI;

namespace DSMM.Common
{
    public class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PauseScreenUI), "Show")]
        public static void OnShow(bool instant = false)
        {
            if (NetworkManager.Instance.IsConnected())
                Time.timeScale = 1.0f;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController), "Awake")]
        public static void OnPlayerControllerAwake(ref bool __runOriginal)
        {
            if (NetworkManager.Instance.IsConnected() && PlayerController.Instance != null)
                __runOriginal = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SwordTrigger), "OnTriggerEnter2D")]
        public static void OnSwordTriggerEnter2D(SwordTrigger __instance, Collider2D collision)
        {
            if(NetworkManager.Instance.IsConnected())
            {
                StageChangePacket packet = new StageChangePacket
                {
                    StageID = (int)__instance._achievementId,
                };

                NetworkManager.Instance.SendPacketToAll(packet, true, EP2PSend.k_EP2PSendUnreliableNoDelay);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController), "Update")]
        public static void OnPlayerUpdate(PlayerController __instance, ref bool __runOriginal)
        {
            if (NetworkManager.Instance.IsConnected()) 
            {
                if(__instance.gameObject.name != SteamUser.GetSteamID().m_SteamID.ToString())
                {
                    __runOriginal = false;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerController), "TeleportPlayer")]
        public static void OnPlayerTeleport(PlayerController __instance, Vector3 pos)
        {
            if (NetworkManager.Instance.IsConnected())
            {
                if (__instance.gameObject.name != SteamUser.GetSteamID().m_SteamID.ToString())
                    return;

                PlayerTeleportPacket packet = new PlayerTeleportPacket
                {
                    Position = new Math.Vector3(pos)
                };

                NetworkManager.Instance.SendPacketToAll(packet);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Sword), "Grow")]
        public static void OnSwordGrow(Sword __instance)
        {
            if (NetworkManager.Instance.IsConnected())
            {
                if (NetworkManager.Instance.IsCLient && !NetworkManager.Instance.HaveRecievePrimaryInfo)
                    return;

                if (__instance.transform.parent.parent.gameObject.name != SteamUser.GetSteamID().m_SteamID.ToString())
                    return;

                SwordChangePacket packet = new SwordChangePacket
                {
                    SwordChangeType = SwordChangeType.Grow
                };

                NetworkManager.Instance.SendPacketToAll(packet);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Sword), "Shrink")]
        public static void OnSwordShrink(Sword __instance)
        {
            if (NetworkManager.Instance.IsConnected())
            {
                if (NetworkManager.Instance.IsCLient && !NetworkManager.Instance.HaveRecievePrimaryInfo)
                    return;

                if (__instance.transform.parent.parent.gameObject.name != SteamUser.GetSteamID().m_SteamID.ToString())
                    return;

                SwordChangePacket packet = new SwordChangePacket
                {
                    SwordChangeType = SwordChangeType.Shrink
                };

                NetworkManager.Instance.SendPacketToAll(packet);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameplayUtils), "RestartGame")]
        public static void OnRestartGamePrefix(ref bool __runOriginal, bool clearLaps = false)
        {
            if (NetworkManager.Instance.IsConnected())
                __runOriginal = false;      
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Main), "Start")]
        public static void OnStart()
        {
            if (UIManager.Instance == null)
                return;

            if (UIManager.Instance.StartMultiplayerButton != null)
                return;

            UIManager.Instance.CreateMultiplayerButton();
            UIManager.Instance.CreateMultiplayerSubMenu();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Main), "Update")]
        public static void OnUpdateMain(ref bool __runOriginal)
        {
            if (NetworkManager.Instance.IsConnected())
            {
                __runOriginal = false;

                switch (Main.Instance._state)
                {
                    case GameState.Title:
                        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                        {
                            Main.Instance.StartGame();
                        }

                        return;
                    case GameState.Game:
                        Main.Instance._playtime += Time.deltaTime;
                        _totalLapTime += Time.deltaTime;
                        break;
                    case GameState.Complete:
                        if (Input.GetMouseButtonDown(0))
                        {
                            Main.Instance._state = GameState.Restarting;

                            RestartGamePacket packet = new RestartGamePacket
                            {
                                Timestamp = (float)Utils.GetUnixTime()
                            };

                            NetworkManager.Instance.SendPacketToAll(packet, false);
                        }

                        return;
                }

                for (int i = 0; i < BaseEntity.s_allEntities.Count; i++)
                {
                    BaseEntity.s_allEntities[i].TriggerUpdate();
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Sword), "OnCollisionEnter2D")]
        public static void OnSwordCollisionEnter(Sword __instance, ref bool __runOriginal, Collider2D collision)
        {
            if (NetworkManager.Instance.IsConnected())
            {
                if (__instance.transform.parent.parent.gameObject.name != SteamUser.GetSteamID().m_SteamID.ToString())
                    __runOriginal = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BaseActor), "ControlledFixedUpdate")]
        public static void OnControlledFixedUpdatePrefix(BaseActor __instance, ref bool __runOriginal)
        {
            if (NetworkManager.Instance.IsConnected())
            {
                if (ulong.TryParse(__instance.gameObject.transform.parent.gameObject.name, out ulong steamIDValue))
                {
                    CSteamID steamID = new CSteamID(steamIDValue);

                    if(NetworkManager.Instance.GetLocalPlayer().SteamID != steamID.m_SteamID)
                    {
                        if (!NetworkManager.Instance.IsPlayer(steamID.m_SteamID))
                            return;

                        __runOriginal = false;

                        float velocityMagnitud = NetworkManager.Instance.GetPlayer(steamID.m_SteamID).VelocityMagnitude;

                        var propertyIsGrounded = typeof(BaseActor).GetProperty("_isGrounded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var fieldFallThroughTiles = typeof(BaseActor).GetField("_fallThroughTiles", BindingFlags.Instance | BindingFlags.NonPublic);

                        HashSet<Collider2D> collider2Ds = (HashSet<Collider2D>)fieldFallThroughTiles.GetValue(__instance);

                        RaycastHit2D raycastHit2D = Physics2D.CircleCast(__instance.gameObject.transform.position + Vector3.up * 0.5f, 0.25f, Vector2.down, 0.65f, LayerUtils.DEFAULT_MASK);
                        bool isGrounded = __instance._isGrounded;
                        propertyIsGrounded.SetValue(__instance, raycastHit2D.collider != null && !collider2Ds.Contains(raycastHit2D.collider));
                        if (__instance._animator != null)
                        {
                            __instance._animator._isGrounded = __instance._isGrounded;
                        }

                        if (isGrounded != __instance._isGrounded)
                        {
                            if (__instance._isGrounded)
                            {
                                __instance._jumpCount = 0;
                                AudioManager.Instance.PlayOneShot(__instance._landSfx, __instance._landSfxVolume);
                            }
                            else
                            {
                                AudioManager.Instance.PlayOneShot(__instance._liftSfx, __instance._liftSfxVolume);
                            }
                        }

                        if (__instance._animator != null)
                        {
                            float moveSpeed = Mathf.Clamp01(velocityMagnitud / __instance._maxMoveSpeed);
                            __instance._animator.SetMoveSpeed(moveSpeed);
                        }
                    }
                }
            }
        }
    }
}