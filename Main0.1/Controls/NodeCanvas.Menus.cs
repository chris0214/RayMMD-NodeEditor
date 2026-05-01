using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Controls;

public sealed partial class NodeCanvas
{
    private void BuildContextMenusCore()
    {
        _canvasMenu.Items.Clear();
        if (string.Equals(_filterDescription, "Ray-MMD", StringComparison.Ordinal))
        {
            AddRayMaterialMenus(_canvasMenu);
            AddLayoutMenu(_canvasMenu);
        }
        else
        if (_graph.WorkspaceMode == GraphWorkspaceMode.ScenePostProcess)
        {
            AddScenePostProcessMenus(_canvasMenu);
        }
        else if (_graph.WorkspaceMode == GraphWorkspaceMode.BufferPass)
        {
            AddBufferPassMenus(_canvasMenu);
        }
        else
        {
            AddCategoryMenu(_canvasMenu, LocalizationService.Get("category.Input", "Input"), NodeCategory.Input);
            AddGeometryMenu(_canvasMenu);
            AddTextureMenu(_canvasMenu);
            AddShadingMenu(_canvasMenu);
            AddExperimentalMenu(_canvasMenu);
            AddMathMenu(_canvasMenu);
            AddLayoutMenu(_canvasMenu);
            AddObjectLayerMenu(_canvasMenu);
            AddCategoryMenu(_canvasMenu, LocalizationService.Get("category.Output", "Output"), NodeCategory.Output);
        }

        _nodeMenu.Items.Clear();
        _nodeMenu.Items.Add(LocalizationService.Get("menu.node.delete", "Delete Node"), null, (_, _) => DeleteSelectedNode());
        _nodeMenu.Items.Add(LocalizationService.Get("menu.node.create_frame", "Create Frame From Selection"), null, (_, _) => CreateFrameFromSelection());
        _nodeMenu.Items.Add(LocalizationService.Get("menu.node.edit_group", "Edit Group"), null, (_, _) =>
        {
            if (_selectedNode?.Kind == NodeKind.Group)
            {
                OpenGroupNodeAction?.Invoke(_selectedNode.Id);
            }
        });
        _nodeMenu.Items.Add(LocalizationService.Get("menu.node.rename_group", "Rename Group"), null, (_, _) =>
        {
            if (_selectedNode?.Kind == NodeKind.Group)
            {
                RenameGroupAction?.Invoke();
            }
        });
        _nodeMenu.Items.Add(LocalizationService.Get("menu.node.sync_group_interface", "Sync Group Interface"), null, (_, _) =>
        {
            if (_selectedNode?.Kind == NodeKind.Group)
            {
                SyncGroupInterfaceAction?.Invoke();
            }
        });
        _nodeMenu.Items.Add(LocalizationService.Get("menu.node.ungroup", "Ungroup"), null, (_, _) =>
        {
            if (_selectedNode?.Kind == NodeKind.Group)
            {
                UngroupAction?.Invoke();
            }
        });
        _nodeMenu.Items.Add(LocalizationService.Get("menu.node.exit_group_editor", "Exit Group Editor"), null, (_, _) =>
        {
            if (_selectedNode?.Kind is NodeKind.GroupInput or NodeKind.GroupOutput)
            {
                ExitGroupEditorAction?.Invoke();
            }
        });
        _nodeMenu.Items.Add(LocalizationService.Get("menu.node.reset_view", "Reset View"), null, (_, _) => ResetViewport());
        _nodeMenu.Items.Add(LocalizationService.Get("menu.node.fit_view", "Fit Graph To View"), null, (_, _) => FitGraphToView());
    }

