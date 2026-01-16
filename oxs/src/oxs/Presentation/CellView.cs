using Godot;
using OXS.Core;
using OXS.Presentation.Theme;

namespace OXS.Presentation;

public partial class CellView : Control
{
    [Signal]
    public delegate void CellClickedEventHandler(int row, int col);

    private CellState _state = CellState.Empty;
    private int _row;
    private int _col;

    // Animation state
    private float _pieceScale = 0f;
    private float _pieceAlpha = 0f;
    private bool _isHovered = false;
    private bool _isWinningCell = false;
    private float _winPulse = 0f;
    private Tween? _winTween;

    public int Row => _row;
    public int Col => _col;

    public CellState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                if (_state != CellState.Empty)
                {
                    AnimatePiecePlacement();
                }
                else
                {
                    _pieceScale = 0f;
                    _pieceAlpha = 0f;
                }
                QueueRedraw();
            }
        }
    }

    public bool IsWinningCell
    {
        get => _isWinningCell;
        set
        {
            _isWinningCell = value;
            if (_isWinningCell)
            {
                AnimateWinCelebration();
            }
            else
            {
                _winTween?.Kill();
                _winPulse = 0f;
            }
            QueueRedraw();
        }
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;

        // Connect mouse enter/exit signals for hover effect
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public void Initialize(int row, int col)
    {
        _row = row;
        _col = col;
        _pieceScale = 0f;
        _pieceAlpha = 0f;
        _isWinningCell = false;
        _winPulse = 0f;
    }

    private void AnimatePiecePlacement()
    {
        var tween = CreateTween();
        tween.SetParallel(true);
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Back);

        // Scale from 0 to 1 with overshoot
        tween.TweenMethod(Callable.From<float>(SetPieceScale), 0f, 1f, GameTheme.Animation.PiecePlacement);

        // Fade in (faster than scale)
        tween.TweenMethod(Callable.From<float>(SetPieceAlpha), 0f, 1f, GameTheme.Animation.PiecePlacement * 0.5f);
    }

    private void SetPieceScale(float value)
    {
        _pieceScale = value;
        QueueRedraw();
    }

    private void SetPieceAlpha(float value)
    {
        _pieceAlpha = value;
        QueueRedraw();
    }

    private void AnimateWinCelebration()
    {
        _winTween?.Kill();
        _winTween = CreateTween();
        _winTween.SetLoops(6);

        _winTween.TweenMethod(Callable.From<float>(SetWinPulse), 0f, 1f, GameTheme.Animation.WinCelebration * 0.5f);
        _winTween.TweenMethod(Callable.From<float>(SetWinPulse), 1f, 0f, GameTheme.Animation.WinCelebration * 0.5f);
    }

    private void SetWinPulse(float value)
    {
        _winPulse = value;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var rect = GetRect();
        var size = Mathf.Min(rect.Size.X, rect.Size.Y);
        var padding = size * GameTheme.Sizes.PiecePadding;
        var lineWidth = GameTheme.Sizes.PieceLineWidth;

        // Draw glass background
        DrawGlassBackground(rect);

        // Draw hover highlight
        if (_isHovered && _state == CellState.Empty)
        {
            DrawRoundedRect(new Rect2(Vector2.Zero, rect.Size), GameTheme.Colors.ButtonHover, GameTheme.Sizes.BorderRadius);
        }

        // Draw win highlight
        if (_isWinningCell)
        {
            var highlightColor = GameTheme.Colors.WinHighlight;
            highlightColor.A = 0.2f + _winPulse * 0.3f;
            DrawRoundedRect(new Rect2(Vector2.Zero, rect.Size), highlightColor, GameTheme.Sizes.BorderRadius);
        }

        // Draw piece with animation
        if (_state != CellState.Empty && _pieceScale > 0.01f)
        {
            var center = new Vector2(size / 2, size / 2);

            switch (_state)
            {
                case CellState.X:
                    DrawXPiece(center, size, padding, lineWidth);
                    break;
                case CellState.O:
                    DrawOPiece(center, size, padding, lineWidth);
                    break;
            }
        }
    }

    private void DrawGlassBackground(Rect2 rect)
    {
        // Draw semi-transparent background
        DrawRoundedRect(new Rect2(Vector2.Zero, rect.Size), GameTheme.Colors.GlassBackground, GameTheme.Sizes.BorderRadius);

        // Draw border
        DrawRoundedRectOutline(new Rect2(Vector2.Zero, rect.Size), GameTheme.Colors.GlassBorder, GameTheme.Sizes.BorderRadius, GameTheme.Sizes.BorderWidth);

        // Draw subtle highlight at top (glassmorphism characteristic)
        var highlightColor = new Color(1f, 1f, 1f, 0.1f);
        var highlightRect = new Rect2(Vector2.Zero, new Vector2(rect.Size.X, rect.Size.Y * 0.3f));
        DrawRoundedRect(highlightRect, highlightColor, GameTheme.Sizes.BorderRadius, true);
    }

    private void DrawRoundedRect(Rect2 rect, Color color, float radius, bool topOnly = false)
    {
        // Approximate rounded rectangle with polygon
        var points = GetRoundedRectPoints(rect, radius, topOnly);
        DrawColoredPolygon(points, color);
    }

    private void DrawRoundedRectOutline(Rect2 rect, Color color, float radius, float width)
    {
        var points = GetRoundedRectPoints(rect, radius, false);
        // Close the loop
        var pointsList = new System.Collections.Generic.List<Vector2>(points) { points[0] };
        DrawPolyline(pointsList.ToArray(), color, width, true);
    }

    private Vector2[] GetRoundedRectPoints(Rect2 rect, float radius, bool topOnly)
    {
        var points = new System.Collections.Generic.List<Vector2>();
        var segments = 8;
        radius = Mathf.Min(radius, Mathf.Min(rect.Size.X / 2, rect.Size.Y / 2));

        // Top-left corner
        for (int i = 0; i <= segments; i++)
        {
            var angle = Mathf.Pi + (Mathf.Pi / 2) * i / segments;
            points.Add(new Vector2(radius + Mathf.Cos(angle) * radius, radius + Mathf.Sin(angle) * radius));
        }

        // Top-right corner
        for (int i = 0; i <= segments; i++)
        {
            var angle = -Mathf.Pi / 2 + (Mathf.Pi / 2) * i / segments;
            points.Add(new Vector2(rect.Size.X - radius + Mathf.Cos(angle) * radius, radius + Mathf.Sin(angle) * radius));
        }

        if (topOnly)
        {
            // Close at top
            points.Add(new Vector2(rect.Size.X, rect.Size.Y));
            points.Add(new Vector2(0, rect.Size.Y));
        }
        else
        {
            // Bottom-right corner
            for (int i = 0; i <= segments; i++)
            {
                var angle = (Mathf.Pi / 2) * i / segments;
                points.Add(new Vector2(rect.Size.X - radius + Mathf.Cos(angle) * radius, rect.Size.Y - radius + Mathf.Sin(angle) * radius));
            }

            // Bottom-left corner
            for (int i = 0; i <= segments; i++)
            {
                var angle = Mathf.Pi / 2 + (Mathf.Pi / 2) * i / segments;
                points.Add(new Vector2(radius + Mathf.Cos(angle) * radius, rect.Size.Y - radius + Mathf.Sin(angle) * radius));
            }
        }

        return points.ToArray();
    }

    private void DrawXPiece(Vector2 center, float size, float padding, float lineWidth)
    {
        var color = GameTheme.Colors.XPrimary;
        color.A = _pieceAlpha;

        var glowColor = GameTheme.Colors.XGlow;
        glowColor.A = _pieceAlpha * 0.5f;

        // Apply scale animation
        var scaledPadding = padding + (size / 2 - padding) * (1 - _pieceScale);
        var halfSize = (size / 2) - scaledPadding;

        // Draw glow (thicker, semi-transparent lines behind)
        var glowWidth = lineWidth + GameTheme.Sizes.GlowRadius;
        DrawLine(
            center + new Vector2(-halfSize, -halfSize),
            center + new Vector2(halfSize, halfSize),
            glowColor, glowWidth, true
        );
        DrawLine(
            center + new Vector2(halfSize, -halfSize),
            center + new Vector2(-halfSize, halfSize),
            glowColor, glowWidth, true
        );

        // Draw main X
        DrawLine(
            center + new Vector2(-halfSize, -halfSize),
            center + new Vector2(halfSize, halfSize),
            color, lineWidth, true
        );
        DrawLine(
            center + new Vector2(halfSize, -halfSize),
            center + new Vector2(-halfSize, halfSize),
            color, lineWidth, true
        );
    }

    private void DrawOPiece(Vector2 center, float size, float padding, float lineWidth)
    {
        var color = GameTheme.Colors.OPrimary;
        color.A = _pieceAlpha;

        var glowColor = GameTheme.Colors.OGlow;
        glowColor.A = _pieceAlpha * 0.5f;

        // Apply scale animation
        var scaledPadding = padding + (size / 2 - padding) * (1 - _pieceScale);
        var radius = (size / 2) - scaledPadding;

        // Draw glow
        var glowWidth = lineWidth + GameTheme.Sizes.GlowRadius;
        DrawArc(center, radius, 0, Mathf.Tau, 48, glowColor, glowWidth, true);

        // Draw main O
        DrawArc(center, radius, 0, Mathf.Tau, 48, color, lineWidth, true);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.ButtonIndex == MouseButton.Left &&
            mouseEvent.Pressed)
        {
            EmitSignal(SignalName.CellClicked, _row, _col);
        }
    }

    private void OnMouseEntered()
    {
        _isHovered = true;
        QueueRedraw();
    }

    private void OnMouseExited()
    {
        _isHovered = false;
        QueueRedraw();
    }
}
