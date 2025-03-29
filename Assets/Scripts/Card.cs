using System;

public class Card
{
    public int value;
    public string suit;

    public Card(int value, string suit)
    {
        this.value = value;
        this.suit = suit;
    }

    public override string ToString()
    {
        string cardValue = value switch
        {
            1 => "Ace",
            11 => "Jack",
            12 => "Queen",
            13 => "King",
            _ => value.ToString() // For numbers 2-10, just show the number
        };

        return $"{cardValue} of {suit}";
    }
}
