public struct CardData
{
    public CardRank Rank { get; }
    public CardSuit Suit { get; }

    public CardData(CardRank rank, CardSuit suit)
    {
        Rank = rank;
        Suit = suit;
    }
}