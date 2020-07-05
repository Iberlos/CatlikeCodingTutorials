using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayFastButtonBehavior : MonoBehaviour
{
    [SerializeField]
    private Sprite pausedButtonImage = default, pausedButtonIcon = default, playButtonImage = default, playButtonIcon = default, fastButtonImage = default, fastButtonIcon = default;

    private Image buttonImage, iconImage;

    private void Start()
    {
        buttonImage = GetComponent<Image>();
        iconImage = transform.GetChild(0).GetComponent<Image>();
    }

    public void Pause()
    {
        buttonImage.sprite = pausedButtonImage;
        iconImage.sprite = pausedButtonIcon;
        Game.instance.SetPlaySpeed(GameSpeedState.Paused);
    }

    public void ToggleGameSpeed()
    {
        if(Game.instance.GameSpeedState == GameSpeedState.Paused || Game.instance.GameSpeedState == GameSpeedState.Fast)
        {
            Game.instance.SetPlaySpeed(GameSpeedState.Playing);
            buttonImage.sprite = playButtonImage;
            iconImage.sprite = playButtonIcon;
        }
        else
        {
            Game.instance.SetPlaySpeed(GameSpeedState.Fast);
            buttonImage.sprite = fastButtonImage;
            iconImage.sprite = fastButtonIcon;
        }
        
    }
}
