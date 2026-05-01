namespace RayMmdNodeEditor.Services;

public static class RayQualityPresets
{
    public static IReadOnlyList<(string Label, string Value)> Choices { get; } =
    [
        ("性能优先", "Performance"),
        ("均衡", "Balanced"),
        ("高质量", "High"),
        ("出图极限", "Extreme"),
    ];

    public static void Apply(string preset, RayDocument document)
    {
        var ray = document.RayConfValues;
        var advanced = document.AdvancedConfValues;

        switch (preset)
        {
            case "Performance":
                Set(ray, "SUN_SHADOW_QUALITY", "1");
                Set(ray, "SSDO_QUALITY", "0");
                Set(ray, "SSR_QUALITY", "0");
                Set(ray, "SSSS_QUALITY", "0");
                Set(ray, "BOKEH_QUALITY", "0");
                Set(ray, "HDR_BLOOM_MODE", "0");
                Set(ray, "HDR_FLARE_MODE", "0");
                Set(ray, "HDR_STAR_MODE", "0");
                Set(ray, "AA_QUALITY", "1");
                Set(ray, "POST_DISPERSION_MODE", "0");
                Set(advanced, "mSSRRangeMax", "500.0");
                Set(advanced, "mSSDOIntensityMax", "4.0");
                break;

            case "High":
                Set(ray, "SUN_SHADOW_QUALITY", "4");
                Set(ray, "SSDO_QUALITY", "4");
                Set(ray, "SSR_QUALITY", "2");
                Set(ray, "SSSS_QUALITY", "1");
                Set(ray, "HDR_BLOOM_MODE", "4");
                Set(ray, "AA_QUALITY", "3");
                Set(ray, "POST_DISPERSION_MODE", "1");
                Set(advanced, "mSSRRangeMax", "1200.0");
                Set(advanced, "mSSDOIntensityMax", "12.0");
                break;

            case "Extreme":
                Set(ray, "SUN_SHADOW_QUALITY", "5");
                Set(ray, "SSDO_QUALITY", "6");
                Set(ray, "SSR_QUALITY", "3");
                Set(ray, "SSSS_QUALITY", "1");
                Set(ray, "HDR_BLOOM_MODE", "4");
                Set(ray, "HDR_FLARE_MODE", "3");
                Set(ray, "HDR_STAR_MODE", "4");
                Set(ray, "AA_QUALITY", "5");
                Set(ray, "POST_DISPERSION_MODE", "2");
                Set(advanced, "mSSRRangeMax", "1800.0");
                Set(advanced, "mSSDOIntensityMax", "16.0");
                Set(advanced, "mFXAAQualitySubpix", "0.75");
                break;

            default:
                Set(ray, "SUN_SHADOW_QUALITY", "3");
                Set(ray, "SSDO_QUALITY", "2");
                Set(ray, "SSR_QUALITY", "0");
                Set(ray, "SSSS_QUALITY", "1");
                Set(ray, "BOKEH_QUALITY", "0");
                Set(ray, "HDR_BLOOM_MODE", "4");
                Set(ray, "HDR_FLARE_MODE", "0");
                Set(ray, "HDR_STAR_MODE", "0");
                Set(ray, "AA_QUALITY", "1");
                Set(ray, "POST_DISPERSION_MODE", "1");
                Set(advanced, "mSSRRangeMax", "1000.0");
                Set(advanced, "mSSDOIntensityMax", "10.0");
                break;
        }
    }

    private static void Set(Dictionary<string, string> values, string key, string value)
    {
        values[key] = value;
    }
}
