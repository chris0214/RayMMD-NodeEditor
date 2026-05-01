using System.Text;
using System.Threading.Tasks;
using RayMmdNodeEditor.Graph;
using RayMmdNodeEditor.Services;

namespace RayMmdNodeEditor;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
        {
            var compiler = new RayMaterialCompiler();
            var result = compiler.Compile(RayGraphFactory.CreateDefault());
            Console.WriteLine(result.Success ? "OK" : "FAILED");
            Console.WriteLine(result.MaterialText.Split(Environment.NewLine).FirstOrDefault() ?? string.Empty);
            foreach (var message in result.Messages)
            {
                Console.WriteLine(message);
            }

            var mathResult = compiler.Compile(CreateCompatibleMathSelfTestGraph());
            Console.WriteLine(mathResult.Success && mathResult.MaterialText.Contains("const float smoothness = 0.75;", StringComparison.Ordinal)
                ? "COMPAT_MATH_OK"
                : "COMPAT_MATH_FAILED");
            foreach (var message in mathResult.Messages)
            {
                Console.WriteLine(message);
            }

            var advancedDocument = CreateAdvancedSelfTestDocument();
            var advanced = new RayAdvancedMaterialCompiler().Compile(advancedDocument);
            var advancedChecks = new (string Name, bool Passed)[]
            {
                ("success", advanced.Success),
                ("common:smoothness", advanced.CommonPatchBlock.Contains("RayNode_GetSmoothness", StringComparison.Ordinal)),
                ("common:texture-sample", advanced.CommonPatchBlock.Contains("tex2D(RayNodeTex0Samp", StringComparison.Ordinal)),
                ("common:uv-rotate", advanced.CommonPatchBlock.Contains("cos(", StringComparison.Ordinal)),
                ("common:time", advanced.CommonPatchBlock.Contains("time", StringComparison.Ordinal)),
                ("common:frac", advanced.CommonPatchBlock.Contains("frac(", StringComparison.Ordinal)),
                ("common:albedo", advanced.CommonPatchBlock.Contains("RayNode_GetAlbedo", StringComparison.Ordinal)),
                ("common:color-adjust", advanced.CommonPatchBlock.Contains("RayNode_ColorAdjust", StringComparison.Ordinal)),
                ("common:layer-blend", advanced.CommonPatchBlock.Contains("RayNode_LayerBlend", StringComparison.Ordinal)),
                ("common:checker", advanced.CommonPatchBlock.Contains("fmod(floor", StringComparison.Ordinal)),
                ("common:custom-a", advanced.CommonPatchBlock.Contains("RayNode_GetCustomA", StringComparison.Ordinal)),
                ("common:custom-b", advanced.CommonPatchBlock.Contains("RayNode_GetCustomB", StringComparison.Ordinal)),
                ("shading:apply", advanced.ShadingPatchBlock.Contains("RayNode_ApplyShading", StringComparison.Ordinal)),
                ("shading:ssao", advanced.ShadingPatchBlock.Contains("RayNode_ShadingSsao", StringComparison.Ordinal)),
                ("shading:scene-color", advanced.ShadingPatchBlock.Contains("ScnSamp", StringComparison.Ordinal)),
                ("shading:ibl", advanced.ShadingPatchBlock.Contains("RayNode_Ibl", StringComparison.Ordinal)),
                ("shading:shadow", advanced.ShadingPatchBlock.Contains("RayNode_ShadowFactor", StringComparison.Ordinal)),
                ("shading:ssr", advanced.ShadingPatchBlock.Contains("RayNode_SsrReflection", StringComparison.Ordinal)),
                ("shading:outline", advanced.ShadingPatchBlock.Contains("RayNode_OutlineColor", StringComparison.Ordinal)),
                ("shading:fog", advanced.ShadingPatchBlock.Contains("RayNode_FogColor", StringComparison.Ordinal)),
                ("shading:ibl-split", advanced.ShadingPatchBlock.Contains("ReflectionMask", StringComparison.Ordinal) || advanced.ShadingPatchBlock.Contains("RayNode_IblSpecular", StringComparison.Ordinal)),
                ("shading:channel-split", advanced.ShadingPatchBlock.Contains("Gbuffer8Map", StringComparison.Ordinal)),
                ("shading:diagnostic", advanced.ShadingPatchBlock.Contains("material.customDataA", StringComparison.Ordinal)),
                ("shading:fog-depth-blend", advanced.ShadingPatchBlock.Contains("lerp(float3(tex2Dlod(ScnSamp", StringComparison.Ordinal) || advanced.ShadingPatchBlock.Contains("RayNode_FogColor(coord)", StringComparison.Ordinal)),
                ("controller", advanced.CommonPatchBlock.Contains("RayNodeCtrl", StringComparison.Ordinal) || advanced.ShadingPatchBlock.Contains("RayNodeCtrl", StringComparison.Ordinal)),
                ("texture-declaration", advanced.CommonPatchBlock.Contains("RayNodeTex", StringComparison.Ordinal)),
                ("common-patch-position", ValidateAdvancedCommonPatchPosition(advancedDocument, advanced)),
            };
            var advancedLooksComplete = advancedChecks.All(check => check.Passed);
            Console.WriteLine(advancedLooksComplete ? "ADVANCED_OK" : "ADVANCED_FAILED");
            foreach (var check in advancedChecks.Where(check => !check.Passed))
            {
                Console.WriteLine($"ADVANCED_MISSING {check.Name}");
            }
            foreach (var message in advanced.Messages)
            {
                Console.WriteLine(message);
            }
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    private static bool ValidateAdvancedCommonPatchPosition(RayDocument document, RayAdvancedMaterialCompileResult advanced)
    {
        var commonPath = Path.Combine(document.RayRootPath, "Materials", "material_common_2.0.fxsub");
        if (!File.Exists(commonPath) || string.IsNullOrWhiteSpace(advanced.CommonPatchBlock))
        {
            return true;
        }

        var patched = RayAdvancedCommonPatcher.Patch(File.ReadAllText(commonPath), advanced);
        var structIndex = patched.IndexOf("struct MaterialParam", StringComparison.Ordinal);
        var patchIndex = patched.IndexOf("// RAY_MMD_NODE_EDITOR_ADVANCED_BEGIN", StringComparison.Ordinal);
        var encodeIndex = patched.IndexOf("GbufferParam EncodeGbuffer", StringComparison.Ordinal);
        return structIndex >= 0 &&
               patchIndex > structIndex &&
               encodeIndex > patchIndex &&
               !patched.Contains("float2 point =", StringComparison.Ordinal);
    }

    private static NodeGraph CreateCompatibleMathSelfTestGraph()
    {
        var graph = new NodeGraph { WorkspaceMode = GraphWorkspaceMode.ObjectMaterial };
        var scalar = graph.AddNode(NodeKind.Scalar, 40, 40);
        scalar.Properties["Value"] = "3.75";
        var frac = graph.AddNode(NodeKind.Frac, 240, 40);
        var output = graph.AddNode(NodeKind.RayMaterialOutput, 460, 40);
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = scalar.Id, SourcePin = "Value", TargetNodeId = frac.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = frac.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "Smoothness" });
        return graph;
    }

    private static RayDocument CreateAdvancedSelfTestDocument()
    {
        var graph = new NodeGraph { WorkspaceMode = GraphWorkspaceMode.ObjectMaterial };
        var time = graph.AddNode(NodeKind.Time, 40, -140);
        var rotate = graph.AddNode(NodeKind.UvRotate, 230, -120);
        rotate.Properties["Angle"] = "0.25";
        var panner = graph.AddNode(NodeKind.Panner, 420, -100);
        panner.Properties["SpeedU"] = "0.02";
        panner.Properties["SpeedV"] = "0.01";
        var a = graph.AddNode(NodeKind.RayTextureSlot, 40, 40);
        a.Properties["File"] = "roughness_a.png";
        a.Properties["MapType"] = "Roughness";
        var b = graph.AddNode(NodeKind.RayTextureSlot, 40, 200);
        b.Properties["File"] = "roughness_b.png";
        b.Properties["MapType"] = "Roughness";
        var frac = graph.AddNode(NodeKind.Frac, 230, 290);
        var rampT = graph.AddNode(NodeKind.Scalar, 250, -300);
        rampT.Properties["Value"] = "0.35";
        var ramp = graph.AddNode(NodeKind.ColorRamp, 430, -300);
        ramp.Properties["StartR"] = "0.2";
        ramp.Properties["StartG"] = "0.4";
        ramp.Properties["StartB"] = "0.9";
        ramp.Properties["EndR"] = "1.0";
        ramp.Properties["EndG"] = "0.8";
        ramp.Properties["EndB"] = "0.3";
        ramp.Properties["Mode"] = "Smooth";
        var adjust = graph.AddNode(NodeKind.ColorAdjust, 650, -300);
        adjust.Properties["Exposure"] = "0.2";
        adjust.Properties["Saturation"] = "1.2";
        var checker = graph.AddNode(NodeKind.CheckerTexture, 650, -80);
        checker.Properties["Scale"] = "8.0";
        var blend = graph.AddNode(NodeKind.LayerBlend, 850, -260);
        blend.Properties["LayerMode"] = "Multiply";
        blend.Properties["MaskInvert"] = "False";
        var wetness = graph.AddNode(NodeKind.Wetness, 1040, -80);
        wetness.Properties["Wetness"] = "0.7";
        var layer = graph.AddNode(NodeKind.RayMaterialLayer, 1050, 120);
        layer.Properties["Mask"] = "0.35";
        var triplanar = graph.AddNode(NodeKind.TriplanarBoxmap, 1040, 300);
        triplanar.Properties["ResourceName"] = "snow_detail.png";
        var boxMask = graph.AddNode(NodeKind.BoxMask, 1240, 180);
        var sphereMask = graph.AddNode(NodeKind.SphereMask, 1240, 320);
        var slopeMask = graph.AddNode(NodeKind.SlopeMask, 1240, 460);
        var multiply = graph.AddNode(NodeKind.Multiply, 280, 120);
        var customA = graph.AddNode(NodeKind.Scalar, 760, 80);
        customA.Properties["Value"] = "0.42";
        var output = graph.AddNode(NodeKind.RayMaterialOutput, 520, 120);
        output.Properties["CustomMode"] = "Cloth";
        var sceneColor = graph.AddNode(NodeKind.RaySceneColor, 1060, -340);
        var ssao = graph.AddNode(NodeKind.RaySsao, 1060, -180);
        var ibl = graph.AddNode(NodeKind.RayIblReflection, 1060, -520);
        var shadow = graph.AddNode(NodeKind.RayShadowData, 1060, -660);
        var debug = graph.AddNode(NodeKind.RayDebugView, 1060, -800);
        debug.Properties["Channel"] = "IBL";
        var ssr = graph.AddNode(NodeKind.RaySsrReflection, 1280, -760);
        var outline = graph.AddNode(NodeKind.RayOutlineChannel, 1280, -620);
        var fog = graph.AddNode(NodeKind.RayFogChannel, 1280, -480);
        var iblSplit = graph.AddNode(NodeKind.RayIblSplit, 1480, -1040);
        var channelSplit = graph.AddNode(NodeKind.RayChannelSplit, 1480, -1180);
        var diagnostic = graph.AddNode(NodeKind.RayMaterialDiagnostic, 1480, -1320);
        diagnostic.Properties["Channel"] = "CustomA";
        var fogDepthBlend = graph.AddNode(NodeKind.RayFogDepthBlend, 1680, -1120);
        var debugController = graph.AddNode(NodeKind.RayDebugController, 1280, -900);
        debugController.Properties["Channel"] = "SSR";
        var controller = graph.AddNode(NodeKind.RayControllerInput, 1240, 20);
        controller.Properties["Item"] = "SunLight+";
        var post = graph.AddNode(NodeKind.RayPostParameter, 1240, -120);
        post.Properties["Parameter"] = "Exposure";
        var shadeAddA = graph.AddNode(NodeKind.Add, 1500, -760);
        var shadeAddB = graph.AddNode(NodeKind.Add, 1680, -680);
        var shadeAddC = graph.AddNode(NodeKind.Add, 1860, -600);
        var shadeAddD = graph.AddNode(NodeKind.Add, 2040, -520);
        var shadingOutput = graph.AddNode(NodeKind.RayShadingOutput, 1280, -260);
        shadingOutput.Properties["BlendMode"] = "Screen";
        shadingOutput.Properties["MaskSource"] = "SSAO";
        shadingOutput.Properties["Intensity"] = "0.45";
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rotate.Id, SourcePin = "UV", TargetNodeId = panner.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = time.Id, SourcePin = "Value", TargetNodeId = panner.Id, TargetPin = "Time" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = panner.Id, SourcePin = "UV", TargetNodeId = a.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = a.Id, SourcePin = "Texture", TargetNodeId = multiply.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = b.Id, SourcePin = "Texture", TargetNodeId = frac.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = frac.Id, SourcePin = "Result", TargetNodeId = multiply.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = multiply.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "Smoothness" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = rampT.Id, SourcePin = "Value", TargetNodeId = ramp.Id, TargetPin = "T" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = ramp.Id, SourcePin = "Color", TargetNodeId = adjust.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = adjust.Id, SourcePin = "Color", TargetNodeId = blend.Id, TargetPin = "Background" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = checker.Id, SourcePin = "Color", TargetNodeId = blend.Id, TargetPin = "Foreground" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = checker.Id, SourcePin = "Factor", TargetNodeId = blend.Id, TargetPin = "Mask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = customA.Id, SourcePin = "Value", TargetNodeId = output.Id, TargetPin = "CustomA" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = checker.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "CustomB" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = blend.Id, SourcePin = "Result", TargetNodeId = wetness.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = controller.Id, SourcePin = "Value", TargetNodeId = wetness.Id, TargetPin = "Wetness" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = boxMask.Id, SourcePin = "Mask", TargetNodeId = wetness.Id, TargetPin = "Porosity" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = wetness.Id, SourcePin = "Color", TargetNodeId = layer.Id, TargetPin = "LayerColor" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = triplanar.Id, SourcePin = "Color", TargetNodeId = layer.Id, TargetPin = "BaseColor" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = slopeMask.Id, SourcePin = "Mask", TargetNodeId = layer.Id, TargetPin = "Mask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = sphereMask.Id, SourcePin = "Mask", TargetNodeId = layer.Id, TargetPin = "BaseSmoothness" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = layer.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Albedo" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = layer.Id, SourcePin = "Smoothness", TargetNodeId = output.Id, TargetPin = "Metalness" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = sceneColor.Id, SourcePin = "Color", TargetNodeId = ssr.Id, TargetPin = "BaseColor" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = post.Id, SourcePin = "Value", TargetNodeId = ssr.Id, TargetPin = "Mask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = ssr.Id, SourcePin = "Color", TargetNodeId = shadeAddA.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = outline.Id, SourcePin = "Color", TargetNodeId = shadeAddA.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shadeAddA.Id, SourcePin = "Result", TargetNodeId = shadeAddB.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = fog.Id, SourcePin = "Color", TargetNodeId = shadeAddB.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shadeAddB.Id, SourcePin = "Result", TargetNodeId = shadeAddC.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = debugController.Id, SourcePin = "Color", TargetNodeId = shadeAddC.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shadeAddC.Id, SourcePin = "Result", TargetNodeId = shadeAddD.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = diagnostic.Id, SourcePin = "Color", TargetNodeId = shadeAddD.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shadeAddD.Id, SourcePin = "Result", TargetNodeId = shadingOutput.Id, TargetPin = "Add" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = ssao.Id, SourcePin = "Visibility", TargetNodeId = shadingOutput.Id, TargetPin = "Multiply" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = channelSplit.Id, SourcePin = "SceneColor", TargetNodeId = fogDepthBlend.Id, TargetPin = "BaseColor" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = channelSplit.Id, SourcePin = "Fog", TargetNodeId = fogDepthBlend.Id, TargetPin = "FogColor" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = channelSplit.Id, SourcePin = "Depth", TargetNodeId = fogDepthBlend.Id, TargetPin = "Depth" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = iblSplit.Id, SourcePin = "ReflectionMask", TargetNodeId = fogDepthBlend.Id, TargetPin = "Mask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = fogDepthBlend.Id, SourcePin = "Color", TargetNodeId = shadingOutput.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shadow.Id, SourcePin = "Color", TargetNodeId = ibl.Id, TargetPin = "BaseColor" });
        return new RayDocument
        {
            MaterialMode = RayMaterialModes.Advanced,
            Graph = graph,
        };
    }
}
