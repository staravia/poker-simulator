using System.Collections.Generic;
using UnityEngine.XR;

public struct WinningHandArgs
{
    public int WinningHandIndex { get; }
    public List<CardData> WinningCards { get; }
    public PokerHandType HandType { get; }
    public List<PokerHandType> PlayedHands { get; }

    public WinningHandArgs(PokerHandType handType, List<CardData> winningCards, int handIndex = -1, List<PokerHandType> playedHands = null)
    {
        WinningCards = winningCards;
        HandType = handType;
        WinningHandIndex = handIndex;

        PlayedHands = playedHands;
    }
}