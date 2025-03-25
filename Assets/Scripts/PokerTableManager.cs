using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PokerTableManager : MonoBehaviour
{
    [SerializeField] private List<CardDisplay> _riverCardDisplays;
    [SerializeField] private TextMeshProUGUI _handsPlayedText;
    [SerializeField] private TextMeshProUGUI _handsWonText;
    [SerializeField] private int _totalHands = 6;
    [SerializeField] private RectTransform _cardPairsContainer;
    [SerializeField] private CardPairDisplay _cardPairDisplayPrefab;

    private Dictionary<CardPairData, CardPairDisplay> _hands = new Dictionary<CardPairData, CardPairDisplay>();
    private Dictionary<CardData, CardDisplay> _riverCards = new Dictionary<CardData, CardDisplay>();
    private List<CardPairDisplay> _cardPairDisplays = new List<CardPairDisplay>();
    // private Stack<CardData> _deck = new Stack<CardData>();

    private Dictionary<PokerHandType, int> _handTypePlayedCount = new Dictionary<PokerHandType, int>();
    private Dictionary<PokerHandType, int> _handTypeWonCount = new Dictionary<PokerHandType, int>();

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
        foreach (var display in _cardPairDisplays)
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
        var result = PokerRuleSet.GetWinningHand(_hands.Keys.ToList(), _riverCards.Keys.ToList());

        var display = _cardPairDisplays[result.WinningHandIndex];
        display.HighlightWinningCards(result.WinningCards);

        // Display winning cards/hands
        foreach (var card in _riverCards)
        {
            card.Value.SetDisabled(true);
        }

        foreach (var card in result.WinningCards)
        {
            if (_riverCards.ContainsKey(card))
            {
                _riverCards[card].SetDisabled(false);
            }
        }

        // Count Hand Types
        if (_handTypeWonCount.ContainsKey(result.HandType))
            _handTypeWonCount[result.HandType]++;
        else
            _handTypeWonCount.Add(result.HandType, 1);

        foreach (var playedHand in result.PlayedHands)
        {
            if (_handTypePlayedCount.ContainsKey(playedHand))
                _handTypePlayedCount[playedHand]++;
            else
                _handTypePlayedCount.Add(playedHand, 1);
        }

        // Update winning text
        _handsWonText.text = "<size=14><b>Hands Won</b></size>\n";
        _handsPlayedText.text = "<size=14><b>Hands Played</b></size>\n";

        var total = 0;
        foreach (var entry in _handTypePlayedCount)
        {
            total += entry.Value;
        }
        foreach (var entry in _handTypePlayedCount)
        {
            _handsPlayedText.text += $"{entry.Key}: {entry.Value} ({(float)entry.Value / total * 100:0.00}%)\n";
        }
        _handsPlayedText.text += $"\n<size=14>Hands Played: {total}</size>";

        total = 0;
        foreach (var entry in _handTypeWonCount)
        {
            total += entry.Value;
        }
        foreach (var entry in _handTypeWonCount)
        {
            _handsWonText.text += $"{entry.Key}: {entry.Value} ({(float)entry.Value / total * 100:0.00}%)\n";
        }
        _handsWonText.text += $"\n<size=14>Rounds Played: {total}</size>";
        _handsWonText.text += $"\n<size=14>Winning Hand: {result.HandType}</size>";

        // if (result.HandType == PokerHandType.HighCard)
        //     _isPaused = true;
    }

    private void Start()
    {
        for (var i = 0; i < _totalHands; i++)
        {
            var display = Instantiate(_cardPairDisplayPrefab);
            display.Initialize(_cardPairsContainer);
            _cardPairDisplays.Add(display);
        }
    }

    private bool _isPaused;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _isPaused = !_isPaused;
        }

        if (!_isPaused)
        {
            var deck = CreateDeck();
            SetCards(deck);
            DisplayWinningCards();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            var deck = CreateDeck();
            SetCards(deck);
            DisplayWinningCards();
        }
    }
}
