using Godot;
using OXS.Networking;
using OXS.Networking.Transports;
using OXS.Presentation.Theme;

namespace OXS.Presentation;

/// <summary>
/// Multiplayer menu with tabs for LAN, WAN, and Steam modes.
/// </summary>
public partial class MultiplayerMenu : Control
{
    private static NetworkConfig? _pendingNetworkConfig;
    private static int _pendingBoardSize = 3;

    private ColorRect? _background;
    private TabContainer? _tabContainer;
    private Label? _titleLabel;
    private Label? _statusLabel;
    private Button? _backButton;

    // LAN controls
    private Button? _lanHostButton;
    private Button? _lanJoinButton;
    private LineEdit? _lanIpInput;
    private LineEdit? _lanPortInput;
    private Label? _lanLocalIpLabel;

    // WAN controls
    private Button? _wanHostButton;
    private Button? _wanJoinButton;
    private LineEdit? _wanRoomCodeInput;
    private Label? _wanRoomCodeLabel;

    // Steam controls
    private Button? _steamHostButton;
    private Button? _steamBrowseButton;
    private Label? _steamStatusLabel;

    // Board size
    private HSlider? _boardSizeSlider;
    private Label? _boardSizeLabel;
    private int _boardSize = 3;

    private NetworkManager? _networkManager;

    public static NetworkConfig? ConsumePendingNetworkConfig()
    {
        var config = _pendingNetworkConfig;
        _pendingNetworkConfig = null;
        return config;
    }

    public static int ConsumePendingBoardSize()
    {
        var size = _pendingBoardSize;
        _pendingBoardSize = 3;
        return size;
    }

