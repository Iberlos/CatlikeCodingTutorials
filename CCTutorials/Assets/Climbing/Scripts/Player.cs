using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Transform dialogBoxWorldTransform;
    [SerializeField]
    private DialogBox dialogBox;

    public Planet CurrentPlanet {
        get => currentPlannet;
        set
        {
            if(currentPlannet == null)
            {
                currentPlannet = value;
            }
            else
            {
                Debug.LogError("Tried to reset the planet reference before reaching space!", value.gameObject);
            }
        }
    }

    private Planet currentPlannet;

    private void Start()
    {
        dialogBox = Instantiate(dialogBox, canvas.transform);
        dialogBox.Owner = this;
        dialogBox.gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector3 dialogBoxPos = Camera.main.WorldToScreenPoint(dialogBoxWorldTransform.position);
        dialogBox.transform.position = dialogBoxPos;
    }

    public void EvaluateGravity(float gravity)
    {
        if(gravity == 0)
        {
            currentPlannet = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentPlannet == null)
        {
            Planet p = collision.transform.root.GetComponent<Planet>();
            if(p != null)
            {
                currentPlannet = p;
                dialogBox.gameObject.SetActive(true);
                dialogBox.InitializePlanet();
                p.Visit();
            }
        }
    }
}
