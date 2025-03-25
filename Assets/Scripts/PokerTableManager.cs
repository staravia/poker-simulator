using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokerTableManager : MonoBehaviour
{
    [SerializeField] private List<CardDisplay> _riverCardDisplays;
    [SerializeField] private int _totalHands = 6;
    [SerializeField] private RectTransform _cardPairsContainer;
    [SerializeField] private CardPairDisplay _cardPairDisplayPrefab;

    private Dictionary<CardPairData, CardPairDisplay> _hands = new Dictionary<CardPairData, CardPairDisplay>();
    private Dictionary<CardData, CardDisplay> _riverCards = new Dictionary<CardData, CardDisplay>();
    private List<CardPairDisplay> _cachedPairDisplays = new List<CardPairDisplay>();
    // private Stack<CardData> _deck = new Stack<CardData>();

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

    private void SetCards(Stack<CardData> deck)
    {
        if (deck == null)
        {
            Debug.LogError("Deck does not exist.");
            return;
        }

        if (deck.Count < _hands.Count * 2 + 5)
        {
            Debug.LogError("Not enough cards in deck.");
            return;
        }

        _hands.Clear();
        foreach (var display in _cachedPairDisplays)
        {
            var cardA = deck.Pop();
            var cardB = deck.Pop();
            var data = new CardPairData(cardA, cardB);
            _hands.Add(data, display);

            display.SetDisplay(data);
        }

        _riverCards.Clear();
        foreach (var display in _riverCardDisplays)
        {
            var card = deck.Pop();
            _riverCards.Add(card, display);
            display.SetDisplay(card);
        }
    }

    private void DisplayWinningCards()
    {
        var winningHand = PokerRuleSet.GetWinningHand(_hands.Keys.ToList(), _riverCards.Keys.ToList());

        var winningPairDisplay = _cachedPairDisplays[winningHand.index];
        winningPairDisplay.HighlightWinningCards(winningHand.winningCards);

        foreach (var card in _riverCards)
        {
            card.Value.SetDisabled(true);
        }

        foreach (var card in winningHand.winningCards)
        {
            if (_riverCards.ContainsKey(card))
            {
                _riverCards[card].SetDisabled(false);
            }
        }
    }

    private void Start()
    {
        for (var i = 0; i < _totalHands; i++)
        {
            var display = Instantiate(_cardPairDisplayPrefab);
            display.Initialize(_cardPairsContainer);
            _cachedPairDisplays.Add(display);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var deck = CreateDeck();
            SetCards(deck);
            DisplayWinningCards();
        }
    }
}
