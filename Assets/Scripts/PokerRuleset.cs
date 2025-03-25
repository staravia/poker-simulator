using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public static class PokerRuleSet
{
    public static WinningHandArgs GetWinningHand(List<CardPairData> pairs, List<CardData> river)
    {
        var winningHandType = PokerHandType.Invalid;
        var winningHandIndex = -1;
        var playedHandTypes = new List<PokerHandType>();
        var isDraw = false;
        List<CardData> winningCards = null;

        for (int i = 0; i < pairs.Count; i++)
        {
            var result = GetHandType(pairs[i], river);
            playedHandTypes.Add(result.WinningHandType);

            if (result.WinningHandType > winningHandType)
            {
                winningHandType = result.WinningHandType;
                winningHandIndex = i;
                winningCards = result.WinningCards;
                isDraw = false;
            }
            else if (result.WinningHandType == winningHandType)
            {
                var comparison = CompareHands(result.WinningCards, winningCards);
                if (comparison == 0)
                {
                    isDraw = true;
                    winningCards.AddRange(result.WinningCards);
                }
                else if (comparison > 0)
                {
                    winningHandIndex = i;
                    winningCards = result.WinningCards;
                    isDraw = false;
                }
            }
        }

        if (isDraw)
        {
            winningHandType = PokerHandType.Draw;
        }

        return new WinningHandArgs(winningHandType, winningCards, winningHandIndex, playedHandTypes, pairs, river);
    }

    private static int CompareHands(List<CardData> handA, List<CardData> handB)
    {
        for (int i = 0; i < handA.Count; i++)
        {
            if (handA.Count <= i)
                return -1;
            if (handB.Count <= i)
                return 1;

            if (handA[i].Rank > handB[i].Rank)
                return 1;
            if (handA[i].Rank < handB[i].Rank)
                return -1;
        }
        return 0;
    }

    public static WinningHandArgs GetHandType(CardPairData pair, List<CardData> river)
    {
        var suitCount = new Dictionary<CardSuit, int>();
        var rankCount = new Dictionary<CardRank, int>();
        var isStraightFlush = false;
        var isStraight = false;
        var isFlush = false;
        var isRoyalFlush = false;
        var hasFourOfAKind = false;
        var hasThreeOfAKind = false;
        var hasTwoPair = false;
        var hasPair = false;
        var winningSuit = CardSuit.Clubs;

        var winningCards = new List<CardData>();
        var pairedCards = new List<CardData>();
        var cards = new List<CardData>(river);
        cards.Add(pair.CardA);
        cards.Add(pair.CardB);

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

        // Check for flush
        foreach (var suit in suitCount)
        {
            if (suit.Value >= 5)
            {
                isFlush = true;
                winningSuit = suit.Key;
                break;
            }
        }

        // Check for straight flush
        var consecutiveCount = 1;
        for (var i = 1; i < cards.Count; i++)
        {
            if (cards[i].Rank == cards[i - 1].Rank + 1 && cards[i].Suit == cards[i - 1].Suit)
            {
                consecutiveCount++;
                if (consecutiveCount >= 5)
                {
                    winningCards = cards.Skip(i - 4).Take(5).ToList();

                    if (cards[i - 4].Rank == CardRank.Ten && cards[i].Rank == CardRank.Ace)
                    {
                        isRoyalFlush = true;
                    }
                    else
                    {
                        isStraightFlush = true;
                    }

                    break;
                }
            }
            else if (cards[i].Rank != cards[i - 1].Rank)
            {
                consecutiveCount = 1;
            }
        }

        // Check for straights
        if (!isStraightFlush)
        {
            consecutiveCount = 1;
            for (var i = 1; i < cards.Count; i++)
            {
                if (cards[i].Rank == cards[i - 1].Rank + 1)
                {
                    consecutiveCount++;
                    if (consecutiveCount >= 5)
                    {
                        winningCards = cards.Skip(i - 4).Take(5).ToList();
                        isStraight = true;
                        break;
                    }
                }
                else if (cards[i].Rank != cards[i - 1].Rank)
                {
                    consecutiveCount = 1;
                }
            }

            // Check for Ace to 5 straight flush (Ace is treated as 1)
            var hasAce = cards.Any(card => card.Rank == CardRank.Ace);
            if (!isStraightFlush && hasAce)
            {
                var aceToFiveCards = cards.Where(card => card.Rank >= CardRank.Two && card.Rank <= CardRank.Five).OrderBy(card => card.Rank).ToList();

                if (aceToFiveCards.Count == 5)
                {
                    consecutiveCount = 1;
                    for (var i = 1; i < aceToFiveCards.Count; i++)
                    {
                        if (aceToFiveCards[i].Rank == aceToFiveCards[i - 1].Rank + 1 && aceToFiveCards[i].Suit == aceToFiveCards[i - 1].Suit)
                        {
                            consecutiveCount++;
                            if (consecutiveCount >= 5)
                            {
                                winningCards = aceToFiveCards;
                                isStraightFlush = true;
                                break;
                            }
                        }
                        else if (aceToFiveCards[i].Rank != aceToFiveCards[i - 1].Rank)
                        {
                            consecutiveCount = 1;
                        }

                        if (aceToFiveCards[i].Rank == CardRank.Six)
                            break;
                    }
                }
            }

            // Check for Ace to 5 straight (Ace is treated as 1)
            if (!isStraight && !isStraightFlush && hasAce)
            {
                var aceToFiveCards = cards.Where(card => card.Rank >= CardRank.Two && card.Rank <= CardRank.Five).OrderBy(card => card.Rank).ToList();

                if (aceToFiveCards.Count == 5)
                {
                    consecutiveCount = 1;
                    for (var i = 1; i < aceToFiveCards.Count; i++)
                    {
                        if (aceToFiveCards[i].Rank == aceToFiveCards[i - 1].Rank + 1)
                        {
                            consecutiveCount++;
                            if (consecutiveCount >= 5)
                            {
                                winningCards = aceToFiveCards;
                                isStraight = true;
                                break;
                            }
                        }
                        else if (aceToFiveCards[i].Rank != aceToFiveCards[i - 1].Rank)
                        {
                            consecutiveCount = 1;
                        }

                        if (aceToFiveCards[i].Rank == CardRank.Six)
                            break;
                    }
                }
            }
        }

        // Check for pairs
        foreach (var rank in rankCount)
        {
            if (rank.Value == 4)
            {
                hasFourOfAKind = true;
                winningCards = cards.Where(c => c.Rank == rank.Key).ToList();
                break;
            }

            if (rank.Value == 3)
            {
                hasThreeOfAKind = true;

                if (hasPair)
                {
                    pairedCards.AddRange(cards.Where(c => c.Rank == rank.Key));
                    winningCards = pairedCards;
                    break;
                }
                else
                {
                    pairedCards = cards.Where(c => c.Rank == rank.Key).ToList();
                }
            }
            else if (rank.Value == 2)
            {
                if (hasPair)
                {
                    hasTwoPair = true;
                    pairedCards.AddRange(cards.Where(c => c.Rank == rank.Key));
                }
                else if (hasThreeOfAKind)
                {
                    pairedCards.AddRange(cards.Where(c => c.Rank == rank.Key));
                    winningCards = pairedCards;
                    break;
                }
                else
                {
                    hasPair = true;
                    pairedCards = cards.Where(c => c.Rank == rank.Key).ToList();
                }
            }
        }

        if (winningCards.Count == 0)
            winningCards = pairedCards;

        // Get Kicker cards
        cards.Sort((x, y) => y.Rank.CompareTo(x.Rank));
        foreach (var card in cards)
        {
            if (winningCards.Count >= 5)
                break;

            if (winningCards.Contains(card))
                continue;

            winningCards.Add(card);
        }

        if (isRoyalFlush)
            return new WinningHandArgs(PokerHandType.RoyalFlush, winningCards);
        if (isStraightFlush)
            return new WinningHandArgs(PokerHandType.StraightFlush, winningCards);
        if (hasFourOfAKind)
            return new WinningHandArgs(PokerHandType.FourOfAKind, winningCards);
        if (hasThreeOfAKind && hasPair)
            return new WinningHandArgs(PokerHandType.FullHouse, winningCards);
        if (isFlush)
        {
            winningCards = cards.Where(c => c.Suit == winningSuit).OrderByDescending(c => c.Rank).Take(5).ToList();
            return new WinningHandArgs(PokerHandType.Flush, winningCards);
        }
        if (isStraight)
            return new WinningHandArgs(PokerHandType.Straight, winningCards);
        if (hasThreeOfAKind)
            return new WinningHandArgs(PokerHandType.ThreeOfAKind, winningCards);
        if (hasTwoPair)
            return new WinningHandArgs(PokerHandType.TwoPair, winningCards);
        if (hasPair)
            return new WinningHandArgs(PokerHandType.OnePair, winningCards);

        return new WinningHandArgs(PokerHandType.HighCard, cards.OrderByDescending(c => c.Rank).Take(5).ToList());
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
    RoyalFlush = 9,
    Draw = 10
}