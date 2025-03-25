using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerTableManager : MonoBehaviour
{
    [SerializeField] private CardDisplay _cardDisplay;

    private List<CardData> CreateDeck()
    {
        var cards = new List<CardData>();
        foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
            {
                cards.Add(new CardData(rank, suit));
            }
        }

        return cards;
    }

    private void ShuffleDeck(List<CardData> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            var random = Random.value;
            var index = (int)((i + 1) * random);
            var temp = deck[i];
            deck[i] = deck[index];
            deck[index] = temp;
        }
    }

    private List<CardData> _deck;
    private int _index;

    private void Start()
    {
        var deck = CreateDeck();
        ShuffleDeck(deck);
        _deck = deck;
    }

    private void Update()
    {
        if (_deck == null)
            return;

        _index = _index >= _deck.Count - 1 ? 0 : _index + 1;
        _cardDisplay.SetDisplay(_deck[_index]);
    }
}