    public override void _Ready()
    {
        _networkManager = GetNode<NetworkManager>("/root/NetworkManager");

        SetupBackground();
        SetupUI();

        // Fade in animation
        Modulate = new Color(1, 1, 1, 0);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0f, GameTheme.Animation.SceneTransition);
    }

    private void SetupBackground()
    {
        _background = GetNodeOrNull<ColorRect>("Background");
        if (_background == null)
        {
            _background = new ColorRect();
            _background.SetAnchorsPreset(LayoutPreset.FullRect);
            _background.Name = "Background";
            AddChild(_background);
            MoveChild(_background, 0);
        }

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
            _background.Color = GameTheme.Colors.GradientStart;
        }
    }

    private void SetupUI()
    {
        // Title
        _titleLabel = GetNode<Label>("VBoxContainer/TitleLabel");
        StyleLabel(_titleLabel, 48);

        // Tab container
        _tabContainer = GetNode<TabContainer>("VBoxContainer/TabContainer");
        StyleTabContainer(_tabContainer);

        // Status label
        _statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");
        StyleLabel(_statusLabel, 16);
        _statusLabel.Text = "";

        // Back button
        _backButton = GetNode<Button>("VBoxContainer/BackButton");
        StyleButton(_backButton);
        _backButton.Pressed += OnBackPressed;

        // Board size
        SetupBoardSizeControls();

        // Setup each tab
        SetupLanTab();
        SetupWanTab();
        SetupSteamTab();
    }

    private void SetupBoardSizeControls()
    {
        _boardSizeSlider = GetNode<HSlider>("VBoxContainer/BoardSizeSlider");
        _boardSizeLabel = GetNode<Label>("VBoxContainer/BoardSizeLabel");

        StyleLabel(_boardSizeLabel, 18);

        var grabber = new StyleBoxFlat();
        grabber.BgColor = GameTheme.Colors.TextPrimary;
        grabber.SetCornerRadiusAll(8);
        _boardSizeSlider.AddThemeStyleboxOverride("grabber", grabber);
        _boardSizeSlider.AddThemeStyleboxOverride("grabber_highlight", grabber);

        var sliderStyle = new StyleBoxFlat();
        sliderStyle.BgColor = GameTheme.Colors.GlassBackground;
        sliderStyle.SetCornerRadiusAll(4);
        _boardSizeSlider.AddThemeStyleboxOverride("slider", sliderStyle);

        _boardSizeSlider.ValueChanged += OnBoardSizeChanged;
    }

    private void SetupLanTab()
    {
        var lanTab = _tabContainer!.GetNode<Control>("LAN");

        _lanHostButton = lanTab.GetNode<Button>("VBox/HostButton");
        _lanJoinButton = lanTab.GetNode<Button>("VBox/JoinButton");
        _lanIpInput = lanTab.GetNode<LineEdit>("VBox/IpContainer/IpInput");
        _lanPortInput = lanTab.GetNode<LineEdit>("VBox/IpContainer/PortInput");
        _lanLocalIpLabel = lanTab.GetNode<Label>("VBox/LocalIpLabel");

        StyleButton(_lanHostButton);
        StyleButton(_lanJoinButton);
        StyleLineEdit(_lanIpInput);
        StyleLineEdit(_lanPortInput);
        StyleLabel(_lanLocalIpLabel, 14);

        // Display local IP
        var localIp = ENetPeerFactory.GetLocalIPAddress() ?? "Unknown";
        _lanLocalIpLabel.Text = $"Your IP: {localIp}";

        _lanHostButton.Pressed += OnLanHostPressed;
        _lanJoinButton.Pressed += OnLanJoinPressed;
    }

    private void SetupWanTab()
    {
        var wanTab = _tabContainer!.GetNode<Control>("WAN");

        _wanHostButton = wanTab.GetNode<Button>("VBox/HostButton");
        _wanJoinButton = wanTab.GetNode<Button>("VBox/JoinButton");
        _wanRoomCodeInput = wanTab.GetNode<LineEdit>("VBox/RoomCodeInput");
        _wanRoomCodeLabel = wanTab.GetNode<Label>("VBox/RoomCodeLabel");

        StyleButton(_wanHostButton);
        StyleButton(_wanJoinButton);
        StyleLineEdit(_wanRoomCodeInput);
        StyleLabel(_wanRoomCodeLabel, 14);

        _wanRoomCodeLabel.Text = "Enter room code to join";

        _wanHostButton.Pressed += OnWanHostPressed;
        _wanJoinButton.Pressed += OnWanJoinPressed;
    }

    private void SetupSteamTab()
    {
        var steamTab = _tabContainer!.GetNode<Control>("Steam");

        _steamHostButton = steamTab.GetNode<Button>("VBox/HostButton");
        _steamBrowseButton = steamTab.GetNode<Button>("VBox/BrowseButton");
        _steamStatusLabel = steamTab.GetNode<Label>("VBox/StatusLabel");

        StyleButton(_steamHostButton);
        StyleButton(_steamBrowseButton);
        StyleLabel(_steamStatusLabel, 14);

        // Steam not yet implemented
        _steamStatusLabel.Text = "Steam integration coming soon!";
        _steamHostButton.Disabled = true;
        _steamBrowseButton.Disabled = true;

        _steamHostButton.Pressed += OnSteamHostPressed;
        _steamBrowseButton.Pressed += OnSteamBrowsePressed;
    }

    private void StyleLabel(Label label, int fontSize)
    {
        label.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        label.AddThemeFontSizeOverride("font_size", fontSize);
    }

    private void StyleButton(Button button)
    {
        button.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        button.AddThemeColorOverride("font_hover_color", GameTheme.Colors.TextPrimary);
        button.AddThemeColorOverride("font_pressed_color", GameTheme.Colors.TextSecondary);
        button.AddThemeColorOverride("font_disabled_color", GameTheme.Colors.TextSecondary);
        button.AddThemeFontSizeOverride("font_size", 20);

        var normalStyle = CreateGlassStyleBox(GameTheme.Colors.GlassBackground);
        var hoverStyle = CreateGlassStyleBox(GameTheme.Colors.ButtonHover);
        var pressedStyle = CreateGlassStyleBox(GameTheme.Colors.ButtonPressed);
        var disabledStyle = CreateGlassStyleBox(new Color(0.2f, 0.2f, 0.2f, 0.3f));

        button.AddThemeStyleboxOverride("normal", normalStyle);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        button.AddThemeStyleboxOverride("focus", normalStyle);
        button.AddThemeStyleboxOverride("disabled", disabledStyle);

        button.MouseEntered += () => OnButtonHovered(button, true);
        button.MouseExited += () => OnButtonHovered(button, false);
    }

    private void StyleLineEdit(LineEdit lineEdit)
    {
        lineEdit.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        lineEdit.AddThemeColorOverride("font_placeholder_color", GameTheme.Colors.TextSecondary);
        lineEdit.AddThemeFontSizeOverride("font_size", 18);

        var style = CreateGlassStyleBox(GameTheme.Colors.GlassBackground);
        lineEdit.AddThemeStyleboxOverride("normal", style);
        lineEdit.AddThemeStyleboxOverride("focus", style);
    }

    private void StyleTabContainer(TabContainer tabs)
    {
        tabs.AddThemeColorOverride("font_selected_color", GameTheme.Colors.TextPrimary);
        tabs.AddThemeColorOverride("font_unselected_color", GameTheme.Colors.TextSecondary);
        tabs.AddThemeFontSizeOverride("font_size", 18);

        var panelStyle = CreateGlassStyleBox(GameTheme.Colors.GlassBackground);
        tabs.AddThemeStyleboxOverride("panel", panelStyle);

        var tabStyle = CreateGlassStyleBox(GameTheme.Colors.GlassBackground);
        tabStyle.ContentMarginTop = 8;
        tabStyle.ContentMarginBottom = 8;
        tabs.AddThemeStyleboxOverride("tab_selected", tabStyle);
        tabs.AddThemeStyleboxOverride("tab_unselected", tabStyle);
        tabs.AddThemeStyleboxOverride("tab_hovered", tabStyle);
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
        if (button.Disabled) return;

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

    private void OnBoardSizeChanged(double value)
    {
        _boardSize = (int)value;
        _boardSizeLabel!.Text = $"Board Size: {_boardSize}x{_boardSize}";
    }

    private void SetStatus(string message)
    {
        _statusLabel!.Text = message;
    }

    // LAN handlers
    private void OnLanHostPressed()
    {
        var factory = new ENetPeerFactory();
        var port = int.TryParse(_lanPortInput!.Text, out var p) ? p : ENetPeerFactory.DefaultPort;

        SetStatus("Starting server...");

        var error = _networkManager!.HostGame(port);
        if (error == Error.Ok)
        {
            SetStatus($"Hosting on port {port}. Waiting for player...");
            _pendingBoardSize = _boardSize;
            _pendingNetworkConfig = new NetworkConfig(ConnectionType.LAN, true, "", port);

            // Wait for player to connect before starting game
            _networkManager.PlayerConnected += OnPlayerConnected;
        }
        else
        {
            SetStatus($"Failed to start server: {error}");
        }
    }

    private void OnLanJoinPressed()
    {
        var factory = new ENetPeerFactory();
        var ip = _lanIpInput!.Text;
        var port = int.TryParse(_lanPortInput!.Text, out var p) ? p : ENetPeerFactory.DefaultPort;

        if (string.IsNullOrWhiteSpace(ip))
        {
            SetStatus("Please enter an IP address");
            return;
        }

        SetStatus($"Connecting to {ip}:{port}...");

        var error = _networkManager!.JoinGame(ip, port);
        if (error == Error.Ok)
        {
            _pendingBoardSize = _boardSize;
            _pendingNetworkConfig = new NetworkConfig(ConnectionType.LAN, false, ip, port);

            // Wait for connection confirmation
            _networkManager.PlayerConnected += OnPlayerConnected;
            _networkManager.ConnectionFailed += OnConnectionFailed;
        }
        else
        {
            SetStatus($"Failed to connect: {error}");
        }
    }

    // WAN handlers
    private void OnWanHostPressed()
    {
        SetStatus("WAN hosting not yet implemented");
        // TODO: Implement WebRTC hosting
    }

    private void OnWanJoinPressed()
    {
        SetStatus("WAN joining not yet implemented");
        // TODO: Implement WebRTC joining
    }

    // Steam handlers
    private void OnSteamHostPressed()
    {
        SetStatus("Steam hosting not yet implemented");
    }

    private void OnSteamBrowsePressed()
    {
        SetStatus("Steam lobby browser not yet implemented");
    }

    private void OnPlayerConnected(long peerId)
    {
        GD.Print($"[MultiplayerMenu] Player connected: {peerId}");
        _networkManager!.PlayerConnected -= OnPlayerConnected;
        _networkManager.ConnectionFailed -= OnConnectionFailed;

        SetStatus("Player connected! Starting game...");

        // Transition to networked game
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0f, GameTheme.Animation.SceneTransition);
        tween.TweenCallback(Callable.From(LoadNetworkedGame));
    }

    private void OnConnectionFailed()
    {
        _networkManager!.PlayerConnected -= OnPlayerConnected;
        _networkManager.ConnectionFailed -= OnConnectionFailed;
        SetStatus("Connection failed!");
    }

    private void OnBackPressed()
    {
        // Disconnect if connected
        _networkManager?.Disconnect();

        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0f, GameTheme.Animation.SceneTransition);
        tween.TweenCallback(Callable.From(LoadMainMenu));
    }

    private void LoadNetworkedGame()
    {
        var scene = GD.Load<PackedScene>("res://Presentation/Scenes/NetworkedGame.tscn");
        if (scene == null)
        {
            GD.PrintErr("[MultiplayerMenu] Failed to load NetworkedGame.tscn!");
            return;
        }
        GetTree().ChangeSceneToPacked(scene);
    }

    private void LoadMainMenu()
    {
        var scene = GD.Load<PackedScene>("res://Presentation/Scenes/Main.tscn");
        if (scene == null)
        {
            GD.PrintErr("[MultiplayerMenu] Failed to load Main.tscn!");
            return;
        }
        GetTree().ChangeSceneToPacked(scene);
    }
}

/// <summary>
/// Configuration for network game.
/// </summary>
public record NetworkConfig(
    ConnectionType Type,
    bool IsHost,
    string Address = "",
    int Port = 7777,
    string RoomCode = "",
    ulong SteamLobbyId = 0
);
