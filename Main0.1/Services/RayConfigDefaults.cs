namespace RayMmdNodeEditor.Services;

public static class RayConfigDefaults
{
    public static Dictionary<string, string> CreateRayConfDefaults()
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["SUN_LIGHT_ENABLE"] = "1",
            ["SUN_SHADOW_QUALITY"] = "3",
            ["IBL_QUALITY"] = "1",
            ["FOG_ENABLE"] = "1",
            ["MULTI_LIGHT_ENABLE"] = "1",
            ["OUTLINE_QUALITY"] = "0",
            ["TOON_ENABLE"] = "0",
            ["SSDO_QUALITY"] = "2",
            ["SSR_QUALITY"] = "0",
            ["SSSS_QUALITY"] = "1",
            ["BOKEH_QUALITY"] = "0",
            ["HDR_EYE_ADAPTATION"] = "0",
            ["HDR_BLOOM_MODE"] = "4",
            ["HDR_FLARE_MODE"] = "0",
            ["HDR_STAR_MODE"] = "0",
            ["HDR_TONEMAP_OPERATOR"] = "4",
            ["AA_QUALITY"] = "1",
            ["POST_DISPERSION_MODE"] = "1",
        };
    }

    public static Dictionary<string, string> CreateAdvancedDefaults()
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mLightIntensityMin"] = "1.0",
            ["mLightIntensityMax"] = "10.0",
            ["mLightDistance"] = "1000",
            ["mLightPlaneNear"] = "0.1",
            ["mLightPlaneFar"] = "400.0",
            ["mTemperature"] = "6600.0",
            ["mEnvLightIntensityMin"] = "1.0",
            ["mEnvLightIntensityMax"] = "6.2831852",
            ["mBloomIntensityMin"] = "1.0",
            ["mBloomIntensityMax"] = "20.0",
            ["mBloomGhostThresholdMax"] = "10.0",
            ["mExposureMin"] = "2.0",
            ["mExposureMax"] = "8.0",
            ["mExposureEyeAdapationMin"] = "0.0",
            ["mExposureEyeAdapationMax"] = "8.0",
            ["mVignetteInner"] = "1.0",
            ["mVignetteOuter"] = "3.5",
            ["mPointLightNear"] = "1.0",
            ["mPointLightFar"] = "400.0",
            ["mPSSMCascadeZMin"] = "5",
            ["mPSSMCascadeZMax"] = "1500",
            ["mPSSMCascadeLambda"] = "0.5",
            ["mPSSMDepthZMin"] = "0",
            ["mPSSMDepthZMax"] = "4000.0",
            ["mSSRRangeMax"] = "1000.0",
            ["mSSRRangeScale"] = "0.75",
            ["mSSRThreshold"] = "1.0",
            ["mSSRFadeStart"] = "0.8",
            ["mSSSSIntensityMin"] = "0.04",
            ["mSSSSIntensityMax"] = "0.02",
            ["mSSDOParams"] = "{2.0, 2.0, 0.03, 0.15}",
            ["mSSDOBiasNear"] = "0.125",
            ["mSSDOBiasFar"] = "0.0005",
            ["mSSDOBiasFalloffNear"] = "20.0",
            ["mSSDOIntensityMin"] = "2.4",
            ["mSSDOIntensityMax"] = "10.0",
            ["mSSDOBlurFalloff"] = "200.0",
            ["mSSDOBlurSharpnessMin"] = "1.0",
            ["mSSDOBlurSharpnessMax"] = "8.0",
            ["mFXAAQualitySubpix"] = "0.5",
            ["mFXAAQualityEdgeThreshold"] = "0.166",
            ["mFXAAQualityEdgeThresholdMin"] = "0.0333",
        };
    }

    public static IReadOnlyList<RayConfigGroup> CreateAdvancedGroups()
    {
        return
        [
            new RayConfigGroup("高级 / 光照", ["mLightIntensityMin", "mLightIntensityMax", "mLightDistance", "mLightPlaneNear", "mLightPlaneFar", "mTemperature", "mPointLightNear", "mPointLightFar"]),
            new RayConfigGroup("高级 / 环境与曝光", ["mEnvLightIntensityMin", "mEnvLightIntensityMax", "mExposureMin", "mExposureMax", "mExposureEyeAdapationMin", "mExposureEyeAdapationMax", "mVignetteInner", "mVignetteOuter"]),
            new RayConfigGroup("高级 / Bloom", ["mBloomIntensityMin", "mBloomIntensityMax", "mBloomGhostThresholdMax"]),
            new RayConfigGroup("高级 / 阴影 PSSM", ["mPSSMCascadeZMin", "mPSSMCascadeZMax", "mPSSMCascadeLambda", "mPSSMDepthZMin", "mPSSMDepthZMax"]),
            new RayConfigGroup("高级 / SSR", ["mSSRRangeMax", "mSSRRangeScale", "mSSRThreshold", "mSSRFadeStart"]),
            new RayConfigGroup("高级 / SSSS", ["mSSSSIntensityMin", "mSSSSIntensityMax"]),
            new RayConfigGroup("高级 / SSDO", ["mSSDOParams", "mSSDOBiasNear", "mSSDOBiasFar", "mSSDOBiasFalloffNear", "mSSDOIntensityMin", "mSSDOIntensityMax", "mSSDOBlurFalloff", "mSSDOBlurSharpnessMin", "mSSDOBlurSharpnessMax"]),
            new RayConfigGroup("高级 / FXAA", ["mFXAAQualitySubpix", "mFXAAQualityEdgeThreshold", "mFXAAQualityEdgeThresholdMin"]),
        ];
    }

    public static Dictionary<string, string> CreateMaterialCommonDefaults()
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["TEXTURE_FILTER"] = "ANISOTROPIC",
            ["TEXTURE_MIP_FILTER"] = "ANISOTROPIC",
            ["TEXTURE_ANISOTROPY_LEVEL"] = "16",
            ["ALPHA_THRESHOLD"] = "0.999",
        };
    }

    public static Dictionary<string, string> CreateLightingPatchDefaults()
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ENABLE_DIRECTIONAL_LIGHTING_PATCH"] = "0",
            ["ENABLE_MULTI_LIGHTING_PATCH"] = "0",
            ["LIGHTING_PATCH_PRESET"] = "Custom",
            ["LIGHTING_PATCH_MODE"] = "HalfLambert",
            ["LIGHTING_PATCH_BLEND"] = "0.35",
            ["LIGHTING_PATCH_BLEND_SOURCE"] = "Constant",
            ["LIGHTING_PATCH_TINT"] = "{1.0, 1.0, 1.0}",
            ["LIGHTING_PATCH_TINT_SOURCE"] = "Constant",
            ["LIGHTING_PATCH_SHADOW_AMOUNT"] = "1.0",
            ["LIGHTING_PATCH_SHADOW_SOURCE"] = "Constant",
            ["LIGHTING_PATCH_SPECULAR_KEEP"] = "1.0",
            ["LIGHTING_PATCH_SPECULAR_SOURCE"] = "Constant",
        };
    }
}

public sealed record RayConfigGroup(string Title, IReadOnlyList<string> Keys);