    private void AddRayMaterialMenus(ContextMenuStrip menu)
    {
        var materialMenu = new ToolStripMenuItem("Ray 材质");
        AddNodeGroup(
            materialMenu,
            "输入",
            NodeKind.Scalar,
            NodeKind.Color,
            NodeKind.Float2Value,
            NodeKind.Float3Value,
            NodeKind.Float4Value,
            NodeKind.Time,
            NodeKind.TexCoord,
            NodeKind.TextureCoordinate,
            NodeKind.MatCapUv,
            NodeKind.LocalNormal,
            NodeKind.WorldNormal,
            NodeKind.ViewDirection,
            NodeKind.RayLightDirection,
            NodeKind.RayTextureSlot);
        AddNodeGroup(
            materialMenu,
            "Ray 通道",
            NodeKind.RaySceneColor,
            NodeKind.RaySceneDepth,
            NodeKind.RaySceneNormal,
            NodeKind.RaySsao,
            NodeKind.RayMultiLight,
            NodeKind.RayAccumulatedLighting,
            NodeKind.RaySunLightData,
            NodeKind.RayDebugView,
            NodeKind.RayDebugController,
            NodeKind.RayShadowData,
            NodeKind.RayIblReflection,
            NodeKind.RaySsrReflection,
            NodeKind.RayOutlineChannel,
            NodeKind.RayFogChannel,
            NodeKind.RayIblSplit,
            NodeKind.RayChannelSplit,
            NodeKind.RayMaterialDiagnostic,
            NodeKind.RayFogDepthBlend,
            NodeKind.DepthFade,
            NodeKind.RayControllerInput,
            NodeKind.RayPostParameter);
        AddNodeGroup(
            materialMenu,
            "程序纹理",
            NodeKind.NoiseTexture,
            NodeKind.VoronoiTexture,
            NodeKind.FbmNoise,
            NodeKind.CellEdgeTexture,
            NodeKind.GradientTexture,
            NodeKind.CheckerTexture,
            NodeKind.BrickTexture,
            NodeKind.WaveTexture);
        AddNodeGroup(
            materialMenu,
            "自发光",
            NodeKind.RayEmissivePulse);
        AddNodeGroup(
            materialMenu,
            "光照辅助",
            NodeKind.Lambert,
            NodeKind.HalfLambert,
            NodeKind.RayLightingMix,
            NodeKind.Fresnel,
            NodeKind.FresnelSchlick,
            NodeKind.RimLight,
            NodeKind.ShadowThreshold,
            NodeKind.ShadowSoftness,
            NodeKind.ShadowColorMix,
            NodeKind.MatCapMix,
            NodeKind.ToonRampSample,
            NodeKind.GenericRampSample);
        AddNodeGroup(
            materialMenu,
            "Ray Custom / PBR",
            NodeKind.RayCustomData,
            NodeKind.RayReflectionBridge,
            NodeKind.RayClearCoatBridge,
            NodeKind.RayAnisotropyBridge,
            NodeKind.RayClothBridge,
            NodeKind.RaySkinSssBridge,
            NodeKind.RayToonCelBridge,
            NodeKind.RayBrdfToRayBridge,
            NodeKind.RaySkinAdvanced,
            NodeKind.RayMaterialLayer,
            NodeKind.ClearCoat,
            NodeKind.Bssrdf,
            NodeKind.SubsurfaceScattering,
            NodeKind.SkinPreintegratedLut,
            NodeKind.GGXSpecular,
            NodeKind.BurleyDiffuse,
            NodeKind.BRDFLighting,
            NodeKind.SmithJointGGX,
            NodeKind.CookTorranceSpecular,
            NodeKind.AnisotropicGGXSpecular,
            NodeKind.KelemenSzirmayKalosSpecular);
        AddNodeGroup(
            materialMenu,
            "风格化",
            NodeKind.DiffuseShadow,
            NodeKind.ShadowRampColor,
            NodeKind.GenshinRamp,
            NodeKind.SnowBreakRamp,
            NodeKind.MatCapBlendMode,
            NodeKind.Wetness,
            NodeKind.RaySnowLayer,
            NodeKind.RayDustLayer,
            NodeKind.RayEdgeWear,
            NodeKind.BoxMask,
            NodeKind.SphereMask,
            NodeKind.SlopeMask,
            NodeKind.TriplanarBoxmap);
        AddNodeGroup(
            materialMenu,
            "输出",
            NodeKind.RayMaterialOutput,
            NodeKind.RayShadingOutput);
        menu.Items.Add(materialMenu);

        var mathMenu = new ToolStripMenuItem("数学");
        AddNodeGroup(
            mathMenu,
            "基础运算",
            NodeKind.Add,
            NodeKind.Subtract,
            NodeKind.Multiply,
            NodeKind.Divide,
            NodeKind.Min,
            NodeKind.Max,
            NodeKind.Abs,
            NodeKind.Sign,
            NodeKind.Power,
            NodeKind.Modulo,
            NodeKind.Lerp);
        AddNodeGroup(
            mathMenu,
            "范围",
            NodeKind.Clamp,
            NodeKind.Step,
            NodeKind.SmoothStep,
            NodeKind.LessThan,
            NodeKind.GreaterThan,
            NodeKind.LessEqual,
            NodeKind.GreaterEqual,
            NodeKind.Equal,
            NodeKind.NotEqual,
            NodeKind.Remap,
            NodeKind.Saturate,
            NodeKind.OneMinus);
        AddNodeGroup(
            mathMenu,
            "取整与函数",
            NodeKind.Floor,
            NodeKind.Frac,
            NodeKind.Ceil,
            NodeKind.Truncate,
            NodeKind.Round,
            NodeKind.SquareRoot,
            NodeKind.ReciprocalSquareRoot,
            NodeKind.Logarithm,
            NodeKind.Exponent);
        AddNodeGroup(
            mathMenu,
            "三角函数",
            NodeKind.Sine,
            NodeKind.Cosine,
            NodeKind.Tangent,
            NodeKind.ArcSine,
            NodeKind.ArcCosine,
            NodeKind.ArcTangent,
            NodeKind.ToRadians,
            NodeKind.ToDegrees);
        AddNodeGroup(
            mathMenu,
            "向量",
            NodeKind.Normalize,
            NodeKind.Dot,
            NodeKind.Cross,
            NodeKind.Length,
            NodeKind.Distance,
            NodeKind.Project);
        AddNodeGroup(
            mathMenu,
            "通道",
            NodeKind.SplitColor,
            NodeKind.SplitXY,
            NodeKind.SplitXYZ,
            NodeKind.SplitXYZW,
            NodeKind.ComposeColor,
            NodeKind.AppendFloat2,
            NodeKind.MergeXYZ,
            NodeKind.MergeXYZW,
            NodeKind.ComponentMask);
        AddNodeGroup(
            mathMenu,
            "颜色调整",
            NodeKind.ColorRamp,
            NodeKind.RgbCurve,
            NodeKind.ColorAdjust,
            NodeKind.LayerBlend);
        AddNodeGroup(
            mathMenu,
            "UV",
            NodeKind.UvTransform,
            NodeKind.Panner,
            NodeKind.UvRotate,
            NodeKind.ParallaxUv,
            NodeKind.NormalMap,
            NodeKind.RayNormalStrength,
            NodeKind.DetailNormalBlend);
        menu.Items.Add(mathMenu);
    }

