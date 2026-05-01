using System.Globalization;
using System.Text;
using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Services;

public sealed class RayMaterialCompiler
{
    private readonly List<string> _messages = [];
    private NodeGraph _graph = new();

    public RayMaterialCompileResult Compile(NodeGraph graph)
    {
        _graph = graph;
        _messages.Clear();

        var output = graph.Nodes.FirstOrDefault(node => node.Kind == NodeKind.RayMaterialOutput);
        if (output is null)
        {
            return new RayMaterialCompileResult(false, string.Empty, ["Ray Material Output node is required."]);
        }

        var material = RayMaterialParameters.CreateDefaults();
        material.Albedo.ApplyDiffuse = ReadSwitch(output, "AlbedoApplyDiffuse", defaultValue: true);
        material.Albedo.MorphColor = ReadSwitch(output, "AlbedoApplyMorphColor", defaultValue: false);
        material.Alpha.Swizzle = MapSwizzle(ReadText(output, "AlphaMapSwizzle", "A"));
        material.Emissive.MorphColor = ReadSwitch(output, "EmissiveApplyMorphColor", defaultValue: false);
        material.Emissive.EmissiveMorphIntensity = ReadSwitch(output, "EmissiveApplyMorphIntensity", defaultValue: false);
        material.Emissive.EmissiveBlink = ReadSwitch(output, "EmissiveApplyBlink", defaultValue: false);
        material.EmissiveBlinkExpression = FormatVector([
            ReadFloat(output, "EmissiveBlinkR", 1.0),
            ReadFloat(output, "EmissiveBlinkG", 1.0),
            ReadFloat(output, "EmissiveBlinkB", 1.0),
        ], 3);
        material.EmissiveIntensityExpression = FormatScalar(ReadFloat(output, "EmissiveIntensity", 1.0));

        ApplyColorSlot(output, "Albedo", material.Albedo, "albedo");
        ApplyColorSlot(output, "SubAlbedo", material.SubAlbedo, "albedoSub");
        ApplyScalarSlot(output, "Alpha", material.Alpha, "alpha");
        ApplyTextureOnlySlot(output, "Normal", material.Normal);
        ApplyTextureOnlySlot(output, "SubNormal", material.SubNormal);
        ApplyScalarSlot(output, "Smoothness", material.Smoothness, "smoothness");
        ApplyScalarSlot(output, "Metalness", material.Metalness, "metalness");
        ApplyColorSlot(output, "Specular", material.Specular, "specular");
        ApplyScalarSlot(output, "Occlusion", material.Occlusion, "occlusion");
        ApplyScalarSlot(output, "Parallax", material.Parallax, "parallaxMapScale");
        ApplyColorSlot(output, "Emissive", material.Emissive, "emissive");
        ApplyScalarSlot(output, "CustomA", material.CustomA, "customA");
        ApplyColorSlot(output, "CustomB", material.CustomB, "customB");

        var subAlbedoMode = MapSubAlbedoMode(ReadText(output, "SubAlbedoMode", "None"));
        if (subAlbedoMode != 0)
        {
            material.SubAlbedo.Enable = subAlbedoMode;
        }
        else if (HasConnection(output, "SubAlbedo"))
        {
            material.SubAlbedo.Enable = 1;
        }

        if (HasConnection(output, "Emissive") || ReadSwitch(output, "EmissiveEnabled", defaultValue: false) != 0)
        {
            material.EmissiveEnable = 1;
        }

        material.CustomEnable = MapCustomMode(ReadText(output, "CustomMode", "None"));
        var text = material.ToFxText();
        return new RayMaterialCompileResult(_messages.Count == 0, text, _messages.ToList());
    }

    private void ApplyTextureOnlySlot(GraphNode output, string inputPin, RayMaterialSlot slot)
    {
        var result = ResolveInput(output, inputPin);
        if (result.Texture is not null)
        {
            slot.ApplyTexture(result.Texture);
            if (result.Scale is { } scale)
            {
                slot.ConstExpression = FormatScalar(scale.Components[0]);
            }
            return;
        }

        if (result.HasValue)
        {
            _messages.Add($"{inputPin} only supports Ray Texture Slot in the compatible first version.");
        }
    }

    private void ApplyScalarSlot(GraphNode output, string inputPin, RayMaterialSlot slot, string constName)
    {
        var result = ResolveInput(output, inputPin);
        if (result.Texture is not null)
        {
            slot.ApplyTexture(result.Texture);
            if (result.Scale is { } scale)
            {
                slot.ApplyScale = 1;
                slot.ConstExpression = FormatScalar(scale.Components[0]);
            }
            return;
        }

        if (result.Value is { } value)
        {
            slot.MapFrom = 0;
            slot.ConstExpression = FormatScalar(value.Components[0]);
            return;
        }

        if (!string.IsNullOrWhiteSpace(slot.ConstExpression))
        {
            slot.ConstExpression = slot.ConstExpression;
        }
    }

    private void ApplyColorSlot(GraphNode output, string inputPin, RayMaterialSlot slot, string constName)
    {
        var result = ResolveInput(output, inputPin);
        if (result.Texture is not null)
        {
            slot.ApplyTexture(result.Texture);
            if (result.Scale is { } scale)
            {
                slot.ApplyScale = 1;
                slot.ConstExpression = FormatVector(scale.Components, slot.Components);
            }
            return;
        }

        if (result.Value is { } value)
        {
            slot.MapFrom = 0;
            slot.ConstExpression = FormatVector(value.Components, slot.Components);
        }
    }

    private bool HasConnection(GraphNode node, string inputPin) => _graph.FindInputConnection(node.Id, inputPin) is not null;

    private EvalResult ResolveInput(GraphNode node, string inputPin)
    {
        var connection = _graph.FindInputConnection(node.Id, inputPin);
        if (connection is null)
        {
            return EvalResult.Empty;
        }

        var source = _graph.FindNode(connection.SourceNodeId);
        return source is null ? EvalResult.Empty : Evaluate(source, connection.SourcePin, []);
    }

