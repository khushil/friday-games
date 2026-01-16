using Godot;
using OXS.Core.AI;
using OXS.Presentation.Theme;

namespace OXS.Presentation;

public partial class MainMenu : Control
{
    private int _boardSize = 3;
    private static GameConfig? _pendingConfig;

    private Label? _titleLabel;
    private Control? _menuContainer;
    private ColorRect? _background;

    public static GameConfig? ConsumePendingConfig()
    {
        var config = _pendingConfig;
        _pendingConfig = null;
        return config;
    }

    public override void _Ready()
    {
        SetupBackground();
        SetupMenuContainer();
        SetupTitle();
        SetupButtons();
        SetupSlider();

        // Fade in animation
        Modulate = new Color(1, 1, 1, 0);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0f, GameTheme.Animation.SceneTransition);
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
            // Fallback: solid gradient-like color
            _background.Color = GameTheme.Colors.GradientStart;
        }
    }

    private void SetupMenuContainer()
    {
        _menuContainer = GetNode<Control>("VBoxContainer");

        // Add padding to container
        if (_menuContainer is VBoxContainer vbox)
        {
            vbox.AddThemeConstantOverride("separation", 16);
        }
    }

    private void SetupTitle()
    {
        _titleLabel = GetNode<Label>("VBoxContainer/TitleLabel");
        _titleLabel.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        _titleLabel.AddThemeFontSizeOverride("font_size", 72);

        // Subtle pulse animation for title
        AnimateTitlePulse();
    }

    private void AnimateTitlePulse()
    {
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(_titleLabel, "modulate:a", 0.7f, GameTheme.Animation.TitlePulse);
        tween.TweenProperty(_titleLabel, "modulate:a", 1.0f, GameTheme.Animation.TitlePulse);
    }

    private void SetupButtons()
    {
        var buttons = new[]
        {
            GetNode<Button>("VBoxContainer/LocalButton"),
            GetNode<Button>("VBoxContainer/EasyAIButton"),
            GetNode<Button>("VBoxContainer/MediumAIButton"),
            GetNode<Button>("VBoxContainer/HardAIButton"),
            GetNode<Button>("VBoxContainer/MultiplayerButton")
        };

        foreach (var button in buttons)
        {
            StyleButton(button);
        }

        // Connect button signals
        buttons[0].Pressed += () => StartGame(GameMode.LocalTwoPlayer, null);
        buttons[1].Pressed += () => StartGame(GameMode.VsAI, AIDifficulty.Easy);
        buttons[2].Pressed += () => StartGame(GameMode.VsAI, AIDifficulty.Medium);
        buttons[3].Pressed += () => StartGame(GameMode.VsAI, AIDifficulty.Hard);
        buttons[4].Pressed += OnMultiplayerPressed;
    }

    private void OnMultiplayerPressed()
    {
        GD.Print("[MainMenu] Opening multiplayer menu");

        // Fade out transition before loading scene
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0f, GameTheme.Animation.SceneTransition);
        tween.TweenCallback(Callable.From(LoadMultiplayerMenu));
    }

    private void LoadMultiplayerMenu()
    {
        var scene = GD.Load<PackedScene>("res://Presentation/Scenes/MultiplayerMenu.tscn");
        if (scene == null)
        {
            GD.PrintErr("[MainMenu] Failed to load MultiplayerMenu.tscn!");
            return;
        }
        GetTree().ChangeSceneToPacked(scene);
    }

    private void StyleButton(Button button)
    {
        // Style text
        button.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        button.AddThemeColorOverride("font_hover_color", GameTheme.Colors.TextPrimary);
        button.AddThemeColorOverride("font_pressed_color", GameTheme.Colors.TextSecondary);
        button.AddThemeFontSizeOverride("font_size", 20);

        // Create glass-like style boxes
        var normalStyle = CreateGlassStyleBox(GameTheme.Colors.GlassBackground);
        var hoverStyle = CreateGlassStyleBox(GameTheme.Colors.ButtonHover);
        var pressedStyle = CreateGlassStyleBox(GameTheme.Colors.ButtonPressed);

        button.AddThemeStyleboxOverride("normal", normalStyle);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        button.AddThemeStyleboxOverride("focus", normalStyle);

        // Add hover animation
        button.MouseEntered += () => OnButtonHovered(button, true);
        button.MouseExited += () => OnButtonHovered(button, false);
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

    private void OnButtonHovered(Button button, bool hovered)
    {
        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Cubic);

        if (hovered)
        {
            tween.TweenProperty(button, "scale", new Vector2(1.02f, 1.02f), GameTheme.Animation.HoverEffect);
        }
        else
        {
            tween.TweenProperty(button, "scale", Vector2.One, GameTheme.Animation.HoverEffect);
        }
    }

    private void SetupSlider()
    {
        var slider = GetNode<HSlider>("VBoxContainer/BoardSizeSlider");
        slider.ValueChanged += OnBoardSizeChanged;

        // Style the slider
        var grabber = new StyleBoxFlat();
        grabber.BgColor = GameTheme.Colors.TextPrimary;
        grabber.SetCornerRadiusAll(8);
        slider.AddThemeStyleboxOverride("grabber", grabber);
        slider.AddThemeStyleboxOverride("grabber_highlight", grabber);

        var sliderStyle = new StyleBoxFlat();
        sliderStyle.BgColor = GameTheme.Colors.GlassBackground;
        sliderStyle.SetCornerRadiusAll(4);
        slider.AddThemeStyleboxOverride("slider", sliderStyle);

        // Style the label
        var label = GetNode<Label>("VBoxContainer/BoardSizeLabel");
        label.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        label.AddThemeFontSizeOverride("font_size", 18);
    }

    private void OnBoardSizeChanged(double value)
    {
        _boardSize = (int)value;
        var label = GetNode<Label>("VBoxContainer/BoardSizeLabel");
        label.Text = $"Board Size: {_boardSize}x{_boardSize}";
    }

    private void StartGame(GameMode mode, AIDifficulty? difficulty)
    {
        var winLength = _boardSize == 5 ? 4 : _boardSize;

        _pendingConfig = new GameConfig(
            BoardSize: _boardSize,
            WinLength: winLength,
            Mode: mode,
            AIDifficulty: difficulty
        );

        GD.Print($"[MainMenu] Starting game: {mode}, BoardSize={_boardSize}");

        // Fade out transition before loading scene
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0f, GameTheme.Animation.SceneTransition);
        tween.TweenCallback(Callable.From(LoadGameScene));
    }

    private void LoadGameScene()
    {
        var gameScene = GD.Load<PackedScene>("res://Presentation/Scenes/Game.tscn");
        if (gameScene == null)
        {
            GD.PrintErr("[MainMenu] Failed to load Game.tscn!");
            return;
        }

        GetTree().ChangeSceneToPacked(gameScene);
    }
}
