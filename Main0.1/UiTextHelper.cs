using System.Text;
using System.Text.RegularExpressions;
using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor;

internal static class UiTextHelper
{
    private static readonly Encoding GbkEncoding = CreateGbkEncoding();

    public static string NodeTitle(NodeDefinition definition)
    {
        var fallback = FirstNonEmpty(
            RecoverText(definition.Title),
            FriendlyNodeKind(definition.Kind),
            SplitPascal(definition.Kind.ToString()));

        return LocalizationService.Get($"node.{definition.Kind}.title", fallback);
    }

    public static string NodeDescription(NodeDefinition definition)
    {
        var recovered = RecoverText(definition.Description);
        var defaultDescription = LocalizationService.Format(
            "node.default.description",
            "Used for {0} node.",
            NodeTitle(definition));

        var fallback = IsUsableText(recovered) ? recovered : defaultDescription;
        return LocalizationService.Get($"node.{definition.Kind}.description", fallback);
    }

    public static string PinLabel(NodePinDefinition pin)
    {
        if (LocalizationService.TryGet($"pin.{pin.Name}", out var localized))
        {
            return localized;
        }

        return string.IsNullOrWhiteSpace(pin.DisplayName) ? pin.Name : pin.DisplayName;
    }

    public static string PropertyLabel(NodePropertyDefinition property)
    {
        if (LocalizationService.TryGet($"property.{property.Name}", out var localized))
        {
            return localized;
        }

        return string.IsNullOrWhiteSpace(property.DisplayName) ? property.Name : property.DisplayName;
    }

    public static string CategoryLabel(NodeCategory category)
    {
        return LocalizationService.Get($"category.{category}", category.ToString());
    }

