using System.Text;
using System.Text.RegularExpressions;
using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Controls;

public sealed class NodeSearchDialog : Form
{
    private readonly TextBox _searchBox = new();
    private readonly ListBox _resultList = new();
    private readonly Label _hintLabel = new();
    private readonly List<NodeDefinition> _definitions;
    private readonly Func<NodeDefinition, string>? _groupLabelProvider;

    public NodeSearchDialog(
        IEnumerable<NodeDefinition> definitions,
        string? hintText = null,
        Func<NodeDefinition, string>? groupLabelProvider = null,
        Func<NodeDefinition, int>? priorityProvider = null)
    {
        _definitions = definitions
            .OrderByDescending(item => priorityProvider?.Invoke(item) ?? 0)
            .ThenBy(item => UiTextHelper.NodeTitle(item))
            .ToList();
        _groupLabelProvider = groupLabelProvider;

        Text = LocalizationService.Get("search.title", "Search Nodes");
        Width = 400;
        Height = 460;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = EditorTheme.WindowBack;
        ForeColor = EditorTheme.TextPrimary;

        _hintLabel.Dock = DockStyle.Top;
        _hintLabel.Height = string.IsNullOrWhiteSpace(hintText) ? 0 : 22;
        _hintLabel.Padding = new Padding(10, 4, 10, 0);
        _hintLabel.Text = hintText ?? string.Empty;
        _hintLabel.ForeColor = EditorTheme.TextMuted;

        _searchBox.Dock = DockStyle.Top;
        _searchBox.Margin = new Padding(8);
        _searchBox.BorderStyle = BorderStyle.FixedSingle;
        _searchBox.BackColor = EditorTheme.PanelAlt;
        _searchBox.ForeColor = EditorTheme.TextPrimary;
        _searchBox.Font = new Font(Font.FontFamily, 10f, FontStyle.Regular);
        _searchBox.TextChanged += (_, _) => RefreshResults();

        _resultList.Dock = DockStyle.Fill;
        _resultList.IntegralHeight = false;
        _resultList.BorderStyle = BorderStyle.None;
        _resultList.BackColor = EditorTheme.Panel;
        _resultList.ForeColor = EditorTheme.TextPrimary;
        _resultList.Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular);
        _resultList.DoubleClick += (_, _) => ConfirmSelection();
        _resultList.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
            }
        };

        _searchBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Down && _resultList.Items.Count > 0)
            {
                _resultList.SelectedIndex = Math.Min(_resultList.Items.Count - 1, Math.Max(0, _resultList.SelectedIndex + 1));
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up && _resultList.Items.Count > 0)
            {
                _resultList.SelectedIndex = Math.Max(0, _resultList.SelectedIndex - 1);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        };

        Controls.Add(_resultList);
        Controls.Add(_searchBox);
        Controls.Add(_hintLabel);

        Shown += (_, _) =>
        {
            _searchBox.Focus();
            RefreshResults();
        };
    }

    public NodeDefinition? SelectedDefinition =>
        _resultList.SelectedItem is SearchItem item ? item.Definition : null;

    private void RefreshResults()
    {
        var query = _searchBox.Text.Trim();
        var matches = _definitions
            .Select(definition => new SearchItem(definition, Score(definition, query)))
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => _definitions.IndexOf(item.Definition))
            .ThenBy(item => UiTextHelper.NodeTitle(item.Definition), StringComparer.OrdinalIgnoreCase)
            .ToList();

        _resultList.BeginUpdate();
        _resultList.Items.Clear();
        foreach (var match in matches)
        {
            _resultList.Items.Add(match with { GroupLabel = _groupLabelProvider?.Invoke(match.Definition) });
        }

        _resultList.EndUpdate();
        if (_resultList.Items.Count > 0)
        {
            _resultList.SelectedIndex = 0;
        }

    }

    private void ConfirmSelection()
    {
        if (SelectedDefinition is null)
        {
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static int Score(NodeDefinition definition, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return 1;
        }

        var normalizedQuery = Normalize(query);
        var kindName = SplitKindName(definition.Kind.ToString());
        var title = Normalize(UiTextHelper.NodeTitle(definition));
        var description = Normalize(UiTextHelper.NodeDescription(definition));
        var kind = Normalize(kindName);
        var keywords = Normalize(GetSearchKeywords(definition.Kind));

        if (title.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 500 - title.IndexOf(normalizedQuery, StringComparison.Ordinal);
        }

        if (kind.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 420 - kind.IndexOf(normalizedQuery, StringComparison.Ordinal);
        }

        if (description.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 260 - description.IndexOf(normalizedQuery, StringComparison.Ordinal);
        }

        if (!string.IsNullOrWhiteSpace(keywords) &&
            keywords.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 220 - keywords.IndexOf(normalizedQuery, StringComparison.Ordinal);
        }

        return IsSubsequence(normalizedQuery, title) ? 180
            : IsSubsequence(normalizedQuery, kind) ? 150
            : IsSubsequence(normalizedQuery, keywords) ? 120
            : IsSubsequence(normalizedQuery, description) ? 80
            : 0;
    }

    private static string GetSearchKeywords(NodeKind kind)
    {
        return kind switch
        {
            NodeKind.OffsetAlongNormal => "vertex vertexshader displacement deform offset normal shell 顶点 位移 变形 法线 外壳",
            NodeKind.VertexWave => "vertex vertexshader displacement deform wave wobble 顶点 位移 变形 波浪",
            NodeKind.AxisMask => "vertex vertexshader displacement deform mask axis falloff 顶点 位移 变形 轴 向 衰减 遮罩",
            NodeKind.Twist => "vertex vertexshader displacement deform twist spiral 顶点 位移 变形 扭曲",
            NodeKind.Bend => "vertex vertexshader displacement deform bend curve 顶点 位移 变形 弯曲",
            NodeKind.NoiseDisplace => "vertex vertexshader displacement deform noise 顶点 位移 变形 噪声",
            NodeKind.ControlObjectPosition => "vertex vertexshader controller control object 顶点 控制器 控制物体",
            NodeKind.ControlObjectValue => "vertex vertexshader controller control object 顶点 控制器 控制物体",
            NodeKind.ControlObjectRotation => "vertex vertexshader controller control object rotation 顶点 控制器 控制物体 旋转",
            NodeKind.ControllerLightDirection => "vertex vertexshader controller light direction 顶点 控制器 灯光 方向",
            NodeKind.ControlObjectVector => "vertex vertexshader controller control object vector 顶点 控制器 控制物体 向量",
            NodeKind.ControlObjectCenter => "vertex vertexshader controller control object center 顶点 控制器 控制物体 中心",
            NodeKind.ControlObjectTransformDirection => "vertex vertexshader controller transform direction 顶点 控制器 变换 方向",
            NodeKind.ControlObjectAngleDirection => "vertex vertexshader controller angle direction 顶点 控制器 角度 方向",
            NodeKind.ControlObjectBonePosition => "vertex vertexshader controller bone 顶点 控制器 骨骼",
            NodeKind.ControlObjectBoneDirection => "vertex vertexshader controller bone direction 顶点 控制器 骨骼 方向",
            NodeKind.RayMaterialOutput => "ray 输出 材质 material material2 material_2.0 fx 结果",
            NodeKind.RayTextureSlot => "ray 贴图 纹理 图片 文件 albedo diffuse roughness smoothness normal metalness specular occlusion emissive 糙度 光滑度 法线 金属 高光 遮蔽 自发光",
            NodeKind.RayIblSplit => "ray ibl environment env light reflection split diffuse specular luma mask 反射 环境光 分离 高光 漫反射 遮罩",
            NodeKind.RayChannelSplit => "ray channel split scene depth normal ssao ssdo shadow ssr fog outline ibl 通道 拆分 深度 法线 阴影 雾 描边 反射",
            NodeKind.RayMaterialDiagnostic => "ray diagnostic debug material albedo normal smoothness metalness custom ssr fog outline ibl 诊断 调试 材质 可视化",
            NodeKind.RayFogDepthBlend => "ray fog depth blend godray scene depth distance mist 雾 深度 混合 远景",
            NodeKind.DepthFade => "ray scene depth fade distance mask 深度 淡出 距离 遮罩 雾",
            NodeKind.RayShadingOutput => "ray final shading output add multiply override 最终着色 输出",
            NodeKind.RaySceneColor => "ray scene color screen buffer 场景颜色 屏幕",
            NodeKind.RaySceneDepth => "ray scene depth gbuffer 深度",
            NodeKind.RaySceneNormal => "ray scene normal gbuffer 法线",
            NodeKind.RaySsao => "ray ssao ssdo ambient occlusion visibility 遮蔽",
            NodeKind.RaySsrReflection => "ray ssr reflection screen space 反射 屏幕空间",
            NodeKind.RayOutlineChannel => "ray outline channel mask 描边 通道 遮罩",
            NodeKind.RayFogChannel => "ray fog godray channel mist 雾 体积光 通道",
            NodeKind.Scalar => "float scalar 数值 常量 强度 倍率 alpha smoothness metalness 粗糙度 糙度 光滑度 金属度 透明度",
            NodeKind.Color => "color 颜色 albedo diffuse tint rgb rgba 主颜色 漫反射 色调",
            NodeKind.Float2Value => "float2 vector2 uv 坐标 二维",
            NodeKind.Float3Value => "float3 vector3 rgb 向量 三维",
            NodeKind.Float4Value => "float4 vector4 rgba 颜色 四维",
            NodeKind.Multiply => "multiply mul 乘法 相乘 倍率 贴图乘常量 粗糙度 糙度 光滑度",
            NodeKind.Add => "add 加法 相加 提亮",
            NodeKind.Subtract => "subtract sub 减法 相减",
            NodeKind.Divide => "divide div 除法 相除",
            NodeKind.OneMinus => "invert inverse one minus 反相 反转 1-x roughness smoothness 粗糙度转光滑度",
            NodeKind.Saturate => "saturate clamp 01 饱和 限制 0到1",
            NodeKind.Clamp => "clamp 限制 范围 最小 最大",
            NodeKind.LessThan => "less than compare mask threshold 小于 比较 阈值 遮罩",
            NodeKind.GreaterThan => "greater than compare mask threshold 大于 比较 阈值 遮罩",
            NodeKind.LessEqual => "less equal compare mask threshold 小于等于 比较 阈值 遮罩",
            NodeKind.GreaterEqual => "greater equal compare mask threshold 大于等于 比较 阈值 遮罩",
            NodeKind.Equal => "equal compare mask threshold 等于 比较 阈值 遮罩",
            NodeKind.NotEqual => "not equal compare mask threshold 不等于 比较 阈值 遮罩",
            NodeKind.Lerp => "lerp mix blend 插值 混合",
            NodeKind.ColorRamp => "color ramp gradient 颜色渐变 色带 遮罩 映射",
            NodeKind.RgbCurve => "rgb curve 曲线 gamma 色阶 调色",
            NodeKind.ColorAdjust => "color adjust exposure contrast saturation gamma gain 调色 曝光 对比度 饱和度",
            NodeKind.LayerBlend => "layer blend mix overlay multiply screen add 图层 混合 叠加 相乘 滤色",
            NodeKind.NoiseTexture => "noise texture procedural perlin value 噪声 程序纹理 颗粒 遮罩",
            NodeKind.VoronoiTexture => "voronoi texture cellular cell procedural 泰森 多边形 细胞 程序纹理 遮罩",
            NodeKind.GradientTexture => "gradient texture 渐变 程序纹理 遮罩 ramp",
            NodeKind.CheckerTexture => "checker texture 棋盘 格子 程序纹理 mask 遮罩",
            NodeKind.WaveTexture => "wave texture 波纹 条纹 圆环 程序纹理 mask 遮罩",
            NodeKind.FbmNoise => "fbm fractal noise 分形 噪声 程序纹理",
            NodeKind.CellEdgeTexture => "cell edge cellular voronoi 边缘 细胞 线框 程序纹理",
            NodeKind.BrickTexture => "brick texture 砖块 程序纹理 格子",
            NodeKind.RayLightDirection => "ray light direction directional controller 灯光 方向 控制器",
            NodeKind.Lambert => "lambert diffuse 漫反射 光照",
            NodeKind.HalfLambert => "half lambert 半兰伯特 柔光 漫反射",
            NodeKind.RayLightingMix => "lighting mix light color intensity 光照 混合",
            NodeKind.RayCustomData => "ray custom data customa customb clearcoat cloth skin cel anisotropy 自定义 材质",
            NodeKind.RayReflectionBridge => "ray reflection bridge smoothness metalness specular roughness 反射 光滑度 金属 高光 粗糙度",
            NodeKind.RayClearCoatBridge => "ray clearcoat bridge custom clear coat varnish 清漆 涂层 customa customb",
            NodeKind.RayAnisotropyBridge => "ray anisotropy bridge custom anisotropic 各向异性 高光 customa customb",
            NodeKind.RayClothBridge => "ray cloth bridge sheen fabric 布料 丝绒 customa customb",
            NodeKind.RaySkinSssBridge => "ray skin sss subsurface bridge 皮肤 次表面 散射 customa customb",
            NodeKind.RayToonCelBridge => "ray toon cel bridge tone based 卡通 阴影 色调 customa customb",
            NodeKind.RayBrdfToRayBridge => "brdf to ray bridge standalone native ggx reflection custom clearcoat anisotropy cloth skin cel 双模式 原生反射",
            NodeKind.ClearCoat => "clear coat clearcoat varnish coating 清漆 涂层 高光",
            NodeKind.Bssrdf or NodeKind.SubsurfaceScattering or NodeKind.SkinPreintegratedLut => "subsurface scattering sss skin bssrdf 皮肤 次表面",
            NodeKind.GGXSpecular or NodeKind.BRDFLighting or NodeKind.SmithJointGGX or NodeKind.CookTorranceSpecular or NodeKind.AnisotropicGGXSpecular or NodeKind.KelemenSzirmayKalosSpecular => "brdf ggx specular pbr anisotropic 高光 各向异性",
            NodeKind.BurleyDiffuse => "burley diffuse brdf 漫反射 pbr",
            NodeKind.DiffuseShadow or NodeKind.ShadowRampColor or NodeKind.GenshinRamp or NodeKind.SnowBreakRamp or NodeKind.MatCapBlendMode => "toon stylized ramp shadow matcap 卡通 色带 阴影 风格化",
            NodeKind.Fresnel or NodeKind.FresnelSchlick => "fresnel rim 边缘光 菲涅尔",
            NodeKind.RimLight => "rim light 边缘光 轮廓光",
            NodeKind.ShadowThreshold or NodeKind.ShadowSoftness or NodeKind.ShadowColorMix => "shadow toon threshold softness color 阴影 卡通",
            NodeKind.MatCapMix => "matcap mix 假高光 金属 眼睛",
            NodeKind.MatCapUv => "matcap uv 法线 贴图坐标 假高光",
            NodeKind.ToonRampSample or NodeKind.GenericRampSample => "toon ramp generic 色带 卡通 阴影",
            NodeKind.TexCoord or NodeKind.TextureCoordinate => "uv texcoord texture coordinate 坐标",
            NodeKind.LocalNormal or NodeKind.WorldNormal => "normal 法线",
            NodeKind.ViewDirection => "view direction camera 视线 观察方向",
            NodeKind.ParallaxUv => "parallax uv height 视差 高度",
            NodeKind.NormalMap => "normal map 法线贴图 强度",
            NodeKind.RayNormalStrength => "normal strength 法线 强度 ray",
            NodeKind.DetailNormalBlend => "detail normal blend 法线 混合 强度",
            NodeKind.UvTransform => "uv transform scale offset 缩放 偏移 uv",
            NodeKind.Panner => "panner scroll move time uv 滚动 移动 动画",
            NodeKind.UvRotate => "uv rotate rotation angle 旋转 角度",
            NodeKind.Time => "time 时间 动画 滚动",
            NodeKind.RayEmissivePulse => "ray emissive pulse blink glow 自发光 闪烁 脉冲 发光 动画",
            _ => string.Empty,
        };
    }

    private static string Normalize(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch) || ch > 127)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }

    private static string SplitKindName(string name)
    {
        return Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
    }

    private static bool IsSubsequence(string needle, string haystack)
    {
        if (needle.Length == 0)
        {
            return true;
        }

        var index = 0;
        foreach (var ch in haystack)
        {
            if (ch == needle[index])
            {
                index++;
                if (index == needle.Length)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private sealed record SearchItem(NodeDefinition Definition, int Score)
    {
        public string? GroupLabel { get; init; }

        public override string ToString()
        {
            return $"{UiTextHelper.NodeTitle(Definition)}    [{(string.IsNullOrWhiteSpace(GroupLabel) ? UiTextHelper.CategoryLabel(Definition.Category) : GroupLabel)}]";
        }
    }
}
