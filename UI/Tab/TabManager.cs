using DSMM.Network;
using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

namespace DSMM.UI.Tab
{
    public class TabManager : MonoBehaviour
    {
        public TextMeshProUGUI Title;
        public TextMeshProUGUI Ping;
        public RectTransform Background;
        public GameObject Layout;
        public GameObject Tab;
        public GameObject ExamplePlayerItem;

        public void Create()
        {
            if (Tab != null)
                return;

            Tab = new GameObject("[Tab]");

            Tab.SetActive(false);

            Tab.transform.SetParent(Main.Instance.gameObject.transform.GetChild(1).gameObject.transform);
            RectTransform TabRec = Tab.AddComponent<RectTransform>();

            TabRec.anchoredPosition = new Vector2(-1.5259e-05f, 178.6f);
            TabRec.localScale = Vector3.one;
            TabRec.sizeDelta = new Vector2(600, 163.6657f);

            GameObject Background = new GameObject("Background");
            Background.transform.SetParent(Tab.transform);

            Image BackgroundImage = Background.AddComponent<Image>();
            this.Background = Background.GetComponent<RectTransform>();

            BackgroundImage.color = new Color(0f, 0f, 0f, 0.6f);

            this.Background.anchoredPosition = Vector2.zero;
            this.Background.localScale = Vector3.one;
            this.Background.sizeDelta = new Vector2(200, 163.6657f);

            GameObject Title = TMP_DefaultControls.CreateText(new TMP_DefaultControls.Resources());
            Title.name = "Title";
            Title.transform.SetParent(Tab.transform);

            RectTransform TitleRec = Title.GetComponent<RectTransform>();

            TitleRec.anchoredPosition = new Vector2(-0.83823f, 66.858f);
            TitleRec.localScale = Vector3.one;
            TitleRec.sizeDelta = new Vector2(586.29f, 26.875f);

            this.Title = Title.GetComponent<TextMeshProUGUI>();
            this.Title.fontSize = 16;
            this.Title.text = "Lobby";
            this.Title.horizontalAlignment = HorizontalAlignmentOptions.Center;
            this.Title.verticalAlignment = VerticalAlignmentOptions.Middle;

            this.Layout = new GameObject("PlayerList");
            this.Layout.transform.SetParent(Tab.transform);

            RectTransform GridLayoutRec = this.Layout.AddComponent<RectTransform>();

            GridLayoutRec.anchoredPosition = new Vector2(-0.8379822f, -5.7711f);
            GridLayoutRec.localScale = Vector3.one;
            GridLayoutRec.sizeDelta = new Vector2(586.287f, 110.0737f);

            GridLayoutGroup GridLayout = this.Layout.AddComponent<GridLayoutGroup>();

            GridLayout.cellSize = new Vector2(180, 20);
            GridLayout.spacing = new Vector2(10, 10);
            GridLayout.childAlignment = TextAnchor.UpperCenter;
            GridLayout.startAxis = Axis.Vertical;

            GameObject Ping = TMP_DefaultControls.CreateText(new TMP_DefaultControls.Resources());
            Ping.name = "Ping";
            Ping.transform.SetParent(Tab.transform);

            RectTransform PingRec = Ping.GetComponent<RectTransform>();

            PingRec.anchoredPosition = new Vector2(-0.83823f, -69f);
            PingRec.localScale = Vector3.one;
            PingRec.sizeDelta = new Vector2(586.29f, 16.049f);

            this.Ping = Ping.GetComponent<TextMeshProUGUI>();
            this.Ping.fontSize = 12;
            this.Ping.text = "Ping";
            this.Ping.horizontalAlignment = HorizontalAlignmentOptions.Center;
            this.Ping.verticalAlignment = VerticalAlignmentOptions.Middle;

            CreateExamplePlayerItem();
        }

