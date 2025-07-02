using DSMM.Network;
using DSMM.Network.Enums;
using Steamworks;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = System.Random;
using Vector3 = DSMM.Math.Vector3;

namespace DSMM.Common
{
    public class Utils
    {
        public static void CreatePlayer(ulong steamId)
        {
            if (NetworkManager.Instance.Players.Where(p => p.SteamID == steamId).Count() != 0)
                return;

            Player player = new Player
            {
                SteamID = steamId,

                PlayerPosition = new Vector3(-3, 1, 0)
            };

            PlayerController playerController = GameObject.Instantiate(PlayerController.Instance.gameObject).GetComponent<PlayerController>();
            playerController.gameObject.name = steamId.ToString();
            playerController._allowControl = false;
            playerController._playerActor._rigidBody.gravityScale = 0;
            playerController._playerActor._collider.gameObject.SetActive(false);
            playerController._sword.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
            playerController._sword._model.gameObject.SetActive(false);
            playerController._playerActor.gameObject.transform.position = player.PlayerPosition.GetVector3();

            if (playerController._playerActor._sprite.transform.childCount > 0)
                GameObject.Destroy(playerController._playerActor._sprite.transform.GetChild(0).gameObject);

            GameObject username = new GameObject("Username");
            username.transform.parent = playerController._playerActor._sprite.gameObject.transform;
            username.transform.localPosition = new UnityEngine.Vector3(0, 1.5f, 0);

            TextMeshPro usernameText = username.AddComponent<TextMeshPro>();
            usernameText.text = NetworkManager.Instance.CurrentGameMode == GameMode.CoOpChaos ? NetworkManager.Instance.CurrentControlType.ToString() : SteamFriends.GetFriendPersonaName(new CSteamID(player.SteamID));
            usernameText.alignment = TextAlignmentOptions.Center;
            usernameText.fontSize = 2;

            NetworkManager.Instance.Players.Add(player);
        }

        public static void CreatePlayer(Player player)
        {
            if (NetworkManager.Instance.Players.Where(p => p.SteamID == player.SteamID).Count() != 0)
                return;

            PlayerController playerController = GameObject.Instantiate(PlayerController.Instance.gameObject).GetComponent<PlayerController>();
            playerController.gameObject.name = player.SteamID.ToString();
            playerController._allowControl = false;
            playerController._playerActor._rigidBody.gravityScale = 0;
            playerController._playerActor._collider.gameObject.SetActive(false);
            playerController._sword.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
            playerController._sword._model.gameObject.SetActive(false);
            playerController._playerActor.gameObject.transform.position = player.PlayerPosition.GetVector3();
            playerController._sword.gameObject.transform.position = player.SwordPosition.GetVector3();
            playerController._sword.gameObject.transform.rotation = Quaternion.Euler(0, 0, player.SwordRotation);

            if(playerController._playerActor._sprite.transform.childCount > 0)
                GameObject.Destroy(playerController._playerActor._sprite.transform.GetChild(0).gameObject);

            GameObject username = new GameObject("Username");
            username.transform.parent = playerController._playerActor._sprite.gameObject.transform;
            username.transform.localPosition = new UnityEngine.Vector3(0, 1.5f, 0);

            TextMeshPro usernameText = username.AddComponent<TextMeshPro>();
            usernameText.text = NetworkManager.Instance.CurrentGameMode == GameMode.CoOpChaos ? NetworkManager.Instance.CurrentControlType.ToString() : SteamFriends.GetFriendPersonaName(new CSteamID(player.SteamID));
            usernameText.alignment = TextAlignmentOptions.Center;
            usernameText.fontSize = 2;

            NetworkManager.Instance.Players.Add(player);
        }

        public static void CreatePlayer(PlayerController playerController)
        {
            if (NetworkManager.Instance.Players.Where(p => p.SteamID == SteamUser.GetSteamID().m_SteamID).Count() != 0)
                return;

            Player player = new Player
            {
                SteamID = SteamUser.GetSteamID().m_SteamID,

                PlayerPosition = new Vector3(playerController._playerActor.gameObject.transform.position),
                SwordPosition = new Vector3(playerController._sword.gameObject.transform.position),
                SwordRotation = playerController._sword.gameObject.transform.rotation.eulerAngles.z
            };

            PlayerController.Instance.gameObject.name = SteamUser.GetSteamID().m_SteamID.ToString();

            GameObject username = new GameObject("Username");
            username.transform.parent = PlayerController.Instance._playerActor._sprite.gameObject.transform;
            username.transform.localPosition = new UnityEngine.Vector3(0, 1.5f, 0);

            TextMeshPro usernameText = username.AddComponent<TextMeshPro>();
            usernameText.text = NetworkManager.Instance.CurrentGameMode == GameMode.CoOpChaos ? NetworkManager.Instance.CurrentControlType.ToString() : SteamFriends.GetFriendPersonaName(new CSteamID(player.SteamID));
            usernameText.alignment = TextAlignmentOptions.Center;
            usernameText.fontSize = 2;

            NetworkManager.Instance.Players.Add(player);
        }

