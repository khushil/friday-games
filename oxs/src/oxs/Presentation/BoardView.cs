using Godot;
using OXS.Core;
using OXS.Presentation.Theme;

namespace OXS.Presentation;

public partial class BoardView : GridContainer
{
    [Signal]
    public delegate void CellClickedEventHandler(int row, int col);

    private readonly List<CellView> _cells = new();
    private int _boardSize;

    public override void _Ready()
    {
        // Apply theme spacing
        AddThemeConstantOverride("h_separation", (int)GameTheme.Sizes.CellGap);
        AddThemeConstantOverride("v_separation", (int)GameTheme.Sizes.CellGap);
    }

    public void Initialize(int boardSize)
    {
        _boardSize = boardSize;
        Columns = boardSize;

        // Clear existing children
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
        _cells.Clear();

        // Create cells with theme sizes
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                var cell = new CellView();
                cell.Initialize(row, col);
                cell.CustomMinimumSize = new Vector2(
                    GameTheme.Sizes.CellSize,
                    GameTheme.Sizes.CellSize
                );
                cell.CellClicked += OnCellClicked;
                AddChild(cell);
                _cells.Add(cell);
            }
        }
    }

    public void UpdateBoard(Board board)
    {
        for (int row = 0; row < _boardSize; row++)
        {
            for (int col = 0; col < _boardSize; col++)
            {
                var index = row * _boardSize + col;
                _cells[index].State = board[row, col];
            }
        }
    }

    public void SetCellState(int row, int col, CellState state)
    {
        var index = row * _boardSize + col;
        _cells[index].State = state;
    }

    public void HighlightWinningCells(List<(int Row, int Col)> winningPositions)
    {
        // Reset all cells first
        ResetHighlights();

        // Highlight winning cells
        foreach (var (row, col) in winningPositions)
        {
            var index = row * _boardSize + col;
            if (index >= 0 && index < _cells.Count)
            {
                _cells[index].IsWinningCell = true;
            }
        }
    }

    public void ResetHighlights()
    {
        foreach (var cell in _cells)
        {
            cell.IsWinningCell = false;
        }
    }

    private void OnCellClicked(int row, int col)
    {
        EmitSignal(SignalName.CellClicked, row, col);
    }
}
