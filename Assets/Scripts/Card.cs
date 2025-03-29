using System;

public class Card
{
    public int value; // 1 = Ace, 11 = Jack, 12 = Queen, 13 = King
    public string suit;

    public Card(int value, string suit)
    {
        this.value = value;
        this.suit = suit;
    }

    public override string ToString()
    {
        return $"{value} of {suit}";
    }
}
