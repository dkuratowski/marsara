using System.Drawing;

namespace RC.RenderSystem
{
    /// <summary>
    /// This static class is used to get the standard CGA colors and palette settings.
    /// </summary>
    public static class CGAColor
    {
        public static Color Black { get { return Color.FromArgb(0, 0, 0); } }
        public static Color Blue { get { return Color.FromArgb(0, 0, 170); } }
        public static Color Green { get { return Color.FromArgb(0, 170, 0); } }
        public static Color Cyan { get { return Color.FromArgb(0, 170, 170); } }
        public static Color Red { get { return Color.FromArgb(170, 0, 0); } }
        public static Color Magenta { get { return Color.FromArgb(170, 0, 170); } }
        public static Color Brown { get { return Color.FromArgb(170, 85, 0); } }
        public static Color White { get { return Color.FromArgb(170, 170, 170); } }
        public static Color Gray { get { return Color.FromArgb(85, 85, 85); } }
        public static Color LightBlue { get { return Color.FromArgb(85, 85, 255); } }
        public static Color LightGreen { get { return Color.FromArgb(85, 255, 85); } }
        public static Color LightCyan { get { return Color.FromArgb(85, 255, 255); } }
        public static Color LightRed { get { return Color.FromArgb(255, 85, 85); } }
        public static Color LightMagenta { get { return Color.FromArgb(255, 85, 255); } }
        public static Color Yellow { get { return Color.FromArgb(255, 255, 85); } }
        public static Color WhiteHigh { get { return Color.FromArgb(255, 255, 255); } }
    }
}