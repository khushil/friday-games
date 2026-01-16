namespace OXS.Core;

public readonly record struct PlayerId(int Value) {
    public static readonly PlayerId X = new(0);
    public static readonly PlayerId O = new(1);

    public PlayerId GetOpponent() => Value == 0 ? O : X;
}
