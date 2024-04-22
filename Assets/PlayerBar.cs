using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class PlayerBar : MonoBehaviour
{
    public string playerName;
    public ulong playerId;
    private bool avatarRecieved = false;

    public TMP_Text playerNameText;
    public RawImage playerIcon;
    public Image ready;

    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    private void Start()
    {
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    void GetPlayerIcon()
    {
        int imageID = SteamFriends.GetLargeFriendAvatar((CSteamID)playerId);
        if(imageID == -1)
        {
            return;
        }
        playerIcon.texture = SteamIconToTexture(imageID);
    }

    public void SetPlayerValues()
    {
        playerNameText.text = playerName;
        if(!avatarRecieved)
        {
            GetPlayerIcon();
        }
    }

    private void Update()
    {
        if(playerNameText.text == "")
        {
            playerNameText.text = "Loading...";
        }
    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if(callback.m_steamID.m_SteamID == playerId)
        {
            playerIcon.texture = SteamIconToTexture(callback.m_iImage);
        }
    }

    private Texture2D SteamIconToTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        avatarRecieved = true;
        return texture;
    }
}