    public void RefreshContextMenusCore()
    {
        BuildContextMenus();
    }

    private void AddScenePostProcessMenusCore(ContextMenuStrip menu)
    {
        var inputMenu = new ToolStripMenuItem(LocalizationService.Get("category.Input", "Input"));
        AddNodeGroup(
            inputMenu,
            LocalizationService.Get("group.input.values", "Values"),
            NodeKind.Scalar,
            NodeKind.Color,
            NodeKind.Float2Value,
            NodeKind.Float3Value,
            NodeKind.Float4Value,
            NodeKind.ScreenUv,
            NodeKind.Time,
            NodeKind.ElapsedTime,
            NodeKind.ViewportPixelSize);
        AddNodeGroup(
            inputMenu,
            LocalizationService.Get("group.input.textures", "Textures"),
            NodeKind.ExternalTexture);
        AddNodeGroup(
            inputMenu,
            LocalizationService.Get("group.input.procedural_noise", "Procedural Noise"),
            NodeKind.DomainWarp,
            NodeKind.NoiseTexture,
            NodeKind.VoronoiTexture,
            NodeKind.FbmNoise,
            NodeKind.MusgraveTexture,
            NodeKind.CellEdgeTexture,
            NodeKind.CurlNoise,
            NodeKind.AnisotropicNoise);
        AddNodeGroup(
            inputMenu,
            LocalizationService.Get("group.input.procedural_pattern", "Procedural Pattern"),
            NodeKind.GradientTexture,
            NodeKind.CheckerTexture,
            NodeKind.BrickTexture,
            NodeKind.WaveTexture);
        menu.Items.Add(inputMenu);

        var layerMenu = new ToolStripMenuItem(LocalizationService.Get("menu.layer", "Layer"));
        AddNodeGroup(
            layerMenu,
            LocalizationService.Get("group.layer.sources", "Sources"),
            NodeKind.LayerSource);
        AddNodeGroup(
            layerMenu,
            LocalizationService.Get("group.layer.compositing", "Compositing"),
            NodeKind.LayerBlend);
        menu.Items.Add(layerMenu);

        AddExperimentalMenu(menu);
        AddMathMenu(menu);
        AddLayoutMenu(menu);
    }