        public void CreateExamplePlayerItem()
        {
            ExamplePlayerItem = new GameObject("Example");
            ExamplePlayerItem.transform.SetParent(Layout.transform);
            ExamplePlayerItem.gameObject.SetActive(false);

            Image ExampleBackground = ExamplePlayerItem.AddComponent<Image>();

            ExampleBackground.color = new Color(0.576f, 0.576f, 0.576f, 0.318f);

            RectTransform ExampleRec = ExamplePlayerItem.GetComponent<RectTransform>();
            ExampleRec.localScale = Vector3.one;

            GameObject Profile = new GameObject("Profile");
            Profile.transform.SetParent(ExamplePlayerItem.transform);

            RectTransform ProfileRec = Profile.AddComponent<RectTransform>();

            ProfileRec.anchoredPosition = new Vector2(-80f, 9.5367e-06f);
            ProfileRec.localScale = Vector3.one;
            ProfileRec.sizeDelta = new Vector2(20f, 20f);
            ProfileRec.localEulerAngles = new Vector3(0, 180, 180);

            RawImage ProfileImg = Profile.AddComponent<RawImage>();

            GameObject Username = TMP_DefaultControls.CreateText(new TMP_DefaultControls.Resources());
            Username.name = "Username";
            Username.transform.SetParent(ExamplePlayerItem.transform);

            RectTransform UsernameRec = Username.GetComponent<RectTransform>();

            UsernameRec.anchoredPosition = new Vector2(12.781f, 7.1526e-06f);
            UsernameRec.localScale = Vector3.one;
            UsernameRec.sizeDelta = new Vector2(154.4379f, 20f);

            TextMeshProUGUI UsernameText = Username.GetComponent<TextMeshProUGUI>();

            UsernameText.fontSize = 12;
            UsernameText.alignment = TextAlignmentOptions.MidlineLeft;

            PlayerItem playerItem = ExamplePlayerItem.AddComponent<PlayerItem>();
            playerItem.ProfilePic = ProfileImg;
            playerItem.Username = UsernameText;
        }

        void Update()
        {
            if (Tab == null)
                return;

            if (Input.GetKey(KeyCode.Tab) && NetworkManager.Instance.IsConnected() && !NetworkManager.Instance.IsConnecting && !PauseButtonUI.Instance._isPaused)
            {
                Tab.SetActive(true);
            }
            else
            {
                Tab.SetActive(false);
            }

            UpdateBackgroundSize();
            UpdateLobbyName();
            UpdatePing();
        }

        public void UpdateLobbyName()
        {
            if (!NetworkManager.Instance.IsConnected())
                return;

            Title.text = NetworkManager.Instance.SteamLobby.LobbyName;
        }

        public void UpdatePing()
        {
            if (!NetworkManager.Instance.IsConnected())
                return;

            int ping = Convert.ToInt32(NetworkManager.Instance.Ping);

            string color;

            switch(ping)
            {
                case int n when (n < 80):
                    color = "green";
                    break;
                case int n when (n >= 80 && n < 150):
                    color = "yellow";
                    break;
                case int n when (n >= 150):
                    color = "red";
                    break;
                default:
                    color = "white";
                    break;
            }

            Ping.text = $"Ping: <color={color}>{ping}</color>";
        }

        public void AddPlayer(CSteamID steamID)
        {
            GameObject player = Instantiate(ExamplePlayerItem, Vector3.zero, Quaternion.identity);
            player.transform.SetParent(Layout.transform);

            player.GetComponent<RectTransform>().localScale = Vector3.one;
                
            player.GetComponent<PlayerItem>().CSteamID = steamID;
            player.GetComponent<PlayerItem>().Init();

            player.gameObject.SetActive(true);
        }

        public void RemovePlayer(CSteamID steamID)
        {
            foreach(Transform child in Layout.transform)
            {
                if(child.name == steamID.ToString())
                {
                    Destroy(child.gameObject);
                    return;
                }
            }
        }

        public void UpdateBackgroundSize()
        {
            if (!NetworkManager.Instance.IsConnected())
                return;

            int sizeX = 0;

            switch (NetworkManager.Instance.Players.Count)
            {
                case int n when (n < 7):
                    sizeX = 400;
                    break;
                case int n when (n >= 9):
                    sizeX = 600;
                    break;
            }

            Background.sizeDelta = new Vector2(sizeX, Background.sizeDelta.y);
        }
    }
}
