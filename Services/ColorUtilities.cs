using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimpleZip.Services
{
    public enum UIColorStates
    {
        Hover,
        Default
    }

    public static class ColorUtilities
    {
        private static SolidColorBrush GetColorBrush(UIColorStates state, string defaultColor, string hoverColor)
        {
            string color = null;

            if (state == UIColorStates.Hover) color = hoverColor;
            if (state == UIColorStates.Default) color = defaultColor;

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

        }

        public static SolidColorBrush BackgroundDropZoneColor(UIColorStates state)
        {
            return GetColorBrush(state, "#CCD9F7", "#869FE0");
        }

        public static SolidColorBrush BtnCancelColor(UIColorStates state)
        {
            return GetColorBrush(state, "#E74C3C", "#F59F95");
        }

        public static SolidColorBrush BtnRetryColor(UIColorStates state)
        {

            return GetColorBrush(state, Colors.Transparent.ToString(), "#BACBF2");
        }
    }
}
