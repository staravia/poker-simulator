using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public static class PokerRuleSet
{
    public static int GetWinningHand(List<CardPairData> pairs, List<CardData> river)
    {
        var winningHandType = PokerHandType.Invalid;
        var winningHandIndex = -1;
        List<CardData> winningCards = null;

        for (int i = 0; i < pairs.Count; i++)
        {
            var handResult = GetHandType(pairs[i], river);

            if (handResult.handType > winningHandType)
            {
                winningHandType = handResult.handType;
                winningHandIndex = i;
                winningCards = handResult.winningCards;
            }
            else if (handResult.handType == winningHandType)
            {
                if (CompareHands(handResult.winningCards, winningCards) > 0)
                {
                    winningHandIndex = i;
                    winningCards = handResult.winningCards;
                }
            }
        }

        return winningHandIndex;
    }

    private static int CompareHands(List<CardData> hand1, List<CardData> hand2)
    {
        for (int i = 0; i < hand1.Count; i++)
        {
            if (hand1[i].Rank > hand2[i].Rank)
                return 1;
            if (hand1[i].Rank < hand2[i].Rank)
                return -1;
        }
        return 0;
    }

    public static (PokerHandType handType, List<CardData> winningCards) GetHandType(CardPairData pair, List<CardData> river)
    {
        var suitCount = new Dictionary<CardSuit, int>();
        var rankCount = new Dictionary<CardRank, int>();
        var kickerCard = CardRank.Two;
        var isStraight = false;
        var isFlush = false;
        var isRoyalFlush = false;
        var hasFourOfAKind = false;
        var hasThreeOfAKind = false;
        var hasTwoPair = false;
        var hasPair = false;

        var winningCards = new List<CardData>();
        var cards = new List<CardData>(river)
    {
        pair.CardA,
        pair.CardB
    };

        cards.Sort((x, y) => x.Rank.CompareTo(y.Rank));

        foreach (var card in cards)
        {
            if (suitCount.ContainsKey(card.Suit))
                suitCount[card.Suit]++;
            else
                suitCount.Add(card.Suit, 1);

            if (rankCount.ContainsKey(card.Rank))
                rankCount[card.Rank]++;
            else
                rankCount.Add(card.Rank, 1);
        }

        foreach (var suit in suitCount)
        {
            if (suit.Value >= 5)
            {
                isFlush = true;
                winningCards = cards.Where(c => c.Suit == suit.Key).OrderByDescending(c => c.Rank).Take(5).ToList();
                break;
            }
        }

        int consecutiveCount = 1;
        for (int i = 1; i < cards.Count; i++)
        {
            if (cards[i].Rank == cards[i - 1].Rank + 1 && cards[i].Suit == cards[i - 1].Suit)
            {
                consecutiveCount++;
                if (consecutiveCount >= 5)
                {
                    isStraight = true;
                    winningCards = cards.Skip(i - 4).Take(5).ToList();
                    kickerCard = cards[i].Rank;

                    if (cards[i - 4].Rank == CardRank.Ten && cards[i].Rank == CardRank.Ace)
                    {
                        isRoyalFlush = true;
                    }

                    break;
                }
            }
            else if (cards[i].Rank != cards[i - 1].Rank)
            {
                consecutiveCount = 1;
            }
        }

        foreach (var rank in rankCount)
        {
            if (rank.Value == 4)
            {
                hasFourOfAKind = true;
                winningCards = cards.Where(c => c.Rank == rank.Key).ToList();
                kickerCard = cards.First(c => c.Rank != rank.Key).Rank;
            }
            else if (rank.Value == 3)
            {
                hasThreeOfAKind = true;

                if (hasPair)
                {
                    winningCards.AddRange(cards.Where(c => c.Rank == rank.Key));
                }
                else
                {
                    winningCards = cards.Where(c => c.Rank == rank.Key).ToList();
                }
            }
            else if (rank.Value == 2)
            {
                if (hasPair)
                {
                    hasTwoPair = true;
                    winningCards.AddRange(cards.Where(c => c.Rank == rank.Key));
                }
                else if (hasThreeOfAKind)
                {
                    winningCards.AddRange(cards.Where(c => c.Rank == rank.Key));
                }
                else
                {
                    hasPair = true;
                    winningCards = cards.Where(c => c.Rank == rank.Key).ToList();
                }
            }
        }

        if (isRoyalFlush)
            return (PokerHandType.RoyalFlush, winningCards);
        if (isStraight && isFlush)
            return (PokerHandType.StraightFlush, winningCards);
        if (hasFourOfAKind)
            return (PokerHandType.FourOfAKind, winningCards);
        if (hasThreeOfAKind && hasPair)
            return (PokerHandType.FullHouse, winningCards);
        if (isFlush)
            return (PokerHandType.Flush, winningCards);
        if (isStraight)
            return (PokerHandType.Straight, winningCards);
        if (hasThreeOfAKind)
            return (PokerHandType.ThreeOfAKind, winningCards);
        if (hasTwoPair)
            return (PokerHandType.TwoPair, winningCards);
        if (hasPair)
            return (PokerHandType.OnePair, winningCards);

        return (PokerHandType.HighCard, cards.OrderByDescending(c => c.Rank).Take(5).ToList());
    }

}

public enum PokerHandType
{
    Invalid = -1,
    HighCard = 0,
    OnePair = 1,
    TwoPair = 2,
    ThreeOfAKind = 3,
    Straight = 4,
    Flush = 5,
    FullHouse = 6,
    FourOfAKind = 7,
    StraightFlush = 8,
    RoyalFlush = 9
}