using Godot;
using OXS.Core;
using OXS.Core.AI;
using OXS.Presentation.Theme;

namespace OXS.Presentation;

public partial class GameController : Control
{
    [Export]
    public NodePath BoardViewPath { get; set; } = null!;

    [Export]
    public NodePath StatusLabelPath { get; set; } = null!;

    [Export]
    public NodePath RematchButtonPath { get; set; } = null!;

    private BoardView _boardView = null!;
    private Label _statusLabel = null!;
    private Button _rematchButton = null!;
    private ColorRect? _background;

    private GameStateMachine _game = null!;
    private GameConfig _config = null!;
    private IAIPlayer? _aiPlayer;
    private PlayerId _humanPlayer = PlayerId.X;

    public override void _Ready()
    {
        SetupBackground();

        _boardView = GetNode<BoardView>(BoardViewPath);
        _statusLabel = GetNode<Label>(StatusLabelPath);
        _rematchButton = GetNode<Button>(RematchButtonPath);

        _boardView.CellClicked += OnCellClicked;
        _rematchButton.Pressed += OnRematchPressed;

        StyleUI();

        // Fade in animation
        Modulate = new Color(1, 1, 1, 0);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0f, GameTheme.Animation.SceneTransition);

        // Auto-start if config is pending from MainMenu
        var config = MainMenu.ConsumePendingConfig();
        if (config != null)
        {
            GD.Print($"[GameController] Starting with config: {config.Mode}, {config.BoardSize}x{config.BoardSize}");
            StartGame(config);
        }
    }

    private void SetupBackground()
    {
        // Check if background already exists in scene
        _background = GetNodeOrNull<ColorRect>("Background");
        if (_background == null)
        {
            // Create background programmatically
            _background = new ColorRect();
            _background.SetAnchorsPreset(LayoutPreset.FullRect);
            _background.Name = "Background";
            AddChild(_background);
            MoveChild(_background, 0);
        }

        // Apply gradient shader
        var shader = GD.Load<Shader>("res://Presentation/Shaders/gradient_background.gdshader");
        if (shader != null)
        {
            var material = new ShaderMaterial { Shader = shader };
            material.SetShaderParameter("color_top", GameTheme.Colors.GradientStart);
            material.SetShaderParameter("color_bottom", GameTheme.Colors.GradientEnd);
            material.SetShaderParameter("noise_intensity", 0.02f);
            material.SetShaderParameter("time_scale", 0.1f);
            _background.Material = material;
        }
        else
        {
            // Fallback: solid color
            _background.Color = GameTheme.Colors.GradientStart;
        }
    }

    private void StyleUI()
    {
        // Style status label
        _statusLabel.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        _statusLabel.AddThemeFontSizeOverride("font_size", 32);

        // Style rematch button with glass effect
        StyleRematchButton();
    }

    private void StyleRematchButton()
    {
        _rematchButton.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        _rematchButton.AddThemeColorOverride("font_hover_color", GameTheme.Colors.TextPrimary);
        _rematchButton.AddThemeColorOverride("font_pressed_color", GameTheme.Colors.TextSecondary);
        _rematchButton.AddThemeFontSizeOverride("font_size", 20);

        var normalStyle = CreateGlassStyleBox(GameTheme.Colors.GlassBackground);
        var hoverStyle = CreateGlassStyleBox(GameTheme.Colors.ButtonHover);
        var pressedStyle = CreateGlassStyleBox(GameTheme.Colors.ButtonPressed);

        _rematchButton.AddThemeStyleboxOverride("normal", normalStyle);
        _rematchButton.AddThemeStyleboxOverride("hover", hoverStyle);
        _rematchButton.AddThemeStyleboxOverride("pressed", pressedStyle);
        _rematchButton.AddThemeStyleboxOverride("focus", normalStyle);

        // Add hover animation
        _rematchButton.MouseEntered += () => OnButtonHovered(true);
        _rematchButton.MouseExited += () => OnButtonHovered(false);
    }

    private StyleBoxFlat CreateGlassStyleBox(Color bgColor)
    {
        var style = new StyleBoxFlat();
        style.BgColor = bgColor;
        style.BorderColor = GameTheme.Colors.GlassBorder;
        style.SetBorderWidthAll((int)GameTheme.Sizes.BorderWidth);
        style.SetCornerRadiusAll((int)GameTheme.Sizes.BorderRadius);
        style.ContentMarginLeft = 20;
        style.ContentMarginRight = 20;
        style.ContentMarginTop = 12;
        style.ContentMarginBottom = 12;
        return style;
    }

    private void OnButtonHovered(bool hovered)
    {
        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Cubic);

        if (hovered)
        {
            tween.TweenProperty(_rematchButton, "scale", new Vector2(1.02f, 1.02f), GameTheme.Animation.HoverEffect);
        }
        else
        {
            tween.TweenProperty(_rematchButton, "scale", Vector2.One, GameTheme.Animation.HoverEffect);
        }
    }

    public void StartGame(GameConfig config)
    {
        _config = config;
        _game = new GameStateMachine(config.BoardSize, config.WinLength);
        _game.GameEnded += OnGameEnded;

        if (config.Mode == GameMode.VsAI && config.AIDifficulty.HasValue)
        {
            _aiPlayer = AIFactory.Create(config.AIDifficulty.Value);
        }

        _boardView.Initialize(config.BoardSize);
        _rematchButton.Visible = false;
        UpdateStatus();
    }

    private void OnCellClicked(int row, int col)
    {
        if (_game == null || _game.Phase == GamePhase.GameOver)
        {
            return;
        }

        // In VsAI mode, only allow human player to move on their turn
        if (_config.Mode == GameMode.VsAI && _game.CurrentPlayer != _humanPlayer)
        {
            return;
        }

        var result = _game.MakeMove(row, col);
        if (result.IsSuccess)
        {
            _boardView.UpdateBoard(_game.Board);
            UpdateStatus();

            // If VsAI mode and game not over, trigger AI move
            if (_config.Mode == GameMode.VsAI &&
                _game.Phase == GamePhase.Playing &&
                _aiPlayer != null)
            {
                CallDeferred(nameof(MakeAIMove));
            }
        }
    }

    private void MakeAIMove()
    {
        if (_aiPlayer == null || _game.Phase == GamePhase.GameOver)
        {
            return;
        }

        var move = _aiPlayer.SelectMove(_game.Board, _game.CurrentPlayer, _config.WinLength);
        var result = _game.MakeMove(move.Row, move.Col);

        if (result.IsSuccess)
        {
            _boardView.UpdateBoard(_game.Board);
            UpdateStatus();
        }
    }

    private void OnGameEnded(GameResult result)
    {
        // Highlight winning cells if there's a winner
        if (result is GameResult.Win win)
        {
            var winningPositions = new List<(int Row, int Col)>();
            foreach (var move in win.WinningLine)
            {
                winningPositions.Add((move.Row, move.Col));
            }
            _boardView.HighlightWinningCells(winningPositions);
        }

        var message = result switch
        {
            GameResult.Win w => $"{(w.Winner == PlayerId.X ? "X" : "O")} Wins!",
            GameResult.Draw => "It's a Draw!",
            _ => ""
        };

        // Animate status text appearance
        _statusLabel.Modulate = new Color(1, 1, 1, 0);
        _statusLabel.Text = message;

        var textTween = CreateTween();
        textTween.SetEase(Tween.EaseType.Out);
        textTween.TweenProperty(_statusLabel, "modulate:a", 1.0f, 0.3f);

        // Animate rematch button appearance with delay
        _rematchButton.Visible = true;
        _rematchButton.Modulate = new Color(1, 1, 1, 0);
        _rematchButton.Scale = new Vector2(0.8f, 0.8f);

        var btnTween = CreateTween();
        btnTween.SetParallel(true);
        btnTween.SetEase(Tween.EaseType.Out);
        btnTween.SetTrans(Tween.TransitionType.Back);
        btnTween.TweenProperty(_rematchButton, "modulate:a", 1.0f, 0.3f).SetDelay(0.2f);
        btnTween.TweenProperty(_rematchButton, "scale", Vector2.One, 0.3f).SetDelay(0.2f);
    }

    private void OnRematchPressed()
    {
        // Reset highlights
        _boardView.ResetHighlights();

        // Alternate starting player
        var startingPlayer = _game.CurrentPlayer == PlayerId.X ? PlayerId.O : PlayerId.X;
        _game.StartNextRound(startingPlayer);
        _boardView.Initialize(_config.BoardSize);
        _rematchButton.Visible = false;
        UpdateStatus();

        // If AI starts, trigger AI move
        if (_config.Mode == GameMode.VsAI &&
            _game.CurrentPlayer != _humanPlayer &&
            _aiPlayer != null)
        {
            CallDeferred(nameof(MakeAIMove));
        }
    }

    private void UpdateStatus()
    {
        if (_game != null && _game.Phase == GamePhase.Playing)
        {
            var player = _game.CurrentPlayer == PlayerId.X ? "X" : "O";
            _statusLabel.Text = $"{player}'s Turn";
        }
    }
}
