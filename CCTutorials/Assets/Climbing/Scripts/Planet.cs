using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Planet : MonoBehaviour
{
    [SerializeField, TextArea]
    private string problemText = default;
    [SerializeField, TextArea]
    private string stayText = default;
    [SerializeField, TextArea]
    private string gameOverText = default;

    public string ProblemText {get => problemText;}
    public string StayText { get => stayText; }
    public string GameOverText { get => stayText; }
    public bool WasVisited { get => wasVisited; }

    private bool wasVisited;

    public void Visit()
    {
        /*Set wasVisited as true*/
        wasVisited = true;
    }
}
