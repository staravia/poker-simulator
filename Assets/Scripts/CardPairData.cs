public struct CardPairData
{
    public CardData CardA { get; }
    public CardData CardB { get; }

    public CardPairData(CardData cardA, CardData cardB)
    {
        CardA = cardA;
        CardB = cardB;
    }
}