    private void AddBufferPassMenusCore(ContextMenuStrip menu)
    {
        AddCategoryMenu(menu, LocalizationService.Get("category.Input", "Input"), NodeCategory.Input);

        var geometryMenu = new ToolStripMenuItem(LocalizationService.Get("menu.geometry", "Geometry"));
        AddNodeGroup(
            geometryMenu,
            LocalizationService.Get("group.geometry.coords_uv", "Coordinates and UV"),
            NodeKind.TextureCoordinate,
            NodeKind.TexCoord,
            NodeKind.SubTextureUv,
            NodeKind.VertexChannel,
            NodeKind.ScreenUv,
            NodeKind.LocalPosition,
            NodeKind.WorldPosition,
            NodeKind.TransformPosition);
        AddNodeGroup(
            geometryMenu,
            LocalizationService.Get("group.geometry.normals_dirs", "Normals and Directions"),
            NodeKind.CameraData,
            NodeKind.LocalNormal,
            NodeKind.WorldNormal,
            NodeKind.SurfaceTangent,
            NodeKind.SurfaceBitangent,
            NodeKind.HairTangent,
            NodeKind.ViewDirection,
            NodeKind.CameraPosition,
            NodeKind.LightDirection,
            NodeKind.TransformVector);
        AddNodeGroup(
            geometryMenu,
            LocalizationService.Get("group.geometry.control_objects", "Control Objects"),
            NodeKind.ControlObjectPosition,
            NodeKind.ControlObjectValue,
            NodeKind.ControlObjectRotation,
            NodeKind.ControllerLightDirection,
            NodeKind.ControlObjectVector,
            NodeKind.ControlObjectCenter,
            NodeKind.ControlObjectTransformDirection,
            NodeKind.ControlObjectAngleDirection);
        AddNodeGroup(
            geometryMenu,
            LocalizationService.Get("group.geometry.control_bones", "Control Bones"),
            NodeKind.ControlObjectBonePosition,
            NodeKind.PointLightDirection,
            NodeKind.ControlObjectBoneDirection);
        menu.Items.Add(geometryMenu);

        AddTextureMenu(menu);
        AddShadingMenu(menu);
        AddMathMenu(menu);
        AddExperimentalMenu(menu);
        AddLayoutMenu(menu);

        var layerMenu = new ToolStripMenuItem(LocalizationService.Get("menu.layer", "Layer"));
        AddNodeGroup(
            layerMenu,
            LocalizationService.Get("group.layer.outputs", "Outputs"),
            NodeKind.LayerSourceOutput);
        menu.Items.Add(layerMenu);
    }

    private void AddObjectLayerMenuCore(ContextMenuStrip menu)
    {
        var layerMenu = new ToolStripMenuItem(LocalizationService.Get("menu.layer", "Layer"));
        AddNodeGroup(
            layerMenu,
            LocalizationService.Get("group.layer.outputs", "Outputs"),
            NodeKind.LayerSourceOutput);
        menu.Items.Add(layerMenu);
    }