    private EvalResult Evaluate(GraphNode node, string outputPin, HashSet<Guid> visiting)
    {
        if (!visiting.Add(node.Id))
        {
            _messages.Add("Cycle detected while evaluating Ray material graph.");
            return EvalResult.Empty;
        }

        try
        {
            return node.Kind switch
            {
                NodeKind.Reroute => ResolveInput(node, "Input"),
                NodeKind.Scalar => EvalResult.FromValue(new ConstValue([ReadFloat(node, "Value", 1.0)])),
                NodeKind.Color => EvalResult.FromValue(new ConstValue([
                    ReadFloat(node, "R", 1.0),
                    ReadFloat(node, "G", 1.0),
                    ReadFloat(node, "B", 1.0),
                    ReadFloat(node, "A", 1.0),
                ])),
                NodeKind.Float2Value => EvalResult.FromValue(new ConstValue([ReadFloat(node, "X", 0.0), ReadFloat(node, "Y", 0.0)])),
                NodeKind.Float3Value => EvalResult.FromValue(new ConstValue([ReadFloat(node, "X", 0.0), ReadFloat(node, "Y", 0.0), ReadFloat(node, "Z", 0.0)])),
                NodeKind.Float4Value => EvalResult.FromValue(new ConstValue([ReadFloat(node, "X", 0.0), ReadFloat(node, "Y", 0.0), ReadFloat(node, "Z", 0.0), ReadFloat(node, "W", 0.0)])),
                NodeKind.RayTextureSlot => EvalResult.FromTexture(CreateTextureBinding(node)),
                NodeKind.Add => EvalBinary(node, (a, b) => a + b, "A", "B"),
                NodeKind.Subtract => EvalBinary(node, (a, b) => a - b, "A", "B"),
                NodeKind.Multiply => EvalMultiply(node),
                NodeKind.Divide => EvalBinary(node, (a, b) => Math.Abs(b) < 0.000001 ? 0.0 : a / b, "A", "B"),
                NodeKind.Min => EvalBinary(node, Math.Min, "A", "B"),
                NodeKind.Max => EvalBinary(node, Math.Max, "A", "B"),
                NodeKind.Power => EvalBinary(node, Math.Pow, "Value", "Exponent"),
                NodeKind.Abs => EvalUnary(node, Math.Abs, "Value"),
                NodeKind.Sign => EvalUnary(node, value => Math.Sign(value), "Value"),
                NodeKind.Floor => EvalUnary(node, Math.Floor, "Value"),
                NodeKind.Frac => EvalUnary(node, value => value - Math.Floor(value), "Value"),
                NodeKind.Ceil => EvalUnary(node, Math.Ceiling, "Value"),
                NodeKind.Truncate => EvalUnary(node, Math.Truncate, "Value"),
                NodeKind.Round => EvalUnary(node, Math.Round, "Value"),
                NodeKind.Modulo => EvalBinary(node, (a, b) => Math.Abs(b) < 0.000001 ? 0.0 : a % b, "A", "B"),
                NodeKind.Sine => EvalUnary(node, Math.Sin, "Value"),
                NodeKind.Cosine => EvalUnary(node, Math.Cos, "Value"),
                NodeKind.Tangent => EvalUnary(node, Math.Tan, "Value"),
                NodeKind.ArcSine => EvalUnary(node, value => Math.Asin(Math.Clamp(value, -1.0, 1.0)), "Value"),
                NodeKind.ArcCosine => EvalUnary(node, value => Math.Acos(Math.Clamp(value, -1.0, 1.0)), "Value"),
                NodeKind.ArcTangent => EvalUnary(node, Math.Atan, "Value"),
                NodeKind.ToRadians => EvalUnary(node, value => value * Math.PI / 180.0, "Value"),
                NodeKind.ToDegrees => EvalUnary(node, value => value * 180.0 / Math.PI, "Value"),
                NodeKind.SquareRoot => EvalUnary(node, value => Math.Sqrt(Math.Max(0.0, value)), "Value"),
                NodeKind.ReciprocalSquareRoot => EvalUnary(node, value => 1.0 / Math.Sqrt(Math.Max(0.00001, value)), "Value"),
                NodeKind.Logarithm => EvalUnary(node, value => Math.Log(Math.Max(0.00001, value)), "Value"),
                NodeKind.Exponent => EvalUnary(node, Math.Exp, "Value"),
                NodeKind.Lerp => EvalLerp(node),
                NodeKind.Clamp => EvalUnary(node, value => Math.Min(ReadFloat(node, "Max", 1.0), Math.Max(ReadFloat(node, "Min", 0.0), value)), "Value"),
                NodeKind.Saturate => EvalUnary(node, value => Math.Min(1.0, Math.Max(0.0, value)), "Color"),
                NodeKind.Step => EvalStep(node),
                NodeKind.SmoothStep => EvalSmoothStep(node),
                NodeKind.LessThan => EvalCompare(node, "LessThan"),
                NodeKind.GreaterThan => EvalCompare(node, "GreaterThan"),
                NodeKind.LessEqual => EvalCompare(node, "LessEqual"),
                NodeKind.GreaterEqual => EvalCompare(node, "GreaterEqual"),
                NodeKind.Equal => EvalCompare(node, "Equal"),
                NodeKind.NotEqual => EvalCompare(node, "NotEqual"),
                NodeKind.Remap => EvalRemap(node),
                NodeKind.OneMinus => EvalUnary(node, value => 1.0 - value, "Value"),
                NodeKind.SplitColor or NodeKind.SplitXY or NodeKind.SplitXYZ or NodeKind.SplitXYZW => EvalSplit(node, outputPin),
                NodeKind.ComposeColor => EvalCompose(node),
                NodeKind.AppendFloat2 => EvalAppendFloat2(node),
                NodeKind.MergeXYZ => EvalMerge(node, 3),
                NodeKind.MergeXYZW => EvalMerge(node, 4),
                NodeKind.ComponentMask => EvalComponentMask(node),
                NodeKind.ColorRamp => EvalColorRamp(node),
                NodeKind.RgbCurve => EvalRgbCurve(node),
                NodeKind.ColorAdjust => EvalColorAdjust(node),
                _ => Unsupported(node),
            };
        }
        finally
        {
            visiting.Remove(node.Id);
        }
    }

