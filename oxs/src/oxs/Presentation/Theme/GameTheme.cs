using Godot;

namespace OXS.Presentation.Theme;

/// <summary>
/// Centralized theme configuration for the OXS game.
/// Glassmorphism style with modern colors and animations.
/// </summary>
public static class GameTheme
{
    /// <summary>
    /// Color palette for the glassmorphism UI.
    /// </summary>
    public static class Colors
    {
        // Background gradient colors
        public static readonly Color GradientStart = new("#667eea");  // Purple-blue
        public static readonly Color GradientEnd = new("#764ba2");    // Deep purple

        // Glass panel colors (semi-transparent)
        public static readonly Color GlassBackground = new(1f, 1f, 1f, 0.15f);
        public static readonly Color GlassBorder = new(1f, 1f, 1f, 0.3f);
        public static readonly Color GlassHighlight = new(1f, 1f, 1f, 0.4f);

        // Piece colors with glow variants
        public static readonly Color XPrimary = new("#00D9FF");       // Cyan
        public static readonly Color XGlow = new(0f, 0.85f, 1f, 0.4f);
        public static readonly Color OPrimary = new("#FF6B9D");       // Pink
        public static readonly Color OGlow = new(1f, 0.42f, 0.62f, 0.4f);

        // UI colors
        public static readonly Color TextPrimary = new(1f, 1f, 1f, 0.95f);
        public static readonly Color TextSecondary = new(1f, 1f, 1f, 0.7f);
        public static readonly Color ButtonHover = new(1f, 1f, 1f, 0.25f);
        public static readonly Color ButtonPressed = new(1f, 1f, 1f, 0.1f);

        // Win celebration
        public static readonly Color WinHighlight = new("#FFD700");   // Gold
    }

    /// <summary>
    /// Animation durations in seconds.
    /// </summary>
    public static class Animation
    {
        public const float PiecePlacement = 0.3f;
        public const float HoverEffect = 0.15f;
        public const float ButtonPress = 0.1f;
        public const float SceneTransition = 0.4f;
        public const float WinCelebration = 0.5f;
        public const float TitlePulse = 2.0f;
    }

    /// <summary>
    /// Size constants for UI elements.
    /// </summary>
    public static class Sizes
    {
        public const float CellSize = 110f;
        public const float CellGap = 8f;
        public const float BorderRadius = 16f;
        public const float BorderWidth = 1.5f;
        public const float GlowRadius = 8f;
        public const float PieceLineWidth = 8f;
        public const float PiecePadding = 0.18f;  // As fraction of cell size
    }
}
