using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerTableManager : MonoBehaviour
{
    [SerializeField] private List<CardDisplay> _riverCardDisplays;
    [SerializeField] private int _totalHands = 6;
    [SerializeField] private RectTransform _cardPairsContainer;
    [SerializeField] private CardPairDisplay _cardPairDisplayPrefab;

    private List<CardPairDisplay> _cardPairDisplays = new List<CardPairDisplay>();
    private List<CardPairData> _hands = new List<CardPairData>();
    private List<CardData> _riverCards = new List<CardData>();
    private Stack<CardData> _deck = new Stack<CardData>();

    private Stack<CardData> CreateDeck()
    {
        var cards = new List<CardData>();
        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
            {
                cards.Add(new CardData(rank, suit));
            }
        }

        return ShuffleDeck(cards);
    }

    private Stack<CardData> ShuffleDeck(List<CardData> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            var random = UnityEngine.Random.value;
            var index = (int)((i + 1) * random);
            var temp = deck[i];
            deck[i] = deck[index];
            deck[index] = temp;
        }

        var stack = new Stack<CardData>(deck);
        return stack;
    }

    private void SetCards()
    {
        if (_deck == null)
        {
            Debug.LogError("Deck does not exist.");
            return;
        }

        if (_deck.Count < _cardPairDisplays.Count * 2 + 5)
        {
            Debug.LogError("Not enough cards in deck.");
            return;
        }

        _hands.Clear();
        foreach (var display in _cardPairDisplays)
        {
            var cardA = _deck.Pop();
            var cardB = _deck.Pop();
            var data = new CardPairData(cardA, cardB);
            _hands.Add(data);

            display.SetDisplay(data);
        }

        _riverCards.Clear();
        foreach (var display in _riverCardDisplays)
        {
            var card = _deck.Pop();
            _riverCards.Add(card);
            display.SetDisplay(card);
        }
    }

    private void Start()
    {
        var deck = CreateDeck();
        _deck = deck;

        for (var i = 0; i < _totalHands; i++)
        {
            var display = Instantiate(_cardPairDisplayPrefab);
            display.Initialize(_cardPairsContainer);
            _cardPairDisplays.Add(display);
        }

        SetCards();

        var handIndex = PokerRuleSet.GetWinningHand(_hands, _riverCards);
        Debug.Log($"WINNING HANDS: {handIndex}");
    }
}