    private EvalResult EvalMultiply(GraphNode node)
    {
        var a = ResolveInput(node, "A");
        var b = ResolveInput(node, "B");
        if (a.Texture is not null && b.Texture is not null)
        {
            _messages.Add("Texture * texture is not supported in Ray compatible mode. Bake the maps first or use a future HLSL injection mode.");
            return EvalResult.Empty;
        }

        if (a.Texture is not null && b.Value is not null)
        {
            return EvalResult.FromTexture(a.Texture, b.Value);
        }

        if (b.Texture is not null && a.Value is not null)
        {
            return EvalResult.FromTexture(b.Texture, a.Value);
        }

        return EvalBinary(node, (x, y) => x * y, "A", "B");
    }

    private EvalResult EvalBinary(GraphNode node, Func<double, double, double> op, string aPin, string bPin)
    {
        var a = ResolveInput(node, aPin).Value;
        var b = ResolveInput(node, bPin).Value;
        if (a is null || b is null)
        {
            _messages.Add($"{node.Kind} requires constant inputs in Ray compatible mode.");
            return EvalResult.Empty;
        }

        return EvalResult.FromValue(ConstValue.Zip(a.Value, b.Value, op));
    }

    private EvalResult EvalUnary(GraphNode node, Func<double, double> op, string pin)
    {
        var value = ResolveInput(node, pin).Value;
        if (value is null)
        {
            _messages.Add($"{node.Kind} requires a constant input in Ray compatible mode.");
            return EvalResult.Empty;
        }

        return EvalResult.FromValue(value.Value.Map(op));
    }

    private EvalResult EvalLerp(GraphNode node)
    {
        var a = ResolveInput(node, "A").Value;
        var b = ResolveInput(node, "B").Value;
        var t = ResolveInput(node, "T").Value ?? new ConstValue([ReadFloat(node, "T", 0.5)]);
        if (a is null || b is null)
        {
            _messages.Add("Lerp requires constant A and B inputs in Ray compatible mode.");
            return EvalResult.Empty;
        }

        return EvalResult.FromValue(ConstValue.Zip(a.Value, b.Value, (x, y) => x + (y - x) * t.Components[0]));
    }

    private EvalResult EvalStep(GraphNode node)
    {
        var edge = ResolveInput(node, "Edge").Value ?? new ConstValue([ReadFloat(node, "Edge", 0.5)]);
        var x = ResolveInput(node, "X").Value;
        if (x is null)
        {
            _messages.Add("Step requires a constant X input in Ray compatible mode.");
            return EvalResult.Empty;
        }

        return EvalResult.FromValue(x.Value.Map(value => value >= edge.Component(0) ? 1.0 : 0.0));
    }

    private EvalResult EvalSmoothStep(GraphNode node)
    {
        var min = ResolveInput(node, "Min").Value ?? new ConstValue([ReadFloat(node, "Min", 0.0)]);
        var max = ResolveInput(node, "Max").Value ?? new ConstValue([ReadFloat(node, "Max", 1.0)]);
        var x = ResolveInput(node, "X").Value;
        if (x is null)
        {
            _messages.Add("SmoothStep requires a constant X input in Ray compatible mode.");
            return EvalResult.Empty;
        }

        var edge0 = min.Component(0);
        var edge1 = max.Component(0);
        return EvalResult.FromValue(x.Value.Map(value =>
        {
            var t = Math.Clamp((value - edge0) / Math.Max(0.00001, edge1 - edge0), 0.0, 1.0);
            return t * t * (3.0 - 2.0 * t);
        }));
    }

    private EvalResult EvalCompare(GraphNode node, string operation)
    {
        var a = ResolveInput(node, "A").Value;
        var b = ResolveInput(node, "B").Value;
        if (a is null || b is null)
        {
            _messages.Add($"{node.Kind} requires constant A and B inputs in Ray compatible mode.");
            return EvalResult.Empty;
        }

        var epsilon = ReadFloat(node, "Epsilon", 0.0001);
        return EvalResult.FromValue(ConstValue.Zip(a.Value, b.Value, (x, y) => operation switch
        {
            "LessThan" => x < y ? 1.0 : 0.0,
            "GreaterThan" => x > y ? 1.0 : 0.0,
            "LessEqual" => x <= y ? 1.0 : 0.0,
            "GreaterEqual" => x >= y ? 1.0 : 0.0,
            "Equal" => Math.Abs(x - y) < epsilon ? 1.0 : 0.0,
            "NotEqual" => Math.Abs(x - y) >= epsilon ? 1.0 : 0.0,
            _ => 0.0,
        }));
    }

    private EvalResult EvalRemap(GraphNode node)
    {
        var value = ResolveInput(node, "Value").Value;
        if (value is null)
        {
            _messages.Add("Remap requires a constant Value input in Ray compatible mode.");
            return EvalResult.Empty;
        }

        var inMin = ReadFloat(node, "InMin", 0.0);
        var inMax = ReadFloat(node, "InMax", 1.0);
        var outMin = ReadFloat(node, "OutMin", 0.0);
        var outMax = ReadFloat(node, "OutMax", 1.0);
        var shouldClamp = ReadText(node, "Clamp", "True").Equals("True", StringComparison.OrdinalIgnoreCase);
        return EvalResult.FromValue(value.Value.Map(component =>
        {
            var remapped = (component - inMin) / Math.Max(0.00001, inMax - inMin) * (outMax - outMin) + outMin;
            return shouldClamp ? Math.Clamp(remapped, Math.Min(outMin, outMax), Math.Max(outMin, outMax)) : remapped;
        }));
    }

