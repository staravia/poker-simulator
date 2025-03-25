using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CardSpriteDictionary", menuName = "Card System/Card Sprite Dictionary")]
public class CardSpriteDictionary : ScriptableObject
{
    [System.Serializable]
    public class CardSpriteEntry
    {
        public CardSuit Suit;
        public CardRank Rank;
        public Sprite Sprite;
    }

    public List<CardSpriteEntry> cardSpriteList = new List<CardSpriteEntry>();

    public Sprite GetSprite(CardSuit suit, CardRank rank)
    {
        foreach (var entry in cardSpriteList)
        {
            if (entry.Suit == suit && entry.Rank == rank)
            {
                return entry.Sprite;
            }
        }
        return null;
    }
}
