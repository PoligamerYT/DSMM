using DSMM.Common;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DSMM.UI.Tab
{
    public class PlayerItem : MonoBehaviour
    {
        public CSteamID CSteamID;

        public TextMeshProUGUI Username;
        public RawImage ProfilePic;

        protected Callback<AvatarImageLoaded_t> ImageLoaded;

        public void Init()
        {
            LoadImage();

            UpdateUsername();
        }

        private void LoadImage()
        {
            ProfilePic.texture = Utils.GetSteamImageAsTexture(SteamFriends.GetLargeFriendAvatar(CSteamID));
        }

        private void UpdateUsername()
        {
            name = CSteamID.ToString();
            Username.text = SteamFriends.GetFriendPersonaName(CSteamID);
        }
    }
}
