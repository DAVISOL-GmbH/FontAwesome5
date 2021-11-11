using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Media;

namespace FontAwesome5
{
    /// <summary>
    /// Provides FontFamilies and Typefaces of FontAwesome5.
    /// </summary>
    public static class Fonts
    {
        static Fonts()
        {
            var path = Path.GetTempPath();
            SaveFontFilesToDirectory(path);
            LoadFromDirectory(path);
        }

        public static void LoadFromResource()
        {
            RegularFontFamily = new FontFamily(new Uri("pack://application:,,,/FontAwesome5.Net.Pro;component/"), "./Fonts/#Font Awesome 5 Pro Regular");
            SolidFontFamily = new FontFamily(new Uri("pack://application:,,,/FontAwesome5.Net.Pro;component/"), "./Fonts/#Font Awesome 5 Pro Solid");
            LightFontFamily = new FontFamily(new Uri("pack://application:,,,/FontAwesome5.Net.Pro;component/"), "./Fonts/#Font Awesome 5 Pro Light");
            DuotoneFontFamily = new FontFamily(new Uri("pack://application:,,,/FontAwesome5.Net.Pro;component/"), "./Fonts/#Font Awesome 5 Duotone Solid");
            BrandsFontFamily = new FontFamily(new Uri("pack://application:,,,/FontAwesome5.Net.Pro;component/"), "./Fonts/#Font Awesome 5 Brands Regular");
        }

        public static void LoadFromDirectory(string path)
        {
            RegularFontFamily = new FontFamily(new Uri($"file:///{path}", UriKind.Absolute), "./#Font Awesome 5 Pro Regular");
            SolidFontFamily = new FontFamily(new Uri($"file:///{path}", UriKind.Absolute), "./#Font Awesome 5 Pro Solid");
            LightFontFamily = new FontFamily(new Uri($"file:///{path}", UriKind.Absolute), "./#Font Awesome 5 Pro Light");
            DuotoneFontFamily = new FontFamily(new Uri($"file:///{path}", UriKind.Absolute), "./#Font Awesome 5 Duotone Solid");
            BrandsFontFamily = new FontFamily(new Uri($"file:///{path}", UriKind.Absolute), "./#Font Awesome 5 Brands Regular");
        }

        public static void SaveFontFilesToDirectory(string path)
        {
            var resManager = new ResourceManager("FontAwesome5.Net.Pro.g", Assembly.GetExecutingAssembly());
            WriteResourceToFile(resManager, $"Fonts/Font Awesome 5 Pro-Solid-900.otf", Path.Combine(path, "Font Awesome 5 Pro-Solid-900.otf"));
            WriteResourceToFile(resManager, $"Fonts/Font Awesome 5 Pro-Regular-400.otf", Path.Combine(path, "Font Awesome 5 Pro-Regular-400.otf"));
            WriteResourceToFile(resManager, $"Fonts/Font Awesome 5 Pro-Light-300.otf", Path.Combine(path, "Font Awesome 5 Pro-Light-300.otf"));
            WriteResourceToFile(resManager, $"Fonts/Font Awesome 5 Duotone-Solid-900.otf", Path.Combine(path, "Font Awesome 5 Duotone-Solid-900.otf"));
            WriteResourceToFile(resManager, $"Fonts/Font Awesome 5 Brands-Regular-400.otf", Path.Combine(path, "Font Awesome 5 Brands-Regular-400.otf"));
        }

        private static void WriteResourceToFile(ResourceManager resManager, string resourceName, string fileName)
        {
            if (File.Exists(fileName))
            {
                return;
            }

            using (var res = resManager.GetStream(Uri.EscapeUriString(resourceName).ToLowerInvariant()))
            {
                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    res.CopyTo(file);
                }
            }
        }

        /// <summary>
        /// FontAwesome5 Regular FontFamily
        /// </summary>
        public static FontFamily RegularFontFamily;

        /// <summary>
        /// FontAwesome5 Solid FontFamily
        /// </summary>
        public static FontFamily SolidFontFamily;

        /// <summary>
        /// FontAwesome5 Light FontFamily
        /// </summary>
        public static FontFamily LightFontFamily;

        /// <summary>
        /// FontAwesome5 Duotone FontFamily
        /// </summary>
        public static FontFamily DuotoneFontFamily;

        /// <summary>
        /// FontAwesome5 Brands FontFamily
        /// </summary>
        public static FontFamily BrandsFontFamily;


        /// <summary>
        /// FontAwesome5 Regular Typeface
        /// </summary>
        public static Typeface RegularTypeface => new Typeface(RegularFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        /// <summary>
        /// FontAwesome5 Solid Typeface
        /// </summary>
        public static Typeface SolidTypeface => new Typeface(SolidFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        /// <summary>
        /// FontAwesome5 Light Typeface
        /// </summary>
        public static Typeface LightTypeface => new Typeface(LightFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        /// <summary>
        /// FontAwesome5 Duotone Typeface
        /// </summary>
        public static Typeface DuotoneTypeface => new Typeface(DuotoneFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        /// <summary>
        /// FontAwesome5 Brands Typeface
        /// </summary>
        public static Typeface BrandsTypeface => new Typeface(BrandsFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
    }
}