    private void AddLayoutMenuCore(ContextMenuStrip menu)
    {
        var layoutMenu = new ToolStripMenuItem(LocalizationService.Get("menu.layout", "Layout"));
        AddNodeGroup(
            layoutMenu,
            LocalizationService.Get("group.layout.organize", "Organize"),
            NodeKind.Frame,
            NodeKind.Reroute);
        menu.Items.Add(layoutMenu);
    }

    private void AddGeometryMenuCore(ContextMenuStrip menu)
    {
        var geometryMenu = new ToolStripMenuItem(LocalizationService.Get("menu.geometry", "Geometry"));
        AddNodeGroup(
            geometryMenu,
            LocalizationService.Get("group.geometry.coords_uv", "Coordinates and UV"),
            NodeKind.TextureCoordinate,
            NodeKind.TexCoord,
            NodeKind.SubTextureUv,
            NodeKind.VertexChannel,
            NodeKind.ScreenUv,
            NodeKind.LocalPosition,
            NodeKind.WorldPosition,
            NodeKind.TransformPosition);
        AddNodeGroup(
            geometryMenu,
            LocalizationService.Get("group.geometry.normals_dirs", "Normals and Directions"),
            NodeKind.CameraData,
            NodeKind.LocalNormal,
            NodeKind.WorldNormal,
            NodeKind.SurfaceTangent,
            NodeKind.SurfaceBitangent,
            NodeKind.ViewDirection,
            NodeKind.CameraPosition,
            NodeKind.LightDirection,
            NodeKind.TransformVector);
        AddNodeGroup(
            geometryMenu,
            LocalizationService.Get("group.geometry.control_objects", "Control Objects"),
            NodeKind.ControlObjectPosition,
            NodeKind.ControlObjectValue,
            NodeKind.ControlObjectRotation,
            NodeKind.ControllerLightDirection,
            NodeKind.ControlObjectVector,
            NodeKind.ControlObjectCenter,
            NodeKind.ControlObjectTransformDirection,
            NodeKind.ControlObjectAngleDirection);
        AddNodeGroup(
            geometryMenu,
            LocalizationService.Get("group.geometry.control_bones", "Control Bones"),
            NodeKind.ControlObjectBonePosition,
            NodeKind.PointLightDirection,
            NodeKind.ControlObjectBoneDirection);
        AddNodeGroup(
            geometryMenu,
            LocalizationService.Get("group.geometry.material_light", "Material and Light"),
            NodeKind.MaterialDiffuse,
            NodeKind.MaterialAmbient,
            NodeKind.MaterialEmissive,
            NodeKind.MaterialSpecularColor,
            NodeKind.MaterialToonColor,
            NodeKind.LightDiffuse,
            NodeKind.LightAmbient,
            NodeKind.EdgeColorNode,
            NodeKind.GroundShadowColorNode,
            NodeKind.TextureAddValueNode,
            NodeKind.TextureMulValueNode,
            NodeKind.SphereAddValueNode,
            NodeKind.SphereMulValueNode);
        menu.Items.Add(geometryMenu);
    }

    private void AddTextureMenuCore(ContextMenuStrip menu)
    {
        var textureMenu = new ToolStripMenuItem(LocalizationService.Get("menu.texture", "Texture"));
        AddNodeGroup(
            textureMenu,
            LocalizationService.Get("group.texture.material_inputs", "Material Inputs"),
            NodeKind.MaterialTexture,
            NodeKind.TriplanarBoxmap,
            NodeKind.MaterialSphereMap,
            NodeKind.MaterialToonTexture,
            NodeKind.EmissiveTexture,
            NodeKind.ParallaxUv,
            NodeKind.NormalMap,
            NodeKind.DetailNormalBlend);
        AddNodeGroup(
            textureMenu,
            LocalizationService.Get("group.texture.external_inputs", "External Inputs"),
            NodeKind.ExternalTexture,
            NodeKind.MatCapAtlasSample);
        AddNodeGroup(
            textureMenu,
            LocalizationService.Get("group.texture.procedural_noise", "Noise"),
            NodeKind.DomainWarp,
            NodeKind.NoiseTexture,
            NodeKind.VoronoiTexture,
            NodeKind.FbmNoise,
            NodeKind.MusgraveTexture,
            NodeKind.CellEdgeTexture,
            NodeKind.CurlNoise,
            NodeKind.AnisotropicNoise);
        AddNodeGroup(
            textureMenu,
            LocalizationService.Get("group.texture.procedural_pattern", "Pattern"),
            NodeKind.GradientTexture,
            NodeKind.CheckerTexture,
            NodeKind.BrickTexture,
            NodeKind.WaveTexture);
        AddNodeGroup(
            textureMenu,
            LocalizationService.Get("group.texture.material_blend", "Material Blend"),
            NodeKind.ApplyTextureValue,
            NodeKind.ApplySphereAdd,
            NodeKind.ApplySphereMul,
            NodeKind.ApplySphereReplace);
        menu.Items.Add(textureMenu);
    }

