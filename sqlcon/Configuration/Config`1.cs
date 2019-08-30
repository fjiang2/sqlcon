using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Sys.Stdio;

namespace sqlcon
{
    public static partial class Config
    {
        private static Configuration cfg = Program.Configuration;


        public static Brush GetSolidBrush(this string key, Color defaultColor)
        {
            if (cfg != null)
                return new SolidColorBrush(GetColor(key, defaultColor));

            return default;
        }

        private static Color GetColor(string key, Color defaultColor)
        {
            string colorString = cfg.GetValue<string>(key);

            if (colorString != null)
            {
                ColorConverter converter = new ColorConverter();

                if (converter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        Color color = (Color)converter.ConvertFrom(null, null, colorString);
                        return color;
                    }
                    catch (Exception)
                    {
                        cerr.WriteLine($"color setting {key} = {colorString} not supported");
                    }
                }
            }

            return defaultColor;
        }

    }
}

