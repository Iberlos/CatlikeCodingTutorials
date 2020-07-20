using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CostAttached : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Buildable building = default;

    public ResourceWallet.SpendingSum AttachedCost { get => building.SpendingSum; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Game.instance.CostAttachedWindow.Initialize(AttachedCost);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Game.instance.CostAttachedWindow.Close();
    }
}
