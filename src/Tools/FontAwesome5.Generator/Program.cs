using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FontAwesome5.Generator
{
    class Program
  {
    #region Private class to wrap source folder

    private class FontAwesomeSourceInformation
    {
      private FontAwesomeSourceInformation(string folderPath, string version)
      {
        FolderPath = folderPath;
        Version = version;
        License = GetLicense(folderPath);
      }

      public static FontAwesomeSourceInformation CreateWithoutVersion(string folderPath)
      {
        return Create(folderPath, null);
      }

      public static FontAwesomeSourceInformation Create(string folderPath, string version)
      {
        return new FontAwesomeSourceInformation(folderPath, version);
      }

      public string FolderPath { get; }

      public string Version { get; }

      public string License { get; }
    }

    #endregion

    private const string FontAwesomeSourceEnvVarName = "FONTAWESOME5_SOURCE_FOLDER";
    private const string NuspecFreeSourceRelativeFilePath = @"src\NuGet\FontAwesome5.template.Free.nuspec";
    private const string NuspecProSourceRelativeFilePath = @"src\NuGet\FontAwesome5.template.Pro.nuspec";
    private const string NuspecTargetRelativeFilePath = @"src\NuGet\FontAwesome5.nuspec";
    private const string FontAwesomeFontsRelativeTargetPath = @"font-awesome\otfs";
    private const string FreeLicense = "Free";
    private const string ProLicense = "Pro";


    private static readonly StringBuilder _content = new StringBuilder();
    private static readonly Stack<string> _indent = new Stack<string>();

    static int Main(string[] args)
    {
      var inputDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
      var sourcePath = inputDirectory;

      if (args.Length > 0)
      {
        sourcePath = args[0];
      }
      else
      {
        var envPath = Environment.GetEnvironmentVariable(FontAwesomeSourceEnvVarName);
        if (!string.IsNullOrEmpty(envPath))
        {
          Console.WriteLine($"Using source path from environment variable {FontAwesomeSourceEnvVarName}: " + envPath);
          sourcePath = envPath;
        }
        else
        {
#if DEBUG
                    Console.WriteLine($"MISSING environment variable {FontAwesomeSourceEnvVarName}. Listing env vars:");
                    foreach (var item in Environment.GetEnvironmentVariables().OfType<DictionaryEntry>())
                    {
                        Console.WriteLine($"  {item.Key} - {item.Value}");
                    }
#else
          Console.WriteLine($"MISSING environment variable {FontAwesomeSourceEnvVarName}.");
#endif

          return -1;
        }
      }

      var result = Generate(inputDirectory, sourcePath);

      Console.WriteLine("All done. I'll leave if you press any key...");
      Console.ReadKey();

      return result;
    }

    static int Generate(string inputDirectory, string sourcePath)
    {
      var sourceDir = GetLatestVersionDir(sourcePath);

      if (sourceDir == null)
      {
        Console.WriteLine("No FontAwesome Desktop was found. Stopping.");
        return -2;
      }

      Console.WriteLine($"Using FontAwesome5 Pro from {sourceDir}");
      Console.WriteLine($"Parsing icons and write enum file ...");

      var configFile = Path.Combine(sourceDir.FolderPath, @"metadata\icons.json");

      //var outputFile = Path.Combine(inputDirectory, @"src\FontAwesome5\EFontAwesomeIcon.cs");
      //var configFile = Path.Combine(inputDirectory, @"Font-Awesome\metadata\icons.json");

      var fa = new FontAwesomeManager(configFile);
      var isProLicense = sourceDir.License == ProLicense;
      var useStyles = (from EStyles style in Enum.GetValues(typeof(EStyles))
                         //where isProLicense || (style != EStyles.Light && style != EStyles.Duotone)
                       select style).ToArray();

      #region Build EFontAwesomeIcon.cs

      var outputFile = Path.Combine(inputDirectory, @"src\FontAwesome5\EFontAwesomeIcon.cs");
      _content.Clear();
      _indent.Clear();

      WriteHeader(sourceDir, "System.ComponentModel");

      WriteClassBegin();

      WriteLine("");
      WriteSummary("FontAwesome5 Icon Styles", "FontAwesome by Dave Gandy (@davegandy)", "The iconic SVG, font, and CSS toolkit", "License https://fontawesome.com/license (C#: MIT License)");
      WriteLine("public enum EFontAwesomeStyle");
      WriteLine("{");
      PushIndent("\t");
      WriteSummary("This Style is used as an undefined state.");
      WriteLine("None = 0,");
      WriteLine("");
      foreach (EStyles style in useStyles)
      {
        WriteSummary($"FontAwesome5 {style:g} Style");
        if (!isProLicense && (style == EStyles.Light || style == EStyles.Duotone))
          WriteLine("[Obsolete(\"This style is only available with the paid FontAwesome Pro license\")]");
        WriteLine("{0},", style);
        WriteLine("");
      }
      WriteBlockEnd();


      WriteLine("");
      WriteSummary("FontAwesome5 Icons", "FontAwesome by Dave Gandy (@davegandy)", "The iconic SVG, font, and CSS toolkit", "License https://fontawesome.com/license (C#: MIT License)");
      WriteLine("public enum EFontAwesomeIcon");
      WriteLine("{");
      PushIndent("\t");
      WriteSummary("Set this value to show no icon.");
      WriteLine("None = 0x0,");

      foreach (EStyles style in useStyles)
      {
        var lowerStyleName = style.ToString().ToLower();
        foreach (var kvp in fa.Icons.Where(i => i.Value.styles.Contains(lowerStyleName)))
        {
          WriteSummary(kvp.Value.label);
          WriteLine($"///<see href=\"http://fontawesome.com/icons/{kvp.Key}?style={lowerStyleName}\" />");
          WriteLine($"[FontAwesomeInformation(\"{kvp.Value.label}\", EFontAwesomeStyle.{style}, 0x{kvp.Value.unicode})]");

          if (kvp.Value.svg.TryGetValue(lowerStyleName, out var svgInfo) && svgInfo.path is string svgPath)
          {
            if (string.IsNullOrEmpty(svgInfo.secondaryPath))
              WriteLine($"[FontAwesomeSvgInformation(\"{svgPath}\", {svgInfo.width}, {svgInfo.height})]");
            else
              WriteLine($"[FontAwesomeSvgInformation(\"{svgPath}\", {svgInfo.width}, {svgInfo.height}, \"{svgInfo.secondaryPath}\")]");
          }
          WriteLine($"{style}_{fa.Convert(kvp.Key)},");
          WriteLine("");
        }
      }

      WriteBlockEnd();

      WriteFontAwesomeInformationAttribute();

      WriteFontAwesomeSvgInformationAttribute();

      WriteMetadata(sourceDir);

      WriteBlockEnd();

      Console.WriteLine();
      Console.WriteLine($"Step 1 - Parsing icons and write enum file done, writing to {outputFile} ...");

      File.WriteAllText(outputFile, _content.ToString());

      #endregion


      #region Build FontAwesomeInternal.cs

      outputFile = Path.Combine(inputDirectory, @"src\FontAwesome5\FontAwesomeInternal.cs");
      _content.Clear();
      _indent.Clear();

      WriteHeader(sourceDir, "System.Collections.Generic");

      WriteClassBegin();

      WriteLine("internal static class FontAwesomeInternal");
      WriteLine("{");
      PushIndent("\t");
      WriteLine("public static Dictionary<EFontAwesomeIcon, FontAwesomeInformation> Information = new Dictionary<EFontAwesomeIcon, FontAwesomeInformation>() {");
      PushIndent("\t");

      foreach (EStyles style in useStyles)
      {
        foreach (var kvp in fa.Icons.Where(i => i.Value.styles.Contains(style.ToString().ToLower())))
        {

          if (kvp.Value.svg.TryGetValue(style.ToString().ToLower(), out var svgInfo))
          {
            if (string.IsNullOrEmpty(svgInfo.secondaryPath))
              WriteLine("{{EFontAwesomeIcon.{0}_{1}, new FontAwesomeInformation(\"{2}\", EFontAwesomeStyle.{0}, \"{3}\", new FontAwesomeSvgInformation(\"{4}\", {5}, {6}))}},",
              style, fa.Convert(kvp.Key), kvp.Value.label, char.ConvertFromUtf32(Convert.ToInt32("0x" + kvp.Value.unicode, 16)),
              svgInfo.path, svgInfo.width, svgInfo.height);
            else
              WriteLine("{{EFontAwesomeIcon.{0}_{1}, new FontAwesomeInformation(\"{2}\", EFontAwesomeStyle.{0}, \"{3}\", new FontAwesomeSvgInformation(\"{4}\", {5}, {6}, \"{7}\"))}},",
              style, fa.Convert(kvp.Key), kvp.Value.label, char.ConvertFromUtf32(Convert.ToInt32("0x" + kvp.Value.unicode, 16)),
              svgInfo.path, svgInfo.width, svgInfo.height, svgInfo.secondaryPath);
          }
          else
          {
            WriteLine("{{EFontAwesomeIcon.{0}_{1}, new FontAwesomeInformation(\"{2}\", EFontAwesomeStyle.{0}, \"{3}\")}},",
                      style, fa.Convert(kvp.Key), kvp.Value.label, char.ConvertFromUtf32(Convert.ToInt32("0x" + kvp.Value.unicode, 16)));

          }
        }
      }

      PopIndent();
      WriteLine("};");

      WriteBlockEnd();

      WriteFontAwesomeInformationClass();
      WriteFontAwesomeSvgInformationClass();

      WriteBlockEnd();

      var fileContents = _content.ToString();
      File.WriteAllText(outputFile, fileContents);

      #endregion

      Console.WriteLine($"Output files written.");

      CopyFonts(inputDirectory, sourceDir);

      UpdateNuspecFile(sourceDir, inputDirectory);

      return 0;
    }

    private static void WriteHeader(FontAwesomeSourceInformation sourceDir, params string[] additionalUsings)
    {
      WriteLine("//------------------------------------------------------------------------------");
      WriteLine("// <auto-generated>");
      WriteLine("//     This code was generated by a tool");
      WriteLine("//");
      WriteLine("//     Changes to this file may cause incorrect behavior and will be lost if");
      WriteLine("//     the code is regenerated.");
      WriteLine($"//     Created at {DateTime.Now.ToString("g", CultureInfo.InvariantCulture)}.");
      WriteLine($"//     Created from Font Awesome version {sourceDir.Version ?? "<unknwon>"}.");
      WriteLine("// </auto-generated>");
      WriteLine("//------------------------------------------------------------------------------");

      WriteLine("using System;");
      if (additionalUsings != null && additionalUsings.Length > 0)
      {
        foreach (var usingName in additionalUsings)
        {
          if (!string.IsNullOrEmpty(usingName))
            WriteLine($"using {usingName};");
        }
      }
    }

    private static void WriteClassBegin()
    {
      WriteLine("namespace FontAwesome5");
      WriteLine("{");
      PushIndent("\t");
    }

    private static void WriteBlockEnd()
    {
      PopIndent();
      WriteLine("}");
    }

    private static void WriteFontAwesomeInformationAttribute()
    {
      WriteLine("");
      WriteSummary("FontAwesome Information Attribute");
      WriteLine("public class FontAwesomeInformationAttribute : Attribute");
      WriteLine("{");
      PushIndent("\t");
      WriteSummary("FontAwesome Style");
      WriteLine("public EFontAwesomeStyle Style { get; set; }");
      WriteSummary("FontAwesome Label");
      WriteLine("public string Label { get; set; }");
      WriteSummary("FontAwesome Unicode");
      WriteLine("public int Unicode { get; set; }");
      WriteLine("");
      WriteSummary("Creates a new instance of this attribute. For internal use only!");
      WriteLine("public FontAwesomeInformationAttribute(string label, EFontAwesomeStyle style, int unicode)");
      WriteLine("{");
      WriteLine("    Label = label;");
      WriteLine("    Style = style;");
      WriteLine("    Unicode = unicode;");
      WriteLine("}");
      PopIndent();
      WriteLine("}");
    }

    private static void WriteFontAwesomeSvgInformationAttribute()
    {
      WriteLine("");
      WriteSummary("FontAwesome SVG Information Attribute");
      WriteLine("public class FontAwesomeSvgInformationAttribute : Attribute");
      WriteLine("{");
      PushIndent("\t");
      WriteSummary("FontAwesome SVG Path");
      WriteLine("public string Path { get; set; }");
      WriteSummary("FontAwesome SVG Width");
      WriteLine("public int Width { get; set; }");
      WriteSummary("FontAwesome SVG Height");
      WriteLine("public int Height { get; set; }");
      WriteSummary("FontAwesome SVG Secondary Path (for duotone icons only, FontAwesome Pro is required)");
      WriteLine("public string SecondaryPath { get; set; }");
      WriteSummary("Checks for duotone SVG");
      WriteLine("public bool IsDuotone => !string.IsNullOrEmpty(SecondaryPath);");
      WriteLine("");
      WriteSummary("Creates a new instance of this attribute. For internal use only!");
      WriteLine("public FontAwesomeSvgInformationAttribute(string path, int width, int height, string secondaryPath = null)");
      WriteLine("{");
      WriteLine("    Path = path;");
      WriteLine("    Width = width;");
      WriteLine("    Height = height;");
      WriteLine("    SecondaryPath = secondaryPath;");
      WriteLine("}");
      PopIndent();
      WriteLine("}");
    }

    private static void WriteFontAwesomeInformationClass()
    {
      WriteLine("");
      WriteSummary("Represents a FontAwesome icon.");
      WriteLine("public class FontAwesomeInformation");
      WriteLine("{");
      PushIndent("\t");
      WriteSummary("FontAwesome Label");
      WriteLine("public string Label { get; set; }");
      WriteSummary("FontAwesome Style");
      WriteLine("public EFontAwesomeStyle Style { get; set; }");
      WriteSummary("FontAwesome Unicode");
      WriteLine("public string Unicode { get; set; }");
      WriteSummary("FontAwesome Svg");
      WriteLine("public FontAwesomeSvgInformation Svg { get; set; }");
      WriteSummary("Checks for duotone SVG");
      WriteLine("public bool IsDuotone => Svg?.IsDuotone ?? false;");
      WriteLine("");
      WriteSummary("Creates a new instance. For internal use only!");
      WriteLine("public FontAwesomeInformation(string label, EFontAwesomeStyle style, string unicode, FontAwesomeSvgInformation svg = null)");
      WriteLine("{");
      WriteLine("    Label = label;");
      WriteLine("    Style = style;");
      WriteLine("    Unicode = unicode;");
      WriteLine("    Svg = svg;");
      WriteLine("}");
      PopIndent();
      WriteLine("}");
    }

    private static void WriteFontAwesomeSvgInformationClass()
    {
      WriteLine("");
      WriteSummary("Represents the SVG data of a FontAwesome icon.");
      WriteLine("public class FontAwesomeSvgInformation");
      WriteLine("{");
      PushIndent("\t");
      WriteSummary("The SVG path that renders the icon.");
      WriteLine("public string Path { get; set; }");
      WriteSummary("The desired width of the icon.");
      WriteLine("public int Width { get; set; }");
      WriteSummary("The desired height of the icon.");
      WriteLine("public int Height { get; set; }");
      WriteSummary("FontAwesome SVG Secondary Path (for duotone icons only, FontAwesome Pro is required)");
      WriteLine("public string SecondaryPath { get; set; }");
      WriteSummary("Checks for duotone SVG");
      WriteLine("public bool IsDuotone => !string.IsNullOrEmpty(SecondaryPath);");
      WriteLine("");
      WriteSummary("Creates a new instance. For internal use only!");
      WriteLine("public FontAwesomeSvgInformation(string path, int width, int height, string secondaryPath = null)");
      WriteLine("{");
      WriteLine("    Path = path;");
      WriteLine("    Width = width;");
      WriteLine("    Height = height;");
      WriteLine("    SecondaryPath = secondaryPath;");
      WriteLine("}");
      PopIndent();
      WriteLine("}");
    }

    private static void WriteMetadata(FontAwesomeSourceInformation sourceDir)
    {
      WriteLine("");
      WriteSummary("FontAwesome License Types");
      WriteLine("public enum FontAwesomeLicense : int");
      WriteLine("{");
      PushIndent("\t");
      WriteSummary("FontAwesome Free License");
      WriteLine(FreeLicense + ",");
      WriteSummary("FontAwesome Pro License (Paid)");
      WriteLine(ProLicense);
      PopIndent();
      WriteLine("}");


      WriteLine("");
      WriteSummary("FontAwesome Package Metadata");
      WriteLine("public static class FontAwesomeMetadata");
      WriteLine("{");
      PushIndent("\t");
      WriteSummary("FontAwesome License");
      WriteLine($"public const FontAwesomeLicense License = FontAwesomeLicense.{(sourceDir.License == ProLicense ? ProLicense : FreeLicense)};");
      WriteSummary("FontAwesome Version");
      WriteLine($"public const string Version = \"{(string.IsNullOrEmpty(sourceDir.Version) ? "unknown" : sourceDir.Version)}\";");
      PopIndent();
      WriteLine("}");
    }

    static void WriteSummary(params string[] summaryLines)
    {
      if (summaryLines == null || summaryLines.Length <= 0)
        return;

      WriteLine("/// <summary>");
      foreach (var summary in summaryLines)
      {
        WriteLine("/// {0}", summary);

      }
      WriteLine("/// </summary>");
    }

    static void WriteLine(string text)
    {
      _content.AppendLine(GetIndent() + text);
    }

    static void WriteLine(string text, params object[] parameter)
    {
      _content.AppendLine(GetIndent() + string.Format(text, parameter));
    }

    static void PushIndent(string indent)
    {
      _indent.Push(indent);
    }

    static void PopIndent()
    {
      _indent.Pop();
    }

    static string GetIndent()
    {
      var indent = "";
      foreach (var entry in _indent)
      {
        indent += entry;
      }

      return indent;
    }

    private static FontAwesomeSourceInformation GetLatestVersionDir(string sourcePath)
    {
      var rootPath = sourcePath; // Path.Combine(sourcePath, "Font-Awesome");

      if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
      {
        Console.WriteLine("The source path is empty or does not exist!");
        return null;
      }

      var testPath = GetMetadataTestPath(rootPath);
      if (!string.IsNullOrEmpty(testPath))
        return FontAwesomeSourceInformation.CreateWithoutVersion(testPath);

      testPath = GetMetadataTestPath(Path.Combine(rootPath, "current"));
      if (!string.IsNullOrEmpty(testPath))
        return FontAwesomeSourceInformation.CreateWithoutVersion(testPath);

      var rootDir = new DirectoryInfo(rootPath);
      var subDirectories = rootDir.GetDirectories().OrderByDescending(x => x.Name).ToArray();

      foreach (var subDir in subDirectories)
      {
        testPath = GetMetadataTestPath(subDir.FullName);
        if (!string.IsNullOrEmpty(testPath))
        {
          var rawVersion = subDir.Name.Trim(' ', '\t', '\r', '\n', 'v');
          return FontAwesomeSourceInformation.Create(testPath, rawVersion);
        }
      }

      return null;
    }

    private static string GetMetadataTestPath(string directoryPath)
    {
      if (string.IsNullOrEmpty(directoryPath))
        return null;

      try
      {
        Console.Write($"Check metadata at {directoryPath} ...");
        if (!Directory.Exists(directoryPath))
        {
          Console.WriteLine(" directory does not exist!");
          return null;
        }

        var testPath = Path.Combine(directoryPath, "metadata", "icons.json");
        if (File.Exists(testPath))
        {
          Console.WriteLine(" verified.");
          return Path.GetDirectoryName(Path.GetDirectoryName(testPath));
        }

        Console.WriteLine(" metadata/icons.json does not exist!");
        return null;
      }
      catch (Exception e)
      {
        var recentColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine($"ERROR => Failed to check path for FontAwesome Desktop at {directoryPath ?? "<null>"}! A {e.GetType()} occurred with message {e.Message}");
        Console.ForegroundColor = recentColor;
      }

      return null;
    }

    private static string GetLicense(string directoryPath)
    {
      if (string.IsNullOrEmpty(directoryPath))
        return null;

      var licenseFilePath = Path.Combine(directoryPath, "LICENSE.txt");

      try
      {
        Console.Write($"Check license at {licenseFilePath} ...");
        if (!File.Exists(licenseFilePath))
        {
          Console.WriteLine(" file does not exist!");
          return null;
        }

        var licenseContents = File.ReadAllText(licenseFilePath);

        if (licenseContents.StartsWith("Font Awesome Free License", StringComparison.OrdinalIgnoreCase))
        {
          Console.WriteLine(" detected Free license");
          return FreeLicense;
        }
        if (licenseContents.StartsWith("Font Awesome Pro License", StringComparison.OrdinalIgnoreCase))
        {
          Console.WriteLine(" detected Pro license");
          return ProLicense;
        }


        Console.WriteLine(" failed to detect license!");
        return null;
      }
      catch (Exception e)
      {
        var recentColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine($"ERROR => Failed to check license for FontAwesome Desktop at {licenseFilePath ?? "<null>"}! A {e.GetType()} occurred with message {e.Message}");
        Console.ForegroundColor = recentColor;
      }

      return null;
    }


    private static void UpdateNuspecFile(FontAwesomeSourceInformation sourceDir, string inputDirectory)
    {
      var sourceRelativePath = sourceDir.License == ProLicense ? NuspecProSourceRelativeFilePath : NuspecFreeSourceRelativeFilePath;
      if (string.IsNullOrEmpty(sourceRelativePath) || string.IsNullOrEmpty(NuspecTargetRelativeFilePath))
        return;

      var nuspecSourceFile = Path.Combine(inputDirectory, sourceRelativePath);
      var nuspecTargetFile = Path.Combine(inputDirectory, NuspecTargetRelativeFilePath);

      Console.WriteLine();
      Console.WriteLine("Updating nuspec file ...");

      var nuspecContents = File.ReadAllText(nuspecSourceFile, Encoding.UTF8);
      nuspecContents = nuspecContents.Replace("$version$", sourceDir.Version);

      File.WriteAllText(nuspecTargetFile, nuspecContents, Encoding.UTF8);

      Console.WriteLine("Done updating nuspec file.");
    }

    private static void CopyFonts(string inputDirectory, FontAwesomeSourceInformation sourceDir)
    {
      var fontsDir = new DirectoryInfo(Path.Combine(sourceDir.FolderPath, "otfs"));
      var fontsTargetPath = Path.Combine(inputDirectory, FontAwesomeFontsRelativeTargetPath);

      Console.WriteLine();
      Console.WriteLine($"Copy fonts from {fontsDir.FullName} to (temporary) build folder at {fontsTargetPath} ...");

      Directory.CreateDirectory(fontsTargetPath);

      foreach (var fontFilePath in fontsDir.EnumerateFiles("*.otf"))
      {
        Console.WriteLine($"Copy font file {fontFilePath.Name} ...");
        File.Copy(fontFilePath.FullName, Path.Combine(fontsTargetPath, fontFilePath.Name), true);
      }

      Console.WriteLine("Done copying fonts.");
    }

  }
}