    private void AddShadingMenuCore(ContextMenuStrip menu)
    {
        var shadingMenu = new ToolStripMenuItem(LocalizationService.Get("menu.shading", "Shading"));
        AddNodeGroup(
            shadingMenu,
            LocalizationService.Get("group.shading.vertex_deformation", "Vertex Deformation"),
            NodeKind.OffsetAlongNormal,
            NodeKind.VertexWave,
            NodeKind.AxisMask,
            NodeKind.Twist,
            NodeKind.Bend,
            NodeKind.NoiseDisplace);
        AddNodeGroup(
            shadingMenu,
            LocalizationService.Get("group.shading.lighting_terms", "Lighting Terms"),
            NodeKind.HalfVector,
            NodeKind.NdotL,
            NodeKind.NdotV,
            NodeKind.NdotH,
            NodeKind.Lambert,
            NodeKind.HalfLambert,
            NodeKind.BlinnPhong,
            NodeKind.WrapLighting,
            NodeKind.GouraudLighting,
            NodeKind.OrenNayarLighting,
            NodeKind.OrenNayarDiffuseReflection,
            NodeKind.OrenNayarBlinn,
            NodeKind.MinnaertLighting,
            NodeKind.KajiyaKay,
            NodeKind.RimLight,
            NodeKind.FresnelSchlick,
            NodeKind.GGXSpecular,
            NodeKind.BurleyDiffuse,
            NodeKind.BRDFLighting,
            NodeKind.SmithJointGGX,
            NodeKind.CookTorranceSpecular,
            NodeKind.AnisotropicGGXSpecular,
            NodeKind.KelemenSzirmayKalosSpecular);
        AddNodeGroup(
            shadingMenu,
            LocalizationService.Get("group.shading.shadow", "Shadow"),
            NodeKind.SelfShadowFactor,
            NodeKind.DiffuseShadow,
            NodeKind.ShadowThreshold,
            NodeKind.ShadowSoftness,
            NodeKind.ShadowColorMix);
        AddNodeGroup(
            shadingMenu,
            LocalizationService.Get("group.shading.result", "Shading Result"),
            NodeKind.BasicLighting,
            NodeKind.VirtualLight);
        AddNodeGroup(
            shadingMenu,
            LocalizationService.Get("group.shading.material_finish", "Material Finish"),
            NodeKind.Wetness,
            NodeKind.ClearCoat,
            NodeKind.DepthFade,
            NodeKind.Bssrdf,
            NodeKind.SubsurfaceScattering,
            NodeKind.MatCapMix,
            NodeKind.FakeEnvReflection,
            NodeKind.ToonRampSample,
            NodeKind.GenshinRamp,
            NodeKind.SnowBreakRamp,
            NodeKind.GenericRampSample,
            NodeKind.PreIntegratedFGD,
            NodeKind.SkinPreintegratedLut,
            NodeKind.DisneyPrincipledLite,
            NodeKind.DisneyPrincipled);
        menu.Items.Add(shadingMenu);
    }

