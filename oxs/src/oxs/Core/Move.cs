namespace OXS.Core;

public readonly record struct Move(int Row, int Col) {
    public int ToIndex(int boardSize) => Row * boardSize + Col;

    public static Move FromIndex(int index, int boardSize) =>
        new(index / boardSize, index % boardSize);
}