    private EvalResult EvalSplit(GraphNode node, string outputPin)
    {
        var value = ResolveInput(node, node.Kind == NodeKind.SplitColor ? "Color" : "Value").Value;
        if (value is null)
        {
            _messages.Add($"{node.Kind} requires a constant input in Ray compatible mode.");
            return EvalResult.Empty;
        }

        var index = outputPin.ToUpperInvariant() switch
        {
            "Y" or "G" => 1,
            "Z" or "B" => 2,
            "W" or "A" => 3,
            _ => 0,
        };
        return EvalResult.FromValue(new ConstValue([value.Value.Component(index)]));
    }

    private EvalResult EvalCompose(GraphNode node)
    {
        return EvalResult.FromValue(new ConstValue([
            ResolveInput(node, "R").Value?.Components[0] ?? 0.0,
            ResolveInput(node, "G").Value?.Components[0] ?? 0.0,
            ResolveInput(node, "B").Value?.Components[0] ?? 0.0,
            ResolveInput(node, "A").Value?.Components[0] ?? 1.0,
        ]));
    }

    private EvalResult EvalAppendFloat2(GraphNode node)
    {
        return EvalResult.FromValue(new ConstValue([
            ResolveInput(node, "X").Value?.Component(0) ?? 0.0,
            ResolveInput(node, "Y").Value?.Component(0) ?? 0.0,
        ]));
    }

    private EvalResult EvalMerge(GraphNode node, int components)
    {
        var values = new[]
        {
            ResolveInput(node, "X").Value?.Component(0) ?? 0.0,
            ResolveInput(node, "Y").Value?.Component(0) ?? 0.0,
            ResolveInput(node, "Z").Value?.Component(0) ?? 0.0,
            ResolveInput(node, "W").Value?.Component(0) ?? 1.0,
        };
        return EvalResult.FromValue(new ConstValue(values.Take(components).ToArray()));
    }

    private EvalResult EvalComponentMask(GraphNode node)
    {
        var value = ResolveInput(node, "Value").Value;
        if (value is null)
        {
            _messages.Add("ComponentMask requires a constant Value input in Ray compatible mode.");
            return EvalResult.Empty;
        }

        var components = ReadText(node, "Channels", "RGBA")
            .ToUpperInvariant()
            .Where(ch => ch is 'R' or 'G' or 'B' or 'A' or 'X' or 'Y' or 'Z' or 'W')
            .Select(ch => ch switch
            {
                'G' or 'Y' => 1,
                'B' or 'Z' => 2,
                'A' or 'W' => 3,
                _ => 0,
            })
            .ToArray();
        if (components.Length == 0)
        {
            components = [0];
        }

        return EvalResult.FromValue(new ConstValue(components.Select(value.Value.Component).ToArray()));
    }

    private EvalResult EvalColorRamp(GraphNode node)
    {
        var a = ResolveInput(node, "A").Value ?? new ConstValue([
            ReadFloat(node, "StartR", 0.0),
            ReadFloat(node, "StartG", 0.0),
            ReadFloat(node, "StartB", 0.0),
            ReadFloat(node, "StartA", 1.0),
        ]);
        var b = ResolveInput(node, "B").Value ?? new ConstValue([
            ReadFloat(node, "EndR", 1.0),
            ReadFloat(node, "EndG", 1.0),
            ReadFloat(node, "EndB", 1.0),
            ReadFloat(node, "EndA", 1.0),
        ]);
        var tValue = ResolveInput(node, "T").Value?.Component(0) ?? 0.5;
        var start = ReadFloat(node, "Start", 0.0);
        var end = ReadFloat(node, "End", 1.0);
        var t = Math.Clamp((tValue - start) / Math.Max(0.00001, end - start), 0.0, 1.0);
        var mode = ReadText(node, "Mode", "Linear").Trim().ToLowerInvariant();
        var stopCount = Math.Clamp((int)Math.Round(ReadFloat(node, "StopCount", 2.0)), 2, 5);
        if (stopCount > 2)
        {
            return EvalResult.FromValue(EvaluateMultiStopRamp(node, stopCount, a, b, t, mode));
        }

        if (mode is "constant" or "step")
        {
            return EvalResult.FromValue(t < 1.0 ? a : b);
        }

        if (mode is "smooth" or "ease" or "easing")
        {
            t = t * t * (3.0 - 2.0 * t);
        }

        return EvalResult.FromValue(ConstValue.Zip(a, b, (x, y) => x + (y - x) * t));
    }

    private static ConstValue EvaluateMultiStopRamp(GraphNode node, int stopCount, ConstValue startColor, ConstValue endColor, double t, string mode)
    {
        var stops = new List<RampStopValue>
        {
            new(0.0, startColor),
        };
        if (stopCount >= 3)
        {
            stops.Add(new(ReadFloat(node, "Mid1", 0.25), ReadRampColor(node, "Mid1", 0.25)));
        }

        if (stopCount >= 4)
        {
            stops.Add(new(ReadFloat(node, "Mid2", 0.5), ReadRampColor(node, "Mid2", 0.5)));
        }

        if (stopCount >= 5)
        {
            stops.Add(new(ReadFloat(node, "Mid3", 0.75), ReadRampColor(node, "Mid3", 0.75)));
        }

        stops.Add(new(1.0, endColor));
        stops = stops
            .Select(stop => stop with { Position = Math.Clamp(stop.Position, 0.0, 1.0) })
            .OrderBy(stop => stop.Position)
            .ToList();

        if (mode is "constant" or "step")
        {
            var constant = stops[0].Color;
            foreach (var stop in stops.Skip(1))
            {
                if (t >= stop.Position)
                {
                    constant = stop.Color;
                }
            }

            return constant;
        }

        for (var index = 0; index < stops.Count - 1; index++)
        {
            var left = stops[index];
            var right = stops[index + 1];
            if (t > right.Position && index < stops.Count - 2)
            {
                continue;
            }

            var local = Math.Clamp((t - left.Position) / Math.Max(0.00001, right.Position - left.Position), 0.0, 1.0);
            if (mode is "smooth" or "ease" or "easing")
            {
                local = local * local * (3.0 - 2.0 * local);
            }

            return ConstValue.Zip(left.Color, right.Color, (x, y) => x + (y - x) * local);
        }

        return stops[^1].Color;
    }

