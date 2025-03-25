using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CardSpriteDictionaryPopulator : MonoBehaviour
{
    public CardSpriteDictionary cardSpriteDictionary;  // Reference to the CardSpriteDictionary asset
    public List<Sprite> cardSprites;  // The list of card sprites, must be in the correct order

    // Method to populate the CardSpriteDictionary
    public void PopulateDictionary()
    {
        if (cardSpriteDictionary == null || cardSprites.Count != 52)
        {
            Debug.LogError("Please assign a valid CardSpriteDictionary and ensure you have exactly 52 sprites.");
            return;
        }

        cardSpriteDictionary.cardSpriteList.Clear();  // Clear the current list before populating

        int spriteIndex = 0;

        // Loop through each suit
        foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
        {
            // Loop through each rank
            foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
            {
                CardSpriteDictionary.CardSpriteEntry entry = new CardSpriteDictionary.CardSpriteEntry
                {
                    Suit = suit,
                    Rank = rank,
                    Sprite = cardSprites[spriteIndex]
                };

                cardSpriteDictionary.cardSpriteList.Add(entry);
                spriteIndex++;
            }
        }

        EditorUtility.SetDirty(cardSpriteDictionary);  // Mark the dictionary as dirty to ensure the changes are saved
        Debug.Log("CardSpriteDictionary has been populated successfully.");
    }
}
