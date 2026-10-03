using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimpleZip.Services
{
    public enum BackgroundStates
    {
        Hover,
        Default
    }

    public static class ColorUtilities
    {
        public static SolidColorBrush GetColorBrush(BackgroundStates state)
        {
            string color = null;

            if (state == BackgroundStates.Hover) color = "#869FE0";
            if (state == BackgroundStates.Default) color = "#CCD9F7";

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

        }
    }
}
