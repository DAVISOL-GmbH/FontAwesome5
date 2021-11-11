using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FontAwesome5.Generator
{
    public enum EStyles
    {
        Solid,
        Regular,
        Brands,
        Light,
        Duotone
    }

    public class FontAwesomeManager
    {
        private static readonly Regex REG_PROP = new Regex(@"\([^)]*\)");

        public FontAwesomeManager(string iconsJson)
        {
            Icons = JsonConvert.DeserializeObject<Dictionary<string, Icon>>(File.ReadAllText(iconsJson));
        }

        public string Convert(string text)
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;

            var stringBuilder = new StringBuilder(textInfo.ToTitleCase(text.Replace("-", " ")));

            stringBuilder
                .Replace("-", string.Empty).Replace("/", "_")
                .Replace(" ", string.Empty).Replace(".", string.Empty)
                .Replace("'", string.Empty);

            var matches = REG_PROP.Matches(stringBuilder.ToString());
            stringBuilder = new StringBuilder(REG_PROP.Replace(stringBuilder.ToString(), string.Empty));
            var hasMatch = false;

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (match.Value.IndexOf("Hand", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    hasMatch = true;
                    break;
                }
            }

            if (hasMatch)
            {
                stringBuilder.Insert(0, "Hand");
            }

            if (char.IsDigit(stringBuilder[0]))
            {
                stringBuilder.Insert(0, '_');
            }

            return stringBuilder.ToString();
        }

        public Dictionary<string, Icon> Icons
        {
            get; set;
        }

        public class Icon
        {
            public string label { get; set; }
            public string unicode { get; set; }
            public List<string> styles { get; set; }
            public Dictionary<string, SVG> svg { get; set; }
        }

        public class SVG
        {
            private string _resolvedPath = null;
            private string _secondaryPath = null;

            public string[] viewBox { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            [JsonIgnore]
            public string path
            {
                get
                {
                    VerifyResolved();
                    return _resolvedPath;
                }
            }
            [JsonIgnore]
            public string secondaryPath
            {
                get
                {
                    VerifyResolved();
                    return _secondaryPath;
                }
            }

            [JsonProperty(nameof(path))]
            public object pathRaw { get; set; }

            private void VerifyResolved()
            {
                if (_resolvedPath != null)
                    return;

                if (pathRaw is string singlePath)
                {
                    _resolvedPath = singlePath;
                    return;
                }

                var pathParts = new List<string>();
                if (pathRaw is IEnumerable<string> manyPaths)
                    pathParts.AddRange(manyPaths);
                else if (pathRaw is Newtonsoft.Json.Linq.JArray jsonArray)
                {
                    foreach (var jToken in jsonArray)
                    {
                        if (jToken.Type == Newtonsoft.Json.Linq.JTokenType.String)
                        {
                            pathParts.Add((string)jToken);
                        }
                    }
                }
                else
                    throw new ArgumentException("This SVG item does not have a valid path!", "path");

                if (pathParts.Count == 0)
                {
                    throw new ArgumentException("This SVG item does not have a valid path!", "path");
                }
                else if (pathParts.Count <= 2)
                {
                    _resolvedPath = pathParts[0];
                    if (pathParts.Count == 2)
                        _secondaryPath = pathParts[1];
                }
                else
                {
                    throw new ArgumentException("This SVG item does not have a valid path!", "path");
                }
            }
        }
    }
}
