using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] private CardSpriteDictionary _spriteDictionary;
    [SerializeField] private Image _bgImage;
    [SerializeField] private Image _faceImage;
    [SerializeField] private Sprite _cardBackSprite;

    public CardData Card { get; private set; }

    public void SetFaceDown()
    {
        _faceImage.sprite = _cardBackSprite;
    }

    public void SetDisplay(CardData data, bool disabled = true)
    {
        Card = data;
        _faceImage.sprite = _spriteDictionary.GetSprite(data.Suit, data.Rank);
        SetDisabled(disabled);
    }

    public void SetDisabled(bool disabled)
    {
        _faceImage.color = disabled ? Color.gray : Color.white;
        _bgImage.color = disabled ? Color.gray : Color.white;
    }
}


