using System;
using ConsoleFileManager.Properties;

namespace ConsoleFileManager.FileManager
{
 /// <summary>
    /// Represents a data class that stores configuration of the application.
    /// </summary>
    public class AppConfig
    {
        /// <summary> The amount of file to show per page. </summary>
        public int FilesPerPage { get; set; }
        
        /// <summary> The symbol to draw window borders with. </summary>
        public string WindowBorderSymbol { get; set; }
        
        /// <summary> The background color of the console. </summary>
        public string BackgroundColor { get; set; }
        
        /// <summary> The foreground color of the console. </summary>
        public string ForegroundColor { get; set; }
        
        /// <summary> The height of the empty info window. </summary>
        public int EmptyInfoWindowHeight { get; set; }

        
        /// <summary>
        /// Initialize a new instance of config with default settings.
        /// </summary>
        public AppConfig()
        {
            FilesPerPage = Settings.Default.filesPerPageDefault;
            WindowBorderSymbol = Settings.Default.windowBorderSymbolDefault;
            BackgroundColor = Settings.Default.backgroundColorDefault;
            ForegroundColor = Settings.Default.foregroundColorDefault;
            EmptyInfoWindowHeight = Settings.Default.emptyInfoWindowHeight;
        }


        /// <summary>
        /// Get the color corresponding specified color name. <para/>
        /// If color name is incorrect then get the default color.
        /// </summary>
        /// <param name="colorName">The name of the color to get.</param>
        /// <param name="defaultValue">The default value for color if</param>
        /// <returns></returns>
        public ConsoleColor ParseColor(string colorName, string defaultValue)
        {
            // try to get the color from config
            var parsed = Enum.TryParse(colorName, true, out ConsoleColor color);

            // if color in config could not be parsed then use default color
            if (!parsed)
            {
                Enum.TryParse(defaultValue, true, out color);

                // write the default color value to config property to make the config correct
                if (BackgroundColor == colorName)
                    BackgroundColor = defaultValue;
                else if (ForegroundColor == colorName)
                    ForegroundColor = defaultValue;
            }

            return color;
        }
    }
}