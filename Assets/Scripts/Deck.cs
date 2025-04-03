using System.Collections.Generic;

public class Deck
{
    private List<Card> cards = new List<Card>();

    public Deck()
    {
        InitializeDeck();
        ShuffleDeck();
    }

    private void InitializeDeck()
    {
        string[] suits = { "hearts", "diamonds", "clubs", "spades" };

        cards.Clear();
        foreach (string suit in suits)
        {
            for (int value = 1; value <= 13; value++)
            {
                cards.Add(new Card(value, suit));
            }
        }
    }

    private void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card temp = cards[k];
            cards[k] = cards[n];
            cards[n] = temp;
        }
    }

    public Card DrawCard()
    {
        if (cards.Count == 0)
        {
            InitializeDeck();
            ShuffleDeck();
        }
        Card drawnCard = cards[0];
        cards.RemoveAt(0);
        return drawnCard;
    }
}
