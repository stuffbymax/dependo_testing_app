// ThemeManager.cs
using System.Drawing;

namespace dependo_testing_app
{
    public static class ThemeManager
    {
        public static bool IsDarkMode { get; private set; } = true;

        private static readonly Color DarkBack = Color.FromArgb(28, 28, 28);
        private static readonly Color DarkSurface = Color.FromArgb(45, 45, 48);
        private static readonly Color DarkText = Color.White;
        private static readonly Color DarkSubtleText = Color.FromArgb(170, 170, 170);
        
        private static readonly Color LightBack = Color.FromArgb(242, 242, 242);
        private static readonly Color LightSurface = Color.White;
        private static readonly Color LightText = Color.Black;
        private static readonly Color LightSubtleText = Color.FromArgb(100, 100, 100);

        public static readonly Color AccentColor = Color.FromArgb(0, 122, 204);

        public static Color Background => IsDarkMode ? DarkBack : LightBack;
        public static Color Surface => IsDarkMode ? DarkSurface : LightSurface;
        public static Color TextColor => IsDarkMode ? DarkText : LightText;
        public static Color SubtleTextColor => IsDarkMode ? DarkSubtleText : LightSubtleText;

        public static void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }
    }
}