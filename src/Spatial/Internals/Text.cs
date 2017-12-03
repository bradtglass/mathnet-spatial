﻿namespace MathNet.Spatial.Internals
{
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal static class Text
    {
        private const string DoublePattern = @"[+-]?\d*(?:[.,]\d+)?(?:[eE][+-]?\d+)?";
        private const string SeparatorPattern = @" *[,; ] *";
        private static readonly string Vector2DPattern = $@"^ *\(?(?<x>{DoublePattern}){SeparatorPattern}(?<y>{DoublePattern})?\)? *$";

        internal static bool TryParse2D(string text, out double x, out double y)
        {
            x = 0;
            y = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var match = Regex.Match(text, Vector2DPattern, RegexOptions.ExplicitCapture);
            if (!match.Success ||
                match.Groups.Count != 3 ||
                match.Groups[0].Captures.Count != 1 ||
                match.Groups[1].Captures.Count != 1 ||
                match.Groups[2].Captures.Count != 1)
            {
                return false;
            }

            return TryParseDouble(match.Groups["x"].Value, out x) &&
                   TryParseDouble(match.Groups["y"].Value, out y);
        }

        private static bool TryParseDouble(string s, out double result)
        {
            return double.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }
    }
}