    private void AddExperimentalMenuCore(ContextMenuStrip menu)
    {
        var experimentalMenu = new ToolStripMenuItem(LocalizationService.Get("menu.experimental", "Experimental"));
        AddNodeGroup(
            experimentalMenu,
            LocalizationService.Get("group.experimental.stylized", "Stylized"),
            NodeKind.RimShadow,
            NodeKind.ShadowRampColor);
        AddNodeGroup(
            experimentalMenu,
            LocalizationService.Get("group.experimental.buffer", "Buffer"),
            NodeKind.RimDepthBuffer,
            NodeKind.RimMaskBuffer);
        AddNodeGroup(
            experimentalMenu,
            LocalizationService.Get("group.experimental.screen", "Screen Space"),
            NodeKind.SceneColor,
            NodeKind.SceneDepth,
            NodeKind.OffscreenBufferSample,
            NodeKind.DepthVisualize,
            NodeKind.ScreenUvOffset,
            NodeKind.BufferCompare,
            NodeKind.DepthEdgeDetect,
            NodeKind.NormalEdgeDetect,
            NodeKind.MaskBufferDebug,
            NodeKind.ScreenSpaceRimCompose);
        menu.Items.Add(experimentalMenu);
    }

    private void AddCategoryMenuCore(ContextMenuStrip menu, string title, NodeCategory category)
    {
        var definitions = NodeRegistry.All
            .Where(item => IsKindAllowed(item.Kind))
            .Where(item => IsNodeVisibleInWorkspaceUi(_graph.WorkspaceMode, item.Kind))
            .Where(item => item.Category == category)
            .OrderBy(item => UiTextHelper.NodeTitle(item))
            .ToList();

        if (definitions.Count == 0)
        {
            return;
        }

        var categoryMenu = new ToolStripMenuItem(title);
        foreach (var definition in definitions)
        {
            AddNodeMenuItem(categoryMenu.DropDownItems, definition);
        }

        menu.Items.Add(categoryMenu);
    }

    private void AddMathMenuCore(ContextMenuStrip menu)
    {
        var mathMenu = new ToolStripMenuItem(LocalizationService.Get("menu.math", "Math"));
        AddMathGroup(mathMenu, LocalizationService.Get("group.math.basic_ops", "Basic Ops"), NodeKind.Add, NodeKind.Subtract, NodeKind.Multiply, NodeKind.Divide, NodeKind.Modulo, NodeKind.Min, NodeKind.Max, NodeKind.Abs, NodeKind.Sign, NodeKind.Clamp, NodeKind.Power, NodeKind.Logarithm, NodeKind.Exponent, NodeKind.SquareRoot, NodeKind.ReciprocalSquareRoot);
        AddMathGroup(mathMenu, LocalizationService.Get("group.math.compare_blend", "Compare and Blend"), NodeKind.Step, NodeKind.SmoothStep, NodeKind.LessThan, NodeKind.GreaterThan, NodeKind.LessEqual, NodeKind.GreaterEqual, NodeKind.Equal, NodeKind.NotEqual, NodeKind.Lerp, NodeKind.VectorMix, NodeKind.ColorRamp, NodeKind.RgbCurve, NodeKind.Saturate, NodeKind.OneMinus, NodeKind.Remap, NodeKind.Sigmoid, NodeKind.Softmax);
        AddMathGroup(mathMenu, LocalizationService.Get("group.math.vector_angle", "Vector and Angle"), NodeKind.Normalize, NodeKind.Dot, NodeKind.Cross, NodeKind.Project, NodeKind.Length, NodeKind.Distance, NodeKind.Reflect, NodeKind.Refract, NodeKind.FaceForward, NodeKind.Fresnel);
        AddMathGroup(mathMenu, LocalizationService.Get("group.math.functions", "Functions"), NodeKind.Floor, NodeKind.Frac, NodeKind.Ceil, NodeKind.Truncate, NodeKind.Round, NodeKind.Sine, NodeKind.Cosine, NodeKind.Tangent, NodeKind.ArcSine, NodeKind.ArcCosine, NodeKind.ArcTangent, NodeKind.ArcTangent2);
        AddMathGroup(mathMenu, LocalizationService.Get("group.math.convert", "Convert"), NodeKind.ToRadians, NodeKind.ToDegrees);
        AddMathGroup(mathMenu, LocalizationService.Get("group.math.color_vector", "Color and Vector"), NodeKind.SplitColor, NodeKind.ComposeColor, NodeKind.SplitXY, NodeKind.SplitXYZ, NodeKind.SplitXYZW, NodeKind.AppendFloat2, NodeKind.MergeXYZ, NodeKind.MergeXYZW);
        AddMathGroup(mathMenu, LocalizationService.Get("group.math.mask", "Mask"), NodeKind.ComponentMask, NodeKind.BoxMask, NodeKind.SphereMask, NodeKind.SlopeMask);
        AddMathGroup(mathMenu, LocalizationService.Get("group.math.uv", "UV"), NodeKind.UvTransform, NodeKind.UvRotate, NodeKind.Panner, NodeKind.ScreenUv, NodeKind.MatCapUv);
        menu.Items.Add(mathMenu);
    }