    private static ConstValue ReadRampColor(GraphNode node, string prefix, double fallback)
    {
        return new ConstValue([
            ReadFloat(node, prefix + "R", fallback),
            ReadFloat(node, prefix + "G", fallback),
            ReadFloat(node, prefix + "B", fallback),
            ReadFloat(node, prefix + "A", 1.0),
        ]);
    }

    private EvalResult EvalRgbCurve(GraphNode node)
    {
        var color = ResolveInput(node, "Color").Value;
        if (color is null)
        {
            _messages.Add("RGB Curve requires a constant Color input in Ray compatible mode.");
            return EvalResult.Empty;
        }

        var inMin = ReadFloat(node, "InMin", 0.0);
        var gamma = ReadFloat(node, "Gamma", 1.0);
        var inMax = ReadFloat(node, "InMax", 1.0);
        var outMin = ReadFloat(node, "OutMin", 0.0);
        var outMax = ReadFloat(node, "OutMax", 1.0);
        var result = Enumerable.Range(0, Math.Max(4, color.Value.Components.Count))
            .Select(index =>
            {
                if (index == 3)
                {
                    return color.Value.Component(index);
                }

                var normalized = Math.Clamp((color.Value.Component(index) - inMin) / Math.Max(0.00001, inMax - inMin), 0.0, 1.0);
                return Math.Pow(Math.Max(0.0, normalized), 1.0 / Math.Max(0.00001, gamma)) * (outMax - outMin) + outMin;
            })
            .ToArray();
        return EvalResult.FromValue(new ConstValue(result));
    }

    private EvalResult EvalColorAdjust(GraphNode node)
    {
        var color = ResolveInput(node, "Color").Value;
        if (color is null)
        {
            _messages.Add("Color Adjust requires a constant Color input in Ray compatible mode.");
            return EvalResult.Empty;
        }

        var exposure = ReadFloat(node, "Exposure", 0.0);
        var temperature = ReadFloat(node, "Temperature", 0.0);
        var tint = ReadFloat(node, "Tint", 0.0);
        var contrast = ReadFloat(node, "Contrast", 1.0);
        var saturation = ReadFloat(node, "Saturation", 1.0);
        var shadowLift = ReadFloat(node, "ShadowLift", 0.0);
        var highlightCompress = Math.Clamp(ReadFloat(node, "HighlightCompress", 0.0), 0.0, 1.0);
        var lift = ReadFloat(node, "Lift", 0.0);
        var gamma = ReadFloat(node, "Gamma", 1.0);
        var gain = ReadFloat(node, "Gain", 1.0);
        var rgb = new[] { color.Value.Component(0), color.Value.Component(1), color.Value.Component(2) }
            .Select(component => component * Math.Pow(2.0, exposure))
            .ToArray();
        rgb[0] += temperature * 0.08 + tint * 0.02;
        rgb[1] += tint * 0.08;
        rgb[2] += -temperature * 0.08;
        var luma = rgb[0] * 0.2126 + rgb[1] * 0.7152 + rgb[2] * 0.0722;
        for (var index = 0; index < rgb.Length; index++)
        {
            rgb[index] = luma + (rgb[index] - luma) * saturation;
            rgb[index] = (rgb[index] - 0.5) * contrast + 0.5;
            rgb[index] += lift + shadowLift * (1.0 - Math.Clamp(luma, 0.0, 1.0));
            rgb[index] = Math.Pow(Math.Max(0.0, rgb[index]), 1.0 / Math.Max(0.00001, gamma)) * gain;
            rgb[index] = rgb[index] + (rgb[index] / (rgb[index] + 1.0) - rgb[index]) * highlightCompress;
        }

        return EvalResult.FromValue(new ConstValue([rgb[0], rgb[1], rgb[2], color.Value.Component(3)]));
    }

    private EvalResult Unsupported(GraphNode node)
    {
        _messages.Add($"{node.Kind} is not part of the Ray-MMD compatible node set.");
        return EvalResult.Empty;
    }

    private RayTextureBinding CreateTextureBinding(GraphNode node)
    {
        return new RayTextureBinding(
            ReadText(node, "Slot", "Albedo"),
            MapSource(ReadText(node, "Source", "File")),
            NormalizeFxPath(ReadText(node, "File", "albedo.png")),
            MapUvFlip(ReadText(node, "UvFlip", "0")),
            MapSwizzle(ReadText(node, "Swizzle", "R")),
            MapMapType(ReadText(node, "MapType", "Smoothness")),
            MapApplyScale(ReadText(node, "ApplyScale", "Auto")),
            ReadSwitch(node, "ColorFlip", defaultValue: false),
            new ConstValue([ReadFloat(node, "LoopX", 1.0), ReadFloat(node, "LoopY", 1.0)]));
    }

    private static string NormalizeFxPath(string path) => path.Replace('\\', '/');

    private static int MapSource(string source)
    {
        return source.Trim().ToLowerInvariant() switch
        {
            "none" or "0" => 0,
            "file" or "1" => 1,
            "animated" or "animation" or "2" => 2,
            "pmxtexture" or "pmx texture" or "texture" or "3" => 3,
            "pmxsphere" or "sphere" or "4" => 4,
            "pmxtoon" or "toon" or "5" => 5,
            "dummyscreen" or "screen" or "6" => 6,
            "pmxambient" or "ambient" or "7" => 7,
            "pmxspecular" or "specular" or "8" => 8,
            "pmxspecularpower" or "specularpower" or "9" => 9,
            _ => 1,
        };
    }

