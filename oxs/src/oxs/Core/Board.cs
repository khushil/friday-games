namespace OXS.Core;

public sealed record Board {
    public int Size { get; }
    public ImmutableArray<CellState> Cells { get; }

    public Board(int size) {
        Size = size;
        Cells = Enumerable.Repeat(CellState.Empty, size * size).ToImmutableArray();
    }

    private Board(int size, ImmutableArray<CellState> cells) {
        Size = size;
        Cells = cells;
    }

    public CellState this[int row, int col] => Cells[row * Size + col];

    public Board WithMove(int row, int col, PlayerId player) {
        var index = row * Size + col;
        if (Cells[index] != CellState.Empty) {
            throw new InvalidOperationException("Cell is already occupied");
        }
        var newCells = Cells.SetItem(index, player == PlayerId.X ? CellState.X : CellState.O);
        return new Board(Size, newCells);
    }

    public bool IsCellEmpty(int row, int col) => this[row, col] == CellState.Empty;

    public bool IsFull => !Cells.Contains(CellState.Empty);
}