    private void AddNodeGroupCore(ToolStripMenuItem parent, string title, params NodeKind[] nodeKinds)
    {
        var filteredNodeKinds = nodeKinds
            .Where(kind => IsKindAllowed(kind))
            .ToArray();

        if (filteredNodeKinds.Length == 0)
        {
            return;
        }

        var groupMenu = new ToolStripMenuItem(title);
        foreach (var kind in filteredNodeKinds)
        {
            AddNodeMenuItem(groupMenu.DropDownItems, NodeRegistry.Get(kind));
        }

        parent.DropDownItems.Add(groupMenu);
    }

    private void AddMathGroupCore(ToolStripMenuItem parent, string title, params NodeKind[] nodeKinds)
    {
        AddNodeGroup(parent, title, nodeKinds);
    }

    private void AddNodeMenuItemCore(ToolStripItemCollection items, NodeDefinition definition)
    {
        var captured = definition.Kind;
        items.Add(UiTextHelper.NodeTitle(definition), null, (_, _) => AddNode(captured));
    }

    private bool PreparePinMenuCore(PinHit pinHit)
    {
        _pinMenu.Items.Clear();

        if (pinHit.IsInput)
        {
            var inputConnection = _graph.FindInputConnection(pinHit.Node.Id, pinHit.Pin.Name);
            if (inputConnection is not null)
            {
                _pinMenu.Items.Add(LocalizationService.Format("menu.pin.disconnect_input", "Disconnect input \"{0}\"", UiTextHelper.PinLabel(pinHit.Pin)), null, (_, _) =>
                {
                    ApplyGraphMutation(() => _graph.RemoveInputConnection(pinHit.Node.Id, pinHit.Pin.Name), changeKind: GraphChangeKind.ConnectionChanged);
                });
            }
        }
        else
        {
            var outgoing = _graph.GetOutgoing(pinHit.Node.Id)
                .Where(item => string.Equals(item.SourcePin, pinHit.Pin.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (outgoing.Count > 0)
            {
                _pinMenu.Items.Add(LocalizationService.Format("menu.pin.disconnect_all_outputs", "Disconnect all outputs from \"{0}\"", UiTextHelper.PinLabel(pinHit.Pin)), null, (_, _) =>
                {
                    ApplyGraphMutation(() => _graph.RemoveOutgoingConnections(pinHit.Node.Id, pinHit.Pin.Name), changeKind: GraphChangeKind.ConnectionChanged);
                });
            }
        }

        return _pinMenu.Items.Count > 0;
    }

    private bool PrepareConnectionMenuCore(GraphConnection connection)
    {
        _connectionMenu.Items.Clear();
        _connectionMenu.Items.Add(LocalizationService.Get("menu.connection.insert_reroute", "Insert Reroute"), null, (_, _) =>
        {
            InsertRerouteOnConnection(connection, _mouseCanvasPoint);
        });
        _connectionMenu.Items.Add(LocalizationService.Get("menu.connection.disconnect", "Disconnect Link"), null, (_, _) =>
        {
            ApplyGraphMutation(() => _graph.RemoveConnection(connection), changeKind: GraphChangeKind.ConnectionChanged);
        });
        return true;
    }
}