    private static int MapUvFlip(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "none" => 0,
            "x" => 1,
            "y" => 2,
            "xy" or "both" => 3,
            _ when int.TryParse(value, out var parsed) => Math.Clamp(parsed, 0, 3),
            _ => 0,
        };
    }

    private static int MapSwizzle(string value)
    {
        return value.Trim().ToUpperInvariant() switch
        {
            "R" => 0,
            "G" => 1,
            "B" => 2,
            "A" => 3,
            _ when int.TryParse(value, out var parsed) => Math.Clamp(parsed, 0, 3),
            _ => 0,
        };
    }

    private static int MapMapType(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "default" => 0,
            "roughness" or "roughnesssqrt" => 1,
            "linearroughness" or "roughnesslinear" => 2,
            "compressednormal" => 1,
            "bumplq" => 2,
            "bumphq" => 3,
            "worldnormal" => 4,
            "specularue4" => 1,
            "specularfrostbite" => 2,
            "speculargrayue4" => 3,
            "speculargrayfrostbite" => 4,
            "specularfixed04" => 5,
            "occlusionlinear" => 1,
            "occlusionuv2srgb" => 2,
            "occlusionuv2linear" => 3,
            "parallaxkeepalpha" => 1,
            _ when int.TryParse(value, out var parsed) => parsed,
            _ => 0,
        };
    }

    private static int MapApplyScale(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "on" or "true" or "1" => 1,
            "off" or "false" or "0" => 0,
            _ => -1,
        };
    }

    private static int MapSubAlbedoMode(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "multiply" or "mul" => 1,
            "power" or "pow" => 2,
            "add" => 3,
            "melanin" => 4,
            "alphablend" or "alpha blend" or "blend" => 5,
            _ => 0,
        };
    }

    private static int MapCustomMode(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "skin" => 1,
            "emissive" => 2,
            "anisotropy" => 3,
            "glass" => 4,
            "cloth" => 5,
            "clearcoat" or "clear coat" => 6,
            "subsurface" => 7,
            "cel" => 8,
            "tonebased" or "tone based" => 9,
            "mask" => 10,
            _ => 0,
        };
    }

    private static string ReadText(GraphNode node, string name, string fallback)
    {
        return node.Properties.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    }

    private static bool ReadBool(GraphNode node, string name)
    {
        return node.Properties.TryGetValue(name, out var value) && bool.TryParse(value, out var parsed) && parsed;
    }

    private static int ReadSwitch(GraphNode node, string name, bool defaultValue)
    {
        if (!node.Properties.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return defaultValue ? 1 : 0;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "on" or "true" or "1" or "yes" => 1,
            "off" or "false" or "0" or "no" => 0,
            _ => defaultValue ? 1 : 0,
        };
    }

    private static double ReadFloat(GraphNode node, string name, double fallback)
    {
        return node.Properties.TryGetValue(name, out var text) &&
               double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }

    private static string FormatScalar(double value) => value.ToString("0.######", CultureInfo.InvariantCulture);

    private static string FormatVector(IReadOnlyList<double> values, int components)
    {
        if (components == 1)
        {
            return FormatScalar(values.Count > 0 ? values[0] : 0.0);
        }

        var parts = Enumerable.Range(0, components)
            .Select(index => FormatScalar(index < values.Count ? values[index] : values.Count > 0 ? values[0] : 0.0));
        return $"float{components}({string.Join(", ", parts)})";
    }

    private readonly record struct EvalResult(ConstValue? Value, RayTextureBinding? Texture, ConstValue? Scale)
    {
        public static readonly EvalResult Empty = new(null, null, null);
        public bool HasValue => Value is not null || Texture is not null;
        public static EvalResult FromValue(ConstValue value) => new(value, null, null);
        public static EvalResult FromTexture(RayTextureBinding texture, ConstValue? scale = null) => new(null, texture, scale);
    }

    private readonly record struct ConstValue(IReadOnlyList<double> Components)
    {
        public double Component(int index) => index < Components.Count ? Components[index] : Components.Count > 0 ? Components[0] : 0.0;

        public ConstValue Map(Func<double, double> op)
        {
            return new ConstValue(Components.Select(op).ToArray());
        }

        public static ConstValue Zip(ConstValue a, ConstValue b, Func<double, double, double> op)
        {
            var count = Math.Max(a.Components.Count, b.Components.Count);
            var result = Enumerable.Range(0, count)
                .Select(index => op(a.Component(index), b.Component(index)))
                .ToArray();
            return new ConstValue(result);
        }
    }

    private readonly record struct RampStopValue(double Position, ConstValue Color);

    private sealed record RayTextureBinding(
        string Slot,
        int MapFrom,
        string File,
        int UvFlip,
        int Swizzle,
        int MapType,
        int ApplyScaleOverride,
        int ColorFlip,
        ConstValue Loop);

    private sealed class RayMaterialParameters
    {
        public RayMaterialSlot Albedo { get; } = new("ALBEDO", "albedo", 3, 3, "1.0") { MapFrom = 3, ApplyDiffuse = 1 };
        public RayMaterialSlot SubAlbedo { get; } = new("ALBEDO_SUB", "albedoSub", 3, 3, "1.0") { Enable = 0 };
        public RayMaterialSlot Alpha { get; } = new("ALPHA", "alpha", 1, 1, "1.0") { MapFrom = 3, Swizzle = 3 };
        public RayMaterialSlot Normal { get; } = new("NORMAL", "normalMapScale", 1, 1, "1.0") { MapFrom = 0 };
        public RayMaterialSlot SubNormal { get; } = new("NORMAL_SUB", "normalSubMapScale", 1, 1, "1.0") { MapFrom = 0 };
        public RayMaterialSlot Smoothness { get; } = new("SMOOTHNESS", "smoothness", 1, 1, "0.0") { MapFrom = 9 };
        public RayMaterialSlot Metalness { get; } = new("METALNESS", "metalness", 1, 1, "0.0") { MapFrom = 0 };
        public RayMaterialSlot Specular { get; } = new("SPECULAR", "specular", 3, 3, "0.5") { MapFrom = 0 };
        public RayMaterialSlot Occlusion { get; } = new("OCCLUSION", "occlusion", 1, 1, "1.0") { MapFrom = 0 };
        public RayMaterialSlot Parallax { get; } = new("PARALLAX", "parallaxMapScale", 1, 1, "1.0") { MapFrom = 0 };
        public RayMaterialSlot Emissive { get; } = new("EMISSIVE", "emissive", 3, 3, "1.0") { MapFrom = 0 };
        public RayMaterialSlot CustomA { get; } = new("CUSTOM_A", "customA", 1, 1, "0.0") { MapFrom = 0 };
        public RayMaterialSlot CustomB { get; } = new("CUSTOM_B", "customB", 3, 3, "0.0") { MapFrom = 0 };
        public int EmissiveEnable { get; set; }
        public string EmissiveBlinkExpression { get; set; } = "float3(1, 1, 1)";
        public string EmissiveIntensityExpression { get; set; } = "1.0";
        public int CustomEnable { get; set; }

        public static RayMaterialParameters CreateDefaults() => new();

        public string ToFxText()
        {
            var builder = new StringBuilder();
            builder.AppendLine("// Ray-MMD material_2.0.fx generated by RayMmdNodeEditor.");
            builder.AppendLine("// 所有参数只写入导出副本；原始 ray-mmd 不会被覆盖。");
            builder.AppendLine();
            AppendSlot(builder, Albedo, "主颜色 Albedo：基础颜色贴图/常量。", includeApplyScale: true, includeApplyDiffuse: true, includeMorphColor: true);
            AppendSlot(builder, SubAlbedo, "副颜色 SubAlbedo：额外颜色层，混合模式由 ALBEDO_SUB_ENABLE 控制。", includeEnable: true, includeApplyScale: true);
            AppendSlot(builder, Alpha, "透明度 Alpha：控制材质透明度，SWIZZLE 选择贴图通道。", includeSwizzle: true);
            AppendSlot(builder, Normal, "法线 Normal：法线或 bump 贴图。MAP_TYPE 选择法线解码方式。", includeType: true);
            AppendSlot(builder, SubNormal, "副法线 SubNormal：第二层法线/细节法线。", includeType: true);
            AppendSlot(builder, Smoothness, "光滑度 Smoothness：Ray 使用 smoothness；roughness 贴图可通过 MAP_TYPE 转换。", includeType: true, includeSwizzle: true, includeApplyScale: true);
            AppendSlot(builder, Metalness, "金属度 Metalness：0 为非金属，1 为金属。", includeSwizzle: true, includeApplyScale: true);
            AppendSlot(builder, Specular, "高光 Specular：高光颜色或高光贴图类型。", includeType: true, includeSwizzle: true, includeApplyScale: true);
            AppendSlot(builder, Occlusion, "遮蔽 Occlusion：环境遮蔽强度。", includeType: true, includeSwizzle: true, includeApplyScale: true);
            AppendSlot(builder, Parallax, "视差 Parallax：高度贴图/视差强度。", includeType: true, includeSwizzle: true);
            builder.AppendLine("// 自发光总开关：0 禁用，1 启用。");
            builder.AppendLine($"#define EMISSIVE_ENABLE {EmissiveEnable}");
            AppendSlot(builder, Emissive, "自发光 Emissive：自发光颜色、贴图、Morph 和眨眼控制。", prefixAlreadyWritten: true, includeApplyScale: true, includeMorphColor: true, includeEmissiveOptions: true);
            builder.AppendLine("// 自发光眨眼颜色倍率；EMISSIVE_MAP_APPLY_BLINK 开启后参与计算。");
            builder.AppendLine($"const float3 emissiveBlink = {EmissiveBlinkExpression};");
            builder.AppendLine("// 自发光整体强度。");
            builder.AppendLine($"const float  emissiveIntensity = {EmissiveIntensityExpression};");
            builder.AppendLine();
            builder.AppendLine("// Custom 材质模式：0 默认，1 Skin，2 Emissive，3 Anisotropy，4 Glass，5 Cloth，6 ClearCoat，7 Subsurface，8 Cel，9 ToneBased，10 Mask。");
            builder.AppendLine($"#define CUSTOM_ENABLE {CustomEnable}");
            builder.AppendLine();
            AppendSlot(builder, CustomA, "Custom A：Ray custom 模式使用的第一个自定义通道。", prefixAlreadyWritten: true, includeSwizzle: true, includeApplyScale: true, includeColorFlip: true);
            AppendSlot(builder, CustomB, "Custom B：Ray custom 模式使用的第二个自定义颜色。", prefixAlreadyWritten: true, includeApplyScale: true, includeColorFlip: true);
            builder.AppendLine("#include \"material_common_2.0.fxsub\"");
            return builder.ToString();
        }

        private static void AppendSlot(
            StringBuilder builder,
            RayMaterialSlot slot,
            string comment,
            bool prefixAlreadyWritten = false,
            bool includeEnable = false,
            bool includeType = false,
            bool includeSwizzle = false,
            bool includeApplyScale = false,
            bool includeApplyDiffuse = false,
            bool includeMorphColor = false,
            bool includeEmissiveOptions = false,
            bool includeColorFlip = false)
        {
            builder.AppendLine($"// {comment}");
            builder.AppendLine("// MAP_FROM: 0 无贴图，1 文件，2 动画贴图，3 PMX 主贴图，4 Sphere，5 Toon，6 DummyScreen，7 Ambient，8 Specular，9 SpecularPower。");
            if (includeEnable)
            {
                builder.AppendLine("// ENABLE: 0 禁用；SubAlbedo 使用 1 Multiply，2 Power，3 Add，4 Melanin，5 AlphaBlend。");
                builder.AppendLine($"#define {slot.Prefix}_ENABLE {slot.Enable}");
            }

            if (!prefixAlreadyWritten)
            {
                builder.AppendLine($"#define {slot.Prefix}_MAP_FROM {slot.MapFrom}");
            }
            else
            {
                builder.AppendLine($"#define {slot.Prefix}_MAP_FROM {slot.MapFrom}");
            }

            if (includeType)
            {
                builder.AppendLine("// MAP_TYPE: 该数值随槽位不同而不同；请参考节点中的贴图类型下拉。");
                builder.AppendLine($"#define {slot.Prefix}_MAP_TYPE {slot.MapType}");
            }

            builder.AppendLine("// MAP_UV_FLIP: 0 不翻转，1 X，2 Y，3 XY。");
            builder.AppendLine($"#define {slot.Prefix}_MAP_UV_FLIP {slot.UvFlip}");
            if (includeColorFlip)
            {
                builder.AppendLine("// MAP_COLOR_FLIP: 1 时反转 custom 贴图颜色。");
                builder.AppendLine($"#define {slot.Prefix}_MAP_COLOR_FLIP {slot.ColorFlip}");
            }

            if (includeSwizzle)
            {
                builder.AppendLine("// MAP_SWIZZLE: 0 R，1 G，2 B，3 A。");
                builder.AppendLine($"#define {slot.Prefix}_MAP_SWIZZLE {slot.Swizzle}");
            }

            if (includeApplyScale || slot.ApplyScale != 0)
            {
                builder.AppendLine("// MAP_APPLY_SCALE: 1 时将贴图结果乘以下方 const 参数。");
                builder.AppendLine($"#define {slot.Prefix}_MAP_APPLY_SCALE {slot.ApplyScale}");
            }

            if (includeApplyDiffuse)
            {
                builder.AppendLine("// MAP_APPLY_DIFFUSE: 1 时主颜色继续乘 PMX 材质 diffuse。");
                builder.AppendLine($"#define {slot.Prefix}_MAP_APPLY_DIFFUSE {slot.ApplyDiffuse}");
            }

            if (includeMorphColor)
            {
                builder.AppendLine("// MAP_APPLY_MORPH_COLOR: 1 时受 MMD 材质 Morph 颜色影响。");
                builder.AppendLine($"#define {slot.Prefix}_MAP_APPLY_MORPH_COLOR {slot.MorphColor}");
            }

            if (includeEmissiveOptions)
            {
                builder.AppendLine("// EMISSIVE_MAP_APPLY_MORPH_INTENSITY: 1 时自发光强度受 Morph 影响。");
                builder.AppendLine($"#define EMISSIVE_MAP_APPLY_MORPH_INTENSITY {slot.EmissiveMorphIntensity}");
                builder.AppendLine("// EMISSIVE_MAP_APPLY_BLINK: 1 时启用 emissiveBlink 眨眼倍率。");
                builder.AppendLine($"#define EMISSIVE_MAP_APPLY_BLINK {slot.EmissiveBlink}");
            }

            builder.AppendLine("// MAP_FILE: MAP_FROM=1 时使用的贴图路径。");
            builder.AppendLine($"#define {slot.Prefix}_MAP_FILE \"{slot.File}\"");
            builder.AppendLine();
            builder.AppendLine("// const 参数：没有贴图时作为固定值；贴图 Apply Scale 开启时作为倍率。");
            builder.AppendLine($"const {(slot.Components == 1 ? "float" : $"float{slot.Components}")} {slot.ConstName} = {slot.ConstExpression};");
            if (slot.Prefix is "NORMAL" or "NORMAL_SUB" or "ALPHA" or "SMOOTHNESS" or "METALNESS" or "OCCLUSION" or "PARALLAX" or "CUSTOM_A")
            {
                builder.AppendLine($"const float {slot.LoopName} = {slot.LoopExpression};");
            }
            else
            {
                builder.AppendLine($"const float2 {slot.LoopName} = {slot.LoopExpression};");
            }
            builder.AppendLine();
        }
    }

    private sealed class RayMaterialSlot
    {
        public RayMaterialSlot(string prefix, string constName, int components, int defaultComponents, string constExpression)
        {
            Prefix = prefix;
            ConstName = constName;
            Components = components;
            ConstExpression = constExpression;
            File = $"{constName}.png";
        }

        public string Prefix { get; }
        public string ConstName { get; }
        public int Components { get; }
        public int Enable { get; set; }
        public int MapFrom { get; set; }
        public int MapType { get; set; }
        public int UvFlip { get; set; }
        public int Swizzle { get; set; }
        public int ApplyScale { get; set; }
        public int ApplyDiffuse { get; set; }
        public int MorphColor { get; set; }
        public int ColorFlip { get; set; }
        public int EmissiveMorphIntensity { get; set; }
        public int EmissiveBlink { get; set; }
        public string File { get; set; }
        public string ConstExpression { get; set; }
        public string LoopExpression { get; set; } = "1.0";
        public string LoopName => ConstName switch
        {
            "normalMapScale" => "normalMapLoopNum",
            "normalSubMapScale" => "normalSubMapLoopNum",
            "parallaxMapScale" => "parallaxMapLoopNum",
            "customA" => "customAMapLoopNum",
            "customB" => "customBMapLoopNum",
            _ => $"{ConstName}MapLoopNum",
        };

        public void ApplyTexture(RayTextureBinding texture)
        {
            MapFrom = texture.MapFrom;
            File = texture.File;
            UvFlip = texture.UvFlip;
            Swizzle = texture.Swizzle;
            MapType = texture.MapType;
            ColorFlip = texture.ColorFlip;
            if (texture.ApplyScaleOverride >= 0)
            {
                ApplyScale = texture.ApplyScaleOverride;
            }

            LoopExpression = texture.Loop.Components.Count >= 2
                ? $"float2({FormatScalar(texture.Loop.Components[0])}, {FormatScalar(texture.Loop.Components[1])})"
                : FormatScalar(texture.Loop.Components[0]);
        }
    }
}
