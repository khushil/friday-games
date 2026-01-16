using Godot;
using OXS.Core;

namespace OXS.Presentation;

public partial class CellView : Control {
    [Signal]
    public delegate void CellClickedEventHandler(int row, int col);

    private CellState _state = CellState.Empty;
    private int _row;
    private int _col;

    public int Row => _row;
    public int Col => _col;

    public CellState State {
        get => _state;
        set {
            _state = value;
            QueueRedraw();
        }
    }

    public void Initialize(int row, int col) {
        _row = row;
        _col = col;
    }

    public override void _Draw() {
        var rect = GetRect();
        var size = Mathf.Min(rect.Size.X, rect.Size.Y);
        var padding = size * 0.15f;
        var lineWidth = size * 0.1f;

        // Draw background
        DrawRect(new Rect2(Vector2.Zero, rect.Size), new Color(0.9f, 0.9f, 0.9f));

        // Draw border
        DrawRect(new Rect2(Vector2.Zero, rect.Size), new Color(0.3f, 0.3f, 0.3f), false, 2);

        switch (_state) {
            case CellState.X:
                DrawX(size, padding, lineWidth);
                break;
            case CellState.O:
                DrawO(size, padding, lineWidth);
                break;
        }
    }

    private void DrawX(float size, float padding, float lineWidth) {
        var color = new Color(0.2f, 0.4f, 0.8f);
        DrawLine(
            new Vector2(padding, padding),
            new Vector2(size - padding, size - padding),
            color, lineWidth
        );
        DrawLine(
            new Vector2(size - padding, padding),
            new Vector2(padding, size - padding),
            color, lineWidth
        );
    }

    private void DrawO(float size, float padding, float lineWidth) {
        var color = new Color(0.8f, 0.3f, 0.3f);
        var center = new Vector2(size / 2, size / 2);
        var radius = (size / 2) - padding;
        DrawArc(center, radius, 0, Mathf.Tau, 32, color, lineWidth);
    }

    public override void _GuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.ButtonIndex == MouseButton.Left &&
            mouseEvent.Pressed) {
            EmitSignal(SignalName.CellClicked, _row, _col);
        }
    }
}
