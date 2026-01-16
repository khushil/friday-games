using Godot;
using OXS.Core;

namespace OXS.Presentation;

public partial class BoardView : GridContainer {
    [Signal]
    public delegate void CellClickedEventHandler(int row, int col);

    private readonly List<CellView> _cells = new();
    private int _boardSize;

    public void Initialize(int boardSize) {
        _boardSize = boardSize;
        Columns = boardSize;

        // Clear existing children
        foreach (var child in GetChildren()) {
            child.QueueFree();
        }
        _cells.Clear();

        // Create cells
        for (int row = 0; row < boardSize; row++) {
            for (int col = 0; col < boardSize; col++) {
                var cell = new CellView();
                cell.Initialize(row, col);
                cell.CustomMinimumSize = new Vector2(100, 100);
                cell.CellClicked += OnCellClicked;
                AddChild(cell);
                _cells.Add(cell);
            }
        }
    }

    public void UpdateBoard(Board board) {
        for (int row = 0; row < _boardSize; row++) {
            for (int col = 0; col < _boardSize; col++) {
                var index = row * _boardSize + col;
                _cells[index].State = board[row, col];
            }
        }
    }

    public void SetCellState(int row, int col, CellState state) {
        var index = row * _boardSize + col;
        _cells[index].State = state;
    }

    private void OnCellClicked(int row, int col) {
        EmitSignal(SignalName.CellClicked, row, col);
    }
}