        public static void CreateProxyPlayer()
        {
            PlayerController proxyPlayerController = GameObject.Instantiate(PlayerController.Instance.gameObject).GetComponent<PlayerController>();
            proxyPlayerController.gameObject.name = "[ProxyPlayerController]";
            proxyPlayerController._allowControl = true;
            proxyPlayerController._playerActor._sprite.gameObject.SetActive(false);
            proxyPlayerController._sword._swordSpriteRenderer.gameObject.SetActive(false);

            PlayerController playerController = PlayerController.Instance;

            playerController._allowControl = false;
            playerController._playerActor._rigidBody.gravityScale = 0;
            playerController._playerActor._collider.gameObject.SetActive(false);
            playerController._sword.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
            playerController._sword._model.gameObject.SetActive(false);
        }

        public static void ResetPlayer()
        {
            PlayerController playerController = PlayerController.Instance;

            playerController._allowControl = true;
            playerController._playerActor._rigidBody.gravityScale = 1;
            playerController._playerActor._collider.gameObject.SetActive(true);
            playerController._sword.gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
            playerController._sword._model.gameObject.SetActive(true);
        }

        public static double GetUnixTime()
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public static void DestroyPlayer(ulong steamId)
        {
            try
            {
                NetworkManager.Instance.Players.Remove(NetworkManager.Instance.Players[NetworkManager.Instance.Players.FindIndex(p => p.SteamID == steamId)]);

                GameObject.Destroy(GameObject.Find(steamId.ToString()));
            }
            catch { }
        }

        public static IEnumerator LerpPosition(Transform transform, Vector3 targetPos, float durationMs)
        {
            UnityEngine.Vector3 startPos = transform.position;
            UnityEngine.Vector3 unityTargetPos = targetPos.GetVector3();

            if(startPos == unityTargetPos)
            {
                yield break;
            }

            float startTime = Time.time;
            float duration = durationMs / 1000f;

            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                transform.position = UnityEngine.Vector3.Lerp(startPos, unityTargetPos, t);
                yield return null;
            }

            transform.position = unityTargetPos;
        }

        public static IEnumerator LerpRotation(Transform transform, Vector3 targetEuler, float durationMs)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(targetEuler.GetVector3());

            if (Quaternion.Angle(startRotation, targetRotation) < 0.1f)
            {
                yield break;
            }

            float startTime = Time.time;
            float duration = durationMs / 1000f;

            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            transform.rotation = targetRotation;
        }

        public static void DestroyAllPlayers()
        {
            if (NetworkManager.Instance.CurrentGameMode == GameMode.CoOpChaos)
                GameObject.Destroy(GameObject.Find("[ProxyPlayerController]"));

            foreach (Player player in NetworkManager.Instance.Players)
            {
                if (player.SteamID == SteamUser.GetSteamID().m_SteamID)
                {
                    GameObject.Destroy(player.GetPlayerController()._playerActor._sprite.transform.GetChild(0).gameObject);

                    GameObject.Find(player.SteamID.ToString()).name = "[PlayerController]";
                }
                else
                {
                    SteamNetworking.CloseP2PSessionWithUser(new CSteamID(player.SteamID));
                    GameObject.Destroy(GameObject.Find(player.SteamID.ToString()));
                }
            }

            NetworkManager.Instance.Players.Clear();
        }

        public static IEnumerator AdjustSword(int targetSwordValue)
        {
            var field = typeof(Sword).GetField("_length", BindingFlags.Instance | BindingFlags.NonPublic);

            int currentSwordValue = (int)field.GetValue(PlayerController.Instance._sword);

            while (currentSwordValue != targetSwordValue)
            {
                if (currentSwordValue < targetSwordValue)
                {
                    PlayerController.Instance._sword.Grow();
                }
                else
                {
                    PlayerController.Instance._sword.Shrink();
                }

                yield return null;
                currentSwordValue = (int)field.GetValue(PlayerController.Instance._sword);
            }
        }

        public static T GetRandomEnumValue<T>() where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            Random random = new Random();
            return (T)values.GetValue(random.Next(values.Length));
        }
    }
}