    public static string FriendlyNodeKind(NodeKind kind)
    {
        return kind switch
        {
            NodeKind.Scalar => "Float1",
            NodeKind.Float2Value => "Float2",
            NodeKind.Float3Value => "Float3",
            NodeKind.Float4Value => "Float4",
            NodeKind.Color => "Color",
            NodeKind.TexCoord => "UV Channel",
            NodeKind.SubTextureUv => "Sub UV",
            NodeKind.VertexChannel => "Vertex Channel",
            NodeKind.MaterialTexture => "Material Texture",
            NodeKind.TriplanarBoxmap => "Triplanar / Boxmap",
            NodeKind.Wetness => "Wetness",
            NodeKind.MaterialOutput => "Material Output",
            NodeKind.FresnelSchlick => "Fresnel Schlick",
            NodeKind.GGXSpecular => "GGX Specular",
            NodeKind.BurleyDiffuse => "Burley Diffuse",
            NodeKind.BRDFLighting => "BRDF Lighting",
            NodeKind.SmithJointGGX => "Smith Joint GGX",
            NodeKind.CookTorranceSpecular => "Cook Torrance Specular",
            NodeKind.PreIntegratedFGD => "PreIntegrated FGD",
            NodeKind.GenshinRamp => "Genshin Ramp",
            NodeKind.SnowBreakRamp => "SnowBreak Ramp",
            NodeKind.GenericRampSample => "Generic Ramp",
            NodeKind.SkinPreintegratedLut => "Skin Preintegrated LUT",
            NodeKind.AnisotropicGGXSpecular => "Anisotropic GGX Specular",
            NodeKind.KelemenSzirmayKalosSpecular => "Kelemen Szirmay-Kalos Specular",
            NodeKind.DisneyPrincipledLite => "Disney Principled Lite",
            NodeKind.DisneyPrincipled => "Disney Principled",
            NodeKind.PrincipledPBR => "Principled PBR",
            NodeKind.MixMaterial => "Mix Material",
            NodeKind.NoiseTexture => "Noise Texture",
            NodeKind.VoronoiTexture => "Voronoi",
            NodeKind.FbmNoise => "FBM",
            NodeKind.DomainWarp => "Domain Warp",
            NodeKind.MusgraveTexture => "Musgrave",
            NodeKind.CellEdgeTexture => "Cell Edge",
            NodeKind.CurlNoise => "Curl Noise",
            NodeKind.AnisotropicNoise => "Anisotropic Noise",
            NodeKind.GradientTexture => "Gradient Texture",
            NodeKind.CheckerTexture => "Checker Texture",
            NodeKind.BrickTexture => "Brick Texture",
            NodeKind.WaveTexture => "Wave Texture",
            NodeKind.MaterialSphereMap => "Sphere Map",
            NodeKind.MaterialToonTexture => "Toon Texture",
            NodeKind.EmissiveTexture => "Emissive Texture",
            NodeKind.ParallaxUv => "Parallax UV",
            NodeKind.NormalMap => "Normal Map",
            NodeKind.SurfaceTangent => "Surface Tangent",
            NodeKind.SurfaceBitangent => "Surface Bitangent",
            NodeKind.HairTangent => "Hair Tangent",
            NodeKind.UseSubTextureFlag => "Use SubTexture",
            NodeKind.EdgeColorNode => "Edge Color",
            NodeKind.GroundShadowColorNode => "Ground Shadow Color",
            NodeKind.TextureAddValueNode => "Texture Add Value",
            NodeKind.TextureMulValueNode => "Texture Mul Value",
            NodeKind.SphereAddValueNode => "Sphere Add Value",
            NodeKind.SphereMulValueNode => "Sphere Mul Value",
            NodeKind.ApplyTextureValue => "Apply Texture Value",
            NodeKind.ApplySphereAdd => "Apply Sphere Add",
            NodeKind.ApplySphereMul => "Apply Sphere Mul",
            NodeKind.ApplySphereReplace => "Apply Sphere Replace",
            NodeKind.ControllerLightDirection => "Controller Light Dir",
            NodeKind.ControlObjectPosition => "Control Object Pos",
            NodeKind.ControlObjectValue => "Control Object Value",
            NodeKind.ControlObjectRotation => "Control Object Rotation",
            NodeKind.ControlObjectVector => "Control Object Vector",
            NodeKind.UvRotate => "UV Rotate",
            NodeKind.TransformPosition => "Transform Position",
            NodeKind.TransformVector => "Transform Vector",
            NodeKind.ControlObjectTransformDirection => "Control Object Axis",
            NodeKind.ControlObjectBoneDirection => "Control Bone Direction",
            NodeKind.ControlObjectCenter => "Control Center",
            NodeKind.ControlObjectAngleDirection => "Control Angle Dir",
            NodeKind.EulerToDirection => "Euler To Direction",
            NodeKind.ScreenUv => "Screen UV",
            NodeKind.SceneViewRay => "Scene View Ray",
            NodeKind.MatCapUv => "MatCap UV",
            NodeKind.ViewportPixelSize => "Viewport Size",
            NodeKind.DirectionToLatLongUv => "Direction To LatLong UV",
            NodeKind.SharedTextureSample => "Shared Texture Sample",
            NodeKind.DiffuseEnvSample => "Diffuse Env Sample",
            NodeKind.SpecularEnvSample => "Specular Env Sample",
            NodeKind.DiffuseEnvBake => "Diffuse Env Bake",
            NodeKind.SpecularEnvBake => "Specular Env Bake",
            NodeKind.EnvironmentLightingPBR => "Environment Lighting PBR",
            NodeKind.PackPipelineSurface => "Pack Pipeline Surface",
            NodeKind.PipelineEnvironmentLighting => "Pipeline Environment Lighting",
            NodeKind.ElapsedTime => "Elapsed Time",
            NodeKind.ComponentMask => "Mask",
            NodeKind.SelfShadowFactor => "Self Shadow",
            NodeKind.DiffuseShadow => "Diffuse Shadow",
            NodeKind.ShadowThreshold => "Shadow Threshold",
            NodeKind.ShadowSoftness => "Shadow Softness",
            NodeKind.ShadowColorMix => "Shadow Color Mix",
            NodeKind.RgbCurve => "RGB Curve",
            NodeKind.ColorRamp => "Color Ramp",
            NodeKind.Sigmoid => "Sigmoid",
            NodeKind.Softmax => "Softmax",
            NodeKind.HalfLambert => "Half Lambert",
            NodeKind.BlinnPhong => "Blinn Phong",
            NodeKind.WrapLighting => "Wrap Lighting",
            NodeKind.GouraudLighting => "Gouraud Lighting",
            NodeKind.OrenNayarLighting => "Oren-Nayar Lighting",
            NodeKind.OrenNayarDiffuseReflection => "Oren-Nayar Diffuse",
            NodeKind.OrenNayarBlinn => "Oren-Nayar Blinn",
            NodeKind.MinnaertLighting => "Minnaert Lighting",
            NodeKind.KajiyaKay => "Kajiya-Kay",
            NodeKind.Bssrdf => "BSSRDF",
            NodeKind.SubsurfaceScattering => "Subsurface Scattering",
            NodeKind.RimLight => "Rim Light",
            NodeKind.MatCapAtlasSample => "MatCap Atlas Sample",
            NodeKind.RayCustomData => "Ray Custom Data",
            NodeKind.RayReflectionBridge => "Ray Reflection Bridge",
            NodeKind.RayClearCoatBridge => "Ray ClearCoat Bridge",
            NodeKind.RayAnisotropyBridge => "Ray Anisotropy Bridge",
            NodeKind.RayClothBridge => "Ray Cloth Bridge",
            NodeKind.RaySkinSssBridge => "Ray Skin/SSS Bridge",
            NodeKind.RayToonCelBridge => "Ray Toon/Cel Bridge",
            NodeKind.RayBrdfToRayBridge => "BRDF to Ray Bridge",
            _ => SplitPascal(kind.ToString()),
        };
    }

    private static string RecoverText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (IsUsableText(text))
        {
            return text.Trim();
        }

        try
        {
            var recovered = Encoding.UTF8.GetString(GbkEncoding.GetBytes(text)).Trim();
            return IsUsableText(recovered) ? recovered : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool IsUsableText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return !LooksCorrupted(text);
    }

    private static bool LooksCorrupted(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        return text.Contains('?') || text.Contains('\uFFFD');
    }

    private static Encoding CreateGbkEncoding()
    {
        try
        {
            return Encoding.GetEncoding(936);
        }
        catch
        {
            return Encoding.UTF8;
        }
    }

    private static string FirstNonEmpty(params string[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private static string SplitPascal(string value)
    {
        return Regex.Replace(value, "([a-z0-9])([A-Z])", "$1 $2");
    }
}
