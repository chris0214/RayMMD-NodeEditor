namespace RayMmdNodeEditor.Controls;

public readonly struct HsvColor
{
    public HsvColor(float hue, float saturation, float value)
    {
        Hue = NormalizeHue(hue);
        Saturation = Math.Clamp(saturation, 0f, 1f);
        Value = Math.Clamp(value, 0f, 1f);
    }

    public float Hue { get; }

    public float Saturation { get; }

    public float Value { get; }

    public Color ToColor(int alpha = 255)
    {
        var hue = NormalizeHue(Hue);
        var saturation = Math.Clamp(Saturation, 0f, 1f);
        var value = Math.Clamp(Value, 0f, 1f);

        if (saturation <= 0.0001f)
        {
            var gray = (int)Math.Round(value * 255f);
            return Color.FromArgb(alpha, gray, gray, gray);
        }

        var chroma = value * saturation;
        var hueSection = hue / 60f;
        var x = chroma * (1f - Math.Abs((hueSection % 2f) - 1f));
        var match = value - chroma;

        float red = 0f;
        float green = 0f;
        float blue = 0f;

        if (hueSection < 1f)
        {
            red = chroma;
            green = x;
        }
        else if (hueSection < 2f)
        {
            red = x;
            green = chroma;
        }
        else if (hueSection < 3f)
        {
            green = chroma;
            blue = x;
        }
        else if (hueSection < 4f)
        {
            green = x;
            blue = chroma;
        }
        else if (hueSection < 5f)
        {
            red = x;
            blue = chroma;
        }
        else
        {
            red = chroma;
            blue = x;
        }

        var r = (int)Math.Round((red + match) * 255f);
        var g = (int)Math.Round((green + match) * 255f);
        var b = (int)Math.Round((blue + match) * 255f);
        return Color.FromArgb(alpha, Math.Clamp(r, 0, 255), Math.Clamp(g, 0, 255), Math.Clamp(b, 0, 255));
    }

    public static HsvColor FromColor(Color color)
    {
        var red = color.R / 255f;
        var green = color.G / 255f;
        var blue = color.B / 255f;

        var max = Math.Max(red, Math.Max(green, blue));
        var min = Math.Min(red, Math.Min(green, blue));
        var delta = max - min;

        var hue = 0f;
        if (delta > 0.0001f)
        {
            if (Math.Abs(max - red) < 0.0001f)
            {
                hue = 60f * (((green - blue) / delta) % 6f);
            }
            else if (Math.Abs(max - green) < 0.0001f)
            {
                hue = 60f * (((blue - red) / delta) + 2f);
            }
            else
            {
                hue = 60f * (((red - green) / delta) + 4f);
            }
        }

        if (hue < 0f)
        {
            hue += 360f;
        }

        var saturation = max <= 0.0001f ? 0f : delta / max;
        return new HsvColor(hue, saturation, max);
    }

    private static float NormalizeHue(float hue)
    {
        var normalized = hue % 360f;
        return normalized < 0f ? normalized + 360f : normalized;
    }
}
