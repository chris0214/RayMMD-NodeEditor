using System.Drawing;

namespace RayMmdNodeEditor.Graph;

public static partial class NodeRegistry
{
    private static void AddRayDefinitions(Dictionary<NodeKind, NodeDefinition> definitions)
    {
        definitions[NodeKind.RayTextureSlot] = new(
            NodeKind.RayTextureSlot,
            Title(NodeKind.RayTextureSlot),
            "设置 Ray material_2.0.fx 的贴图来源、通道、UV 翻转、贴图类型和循环倍率。",
            NodeCategory.Texture,
            Color.FromArgb(78, 148, 168),
            [new NodePinDefinition("UV", "UV", GraphValueType.Float2)],
            [new NodePinDefinition("Texture", "贴图参数", GraphValueType.Float4)],
            [
                new NodePropertyDefinition("Slot", "目标槽", "Albedo", NodePropertyKind.Text),
                new NodePropertyDefinition("Source", "贴图来源", "File", NodePropertyKind.Text),
                new NodePropertyDefinition("File", "贴图文件", "albedo.png", NodePropertyKind.Text, NodePropertyEditorKind.FilePath),
                new NodePropertyDefinition("UvFlip", "UV 翻转", "None", NodePropertyKind.Text),
                new NodePropertyDefinition("Swizzle", "读取通道", "R", NodePropertyKind.Text),
                new NodePropertyDefinition("MapType", "贴图类型", "Default", NodePropertyKind.Text),
                new NodePropertyDefinition("ApplyScale", "使用常量倍率", "Auto", NodePropertyKind.Text),
                new NodePropertyDefinition("ColorFlip", "颜色反转", "Off", NodePropertyKind.Text),
                new NodePropertyDefinition("LoopX", "横向循环", "1.0"),
                new NodePropertyDefinition("LoopY", "纵向循环", "1.0"),
            ]);

        definitions[NodeKind.RayEmissivePulse] = new(
            NodeKind.RayEmissivePulse,
            Title(NodeKind.RayEmissivePulse),
            "高级模式下生成时间驱动的自发光闪烁，可直接接到 Emissive 或作为强度遮罩使用。",
            NodeCategory.Texture,
            Color.FromArgb(192, 116, 64),
            [
                new NodePinDefinition("Color", "颜色", GraphValueType.Float4),
                new NodePinDefinition("Time", "时间", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Color", "颜色输出", GraphValueType.Float4),
                new NodePinDefinition("Factor", "强度", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Speed", "速度", "1.0"),
                new NodePropertyDefinition("Phase", "相位", "0.0"),
                new NodePropertyDefinition("Min", "最小亮度", "0.2"),
                new NodePropertyDefinition("Max", "最大亮度", "1.0"),
            ]);

        definitions[NodeKind.RayLightDirection] = new(
            NodeKind.RayLightDirection,
            Title(NodeKind.RayLightDirection),
            "从 Ray DirectionalLight.pmx 或自定义控制器读取 Position/Direction，输出视图空间灯光方向。",
            NodeCategory.Geometry,
            Color.FromArgb(84, 124, 184),
            [],
            [new NodePinDefinition("Direction", "方向", GraphValueType.Float4)],
            [
                new NodePropertyDefinition("LightSource", "方向来源", "RayDirectional", NodePropertyKind.Text),
                new NodePropertyDefinition("ControllerName", "控制器名", "DirectionalLight.pmx", NodePropertyKind.Text),
                new NodePropertyDefinition("ManualX", "手动 X", "0.3"),
                new NodePropertyDefinition("ManualY", "手动 Y", "0.6"),
                new NodePropertyDefinition("ManualZ", "手动 Z", "0.7"),
            ]);

        definitions[NodeKind.RayLightingMix] = new(
            NodeKind.RayLightingMix,
            Title(NodeKind.RayLightingMix),
            "把基础颜色、Lambert/Half-Lambert 因子和灯光颜色混合，通常接到 Emissive 或 Albedo。",
            NodeCategory.Shading,
            Color.FromArgb(176, 126, 78),
            [
                new NodePinDefinition("BaseColor", "基础颜色", GraphValueType.Float4),
                new NodePinDefinition("LightFactor", "光照因子", GraphValueType.Float1),
                new NodePinDefinition("LightColor", "灯光颜色", GraphValueType.Float4),
                new NodePinDefinition("Intensity", "强度", GraphValueType.Float1),
            ],
            [new NodePinDefinition("Color", "颜色", GraphValueType.Float4)],
            [
                new NodePropertyDefinition("BlendMode", "混合模式", "MultiplyAdd", NodePropertyKind.Text),
                new NodePropertyDefinition("Intensity", "强度", "1.0"),
            ]);

        definitions[NodeKind.RayNormalStrength] = new(
            NodeKind.RayNormalStrength,
            Title(NodeKind.RayNormalStrength),
            "调整高级 Normal 链路强度；可接 NormalMap 或 DetailNormalBlend 后再输出到 Ray Material Output/Normal。",
            NodeCategory.Texture,
            Color.FromArgb(78, 138, 86),
            [
                new NodePinDefinition("Normal", "法线", GraphValueType.Float4),
                new NodePinDefinition("Strength", "强度", GraphValueType.Float1),
            ],
            [new NodePinDefinition("Normal", "法线", GraphValueType.Float4)],
            [new NodePropertyDefinition("Strength", "强度", "1.0")]);

        definitions[NodeKind.RayCustomData] = new(
            NodeKind.RayCustomData,
            Title(NodeKind.RayCustomData),
            "Ray Custom material data helper. Connect A/B to CustomA/CustomB and set the same Custom mode on Ray Material Output.",
            NodeCategory.Shading,
            Color.FromArgb(184, 112, 160),
            [
                new NodePinDefinition("A", "A", GraphValueType.Float1),
                new NodePinDefinition("B", "B", GraphValueType.Float4),
            ],
            [
                new NodePinDefinition("CustomA", "Custom A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "Custom B", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("Mode", "Mode", "ClearCoat", NodePropertyKind.Text),
                new NodePropertyDefinition("A", "A", "0.5"),
                new NodePropertyDefinition("B_R", "B R", "1.0"),
                new NodePropertyDefinition("B_G", "B G", "1.0"),
                new NodePropertyDefinition("B_B", "B B", "1.0"),
                new NodePropertyDefinition("B_A", "B A", "1.0"),
            ]);

        definitions[NodeKind.RayReflectionBridge] = new(
            NodeKind.RayReflectionBridge,
            Title(NodeKind.RayReflectionBridge),
            "Bridge node for Ray native reflection inputs. Feed Smoothness/Metalness/Specular/Occlusion to Ray Material Output.",
            NodeCategory.Shading,
            Color.FromArgb(120, 148, 196),
            [
                new NodePinDefinition("Roughness", "Roughness", GraphValueType.Float1),
                new NodePinDefinition("Metalness", "Metalness", GraphValueType.Float1),
                new NodePinDefinition("SpecularColor", "Specular Color", GraphValueType.Float4),
                new NodePinDefinition("ReflectionStrength", "Reflection Strength", GraphValueType.Float1),
                new NodePinDefinition("Occlusion", "Occlusion", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Smoothness", "Smoothness", GraphValueType.Float1),
                new NodePinDefinition("Metalness", "Metalness", GraphValueType.Float1),
                new NodePinDefinition("Specular", "Specular", GraphValueType.Float4),
                new NodePinDefinition("Occlusion", "Occlusion", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Roughness", "Roughness", "0.35"),
                new NodePropertyDefinition("Metalness", "Metalness", "0.0"),
                new NodePropertyDefinition("SpecularR", "Specular R", "0.5"),
                new NodePropertyDefinition("SpecularG", "Specular G", "0.5"),
                new NodePropertyDefinition("SpecularB", "Specular B", "0.5"),
                new NodePropertyDefinition("ReflectionStrength", "Reflection Strength", "1.0"),
                new NodePropertyDefinition("Occlusion", "Occlusion", "1.0"),
            ]);

        definitions[NodeKind.RayClearCoatBridge] = new(
            NodeKind.RayClearCoatBridge,
            Title(NodeKind.RayClearCoatBridge),
            "Bridge for Ray Custom mode ClearCoat. Connect CustomA/CustomB and set Custom mode to ClearCoat.",
            NodeCategory.Shading,
            Color.FromArgb(184, 112, 160),
            [
                new NodePinDefinition("ClearCoatRoughness", "ClearCoat Roughness", GraphValueType.Float1),
                new NodePinDefinition("Tint", "Tint", GraphValueType.Float4),
            ],
            [
                new NodePinDefinition("CustomA", "Custom A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "Custom B", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("ClearCoatRoughness", "ClearCoat Roughness", "0.2"),
                new NodePropertyDefinition("TintR", "Tint R", "1.0"),
                new NodePropertyDefinition("TintG", "Tint G", "1.0"),
                new NodePropertyDefinition("TintB", "Tint B", "1.0"),
            ]);

        definitions[NodeKind.RayAnisotropyBridge] = new(
            NodeKind.RayAnisotropyBridge,
            Title(NodeKind.RayAnisotropyBridge),
            "Bridge for Ray Custom mode Anisotropy. CustomA is anisotropy strength; CustomB is anisotropic shift/direction.",
            NodeCategory.Shading,
            Color.FromArgb(184, 112, 160),
            [
                new NodePinDefinition("Anisotropy", "Anisotropy", GraphValueType.Float1),
                new NodePinDefinition("Shift", "Shift", GraphValueType.Float4),
            ],
            [
                new NodePinDefinition("CustomA", "Custom A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "Custom B", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("Anisotropy", "Anisotropy", "0.5"),
                new NodePropertyDefinition("ShiftX", "Shift X", "0.0"),
                new NodePropertyDefinition("ShiftY", "Shift Y", "0.0"),
                new NodePropertyDefinition("ShiftZ", "Shift Z", "0.0"),
            ]);

        definitions[NodeKind.RayClothBridge] = new(
            NodeKind.RayClothBridge,
            Title(NodeKind.RayClothBridge),
            "Bridge for Ray Custom mode Cloth. CustomA blends GGX and sheen; CustomB controls sheen color.",
            NodeCategory.Shading,
            Color.FromArgb(184, 112, 160),
            [
                new NodePinDefinition("Sheen", "Sheen", GraphValueType.Float1),
                new NodePinDefinition("SheenColor", "Sheen Color", GraphValueType.Float4),
            ],
            [
                new NodePinDefinition("CustomA", "Custom A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "Custom B", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("Sheen", "Sheen", "0.5"),
                new NodePropertyDefinition("SheenR", "Sheen R", "1.0"),
                new NodePropertyDefinition("SheenG", "Sheen G", "1.0"),
                new NodePropertyDefinition("SheenB", "Sheen B", "1.0"),
            ]);

        definitions[NodeKind.RaySkinSssBridge] = new(
            NodeKind.RaySkinSssBridge,
            Title(NodeKind.RaySkinSssBridge),
            "Bridge for Ray Custom mode Skin/Subsurface. CustomA controls curvature/intensity; CustomB controls scattering color.",
            NodeCategory.Shading,
            Color.FromArgb(184, 112, 160),
            [
                new NodePinDefinition("Curvature", "Curvature", GraphValueType.Float1),
                new NodePinDefinition("ScatterColor", "Scatter Color", GraphValueType.Float4),
            ],
            [
                new NodePinDefinition("CustomA", "Custom A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "Custom B", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("Curvature", "Curvature", "0.45"),
                new NodePropertyDefinition("ScatterR", "Scatter R", "1.0"),
                new NodePropertyDefinition("ScatterG", "Scatter G", "0.45"),
                new NodePropertyDefinition("ScatterB", "Scatter B", "0.32"),
            ]);

        definitions[NodeKind.RayToonCelBridge] = new(
            NodeKind.RayToonCelBridge,
            Title(NodeKind.RayToonCelBridge),
            "Bridge for Ray Custom mode Cel/ToneBased. CustomA is threshold; CustomB is shadow/tone color.",
            NodeCategory.Shading,
            Color.FromArgb(184, 112, 160),
            [
                new NodePinDefinition("Threshold", "Threshold", GraphValueType.Float1),
                new NodePinDefinition("ShadowColor", "Shadow Color", GraphValueType.Float4),
            ],
            [
                new NodePinDefinition("CustomA", "Custom A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "Custom B", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("Threshold", "Threshold", "0.5"),
                new NodePropertyDefinition("ShadowR", "Shadow R", "0.45"),
                new NodePropertyDefinition("ShadowG", "Shadow G", "0.45"),
                new NodePropertyDefinition("ShadowB", "Shadow B", "0.55"),
            ]);

        definitions[NodeKind.RayBrdfToRayBridge] = new(
            NodeKind.RayBrdfToRayBridge,
            Title(NodeKind.RayBrdfToRayBridge),
            "Dual-mode BRDF bridge. Standalone outputs a preview/specular color; RayNative outputs Ray material fields for the native lighting pipeline.",
            NodeCategory.Shading,
            Color.FromArgb(132, 116, 204),
            [
                new NodePinDefinition("Normal", "Normal", GraphValueType.Float4),
                new NodePinDefinition("ViewDir", "View Dir", GraphValueType.Float4),
                new NodePinDefinition("LightDir", "Light Dir", GraphValueType.Float4),
                new NodePinDefinition("Roughness", "Roughness", GraphValueType.Float1),
                new NodePinDefinition("Metalness", "Metalness", GraphValueType.Float1),
                new NodePinDefinition("F0", "F0", GraphValueType.Float4),
                new NodePinDefinition("SpecularTint", "Specular Tint", GraphValueType.Float4),
                new NodePinDefinition("ReflectionStrength", "Reflection Strength", GraphValueType.Float1),
                new NodePinDefinition("Occlusion", "Occlusion", GraphValueType.Float1),
                new NodePinDefinition("CustomA", "Custom A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "Custom B", GraphValueType.Float4),
                new NodePinDefinition("LightColor", "Light Color", GraphValueType.Float4),
            ],
            [
                new NodePinDefinition("StandaloneColor", "Standalone Color", GraphValueType.Float4),
                new NodePinDefinition("Smoothness", "Smoothness", GraphValueType.Float1),
                new NodePinDefinition("Metalness", "Metalness", GraphValueType.Float1),
                new NodePinDefinition("Specular", "Specular", GraphValueType.Float4),
                new NodePinDefinition("Occlusion", "Occlusion", GraphValueType.Float1),
                new NodePinDefinition("CustomA", "Custom A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "Custom B", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("BridgeMode", "Bridge Mode", "RayNative", NodePropertyKind.Text),
                new NodePropertyDefinition("RayModel", "Ray Model", "StandardGGX", NodePropertyKind.Text),
                new NodePropertyDefinition("Roughness", "Roughness", "0.35"),
                new NodePropertyDefinition("Metalness", "Metalness", "0.0"),
                new NodePropertyDefinition("F0R", "F0 R", "0.5"),
                new NodePropertyDefinition("F0G", "F0 G", "0.5"),
                new NodePropertyDefinition("F0B", "F0 B", "0.5"),
                new NodePropertyDefinition("TintR", "Tint R", "1.0"),
                new NodePropertyDefinition("TintG", "Tint G", "1.0"),
                new NodePropertyDefinition("TintB", "Tint B", "1.0"),
                new NodePropertyDefinition("ReflectionStrength", "Reflection Strength", "1.0"),
                new NodePropertyDefinition("Occlusion", "Occlusion", "1.0"),
                new NodePropertyDefinition("CustomA", "Custom A", "0.5"),
                new NodePropertyDefinition("CustomB_R", "Custom B R", "1.0"),
                new NodePropertyDefinition("CustomB_G", "Custom B G", "1.0"),
                new NodePropertyDefinition("CustomB_B", "Custom B B", "1.0"),
            ]);

        definitions[NodeKind.RaySceneColor] = new(
            NodeKind.RaySceneColor,
            Title(NodeKind.RaySceneColor),
            "Samples Ray's current scene color buffer in the final shading stage. Use through Ray Shading Output, not material_common.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [new NodePinDefinition("Color", "Color", GraphValueType.Float4)],
            []);

        definitions[NodeKind.RaySceneDepth] = new(
            NodeKind.RaySceneDepth,
            Title(NodeKind.RaySceneDepth),
            "Reads Ray linear scene depth from the GBuffer in the final shading stage.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [new NodePinDefinition("Depth", "Depth", GraphValueType.Float1)],
            []);

        definitions[NodeKind.RaySceneNormal] = new(
            NodeKind.RaySceneNormal,
            Title(NodeKind.RaySceneNormal),
            "Reads the decoded Ray view-space normal from the current GBuffer material.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [new NodePinDefinition("Normal", "Normal", GraphValueType.Float4)],
            []);

        definitions[NodeKind.RaySsao] = new(
            NodeKind.RaySsao,
            Title(NodeKind.RaySsao),
            "Reads Ray SSDO/SSAO visibility. Returns 1 when Ray's occlusion pass is disabled.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [
                new NodePinDefinition("Visibility", "Visibility", GraphValueType.Float1),
                new NodePinDefinition("BentNormal", "Bent Normal", GraphValueType.Float4),
            ],
            []);

        definitions[NodeKind.RayMultiLight] = new(
            NodeKind.RayMultiLight,
            Title(NodeKind.RayMultiLight),
            "Reads Ray's accumulated multi-light diffuse/specular buffers in the final shading stage.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [
                new NodePinDefinition("Diffuse", "Diffuse", GraphValueType.Float4),
                new NodePinDefinition("Specular", "Specular", GraphValueType.Float4),
                new NodePinDefinition("Combined", "Combined", GraphValueType.Float4),
            ],
            []);

        definitions[NodeKind.RayAccumulatedLighting] = new(
            NodeKind.RayAccumulatedLighting,
            Title(NodeKind.RayAccumulatedLighting),
            "Uses the diffuse/specular/final lighting values already accumulated by Ray at the patch point.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [
                new NodePinDefinition("Final", "Final", GraphValueType.Float4),
                new NodePinDefinition("Diffuse", "Diffuse", GraphValueType.Float4),
                new NodePinDefinition("Specular", "Specular", GraphValueType.Float4),
            ],
            []);

        definitions[NodeKind.RaySunLightData] = new(
            NodeKind.RaySunLightData,
            Title(NodeKind.RaySunLightData),
            "Outputs Ray sun light direction/color/intensity in the final shading stage.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Direction", "Direction", GraphValueType.Float4),
                new NodePinDefinition("Intensity", "Intensity", GraphValueType.Float1),
            ],
            []);

        definitions[NodeKind.RayDebugView] = new(
            NodeKind.RayDebugView,
            Title(NodeKind.RayDebugView),
            "Visualizes Ray internal channels in the final shading stage: lighting, GBuffer, depth, SSAO, shadow, IBL, and scene color.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [new NodePinDefinition("Color", "Color", GraphValueType.Float4)],
            [new NodePropertyDefinition("Channel", "Channel", "FinalLighting", NodePropertyKind.Text)]);

        definitions[NodeKind.RayIblReflection] = new(
            NodeKind.RayIblReflection,
            Title(NodeKind.RayIblReflection),
            "Samples Ray's IBL diffuse/specular buffer in final shading. Use it for reflection tinting and diagnostics.",
            NodeCategory.Shading,
            Color.FromArgb(120, 148, 196),
            [
                new NodePinDefinition("BaseColor", "Base Color", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Diffuse", "IBL Diffuse", GraphValueType.Float4),
                new NodePinDefinition("Specular", "IBL Specular", GraphValueType.Float4),
                new NodePinDefinition("Combined", "Combined", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("DiffuseStrength", "Diffuse Strength", "1.0"),
                new NodePropertyDefinition("SpecularStrength", "Specular Strength", "1.0"),
                new NodePropertyDefinition("FresnelPower", "Fresnel Power", "5.0"),
            ]);

        definitions[NodeKind.RayShadowData] = new(
            NodeKind.RayShadowData,
            Title(NodeKind.RayShadowData),
            "Reads Ray's current sun shadow map in final shading. Returns 1 when sun shadows are disabled.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [
                new NodePinDefinition("Factor", "Factor", GraphValueType.Float1),
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("ShadowR", "Shadow R", "0.12"),
                new NodePropertyDefinition("ShadowG", "Shadow G", "0.14"),
                new NodePropertyDefinition("ShadowB", "Shadow B", "0.18"),
                new NodePropertyDefinition("LitR", "Lit R", "1.0"),
                new NodePropertyDefinition("LitG", "Lit G", "1.0"),
                new NodePropertyDefinition("LitB", "Lit B", "1.0"),
            ]);

        definitions[NodeKind.RaySkinAdvanced] = new(
            NodeKind.RaySkinAdvanced,
            Title(NodeKind.RaySkinAdvanced),
            "Skin/SSS helper that feeds Ray native Skin/Subsurface custom data and can also output a warm scattering color.",
            NodeCategory.Shading,
            Color.FromArgb(184, 112, 160),
            [
                new NodePinDefinition("BaseColor", "Base Color", GraphValueType.Float4),
                new NodePinDefinition("Thickness", "Thickness", GraphValueType.Float1),
                new NodePinDefinition("Curvature", "Curvature", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("CustomA", "Custom A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "Custom B", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("ScatterR", "Scatter R", "1.0"),
                new NodePropertyDefinition("ScatterG", "Scatter G", "0.45"),
                new NodePropertyDefinition("ScatterB", "Scatter B", "0.32"),
                new NodePropertyDefinition("Strength", "Strength", "0.65"),
                new NodePropertyDefinition("Thickness", "Thickness", "0.5"),
                new NodePropertyDefinition("Curvature", "Curvature", "0.45"),
            ]);

        definitions[NodeKind.RaySnowLayer] = new(
            NodeKind.RaySnowLayer,
            Title(NodeKind.RaySnowLayer),
            "Adds snow accumulation based on slope and mask. Use outputs for albedo/smoothness/occlusion.",
            NodeCategory.Shading,
            Color.FromArgb(176, 126, 78),
            [
                new NodePinDefinition("BaseColor", "Base Color", GraphValueType.Float4),
                new NodePinDefinition("Normal", "Normal", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
                new NodePinDefinition("Smoothness", "Smoothness", GraphValueType.Float1),
                new NodePinDefinition("Occlusion", "Occlusion", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Amount", "Amount", "0.65"),
                new NodePropertyDefinition("SlopeSharpness", "Slope Sharpness", "3.0"),
                new NodePropertyDefinition("SnowR", "Snow R", "0.92"),
                new NodePropertyDefinition("SnowG", "Snow G", "0.96"),
                new NodePropertyDefinition("SnowB", "Snow B", "1.0"),
                new NodePropertyDefinition("Smoothness", "Smoothness", "0.35"),
            ]);

        definitions[NodeKind.RayDustLayer] = new(
            NodeKind.RayDustLayer,
            Title(NodeKind.RayDustLayer),
            "Adds dust/dirt using mask, slope, and occlusion-friendly darkening.",
            NodeCategory.Shading,
            Color.FromArgb(176, 126, 78),
            [
                new NodePinDefinition("BaseColor", "Base Color", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
                new NodePinDefinition("Occlusion", "Occlusion", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
                new NodePinDefinition("Smoothness", "Smoothness", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Amount", "Amount", "0.5"),
                new NodePropertyDefinition("DustR", "Dust R", "0.45"),
                new NodePropertyDefinition("DustG", "Dust G", "0.38"),
                new NodePropertyDefinition("DustB", "Dust B", "0.28"),
                new NodePropertyDefinition("Smoothness", "Smoothness", "0.18"),
            ]);

        definitions[NodeKind.RayEdgeWear] = new(
            NodeKind.RayEdgeWear,
            Title(NodeKind.RayEdgeWear),
            "Creates an edge-wear mask from normal, view direction, noise, and optional mask.",
            NodeCategory.Shading,
            Color.FromArgb(176, 126, 78),
            [
                new NodePinDefinition("BaseColor", "Base Color", GraphValueType.Float4),
                new NodePinDefinition("Normal", "Normal", GraphValueType.Float4),
                new NodePinDefinition("Noise", "Noise", GraphValueType.Float1),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
                new NodePinDefinition("Metalness", "Metalness", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Amount", "Amount", "0.35"),
                new NodePropertyDefinition("Power", "Power", "4.0"),
                new NodePropertyDefinition("WearR", "Wear R", "0.8"),
                new NodePropertyDefinition("WearG", "Wear G", "0.74"),
                new NodePropertyDefinition("WearB", "Wear B", "0.62"),
                new NodePropertyDefinition("Metalness", "Metalness", "0.0"),
            ]);

        definitions[NodeKind.RayMaterialLayer] = new(
            NodeKind.RayMaterialLayer,
            Title(NodeKind.RayMaterialLayer),
            "Layer material fields with a shared mask: albedo, smoothness, metalness, specular, and occlusion.",
            NodeCategory.Shading,
            Color.FromArgb(176, 126, 78),
            [
                new NodePinDefinition("BaseColor", "Base Color", GraphValueType.Float4),
                new NodePinDefinition("LayerColor", "Layer Color", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
                new NodePinDefinition("BaseSmoothness", "Base Smoothness", GraphValueType.Float1),
                new NodePinDefinition("LayerSmoothness", "Layer Smoothness", GraphValueType.Float1),
                new NodePinDefinition("BaseMetalness", "Base Metalness", GraphValueType.Float1),
                new NodePinDefinition("LayerMetalness", "Layer Metalness", GraphValueType.Float1),
                new NodePinDefinition("BaseSpecular", "Base Specular", GraphValueType.Float4),
                new NodePinDefinition("LayerSpecular", "Layer Specular", GraphValueType.Float4),
                new NodePinDefinition("BaseOcclusion", "Base Occlusion", GraphValueType.Float1),
                new NodePinDefinition("LayerOcclusion", "Layer Occlusion", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Smoothness", "Smoothness", GraphValueType.Float1),
                new NodePinDefinition("Metalness", "Metalness", GraphValueType.Float1),
                new NodePinDefinition("Specular", "Specular", GraphValueType.Float4),
                new NodePinDefinition("Occlusion", "Occlusion", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Mask", "Mask", "0.5"),
                new NodePropertyDefinition("BlendMode", "Blend Mode", "Lerp", NodePropertyKind.Text),
            ]);

        definitions[NodeKind.RaySsrReflection] = new(
            NodeKind.RaySsrReflection,
            Title(NodeKind.RaySsrReflection),
            "Samples Ray SSR result in final shading. Requires SSR_QUALITY > 0.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [
                new NodePinDefinition("BaseColor", "Base Color", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Reflection", "Reflection", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Intensity", "Intensity", "1.0"),
                new NodePropertyDefinition("FresnelPower", "Fresnel Power", "5.0"),
            ]);

        definitions[NodeKind.RayOutlineChannel] = new(
            NodeKind.RayOutlineChannel,
            Title(NodeKind.RayOutlineChannel),
            "Samples Ray OutlineMap in final shading. Requires OUTLINE_QUALITY > 0.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("TintR", "Tint R", "1.0"),
                new NodePropertyDefinition("TintG", "Tint G", "1.0"),
                new NodePropertyDefinition("TintB", "Tint B", "1.0"),
                new NodePropertyDefinition("Intensity", "Intensity", "1.0"),
            ]);

        definitions[NodeKind.RayFogChannel] = new(
            NodeKind.RayFogChannel,
            Title(NodeKind.RayFogChannel),
            "Samples Ray FogMap in final shading. Requires FOG_ENABLE.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Amount", "Amount", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Intensity", "Intensity", "1.0"),
                new NodePropertyDefinition("UseAlpha", "Use Alpha", "True", NodePropertyKind.Text),
            ]);

        definitions[NodeKind.RayIblSplit] = new(
            NodeKind.RayIblSplit,
            Title(NodeKind.RayIblSplit),
            "Separates Ray EnvLightMap into diffuse/specular/combined channels for final shading masks, reflection tinting, and diagnostics.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Diffuse", "Diffuse", GraphValueType.Float4),
                new NodePinDefinition("Specular", "Specular", GraphValueType.Float4),
                new NodePinDefinition("Combined", "Combined", GraphValueType.Float4),
                new NodePinDefinition("DiffuseLuma", "Diffuse Luma", GraphValueType.Float1),
                new NodePinDefinition("SpecularLuma", "Specular Luma", GraphValueType.Float1),
                new NodePinDefinition("ReflectionMask", "Reflection Mask", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("DiffuseStrength", "Diffuse Strength", "1.0"),
                new NodePropertyDefinition("SpecularStrength", "Specular Strength", "1.0"),
                new NodePropertyDefinition("Mask", "Mask", "1.0"),
                new NodePropertyDefinition("FresnelPower", "Fresnel Power", "5.0"),
            ]);

        definitions[NodeKind.RayChannelSplit] = new(
            NodeKind.RayChannelSplit,
            Title(NodeKind.RayChannelSplit),
            "One node access to Ray final-shading channels: scene color/depth/normal, SSAO, shadow, SSR, fog, outline, and IBL.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [
                new NodePinDefinition("SceneColor", "Scene Color", GraphValueType.Float4),
                new NodePinDefinition("Depth", "Depth", GraphValueType.Float1),
                new NodePinDefinition("Depth01", "Depth 0-1", GraphValueType.Float1),
                new NodePinDefinition("Normal", "Normal", GraphValueType.Float4),
                new NodePinDefinition("SSAO", "SSAO/SSDO", GraphValueType.Float1),
                new NodePinDefinition("Shadow", "Shadow", GraphValueType.Float1),
                new NodePinDefinition("SSR", "SSR", GraphValueType.Float4),
                new NodePinDefinition("Fog", "Fog", GraphValueType.Float4),
                new NodePinDefinition("Outline", "Outline", GraphValueType.Float4),
                new NodePinDefinition("IblDiffuse", "IBL Diffuse", GraphValueType.Float4),
                new NodePinDefinition("IblSpecular", "IBL Specular", GraphValueType.Float4),
                new NodePinDefinition("IblCombined", "IBL Combined", GraphValueType.Float4),
            ],
            [
                new NodePropertyDefinition("DepthMax", "Depth Max", "100.0"),
            ]);

        definitions[NodeKind.RayMaterialDiagnostic] = new(
            NodeKind.RayMaterialDiagnostic,
            Title(NodeKind.RayMaterialDiagnostic),
            "Visualizes Ray material fields and Ray channel data as color/value outputs in the final shading patch.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Value", "Value", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Channel", "Channel", "Smoothness", NodePropertyKind.Text),
                new NodePropertyDefinition("Scale", "Scale", "1.0"),
                new NodePropertyDefinition("Bias", "Bias", "0.0"),
                new NodePropertyDefinition("Gamma", "Gamma", "1.0"),
            ]);

        definitions[NodeKind.RayFogDepthBlend] = new(
            NodeKind.RayFogDepthBlend,
            Title(NodeKind.RayFogDepthBlend),
            "Blends a base color toward Ray fog using scene depth, FogMap amount, and optional mask. Use through Ray Shading Output.",
            NodeCategory.Shading,
            Color.FromArgb(120, 148, 196),
            [
                new NodePinDefinition("BaseColor", "Base Color", GraphValueType.Float4),
                new NodePinDefinition("FogColor", "Fog Color", GraphValueType.Float4),
                new NodePinDefinition("Mask", "Mask", GraphValueType.Float1),
                new NodePinDefinition("Depth", "Depth", GraphValueType.Float1),
            ],
            [
                new NodePinDefinition("Color", "Color", GraphValueType.Float4),
                new NodePinDefinition("Amount", "Amount", GraphValueType.Float1),
            ],
            [
                new NodePropertyDefinition("Near", "Near", "20.0"),
                new NodePropertyDefinition("Far", "Far", "250.0"),
                new NodePropertyDefinition("Intensity", "Intensity", "1.0"),
                new NodePropertyDefinition("UseRayFogAmount", "Use Ray Fog Amount", "True", NodePropertyKind.Text),
                new NodePropertyDefinition("FogR", "Fog R", "0.65"),
                new NodePropertyDefinition("FogG", "Fog G", "0.72"),
                new NodePropertyDefinition("FogB", "Fog B", "0.82"),
            ]);

        definitions[NodeKind.RayControllerInput] = new(
            NodeKind.RayControllerInput,
            Title(NodeKind.RayControllerInput),
            "Reads a float controller item from ray_controller.pmx or a custom controller.",
            NodeCategory.Input,
            Color.FromArgb(84, 124, 184),
            [],
            [new NodePinDefinition("Value", "Value", GraphValueType.Float1)],
            [
                new NodePropertyDefinition("ControllerName", "Controller Name", "ray_controller.pmx", NodePropertyKind.Text),
                new NodePropertyDefinition("Item", "Item", "SunLight+", NodePropertyKind.Text),
                new NodePropertyDefinition("Fallback", "Fallback", "0.0"),
            ]);

        definitions[NodeKind.RayPostParameter] = new(
            NodeKind.RayPostParameter,
            Title(NodeKind.RayPostParameter),
            "Outputs common Ray post-process controller values such as Exposure, Bloom, Vignette, Dispersion, and DOF controls.",
            NodeCategory.Input,
            Color.FromArgb(84, 124, 184),
            [],
            [new NodePinDefinition("Value", "Value", GraphValueType.Float1)],
            [new NodePropertyDefinition("Parameter", "Parameter", "Exposure", NodePropertyKind.Text)]);

        definitions[NodeKind.RayDebugController] = new(
            NodeKind.RayDebugController,
            Title(NodeKind.RayDebugController),
            "Node-level version of Ray DebugController channels: CustomData, SSAO, SSDO, SSR, PSSM, Outline.",
            NodeCategory.Input,
            Color.FromArgb(92, 122, 164),
            [],
            [new NodePinDefinition("Color", "Color", GraphValueType.Float4)],
            [new NodePropertyDefinition("Channel", "Channel", "SSAO", NodePropertyKind.Text)]);

        definitions[NodeKind.RayShadingOutput] = new(
            NodeKind.RayShadingOutput,
            Title(NodeKind.RayShadingOutput),
            "Final Ray shading patch output. Color overrides final lighting, Add adds to it, and Multiply tints it. This stays in the same graph; no post workspace is needed.",
            NodeCategory.Output,
            Color.FromArgb(214, 132, 72),
            [
                new NodePinDefinition("Color", "Override Color", GraphValueType.Float4),
                new NodePinDefinition("Add", "Add Color", GraphValueType.Float4),
                new NodePinDefinition("Multiply", "Multiply Color", GraphValueType.Float4),
            ],
            [],
            [
                new NodePropertyDefinition("BlendMode", "Blend Mode", "Override", NodePropertyKind.Text),
                new NodePropertyDefinition("Intensity", "Intensity", "1.0"),
                new NodePropertyDefinition("MaskSource", "Mask Source", "Constant", NodePropertyKind.Text),
                new NodePropertyDefinition("Mask", "Mask", "1.0"),
            ]);

        definitions[NodeKind.RayMaterialOutput] = new(
            NodeKind.RayMaterialOutput,
            Title(NodeKind.RayMaterialOutput),
            "导出原版 Ray-MMD 兼容的 material_2.0.fx 参数。",
            NodeCategory.Output,
            Color.FromArgb(214, 162, 72),
            [
                new NodePinDefinition("Albedo", "主颜色", GraphValueType.Float4),
                new NodePinDefinition("SubAlbedo", "副颜色", GraphValueType.Float4),
                new NodePinDefinition("Alpha", "透明度", GraphValueType.Float1),
                new NodePinDefinition("Normal", "法线", GraphValueType.Float4),
                new NodePinDefinition("SubNormal", "副法线", GraphValueType.Float4),
                new NodePinDefinition("Smoothness", "光滑度", GraphValueType.Float1),
                new NodePinDefinition("Metalness", "金属度", GraphValueType.Float1),
                new NodePinDefinition("Specular", "高光", GraphValueType.Float4),
                new NodePinDefinition("Occlusion", "遮蔽", GraphValueType.Float1),
                new NodePinDefinition("Parallax", "视差", GraphValueType.Float1),
                new NodePinDefinition("Emissive", "自发光", GraphValueType.Float4),
                new NodePinDefinition("CustomA", "自定义 A", GraphValueType.Float1),
                new NodePinDefinition("CustomB", "自定义 B", GraphValueType.Float4),
            ],
            [],
            [
                new NodePropertyDefinition("AlbedoApplyDiffuse", "主颜色乘 PMX Diffuse", "On", NodePropertyKind.Text),
                new NodePropertyDefinition("AlbedoApplyMorphColor", "主颜色受材质 Morph 影响", "Off", NodePropertyKind.Text),
                new NodePropertyDefinition("SubAlbedoMode", "副颜色混合模式", "None", NodePropertyKind.Text),
                new NodePropertyDefinition("AlphaMapSwizzle", "透明度通道", "A", NodePropertyKind.Text),
                new NodePropertyDefinition("EmissiveEnabled", "启用自发光", "Off", NodePropertyKind.Text),
                new NodePropertyDefinition("EmissiveApplyMorphColor", "自发光受 Morph 颜色影响", "Off", NodePropertyKind.Text),
                new NodePropertyDefinition("EmissiveApplyMorphIntensity", "自发光受 Morph 强度影响", "Off", NodePropertyKind.Text),
                new NodePropertyDefinition("EmissiveApplyBlink", "自发光眨眼控制", "Off", NodePropertyKind.Text),
                new NodePropertyDefinition("EmissiveBlinkR", "自发光 Blink R", "1.0"),
                new NodePropertyDefinition("EmissiveBlinkG", "自发光 Blink G", "1.0"),
                new NodePropertyDefinition("EmissiveBlinkB", "自发光 Blink B", "1.0"),
                new NodePropertyDefinition("EmissiveIntensity", "自发光强度", "1.0"),
                new NodePropertyDefinition("CustomMode", "Custom 材质模式", "None", NodePropertyKind.Text),
            ]);
    }
}
