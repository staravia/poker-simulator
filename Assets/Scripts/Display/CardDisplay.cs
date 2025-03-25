using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] private CardSpriteDictionary _spriteDictionary;
    [SerializeField] private Image _faceImage;
    [SerializeField] private Sprite _cardBackSprite;

    public void SetFaceDown()
    {
        _faceImage.sprite = _cardBackSprite;
    }

    public void SetDisplay(CardData data)
    {
        SetDisplay(data.Suit, data.Rank);
    }

    public void SetDisplay(CardSuit suit, CardRank rank)
    {
        _faceImage.sprite = _spriteDictionary.GetSprite(suit, rank);
    }
}


