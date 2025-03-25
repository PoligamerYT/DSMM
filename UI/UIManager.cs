using DSMM.Network;
using DSMM.Network.Enums;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DSMM.UI
{
    public class UIManager : MonoBehaviour
    {
        public Button MultiplayerButton;
        public GameObject MultiplayerSubMenu;
        public TextMeshProUGUI MaxPlayersText;
        public Toggle FriendsOnlyTogle;
        public Button StartMultiplayerButton;
        public Button StopMultiplayerButton;
        public Button InviteFriendsButton;
        public Button CancelMutliplayerButton;
        public TextMeshProUGUI LoadingText;
        public GameObject WheelLayout;
        public Image PlayerViewer;
        public GameObject GameModeLayout;
        public Toggle VanillaGameModeToggle;
        public Toggle CoopChaosGameModeToggle;

        public static UIManager Instance;

        public void Awake()
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

        public void Start()
        {
            CreateMultiplayerButton();
            CreateMultiplayerSubMenu();
        }

        public void CreateMultiplayerButton()
        {
            if (MultiplayerButton != null)
                return;

            VerticalLayoutGroup layout = Main.Instance.transform.GetChild(1).GetChild(6).GetChild(2).GetChild(1).GetComponent<VerticalLayoutGroup>();

            GameObject multiplayerButton = Instantiate(layout.transform.GetChild(3).gameObject, layout.transform);
            
            GameObject title = layout.transform.parent.GetChild(0).gameObject;

            multiplayerButton.name = "Button - Multiplayer";
            multiplayerButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Multiplayer";
            multiplayerButton.transform.SetSiblingIndex(2);

            title.transform.position = new Vector3(title.transform.position.x, title.transform.position.y + 70, 0);

            MultiplayerButton = multiplayerButton.GetComponent<Button>();

            MultiplayerButton.onClick = new Button.ButtonClickedEvent();

            MultiplayerButton.onClick.AddListener(OnClickMultiplayerButton);
        }

        public void CreateMultiplayerSubMenu()
        {
            if (MultiplayerSubMenu != null)
                return;

            GameObject parent = Main.Instance.transform.GetChild(1).GetChild(6).GetChild(1).gameObject;

            GameObject multiplayerSubNenu = Instantiate(parent.transform.GetChild(2).gameObject, parent.transform);

            multiplayerSubNenu.name = "Panel - Multiplayer";

            multiplayerSubNenu.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "MULTIPLAYER";

            VerticalLayoutGroup layout = multiplayerSubNenu.transform.GetChild(1).GetComponent<VerticalLayoutGroup>();

            layout.transform.GetChild(0).gameObject.name = "Text (TMP) - Max Players";

            MaxPlayersText = layout.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            MaxPlayersText.text = "PLAYERS: 5";
            MaxPlayersText.fontSize = 14;

            Slider sliderMaxPlayers = layout.transform.GetChild(0).GetChild(0).GetComponent<Slider>();

            sliderMaxPlayers.maxValue = 8;
            sliderMaxPlayers.minValue = 2;
            sliderMaxPlayers.value = 5;

            sliderMaxPlayers.wholeNumbers = true;

            sliderMaxPlayers.onValueChanged = new Slider.SliderEvent();

            sliderMaxPlayers.onValueChanged.AddListener(OnPlayersSliderChange);

            Destroy(layout.transform.GetChild(1).gameObject);

            multiplayerSubNenu.transform.SetSiblingIndex(3);

            multiplayerSubNenu.SetActive(false);

            List<GameObject> subMenusList = GetPauseScreenUI()._subMenus.ToList();

            subMenusList.Add(multiplayerSubNenu);

            GetPauseScreenUI()._subMenus = subMenusList.ToArray();

            GameObject loadingText = Instantiate(MaxPlayersText.gameObject, layout.transform);

            Destroy(loadingText.transform.GetChild(0).gameObject);

            loadingText.name = "Text (TMP) - Loading";

            LoadingText = loadingText.GetComponent<TextMeshProUGUI>();

            LoadingText.alignment = TextAlignmentOptions.Center;
            LoadingText.text = "Loading...";
            LoadingText.fontSize = 40;

            LoadingText.gameObject.SetActive(false);

            GameObject startMultiplayerButton = Instantiate(parent.transform.GetChild(4).GetChild(1).GetChild(6).gameObject, layout.transform);

            startMultiplayerButton.name = "Button - Start Multiplayer";
            startMultiplayerButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Start Multiplayer";
            startMultiplayerButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 14;

            StartMultiplayerButton = startMultiplayerButton.GetComponent<Button>();

            StartMultiplayerButton.onClick = new Button.ButtonClickedEvent();

            StartMultiplayerButton.onClick.AddListener(OnClickStartMultiplayerButton);

            DestroyImmediate(layout.transform.GetChild(3).gameObject);

            GameObject friendsOnlyTogle = layout.transform.GetChild(3).gameObject;

            friendsOnlyTogle.transform.SetSiblingIndex(1);

            friendsOnlyTogle.gameObject.name = "Text (TMP) - Friends Only";

            friendsOnlyTogle.GetComponent<TextMeshProUGUI>().text = "FRIENDS ONLY";
            friendsOnlyTogle.GetComponent<TextMeshProUGUI>().fontSize = 14;

            FriendsOnlyTogle = friendsOnlyTogle.transform.GetChild(0).GetComponent<Toggle>();

            FriendsOnlyTogle.onValueChanged = new Toggle.ToggleEvent();

            GameObject inviteFriendButton = Instantiate(parent.transform.GetChild(4).GetChild(1).GetChild(6).gameObject, layout.transform);

            inviteFriendButton.name = "Button - Invite Friends";
            inviteFriendButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Invite Friends";
            inviteFriendButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 14;

            InviteFriendsButton = inviteFriendButton.GetComponent<Button>();

            InviteFriendsButton.onClick = new Button.ButtonClickedEvent();

            InviteFriendsButton.onClick.AddListener(OnClickInviteFriendsButton);

            InviteFriendsButton.gameObject.SetActive(false);

            GameObject gameModeLayout = new GameObject();

            gameModeLayout.transform.SetParent(layout.transform);
            gameModeLayout.name = "Game Mode Layout";
            gameModeLayout.transform.localPosition = Vector3.zero;
            gameModeLayout.transform.localScale = Vector3.one;
            gameModeLayout.AddComponent<RectTransform>();
            gameModeLayout.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 80);
            gameModeLayout.transform.SetSiblingIndex(2);

            HorizontalLayoutGroup gameModeHorizontalLayout = gameModeLayout.AddComponent<HorizontalLayoutGroup>();

            GameModeLayout = gameModeLayout.gameObject;

            gameModeHorizontalLayout.childForceExpandWidth = false;
            gameModeHorizontalLayout.childForceExpandHeight = false;
            gameModeHorizontalLayout.childControlWidth = false;
            gameModeHorizontalLayout.childControlHeight = false;

            GameObject gameModeObjectText = new GameObject();

            gameModeObjectText.transform.parent = gameModeLayout.transform;
            gameModeObjectText.name = "Game Mode Text";
            gameModeObjectText.transform.localPosition = Vector3.zero;
            gameModeObjectText.transform.localScale = Vector3.one;

            TextMeshProUGUI gameModeText = gameModeObjectText.AddComponent<TextMeshProUGUI>();

            gameModeText.alignment = TextAlignmentOptions.Left;
            gameModeText.fontSize = 14;
            gameModeText.text = "GAME MODES";

            GameObject gameModeVerticalLayout = new GameObject();

            gameModeVerticalLayout.transform.parent = gameModeLayout.transform;
            gameModeVerticalLayout.name = "Game Mode Togles";
            gameModeVerticalLayout.transform.localPosition = Vector3.zero;
            gameModeVerticalLayout.transform.localScale = Vector3.one;

            VerticalLayoutGroup gameModeVerticalLayout_ = gameModeVerticalLayout.AddComponent<VerticalLayoutGroup>();

            gameModeVerticalLayout_.childForceExpandWidth = false;
            gameModeVerticalLayout_.childForceExpandHeight = false;
            gameModeVerticalLayout_.childControlWidth = false;
            gameModeVerticalLayout_.childControlHeight = false;

            gameModeVerticalLayout.AddComponent<ToggleGroup>();

            GameObject vanillaMode = Instantiate(FriendsOnlyTogle.transform.parent.gameObject, gameModeVerticalLayout.transform);

            vanillaMode.GetComponent<TextMeshProUGUI>().text = "VANILLA";
            vanillaMode.transform.GetChild(0).GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();
            vanillaMode.transform.GetChild(0).GetComponent<Toggle>().group = gameModeVerticalLayout.GetComponent<ToggleGroup>();

            VanillaGameModeToggle = vanillaMode.transform.GetChild(0).GetComponent<Toggle>();

            vanillaMode.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            GameObject coopChaosMode = Instantiate(FriendsOnlyTogle.transform.parent.gameObject, gameModeVerticalLayout.transform);

            coopChaosMode.GetComponent<TextMeshProUGUI>().text = "CO-OP CHAOS";
            coopChaosMode.transform.GetChild(0).GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();
            coopChaosMode.transform.GetChild(0).GetComponent<Toggle>().isOn = false;
            coopChaosMode.transform.GetChild(0).GetComponent<Toggle>().group = gameModeVerticalLayout.GetComponent<ToggleGroup>();

            CoopChaosGameModeToggle = coopChaosMode.transform.GetChild(0).GetComponent<Toggle>();

            coopChaosMode.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            GameObject stopMultiplaterButton = Instantiate(parent.transform.GetChild(4).GetChild(1).GetChild(6).gameObject, layout.transform);

            stopMultiplaterButton.name = "Button - Stop Multiplayer";
            stopMultiplaterButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Stop Multiplayer";
            stopMultiplaterButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 14;

            StopMultiplayerButton = stopMultiplaterButton.GetComponent<Button>();

            StopMultiplayerButton.onClick = new Button.ButtonClickedEvent();

            StopMultiplayerButton.onClick.AddListener(OnClickStopMultiplayerButton);

            StopMultiplayerButton.gameObject.SetActive(false);

            GameObject cancelMutliplayerButton = Instantiate(parent.transform.GetChild(4).GetChild(1).GetChild(6).gameObject, layout.transform);

            cancelMutliplayerButton.name = "Button - Cancel Multiplayer";
            cancelMutliplayerButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Cancel";
            cancelMutliplayerButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 14;

            CancelMutliplayerButton = cancelMutliplayerButton.GetComponent<Button>();

            CancelMutliplayerButton.onClick = new Button.ButtonClickedEvent();

            CancelMutliplayerButton.onClick.AddListener(OnClickCancelMutliplayerButton);

            CancelMutliplayerButton.gameObject.SetActive(false);

            WheelLayout = new GameObject();

            WheelLayout.transform.SetParent(layout.transform);

            WheelLayout.transform.localPosition = Vector3.zero;

            WheelLayout.AddComponent<HorizontalLayoutGroup>();

            WheelLayout.SetActive(false);

            GameObject colorWheel = new GameObject();

            colorWheel.transform.SetParent(WheelLayout.transform);
            colorWheel.name = "Color Wheel";
            colorWheel.transform.localScale = new Vector3(2, 2, 2);

            colorWheel.transform.localPosition = new Vector3(-60, 0, 0);

            Image colorWheelSprite = colorWheel.AddComponent<Image>();

            Texture2D texture = DrawColorWheel(150);

            colorWheelSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            colorWheelSprite.preserveAspect = true;

            colorWheel.GetComponent<RectTransform>().sizeDelta = new Vector2(45, 45);

            colorWheel.AddComponent<ColorPicker>();

            GameObject playerViewer = new GameObject();

            playerViewer.transform.SetParent(WheelLayout.transform);

            playerViewer.transform.localPosition = new Vector3(60, 0, 0);

            playerViewer.name = "Player Viewer";

            PlayerViewer = playerViewer.AddComponent<Image>();

            PlayerViewer.sprite = PlayerController.Instance._playerActor._sprite.sprite;
        }

        public Texture2D DrawColorWheel(int size)
        {
            Texture2D colorWheelTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size / 2f; // Half the size to center the wheel

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Convert (x, y) to coordinates centered at the middle of the texture
                    float dx = x - radius;
                    float dy = y - radius;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg; // Angle in degrees
                    if (angle < 0) angle += 360f; // Normalize angle to [0, 360]

                    // Normalize distance to [0, 1], ensuring outer edge is max and inner center is white
                    float saturation = Mathf.Clamp01(distance / radius);
                    float value = 1f; // Full brightness
                    Color color = Color.HSVToRGB(angle / 360f, saturation, value);

                    // Ensure center is white
                    if (distance < radius * 0.2f) // Adjust the 0.2 to control the white center size
                    {
                        color = Color.Lerp(Color.white, color, distance / (radius * 0.2f));
                    }

                    colorWheelTexture.SetPixel(x, y, color);
                }
            }

            colorWheelTexture.Apply();

            return colorWheelTexture;
        }

        public PauseScreenUI GetPauseScreenUI()
        {
            return Main.Instance.transform.GetChild(1).GetChild(6).GetComponent<PauseScreenUI>();
        }

        public void OnClickMultiplayerButton()
        {
            GetPauseScreenUI().ShowSubmenu(4);
        }

        public void OnClickCancelMutliplayerButton()
        {
            NetworkManager.Instance.CancelMultiplayer();
        }

        public void OnPlayersSliderChange(float value)
        {
            MaxPlayersText.text = "PLAYERS: " + value;
            NetworkManager.Instance.MaxPlayers = Convert.ToInt32(value);
        }

        public ELobbyType GetLobbyType()
        {
            if (FriendsOnlyTogle.isOn)
            {
                return ELobbyType.k_ELobbyTypeFriendsOnly;
            }
            else
            {
                return ELobbyType.k_ELobbyTypePrivate;
            }
        }

        public GameMode GetGameMode()
        {
            if (VanillaGameModeToggle.isOn)
            {
                return GameMode.Vanilla;
            }
            else
            {
                return GameMode.CoOpChaos;
            }
        }

        public void OnClickStartMultiplayerButton()
        {
            Loading();

            NetworkManager.Instance.HostLobby();
        }

        public void Loading()
        {
            MaxPlayersText.gameObject.SetActive(false);
            FriendsOnlyTogle.transform.parent.gameObject.SetActive(false);
            StartMultiplayerButton.gameObject.SetActive(false);
            LoadingText.gameObject.SetActive(true);
            CancelMutliplayerButton.gameObject.SetActive(true);
            GameModeLayout.SetActive(false);
        }

        public void OnClickInviteFriendsButton()
        {
            SteamFriends.ActivateGameOverlayInviteDialog(NetworkManager.Instance.SteamLobby.CurrentLobbyID);
        }

        public void OnClickStopMultiplayerButton()
        {
            NetworkManager.Instance.SteamLobby.LeaveLobby(LeaveType.Unknown);
        }

        public void OnEnterLobby()
        {
            StartMultiplayerButton.gameObject.SetActive(false);
            InviteFriendsButton.gameObject.SetActive(true);
            StopMultiplayerButton.gameObject.SetActive(true);
            MaxPlayersText.gameObject.SetActive(false);
            FriendsOnlyTogle.transform.parent.gameObject.SetActive(false);
            LoadingText.gameObject.SetActive(false);
            CancelMutliplayerButton.gameObject.SetActive(false);
            GameModeLayout.SetActive(false);

            Time.timeScale = 1f;
        }

        public void OnLeaveLobby()
        {
            InviteFriendsButton.gameObject.SetActive(false);
            StopMultiplayerButton.gameObject.SetActive(false);
            StartMultiplayerButton.gameObject.SetActive(true);
            MaxPlayersText.gameObject.SetActive(true);
            FriendsOnlyTogle.transform.parent.gameObject.SetActive(true);
            LoadingText.gameObject.SetActive(false);
            CancelMutliplayerButton.gameObject.SetActive(false);
            GameModeLayout.SetActive(true);
        }
    }
}
