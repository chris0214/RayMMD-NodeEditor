using System.Globalization;

namespace RayMmdNodeEditor.Services;

public static class FloatParser
{
    // Prefer invariant culture first, then fall back to the current culture for user input.
    public static bool TryParse(string text, out float value)
    {
        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
               float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }

    public static string Format(float value)
    {
        return value.ToString("0.0###", CultureInfo.InvariantCulture);
    }
}
