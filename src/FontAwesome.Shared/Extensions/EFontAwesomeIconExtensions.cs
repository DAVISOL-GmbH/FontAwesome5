#if WINDOWS_UWP
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif
namespace FontAwesome5.Extensions
{
    /// <summary>
    /// EFontAwesomeIcon extensions
    /// </summary>
    public static class EFontAwesomeIconExtensions
    {
#if !WINDOWS_UWP
        /// <summary>
        /// Get the Typeface of an icon
        /// </summary>
        public static Typeface GetTypeFace(this EFontAwesomeIcon icon)
        {
            var iconStyle = icon.GetStyle();
            switch (iconStyle)
            {
                case EFontAwesomeStyle.Regular: return Fonts.RegularTypeface;
                case EFontAwesomeStyle.Solid: return Fonts.SolidTypeface;
                case EFontAwesomeStyle.Brands: return Fonts.BrandsTypeface;
            }

            if (FontAwesomeMetadata.License == FontAwesomeLicense.Pro)
            {
                switch (iconStyle)
                {
                    case EFontAwesomeStyle.Light: return Fonts.LightTypeface;
                    case EFontAwesomeStyle.Duotone: return Fonts.DuotoneTypeface;
                }
            }

            return Fonts.RegularTypeface;
        }
#endif
        /// <summary>
        /// Get the FontFamily of an icon
        /// </summary>
        public static FontFamily GetFontFamily(this EFontAwesomeIcon icon)
        {
            var iconStyle = icon.GetStyle();
            switch (iconStyle)
            {
                case EFontAwesomeStyle.Regular: return Fonts.RegularFontFamily;
                case EFontAwesomeStyle.Solid: return Fonts.SolidFontFamily;
                case EFontAwesomeStyle.Brands: return Fonts.BrandsFontFamily;
            }

            if (FontAwesomeMetadata.License == FontAwesomeLicense.Pro)
            {
                switch (iconStyle)
                {
                    case EFontAwesomeStyle.Light: return Fonts.LightFontFamily;
                    case EFontAwesomeStyle.Duotone: return Fonts.DuotoneFontFamily;
                }
            }

            return Fonts.RegularFontFamily;
        }
    }
}
