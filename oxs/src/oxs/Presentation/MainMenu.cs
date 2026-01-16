using Godot;
using OXS.Core.AI;

namespace OXS.Presentation;

public partial class MainMenu : Control {
    private int _boardSize = 3;
    private static GameConfig? _pendingConfig;

    public static GameConfig? ConsumePendingConfig() {
        var config = _pendingConfig;
        _pendingConfig = null;
        return config;
    }

    public override void _Ready() {
        GetNode<Button>("VBoxContainer/LocalButton").Pressed += () => StartGame(GameMode.LocalTwoPlayer, null);
        GetNode<Button>("VBoxContainer/EasyAIButton").Pressed += () => StartGame(GameMode.VsAI, AIDifficulty.Easy);
        GetNode<Button>("VBoxContainer/MediumAIButton").Pressed += () => StartGame(GameMode.VsAI, AIDifficulty.Medium);
        GetNode<Button>("VBoxContainer/HardAIButton").Pressed += () => StartGame(GameMode.VsAI, AIDifficulty.Hard);

        var slider = GetNode<HSlider>("VBoxContainer/BoardSizeSlider");
        slider.ValueChanged += OnBoardSizeChanged;
    }

    private void OnBoardSizeChanged(double value) {
        _boardSize = (int)value;
        var label = GetNode<Label>("VBoxContainer/BoardSizeLabel");
        label.Text = $"Board Size: {_boardSize}x{_boardSize}";
    }

    private void StartGame(GameMode mode, AIDifficulty? difficulty) {
        var winLength = _boardSize == 5 ? 4 : _boardSize;

        _pendingConfig = new GameConfig(
            BoardSize: _boardSize,
            WinLength: winLength,
            Mode: mode,
            AIDifficulty: difficulty
        );

        GD.Print($"[MainMenu] Starting game: {mode}, BoardSize={_boardSize}");

        var gameScene = GD.Load<PackedScene>("res://Presentation/Scenes/Game.tscn");
        if (gameScene == null) {
            GD.PrintErr("[MainMenu] Failed to load Game.tscn!");
            return;
        }

        GetTree().ChangeSceneToPacked(gameScene);
    }
}
