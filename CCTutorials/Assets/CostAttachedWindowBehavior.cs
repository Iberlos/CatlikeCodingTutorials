using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CostAttachedWindowBehavior : MonoBehaviour
{
    [SerializeField]
    private Text foodText = default;
    [SerializeField]
    private Text woodText = default;
    [SerializeField]
    private Text metalText = default;
    [SerializeField]
    private Text crystalText = default;
    [SerializeField]
    private Text coinText = default;

    public void Initialize(ResourceWallet.SpendingSum spendingSum)
    {
        transform.position = Input.mousePosition;
        gameObject.SetActive(true);
        foodText.text = spendingSum.food.ToString();
        woodText.text = spendingSum.wood.ToString();
        metalText.text = spendingSum.metal.ToString();
        crystalText.text = spendingSum.crystal.ToString();
        coinText.text = spendingSum.gold.ToString();
    }

    public void Update()
    {
        transform.position = Input.mousePosition;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
