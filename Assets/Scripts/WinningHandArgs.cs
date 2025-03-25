using System.Collections.Generic;
using UnityEngine.XR;

public struct WinningHandArgs
{
    public int WinningHandIndex { get; }
    public List<CardData> WinningCards { get; }
    public PokerHandType WinningHandType { get; }
    public List<CardPairData> PlayedHands { get; }
    public List<PokerHandType> PlayedHandTypes { get; }
    public List<CardData> RiverCards { get; }

    public WinningHandArgs(PokerHandType winningHandType, List<CardData> winningCards, int handIndex = -1, List<PokerHandType> playedHandTypes = null, List<CardPairData> playedHands = null, List<CardData> riverCards = null)
    {
        WinningCards = winningCards;
        WinningHandType = winningHandType;
        WinningHandIndex = handIndex;
        RiverCards = riverCards;

        PlayedHandTypes = playedHandTypes;
        PlayedHands = playedHands;
    }
}