using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PokerTableManager : MonoBehaviour
{
    [SerializeField] private List<CardDisplay> _riverCardDisplays;
    [SerializeField] private TextMeshProUGUI _handsPlayedText;
    [SerializeField] private TextMeshProUGUI _handsWonText;
    [SerializeField] private int _totalHands = 6;
    [SerializeField] private RectTransform _cardPairsContainer;
    [SerializeField] private CardPairDisplay _cardPairDisplayPrefab;

    private Dictionary<CardData, CardDisplay> _riverCards = new Dictionary<CardData, CardDisplay>();
    private List<CardPairDisplay> _cardPairDisplays = new List<CardPairDisplay>();
    // private Stack<CardData> _deck = new Stack<CardData>();

    private bool _isComputing = false;
    private ConcurrentDictionary<PokerHandType, int> _handTypePlayedCount = new ConcurrentDictionary<PokerHandType, int>();
    private ConcurrentDictionary<PokerHandType, int> _handTypeWonCount = new ConcurrentDictionary<PokerHandType, int>();
    private ConcurrentQueue<WinningHandArgs> _computedWinningHands = new ConcurrentQueue<WinningHandArgs>();

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
        var rng = new System.Random();

        for (int i = deck.Count - 1; i > 0; i--)
        {
            var index = rng.Next(0, i + 1);
            var temp = deck[i];
            deck[i] = deck[index];
            deck[index] = temp;
        }

        var stack = new Stack<CardData>(deck);
        return stack;
    }

    private (List<CardPairData> hands, List<CardData> riverCards) SetCards(Stack<CardData> deck, int handCount)
    {
        if (deck == null)
        {
            // Debug.LogError("Deck does not exist.");
            return (null, null);
        }

        var hands = new List<CardPairData>();
        var riverCards = new List<CardData>();

        if (deck.Count < handCount * 2 + 5)
        {
            // Debug.LogError("Not enough cards in deck.");
            return (null, null);
        }

        for (var i = 0; i < handCount; i++)
        {
            var cardA = deck.Pop();
            var cardB = deck.Pop();
            var data = new CardPairData(cardA, cardB);
            hands.Add(data);
        }

        for (var i = 0; i < _riverCardDisplays.Count; i++)
        {
            var card = deck.Pop();
            riverCards.Add(card);
        }

        return (hands, riverCards);
    }


    private void TryComputeMultipleWinningHands()
    {
        if (_isComputing)
            return;

        _isComputing = true;
        Task.Run(() => HandleScheduleWinningHandsTask());
    }

    private void HandleScheduleWinningHandsTask()
    {
        var numberOfHandsToCompute = 320;

        // Split into smaller batches to prevent frame lag
        int batchSize = 40;
        int batchCount = (int)Math.Ceiling((double)numberOfHandsToCompute / batchSize);

        for (int batch = 0; batch < batchCount; batch++)
        {
            int startIndex = batch * batchSize;
            int endIndex = Math.Min(startIndex + batchSize, numberOfHandsToCompute);

            Parallel.For(startIndex, endIndex, i =>
            {
                var deck = CreateDeck();
                var table = SetCards(deck, _totalHands);
                var result = PokerRuleSet.GetWinningHand(table.hands, table.riverCards);
                _computedWinningHands.Enqueue(result);
            });

            Task.Delay(5).Wait(); // Arbitrary Delay
        }

        _isComputing = false;
    }

    private void ComputeSingleWinningHand()
    {
        var deck = CreateDeck();
        var table = SetCards(deck, _totalHands);
        var result = PokerRuleSet.GetWinningHand(table.hands, table.riverCards);
        _computedWinningHands.Enqueue(result);
    }

    private void TryDisplayWinningCards()
    {
        var currentHandType = PokerHandType.Invalid;
        var winningHands = new List<WinningHandArgs>();
        lock (_computedWinningHands)
        {
            while (_computedWinningHands.Count > 0)
            {
                var result = _computedWinningHands.TryDequeue(out var args);
                if (!result)
                {
                    Debug.LogError($"Failed to dequeue {nameof(_computedWinningHands)}");
                    return;
                }

                _handTypeWonCount.AddOrUpdate(args.WinningHandType, 1, (key, oldValue) => oldValue + 1);
                foreach (var handType in args.PlayedHandTypes)
                {
                    _handTypePlayedCount.AddOrUpdate(handType, 1, (key, oldValue) => oldValue + 1);
                }

                winningHands.Add(args);
            }
        }

        // Update Card display
        foreach (var args in winningHands)
        {
            currentHandType = args.WinningHandType;
            for (int i = 0; i < args.PlayedHands.Count; i++)
            {
                var data = args.PlayedHands[i];
                var display = _cardPairDisplays[i];

                display.SetDisplay(data);
                display.HighlightWinningCards(args.WinningCards);
            }

            _riverCards.Clear();
            for (var i = 0; i < args.RiverCards.Count; i++)
            {
                var data = args.RiverCards[i];
                var riverCardDisplay = _riverCardDisplays[i];
                _riverCards.Add(data, riverCardDisplay);
                riverCardDisplay.SetDisplay(data);
            }

            foreach (var card in _riverCards)
            {
                card.Value.SetDisabled(true);
            }

            foreach (var card in args.WinningCards)
            {
                if (_riverCards.ContainsKey(card))
                {
                    _riverCards[card].SetDisabled(false);
                }
            }
        }

        // Update winning text
        _handsWonText.text = "<size=14><b>Best Hand (Per Round)</b></size>\n";
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
        _handsWonText.text += $"\n<size=14>Winning Hand: {currentHandType}</size>";

        // if (currentHandType == PokerHandType.Draw)
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
            // ComputeSingleWinningHand();
            TryComputeMultipleWinningHands();
            TryDisplayWinningCards();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            ComputeSingleWinningHand();
            TryDisplayWinningCards();
        }
    }
}
