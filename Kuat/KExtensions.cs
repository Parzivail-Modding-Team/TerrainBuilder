using System;

namespace Kuat
{
    internal static class KExtensions
    {
        public static string Select(this string s, int selectionStart, int selectionLength,
            SelectionMode mode = SelectionMode.Inside)
        {
            var start = Math.Min(selectionStart, selectionStart + selectionLength);
            var length = Math.Max(selectionStart, selectionStart + selectionLength) - start;
            if (start < 0)
                start = 0;
            if (start > s.Length)
                start = s.Length;
            if (start + length > s.Length)
                length = s.Length - start;
            switch (mode)
            {
                case SelectionMode.Before:
                    return s.Substring(0, start);
                case SelectionMode.After:
                    return s.Substring(start + length);
                case SelectionMode.Inside:
                    return s.Substring(start, length);
                case SelectionMode.Outside:
                    return s.Substring(0, start) + s.Substring(start + length);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
    }

    internal enum SelectionMode
    {
        Before,
        After,
        Inside,
        Outside
    }
}