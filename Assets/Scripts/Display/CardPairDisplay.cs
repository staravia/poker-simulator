using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPairDisplay : MonoBehaviour
{
    [SerializeField] private CardDisplay _cardA;
    [SerializeField] private CardDisplay _cardB;

    public void SetDisplay(CardPairData data)
    {
        _cardA.SetDisplay(data.CardA);
        _cardB.SetDisplay(data.CardB);
    }

    public void Initialize(RectTransform container)
    {
        transform.SetParent(container);
        transform.localScale = Vector3.one;
    }
}
