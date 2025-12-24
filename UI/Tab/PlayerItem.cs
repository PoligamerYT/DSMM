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
            ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);

            int imageId = SteamFriends.GetLargeFriendAvatar(CSteamID);

            if (imageId > 0)
            {
                ProfilePic.texture = Utils.GetSteamImageAsTexture(imageId);
            }
            else
            {
                ProfilePic.texture = Utils.GetImageFromResourcesAsTexture("DSMM.Resources.default_avatar.png");
            }
        }

        private void OnImageLoaded(AvatarImageLoaded_t callback)
        {
            if (callback.m_steamID != CSteamID)
                return;

            ProfilePic.texture = Utils.GetSteamImageAsTexture(callback.m_iImage);
        }

        private void UpdateUsername()
        {
            name = CSteamID.ToString();
            Username.text = SteamFriends.GetFriendPersonaName(CSteamID);
        }

        private void OnDestroy()
        {
            ImageLoaded?.Dispose();
        }
    }
}
