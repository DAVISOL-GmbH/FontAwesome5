﻿using System;
using Windows.UI.Xaml.Media;

namespace FontAwesome5
{
    /// <summary>
    /// Provides FontFamilies and Typefaces of FontAwesome5.
    /// </summary>
    public static class Fonts
    {
        /// <summary>
        /// FontAwesome5 Regular FontFamily
        /// </summary>
        public static FontFamily RegularFontFamily = new FontFamily("ms-appx:///FontAwesome5.UWP/Fonts/Font Awesome 5 Free-Regular-400.otf#Font Awesome 5 Free");

        /// <summary>
        /// FontAwesome5 Solid FontFamily
        /// </summary>
        public static FontFamily SolidFontFamily = new FontFamily("ms-appx:///FontAwesome5.UWP/Fonts/Font Awesome 5 Free-Solid-900.otf#Font Awesome 5 Free");

        /// <summary>
        /// FontAwesome5 Brands FontFamily
        /// </summary>
        public static FontFamily BrandsFontFamily = new FontFamily("ms-appx:///FontAwesome5.UWP/Fonts/Font Awesome 5 Brands-Regular-400.otf#Font Awesome 5 Brands");

        /// <summary>
        /// FontAwesome5 Duotone FontFamily
        /// </summary>
        [Obsolete("This style is only available with the paid FontAwesome Pro license")]
        public static FontFamily DuotoneFontFamily = null;

        /// <summary>
        /// FontAwesome5 Light FontFamily
        /// </summary>
        [Obsolete("This style is only available with the paid FontAwesome Pro license")]
        public static FontFamily LightFontFamily = null;
    }
}
