using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    [SerializeField]
    private Image backGround;
    [SerializeField]
    private TMPro.TextMeshProUGUI text;
    [SerializeField]
    private Button stayButton;
    [SerializeField]
    private Button leaveButton;
    [SerializeField]
    private Button liftOffButton;
    [SerializeField]
    private Button closeButton;

    public Player Owner
    {
        set
        {
            if(owner == null)
            {
                owner = value;
            }
        }
    }

    private Player owner;

    private void Initialize()
    {
        Time.timeScale = 0f;
        backGround.gameObject.SetActive(true);
        text.gameObject.SetActive(true);
    }

    public void InitializePlanet()
    {
        Close();
        Initialize();
        text.text = "It seems I have landed on a planet where " + owner.CurrentPlanet.ProblemText;
        if (owner.CurrentPlanet.WasVisited)
            liftOffButton.gameObject.SetActive(true);
        stayButton.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);
    }

    public void InitializeStay()
    {
        Close();
        Initialize();
        text.text = owner.CurrentPlanet.StayText;
        closeButton.gameObject.SetActive(true);
    }

    public void InitializeLeave()
    {
        Close();
        Initialize();
        text.text = "I Don't Wanna Live On This Plannet Anymore!";
        closeButton.gameObject.SetActive(true);
    }

    //public void InitializeDialog()
    //{

    //}

    public void Close()
    {
        this.gameObject.SetActive(false);
        backGround.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        stayButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        liftOffButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
