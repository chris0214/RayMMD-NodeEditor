namespace RayMmdNodeEditor.Graph;

public sealed partial class NodeGraph
{
    private static NodeGraph CreateColorSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var color = graph.AddNode(NodeKind.Color, 120, 120);
        color.Properties["R"] = "1.0";
        color.Properties["G"] = "0.0";
        color.Properties["B"] = "0.0";
        color.Properties["A"] = "1.0";

        var output = graph.AddNode(NodeKind.Output, 430, 120);
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = color.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateExternalTextureSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 96);
        var externalTexture = graph.AddNode(NodeKind.ExternalTexture, 360, 88);
        externalTexture.Properties["ResourceName"] = "laughing_man.png";
        externalTexture.Properties["TextureMode"] = "Static";
        externalTexture.Properties["AddressMode"] = "Clamp";
        externalTexture.Properties["FilterMode"] = "Linear";

        var output = graph.AddNode(NodeKind.Output, 700, 120);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = externalTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = externalTexture.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateParallaxSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);

        var heightSample = graph.AddNode(NodeKind.MaterialTexture, 360, 60);
        heightSample.Properties["ColorSpace"] = "Linear";

        var viewDirection = graph.AddNode(NodeKind.ViewDirection, 360, 220);

        var parallaxUv = graph.AddNode(NodeKind.ParallaxUv, 660, 140);
        parallaxUv.Properties["Scale"] = "0.12";
        parallaxUv.Properties["Center"] = "0.5";

        var colorSample = graph.AddNode(NodeKind.MaterialTexture, 940, 120);
        var output = graph.AddNode(NodeKind.Output, 1220, 120);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = heightSample.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = parallaxUv.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = heightSample.Id,
            SourcePin = "A",
            TargetNodeId = parallaxUv.Id,
            TargetPin = "Height",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = viewDirection.Id,
            SourcePin = "Value",
            TargetNodeId = parallaxUv.Id,
            TargetPin = "ViewDir",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = parallaxUv.Id,
            SourcePin = "UV",
            TargetNodeId = colorSample.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = colorSample.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateTriplanarSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 120, 80);
        var triplanar = graph.AddNode(NodeKind.TriplanarBoxmap, 120, 120);
        triplanar.Properties["TextureSource"] = "Material";
        triplanar.Properties["Scale"] = "1.0";
        triplanar.Properties["BlendSharpness"] = "4.0";

        var output = graph.AddNode(NodeKind.Output, 520, 140);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = triplanar.Id,
            TargetPin = "Texture",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = triplanar.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateMaterialUtilitySampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 40, 80);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 260, 60);
        var baseNormal = graph.AddNode(NodeKind.NormalMap, 260, 260);
        baseNormal.Properties["ResourceName"] = "normal.png";
        var detailNormal = graph.AddNode(NodeKind.NormalMap, 260, 420);
        detailNormal.Properties["ResourceName"] = "detail_normal.png";
        detailNormal.Properties["Strength"] = "0.5";

        var blendStrength = graph.AddNode(NodeKind.Scalar, 520, 520);
        blendStrength.Properties["Value"] = "0.5";
        var detailBlend = graph.AddNode(NodeKind.DetailNormalBlend, 580, 320);
        detailBlend.Properties["Strength"] = "1.0";

        var lighting = graph.AddNode(NodeKind.BasicLighting, 900, 140);
        lighting.Properties["UseSpecular"] = "1.0";
        lighting.Properties["UseToon"] = "0.0";

        var worldPosition = graph.AddNode(NodeKind.WorldPosition, 900, 420);
        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 900, 560);

        var boxMask = graph.AddNode(NodeKind.BoxMask, 1180, 340);
        boxMask.Properties["SizeX"] = "4.0";
        boxMask.Properties["SizeY"] = "4.0";
        boxMask.Properties["SizeZ"] = "4.0";
        boxMask.Properties["Falloff"] = "0.5";

        var sphereMask = graph.AddNode(NodeKind.SphereMask, 1180, 500);
        sphereMask.Properties["Radius"] = "2.5";
        sphereMask.Properties["Falloff"] = "0.6";

        var slopeMask = graph.AddNode(NodeKind.SlopeMask, 1180, 660);
        slopeMask.Properties["Axis"] = "Y";
        slopeMask.Properties["Sharpness"] = "2.0";

        var maskMultiply = graph.AddNode(NodeKind.Multiply, 1460, 500);
        maskMultiply.Properties["Type"] = "Float1";
        var maskMultiply2 = graph.AddNode(NodeKind.Multiply, 1720, 560);
        maskMultiply2.Properties["Type"] = "Float1";

        var clearCoat = graph.AddNode(NodeKind.ClearCoat, 1460, 120);
        clearCoat.Properties["Strength"] = "1.0";
        clearCoat.Properties["Gloss"] = "0.85";

        var wetness = graph.AddNode(NodeKind.Wetness, 1740, 160);
        wetness.Properties["Wetness"] = "0.8";
        wetness.Properties["Porosity"] = "0.8";

        var output = graph.AddNode(NodeKind.Output, 2300, 260);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = materialTexture.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = baseNormal.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = detailNormal.Id, TargetPin = "UV" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = baseNormal.Id, SourcePin = "Normal", TargetNodeId = detailBlend.Id, TargetPin = "BaseNormal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = detailNormal.Id, SourcePin = "Normal", TargetNodeId = detailBlend.Id, TargetPin = "DetailNormal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = worldNormal.Id, SourcePin = "Value", TargetNodeId = detailBlend.Id, TargetPin = "ReferenceNormal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = blendStrength.Id, SourcePin = "Value", TargetNodeId = detailBlend.Id, TargetPin = "Strength" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = materialTexture.Id, SourcePin = "Color", TargetNodeId = lighting.Id, TargetPin = "Albedo" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = worldPosition.Id, SourcePin = "Value", TargetNodeId = boxMask.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = worldPosition.Id, SourcePin = "Value", TargetNodeId = sphereMask.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = worldNormal.Id, SourcePin = "Value", TargetNodeId = slopeMask.Id, TargetPin = "Normal" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = boxMask.Id, SourcePin = "Result", TargetNodeId = maskMultiply.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = sphereMask.Id, SourcePin = "Result", TargetNodeId = maskMultiply.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = maskMultiply.Id, SourcePin = "Result", TargetNodeId = maskMultiply2.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = slopeMask.Id, SourcePin = "Result", TargetNodeId = maskMultiply2.Id, TargetPin = "B" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = lighting.Id, SourcePin = "Color", TargetNodeId = clearCoat.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = maskMultiply2.Id, SourcePin = "Result", TargetNodeId = clearCoat.Id, TargetPin = "Mask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = detailBlend.Id, SourcePin = "Normal", TargetNodeId = clearCoat.Id, TargetPin = "Normal" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = clearCoat.Id, SourcePin = "Color", TargetNodeId = wetness.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = maskMultiply2.Id, SourcePin = "Result", TargetNodeId = wetness.Id, TargetPin = "Wetness" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = maskMultiply2.Id, SourcePin = "Result", TargetNodeId = wetness.Id, TargetPin = "Porosity" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = detailBlend.Id, SourcePin = "Normal", TargetNodeId = wetness.Id, TargetPin = "Normal" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = wetness.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });

        return graph;
    }

    private static NodeGraph CreateProceduralNoiseSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 60, 160);
        var warp = graph.AddNode(NodeKind.DomainWarp, 300, 160);
        warp.Properties["NoiseType"] = "Simplex";
        warp.Properties["Dimensions"] = "2D";
        warp.Properties["Scale"] = "3.5";
        warp.Properties["Strength"] = "0.18";

        var curl = graph.AddNode(NodeKind.CurlNoise, 580, 140);
        curl.Properties["NoiseType"] = "Simplex";
        curl.Properties["Scale"] = "5.0";
        curl.Properties["Strength"] = "0.12";
        curl.Properties["Epsilon"] = "0.05";

        var curlScale = graph.AddNode(NodeKind.Float4Value, 580, 320);
        curlScale.Properties["X"] = "0.22";
        curlScale.Properties["Y"] = "0.22";
        curlScale.Properties["Z"] = "0.0";
        curlScale.Properties["W"] = "0.0";

        var curlMultiply = graph.AddNode(NodeKind.Multiply, 860, 220);
        curlMultiply.Properties["Type"] = "Float4";

        var coordAdd = graph.AddNode(NodeKind.Add, 1140, 180);
        coordAdd.Properties["Type"] = "Float4";

        var direction = graph.AddNode(NodeKind.Float4Value, 1140, 520);
        direction.Properties["X"] = "1.0";
        direction.Properties["Y"] = "0.35";
        direction.Properties["Z"] = "0.0";
        direction.Properties["W"] = "0.0";

        var noise = graph.AddNode(NodeKind.NoiseTexture, 1420, 60);
        noise.Properties["NoiseType"] = "Simplex";
        noise.Properties["Dimensions"] = "2D";
        noise.Properties["Scale"] = "6.0";

        var musgrave = graph.AddNode(NodeKind.MusgraveTexture, 1420, 220);
        musgrave.Properties["NoiseType"] = "Perlin";
        musgrave.Properties["Dimensions"] = "2D";
        musgrave.Properties["MusgraveType"] = "Ridged";
        musgrave.Properties["Scale"] = "5.2";
        musgrave.Properties["Octaves"] = "4.0";
        musgrave.Properties["Gain"] = "0.55";
        musgrave.Properties["Offset"] = "0.8";

        var voronoi = graph.AddNode(NodeKind.VoronoiTexture, 1420, 380);
        voronoi.Properties["Dimensions"] = "2D";
        voronoi.Properties["Scale"] = "10.0";
        voronoi.Properties["Jitter"] = "1.0";

        var cellEdge = graph.AddNode(NodeKind.CellEdgeTexture, 1420, 540);
        cellEdge.Properties["Dimensions"] = "2D";
        cellEdge.Properties["Scale"] = "11.0";
        cellEdge.Properties["EdgeWidth"] = "0.16";
        cellEdge.Properties["EdgeSoftness"] = "0.09";

        var aniso = graph.AddNode(NodeKind.AnisotropicNoise, 1420, 700);
        aniso.Properties["NoiseType"] = "Perlin";
        aniso.Properties["Dimensions"] = "2D";
        aniso.Properties["Scale"] = "4.2";
        aniso.Properties["Octaves"] = "4.0";
        aniso.Properties["Gain"] = "0.55";
        aniso.Properties["Anisotropy"] = "4.0";

        var multiply = graph.AddNode(NodeKind.Multiply, 1760, 140);
        multiply.Properties["Type"] = "Float1";
        var multiply2 = graph.AddNode(NodeKind.Multiply, 2020, 260);
        multiply2.Properties["Type"] = "Float1";
        var multiply3 = graph.AddNode(NodeKind.Multiply, 2280, 380);
        multiply3.Properties["Type"] = "Float1";
        var multiply4 = graph.AddNode(NodeKind.Multiply, 2540, 500);
        multiply4.Properties["Type"] = "Float1";

        var dark = graph.AddNode(NodeKind.Color, 2820, 220);
        dark.Properties["R"] = "0.12";
        dark.Properties["G"] = "0.14";
        dark.Properties["B"] = "0.17";
        var light = graph.AddNode(NodeKind.Color, 2820, 420);
        light.Properties["R"] = "0.78";
        light.Properties["G"] = "0.84";
        light.Properties["B"] = "0.88";

        var ramp = graph.AddNode(NodeKind.ColorRamp, 3100, 300);
        ramp.Properties["Start"] = "0.08";
        ramp.Properties["End"] = "0.92";

        var output = graph.AddNode(NodeKind.Output, 3420, 320);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = warp.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = warp.Id, SourcePin = "Coord", TargetNodeId = curl.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = curl.Id, SourcePin = "Vector", TargetNodeId = curlMultiply.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = curlScale.Id, SourcePin = "Value", TargetNodeId = curlMultiply.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = warp.Id, SourcePin = "Coord", TargetNodeId = coordAdd.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = curlMultiply.Id, SourcePin = "Result", TargetNodeId = coordAdd.Id, TargetPin = "B" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = coordAdd.Id, SourcePin = "Result", TargetNodeId = noise.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = coordAdd.Id, SourcePin = "Result", TargetNodeId = musgrave.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = coordAdd.Id, SourcePin = "Result", TargetNodeId = voronoi.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = coordAdd.Id, SourcePin = "Result", TargetNodeId = cellEdge.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = coordAdd.Id, SourcePin = "Result", TargetNodeId = aniso.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = direction.Id, SourcePin = "Value", TargetNodeId = aniso.Id, TargetPin = "Direction" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noise.Id, SourcePin = "Factor", TargetNodeId = multiply.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = musgrave.Id, SourcePin = "Factor", TargetNodeId = multiply.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = multiply.Id, SourcePin = "Result", TargetNodeId = multiply2.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = voronoi.Id, SourcePin = "Factor", TargetNodeId = multiply2.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = multiply2.Id, SourcePin = "Result", TargetNodeId = multiply3.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = cellEdge.Id, SourcePin = "Factor", TargetNodeId = multiply3.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = multiply3.Id, SourcePin = "Result", TargetNodeId = multiply4.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = aniso.Id, SourcePin = "Factor", TargetNodeId = multiply4.Id, TargetPin = "B" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = dark.Id, SourcePin = "Color", TargetNodeId = ramp.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = light.Id, SourcePin = "Color", TargetNodeId = ramp.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = multiply4.Id, SourcePin = "Result", TargetNodeId = ramp.Id, TargetPin = "T" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = ramp.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });

        return graph;
    }

    private static NodeGraph CreatePatternTextureSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 60, 180);

        var gradient = graph.AddNode(NodeKind.GradientTexture, 340, 60);
        gradient.Properties["GradientType"] = "Radial";
        gradient.Properties["Scale"] = "1.6";

        var checker = graph.AddNode(NodeKind.CheckerTexture, 340, 220);
        checker.Properties["Scale"] = "11.0";

        var brick = graph.AddNode(NodeKind.BrickTexture, 340, 380);
        brick.Properties["Scale"] = "7.0";
        brick.Properties["Offset"] = "0.5";

        var wave = graph.AddNode(NodeKind.WaveTexture, 340, 540);
        wave.Properties["WaveType"] = "Bands";
        wave.Properties["WaveProfile"] = "Triangle";
        wave.Properties["Scale"] = "8.0";
        wave.Properties["Distortion"] = "1.4";

        var multiply = graph.AddNode(NodeKind.Multiply, 700, 140);
        multiply.Properties["Type"] = "Float1";
        var multiply2 = graph.AddNode(NodeKind.Multiply, 980, 260);
        multiply2.Properties["Type"] = "Float1";
        var multiply3 = graph.AddNode(NodeKind.Multiply, 1260, 380);
        multiply3.Properties["Type"] = "Float1";

        var dark = graph.AddNode(NodeKind.Color, 1540, 220);
        dark.Properties["R"] = "0.21";
        dark.Properties["G"] = "0.12";
        dark.Properties["B"] = "0.08";
        var light = graph.AddNode(NodeKind.Color, 1540, 420);
        light.Properties["R"] = "0.92";
        light.Properties["G"] = "0.82";
        light.Properties["B"] = "0.66";

        var ramp = graph.AddNode(NodeKind.ColorRamp, 1820, 300);
        ramp.Properties["Start"] = "0.1";
        ramp.Properties["End"] = "0.9";

        var output = graph.AddNode(NodeKind.Output, 2140, 320);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = gradient.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = checker.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = brick.Id, TargetPin = "Coord" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = wave.Id, TargetPin = "Coord" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = gradient.Id, SourcePin = "Factor", TargetNodeId = multiply.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = checker.Id, SourcePin = "Factor", TargetNodeId = multiply.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = multiply.Id, SourcePin = "Result", TargetNodeId = multiply2.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = brick.Id, SourcePin = "Factor", TargetNodeId = multiply2.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = multiply2.Id, SourcePin = "Result", TargetNodeId = multiply3.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = wave.Id, SourcePin = "Factor", TargetNodeId = multiply3.Id, TargetPin = "B" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = dark.Id, SourcePin = "Color", TargetNodeId = ramp.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = light.Id, SourcePin = "Color", TargetNodeId = ramp.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = multiply3.Id, SourcePin = "Result", TargetNodeId = ramp.Id, TargetPin = "T" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = ramp.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });

        return graph;
    }

    private static NodeGraph CreateNormalMapSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 80);
        var normalMap = graph.AddNode(NodeKind.NormalMap, 360, 280);
        normalMap.Properties["ResourceName"] = "normal.png";
        normalMap.Properties["Strength"] = "1.0";
        normalMap.Properties["XChannel"] = "R";
        normalMap.Properties["YChannel"] = "G";
        normalMap.Properties["InvertY"] = "False";

        var lambert = graph.AddNode(NodeKind.Lambert, 680, 220);
        var multiply = graph.AddNode(NodeKind.Multiply, 960, 120);
        multiply.Properties["Type"] = "Float3";
        var output = graph.AddNode(NodeKind.Output, 1240, 120);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = normalMap.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = normalMap.Id,
            SourcePin = "Normal",
            TargetNodeId = lambert.Id,
            TargetPin = "Normal",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "RGB",
            TargetNodeId = multiply.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = lambert.Id,
            SourcePin = "Result",
            TargetNodeId = multiply.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = multiply.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateVertexNormalOffsetSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 120);
        materialTexture.Properties["ColorSpace"] = "Color";

        var localNormal = graph.AddNode(NodeKind.LocalNormal, 360, 340);
        var amount = graph.AddNode(NodeKind.Scalar, 660, 420);
        amount.Properties["Value"] = "0.025";
        var offset = graph.AddNode(NodeKind.OffsetAlongNormal, 960, 340);
        var output = graph.AddNode(NodeKind.Output, 1260, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = localNormal.Id,
            SourcePin = "Value",
            TargetNodeId = offset.Id,
            TargetPin = "Normal",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = amount.Id,
            SourcePin = "Value",
            TargetNodeId = offset.Id,
            TargetPin = "Amount",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = offset.Id,
            SourcePin = "Offset",
            TargetNodeId = output.Id,
            TargetPin = "VertexOffset",
        });

        return graph;
    }

    private static NodeGraph CreateVertexWaveSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 120);
        materialTexture.Properties["ColorSpace"] = "Color";

        var localPosition = graph.AddNode(NodeKind.LocalPosition, 360, 320);
        var localNormal = graph.AddNode(NodeKind.LocalNormal, 360, 460);
        var wave = graph.AddNode(NodeKind.VertexWave, 720, 380);
        wave.Properties["Amplitude"] = "0.03";
        wave.Properties["Frequency"] = "8.0";
        wave.Properties["Speed"] = "2.0";
        wave.Properties["Phase"] = "0.0";
        wave.Properties["AxisX"] = "0.0";
        wave.Properties["AxisY"] = "1.0";
        wave.Properties["AxisZ"] = "0.0";

        var output = graph.AddNode(NodeKind.Output, 1080, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = localPosition.Id,
            SourcePin = "Value",
            TargetNodeId = wave.Id,
            TargetPin = "Position",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = localNormal.Id,
            SourcePin = "Value",
            TargetNodeId = wave.Id,
            TargetPin = "Normal",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = wave.Id,
            SourcePin = "Offset",
            TargetNodeId = output.Id,
            TargetPin = "VertexOffset",
        });

        return graph;
    }

    private static NodeGraph CreateControllerVertexDisplacementSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 120);
        materialTexture.Properties["ColorSpace"] = "Color";

        var worldPosition = graph.AddNode(NodeKind.WorldPosition, 80, 380);
        var controlPosition = graph.AddNode(NodeKind.ControlObjectPosition, 360, 340);
        controlPosition.Properties["Name"] = "Target.x";
        controlPosition.Properties["Item"] = "XYZ";
        var subtract = graph.AddNode(NodeKind.Subtract, 680, 340);
        subtract.Properties["Type"] = "Float4";
        var normalize = graph.AddNode(NodeKind.Normalize, 980, 340);
        var amount = graph.AddNode(NodeKind.Scalar, 980, 500);
        amount.Properties["Value"] = "0.05";
        var multiply = graph.AddNode(NodeKind.Multiply, 1260, 380);
        multiply.Properties["Type"] = "Float4";

        var output = graph.AddNode(NodeKind.Output, 1560, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = texCoord.Id,
            SourcePin = "UV",
            TargetNodeId = materialTexture.Id,
            TargetPin = "UV",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = materialTexture.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = controlPosition.Id,
            SourcePin = "Value",
            TargetNodeId = subtract.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = worldPosition.Id,
            SourcePin = "Value",
            TargetNodeId = subtract.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = subtract.Id,
            SourcePin = "Result",
            TargetNodeId = normalize.Id,
            TargetPin = "Value",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = normalize.Id,
            SourcePin = "Result",
            TargetNodeId = multiply.Id,
            TargetPin = "A",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = amount.Id,
            SourcePin = "Value",
            TargetNodeId = multiply.Id,
            TargetPin = "B",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = multiply.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "VertexOffset",
        });

        return graph;
    }

    private static NodeGraph CreateShellOffsetSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var localNormal = graph.AddNode(NodeKind.LocalNormal, 80, 320);
        var shellAmount = graph.AddNode(NodeKind.Scalar, 360, 420);
        shellAmount.Properties["Value"] = "0.06";
        var shellOffset = graph.AddNode(NodeKind.OffsetAlongNormal, 660, 320);
        var shellColor = graph.AddNode(NodeKind.Color, 360, 120);
        shellColor.Properties["R"] = "0.02";
        shellColor.Properties["G"] = "0.02";
        shellColor.Properties["B"] = "0.02";
        shellColor.Properties["A"] = "1.0";
        var output = graph.AddNode(NodeKind.Output, 980, 180);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = shellColor.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = localNormal.Id,
            SourcePin = "Value",
            TargetNodeId = shellOffset.Id,
            TargetPin = "Normal",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = shellAmount.Id,
            SourcePin = "Value",
            TargetNodeId = shellOffset.Id,
            TargetPin = "Amount",
        });
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = shellOffset.Id,
            SourcePin = "Offset",
            TargetNodeId = output.Id,
            TargetPin = "VertexOffset",
        });

        return graph;
    }

    private static NodeGraph CreateTwistSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 120);
        var localPosition = graph.AddNode(NodeKind.LocalPosition, 80, 360);
        var axisMask = graph.AddNode(NodeKind.AxisMask, 360, 320);
        axisMask.Properties["AxisX"] = "0.0";
        axisMask.Properties["AxisY"] = "1.0";
        axisMask.Properties["AxisZ"] = "0.0";
        axisMask.Properties["Min"] = "0.0";
        axisMask.Properties["Max"] = "12.0";
        var amount = graph.AddNode(NodeKind.Scalar, 360, 500);
        amount.Properties["Value"] = "1.2";
        var twist = graph.AddNode(NodeKind.Twist, 700, 380);
        twist.Properties["AxisX"] = "0.0";
        twist.Properties["AxisY"] = "1.0";
        twist.Properties["AxisZ"] = "0.0";
        var output = graph.AddNode(NodeKind.Output, 1040, 180);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = materialTexture.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = materialTexture.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = axisMask.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = twist.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = amount.Id, SourcePin = "Value", TargetNodeId = twist.Id, TargetPin = "Amount" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = axisMask.Id, SourcePin = "Mask", TargetNodeId = twist.Id, TargetPin = "Mask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = twist.Id, SourcePin = "Offset", TargetNodeId = output.Id, TargetPin = "VertexOffset" });

        return graph;
    }

    private static NodeGraph CreateBendSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 120);
        var localPosition = graph.AddNode(NodeKind.LocalPosition, 80, 360);
        var axisMask = graph.AddNode(NodeKind.AxisMask, 360, 320);
        axisMask.Properties["AxisX"] = "0.0";
        axisMask.Properties["AxisY"] = "1.0";
        axisMask.Properties["AxisZ"] = "0.0";
        axisMask.Properties["Min"] = "0.0";
        axisMask.Properties["Max"] = "12.0";
        var amount = graph.AddNode(NodeKind.Scalar, 360, 500);
        amount.Properties["Value"] = "0.8";
        var bend = graph.AddNode(NodeKind.Bend, 700, 380);
        bend.Properties["DriverX"] = "0.0";
        bend.Properties["DriverY"] = "1.0";
        bend.Properties["DriverZ"] = "0.0";
        bend.Properties["AxisX"] = "0.0";
        bend.Properties["AxisY"] = "0.0";
        bend.Properties["AxisZ"] = "1.0";
        var output = graph.AddNode(NodeKind.Output, 1040, 180);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = materialTexture.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = materialTexture.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = axisMask.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = bend.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = amount.Id, SourcePin = "Value", TargetNodeId = bend.Id, TargetPin = "Amount" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = axisMask.Id, SourcePin = "Mask", TargetNodeId = bend.Id, TargetPin = "Mask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = bend.Id, SourcePin = "Offset", TargetNodeId = output.Id, TargetPin = "VertexOffset" });

        return graph;
    }

    private static NodeGraph CreateNoiseDisplaceSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 120);
        var localPosition = graph.AddNode(NodeKind.LocalPosition, 80, 360);
        var localNormal = graph.AddNode(NodeKind.LocalNormal, 80, 500);
        var amount = graph.AddNode(NodeKind.Scalar, 360, 620);
        amount.Properties["Value"] = "0.03";
        var axisMask = graph.AddNode(NodeKind.AxisMask, 360, 360);
        axisMask.Properties["AxisX"] = "0.0";
        axisMask.Properties["AxisY"] = "1.0";
        axisMask.Properties["AxisZ"] = "0.0";
        axisMask.Properties["Min"] = "0.0";
        axisMask.Properties["Max"] = "12.0";
        var noise = graph.AddNode(NodeKind.NoiseDisplace, 720, 440);
        noise.Properties["Scale"] = "5.0";
        noise.Properties["Speed"] = "1.4";
        noise.Properties["Phase"] = "0.0";
        var output = graph.AddNode(NodeKind.Output, 1080, 180);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = materialTexture.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = materialTexture.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = axisMask.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = noise.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localNormal.Id, SourcePin = "Value", TargetNodeId = noise.Id, TargetPin = "Normal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = amount.Id, SourcePin = "Value", TargetNodeId = noise.Id, TargetPin = "Amplitude" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = axisMask.Id, SourcePin = "Mask", TargetNodeId = noise.Id, TargetPin = "Mask" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noise.Id, SourcePin = "Offset", TargetNodeId = output.Id, TargetPin = "VertexOffset" });

        return graph;
    }

    private static NodeGraph CreateBreathingPulseSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 40, 360, 180, "参考：呼吸 / 脉冲外壳\n基本思路：Time * 速度 -> 正弦 -> 遮罩 -> 外扩强度。", 0.20f, 0.48f, 0.78f);
        AddTemplateFrame(graph, 40, 300, 1760, 460, "顶点形变说明\nAxisMask 用来限制作用范围；OffsetAlongNormal 把脉冲转换成外壳膨胀。", 0.24f, 0.42f, 0.68f);

        var shellColor = graph.AddNode(NodeKind.Color, 80, 120);
        shellColor.Properties["R"] = "0.18";
        shellColor.Properties["G"] = "0.92";
        shellColor.Properties["B"] = "0.95";
        shellColor.Properties["A"] = "1.0";

        var localPosition = graph.AddNode(NodeKind.LocalPosition, 80, 420);
        var localNormal = graph.AddNode(NodeKind.LocalNormal, 80, 560);
        var heightMask = graph.AddNode(NodeKind.AxisMask, 360, 420);
        heightMask.Properties["AxisX"] = "0.0";
        heightMask.Properties["AxisY"] = "1.0";
        heightMask.Properties["AxisZ"] = "0.0";
        heightMask.Properties["Min"] = "0.0";
        heightMask.Properties["Max"] = "12.0";

        var time = graph.AddNode(NodeKind.Time, 360, 640);
        var pulseSpeed = graph.AddNode(NodeKind.Scalar, 640, 640);
        pulseSpeed.Properties["Value"] = "2.4";
        var pulseMultiply = graph.AddNode(NodeKind.Multiply, 900, 640);
        pulseMultiply.Properties["Type"] = "Float1";
        var pulseSine = graph.AddNode(NodeKind.Sine, 1180, 640);
        var pulseBias = graph.AddNode(NodeKind.Scalar, 1460, 560);
        pulseBias.Properties["Value"] = "1.0";
        var pulseAdd = graph.AddNode(NodeKind.Add, 1460, 680);
        pulseAdd.Properties["Type"] = "Float1";
        var pulseHalf = graph.AddNode(NodeKind.Scalar, 1740, 680);
        pulseHalf.Properties["Value"] = "0.5";
        var pulseNormalize = graph.AddNode(NodeKind.Multiply, 2020, 680);
        pulseNormalize.Properties["Type"] = "Float1";
        var amplitude = graph.AddNode(NodeKind.Scalar, 1740, 840);
        amplitude.Properties["Value"] = "0.06";
        var pulseAmplitude = graph.AddNode(NodeKind.Multiply, 2300, 700);
        pulseAmplitude.Properties["Type"] = "Float1";
        var maskedPulse = graph.AddNode(NodeKind.Multiply, 2580, 560);
        maskedPulse.Properties["Type"] = "Float1";
        var baseShell = graph.AddNode(NodeKind.Scalar, 2580, 760);
        baseShell.Properties["Value"] = "0.02";
        var finalAmount = graph.AddNode(NodeKind.Add, 2860, 660);
        finalAmount.Properties["Type"] = "Float1";
        var shellOffset = graph.AddNode(NodeKind.OffsetAlongNormal, 3140, 560);
        var output = graph.AddNode(NodeKind.Output, 3460, 240);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellColor.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = heightMask.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = time.Id, SourcePin = "Value", TargetNodeId = pulseMultiply.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = pulseSpeed.Id, SourcePin = "Value", TargetNodeId = pulseMultiply.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = pulseMultiply.Id, SourcePin = "Result", TargetNodeId = pulseSine.Id, TargetPin = "Value" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = pulseSine.Id, SourcePin = "Result", TargetNodeId = pulseAdd.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = pulseBias.Id, SourcePin = "Value", TargetNodeId = pulseAdd.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = pulseAdd.Id, SourcePin = "Result", TargetNodeId = pulseNormalize.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = pulseHalf.Id, SourcePin = "Value", TargetNodeId = pulseNormalize.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = pulseNormalize.Id, SourcePin = "Result", TargetNodeId = pulseAmplitude.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = amplitude.Id, SourcePin = "Value", TargetNodeId = pulseAmplitude.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = heightMask.Id, SourcePin = "Mask", TargetNodeId = maskedPulse.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = pulseAmplitude.Id, SourcePin = "Result", TargetNodeId = maskedPulse.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = maskedPulse.Id, SourcePin = "Result", TargetNodeId = finalAmount.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = baseShell.Id, SourcePin = "Value", TargetNodeId = finalAmount.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localNormal.Id, SourcePin = "Value", TargetNodeId = shellOffset.Id, TargetPin = "Normal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = finalAmount.Id, SourcePin = "Result", TargetNodeId = shellOffset.Id, TargetPin = "Amount" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellOffset.Id, SourcePin = "Offset", TargetNodeId = output.Id, TargetPin = "VertexOffset" });

        return graph;
    }

    private static NodeGraph CreateEyeSeeThroughSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 40, 500, 150, "EyeSeeThrough\n用头骨朝向和相机前向计算显露强度，驱动眼部材质透明度。", 0.28f, 0.45f, 0.72f);
        AddTemplateFrame(graph, 40, 240, 1640, 520, "使用建议\n1. 把模板应用到眼白 / 虹膜 / 眉毛这类需要透出的材质。\n2. 默认读取 (self) 的“头”骨骼 Z 轴；如模型骨骼命名不同，请改 Bone 节点的 Item。\n3. RevealStart / RevealEnd 控制显露区间，必要时可以切换骨骼轴向或调整相机前向符号。", 0.24f, 0.40f, 0.64f);

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 340, 100);
        materialTexture.Properties["ColorSpace"] = "Color";

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 340, 280);
        var diffuseSplit = graph.AddNode(NodeKind.SplitColor, 620, 280);

        var headDirection = graph.AddNode(NodeKind.ControlObjectBoneDirection, 80, 380);
        headDirection.Properties["Name"] = "(self)";
        headDirection.Properties["Item"] = "頭";
        headDirection.Properties["Axis"] = "Z";

        var viewForward = graph.AddNode(NodeKind.Float4Value, 80, 560);
        viewForward.Properties["X"] = "0.0";
        viewForward.Properties["Y"] = "0.0";
        viewForward.Properties["Z"] = "-1.0";
        viewForward.Properties["W"] = "0.0";

        var cameraForward = graph.AddNode(NodeKind.TransformVector, 360, 560);
        cameraForward.Properties["VectorType"] = "Direction";
        cameraForward.Properties["SourceSpace"] = "View";
        cameraForward.Properties["TargetSpace"] = "World";
        cameraForward.Properties["NormalizeOutput"] = "True";

        var alignment = graph.AddNode(NodeKind.Dot, 660, 470);
        var revealStart = graph.AddNode(NodeKind.Scalar, 660, 620);
        revealStart.Properties["Value"] = "-0.10";
        var revealEnd = graph.AddNode(NodeKind.Scalar, 660, 710);
        revealEnd.Properties["Value"] = "0.35";
        var revealMask = graph.AddNode(NodeKind.SmoothStep, 940, 500);

        var textureAlpha = graph.AddNode(NodeKind.Multiply, 940, 180);
        textureAlpha.Properties["Type"] = "Float1";
        var finalAlpha = graph.AddNode(NodeKind.Multiply, 1220, 220);
        finalAlpha.Properties["Type"] = "Float1";

        var output = graph.AddNode(NodeKind.Output, 1480, 180);
        output.Properties["AlphaMode"] = "AlphaInput";
        output.Properties["AlphaBlendEnable"] = "True";
        output.Properties["AlphaTestEnable"] = "True";
        output.Properties["ZWriteEnable"] = "False";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, texCoord, "UV", materialTexture, "UV");
        Connect(graph, materialDiffuse, "Color", diffuseSplit, "Color");
        Connect(graph, viewForward, "Value", cameraForward, "Vector");
        Connect(graph, headDirection, "Value", alignment, "A");
        Connect(graph, cameraForward, "Value", alignment, "B");
        Connect(graph, revealStart, "Value", revealMask, "Min");
        Connect(graph, revealEnd, "Value", revealMask, "Max");
        Connect(graph, alignment, "Result", revealMask, "X");
        Connect(graph, materialTexture, "A", textureAlpha, "A");
        Connect(graph, diffuseSplit, "A", textureAlpha, "B");
        Connect(graph, textureAlpha, "Result", finalAlpha, "A");
        Connect(graph, revealMask, "Result", finalAlpha, "B");
        Connect(graph, materialTexture, "Color", output, "Color");
        Connect(graph, finalAlpha, "Result", output, "Alpha");

        return graph;
    }

    private static NodeGraph CreateEyeSeeThroughSoftSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 40, 540, 150, "EyeSeeThrough Soft\n更宽的显露区间和更柔和的透明衰减，适合眉毛或半透明眼部叠层。", 0.30f, 0.44f, 0.68f);
        AddTemplateFrame(graph, 40, 240, 1900, 560, "调整建议\n1. Soft 版会更早开始透出，并保留一部分基础可见度。\n2. 如果模型本身贴图 Alpha 很低，可以提高 BaseVisibility。\n3. 如果正面太明显，先降低 MaxVisibility，再收紧 RevealEnd。", 0.22f, 0.38f, 0.60f);

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 100);
        materialTexture.Properties["ColorSpace"] = "Color";

        var materialDiffuse = graph.AddNode(NodeKind.MaterialDiffuse, 360, 280);
        var diffuseSplit = graph.AddNode(NodeKind.SplitColor, 640, 280);

        var headDirection = graph.AddNode(NodeKind.ControlObjectBoneDirection, 80, 380);
        headDirection.Properties["Name"] = "(self)";
        headDirection.Properties["Item"] = "頭";
        headDirection.Properties["Axis"] = "Z";

        var viewForward = graph.AddNode(NodeKind.Float4Value, 80, 560);
        viewForward.Properties["X"] = "0.0";
        viewForward.Properties["Y"] = "0.0";
        viewForward.Properties["Z"] = "-1.0";
        viewForward.Properties["W"] = "0.0";

        var cameraForward = graph.AddNode(NodeKind.TransformVector, 360, 560);
        cameraForward.Properties["VectorType"] = "Direction";
        cameraForward.Properties["SourceSpace"] = "View";
        cameraForward.Properties["TargetSpace"] = "World";
        cameraForward.Properties["NormalizeOutput"] = "True";

        var alignment = graph.AddNode(NodeKind.Dot, 660, 470);
        var revealStart = graph.AddNode(NodeKind.Scalar, 660, 620);
        revealStart.Properties["Value"] = "-0.35";
        var revealEnd = graph.AddNode(NodeKind.Scalar, 660, 710);
        revealEnd.Properties["Value"] = "0.20";
        var revealMask = graph.AddNode(NodeKind.SmoothStep, 960, 500);

        var baseVisibility = graph.AddNode(NodeKind.Scalar, 960, 660);
        baseVisibility.Properties["Value"] = "0.15";
        var maxVisibility = graph.AddNode(NodeKind.Scalar, 960, 750);
        maxVisibility.Properties["Value"] = "0.85";
        var revealScaled = graph.AddNode(NodeKind.Multiply, 1240, 500);
        revealScaled.Properties["Type"] = "Float1";
        var visibility = graph.AddNode(NodeKind.Add, 1520, 560);
        visibility.Properties["Type"] = "Float1";

        var textureAlpha = graph.AddNode(NodeKind.Multiply, 1240, 180);
        textureAlpha.Properties["Type"] = "Float1";
        var finalAlpha = graph.AddNode(NodeKind.Multiply, 1800, 260);
        finalAlpha.Properties["Type"] = "Float1";

        var output = graph.AddNode(NodeKind.Output, 2060, 180);
        output.Properties["AlphaMode"] = "AlphaInput";
        output.Properties["AlphaBlendEnable"] = "True";
        output.Properties["AlphaTestEnable"] = "True";
        output.Properties["ZWriteEnable"] = "False";
        output.Properties["AlphaClipThreshold"] = "0.001";

        Connect(graph, texCoord, "UV", materialTexture, "UV");
        Connect(graph, materialDiffuse, "Color", diffuseSplit, "Color");
        Connect(graph, viewForward, "Value", cameraForward, "Vector");
        Connect(graph, headDirection, "Value", alignment, "A");
        Connect(graph, cameraForward, "Value", alignment, "B");
        Connect(graph, revealStart, "Value", revealMask, "Min");
        Connect(graph, revealEnd, "Value", revealMask, "Max");
        Connect(graph, alignment, "Result", revealMask, "X");
        Connect(graph, maxVisibility, "Value", revealScaled, "A");
        Connect(graph, revealMask, "Result", revealScaled, "B");
        Connect(graph, baseVisibility, "Value", visibility, "A");
        Connect(graph, revealScaled, "Result", visibility, "B");
        Connect(graph, materialTexture, "A", textureAlpha, "A");
        Connect(graph, diffuseSplit, "A", textureAlpha, "B");
        Connect(graph, textureAlpha, "Result", finalAlpha, "A");
        Connect(graph, visibility, "Result", finalAlpha, "B");
        Connect(graph, materialTexture, "Color", output, "Color");
        Connect(graph, finalAlpha, "Result", output, "Alpha");

        return graph;
    }

    private static NodeGraph CreateAuraShellPixelSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 60, 540, 160, "参考：Live Outline_MME/Outline.fx\nPixel 阶段负责流动噪波、菲涅尔收边，以及光壳配色。", 0.22f, 0.56f, 0.70f);
        AddTemplateFrame(graph, 600, 120, 1560, 460, "光壳着色说明\nNoise 纹理向上滚动；Fresnel 负责收紧边缘；ColorRamp 把强度映射成能量色。", 0.18f, 0.42f, 0.52f);

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 160);
        var time = graph.AddNode(NodeKind.Time, 80, 320);
        var noisePanner = graph.AddNode(NodeKind.Panner, 360, 200);
        noisePanner.Properties["SpeedU"] = "0.0";
        noisePanner.Properties["SpeedV"] = "-0.28";
        var noiseTexture = graph.AddNode(NodeKind.ExternalTexture, 660, 180);
        noiseTexture.Properties["ResourceName"] = "noise.png";
        noiseTexture.Properties["ColorSpace"] = "Linear";
        noiseTexture.Properties["AddressMode"] = "Wrap";
        noiseTexture.Properties["FilterMode"] = "Linear";

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 360, 520);
        var viewDirection = graph.AddNode(NodeKind.ViewDirection, 360, 660);
        var fresnel = graph.AddNode(NodeKind.Fresnel, 660, 580);
        fresnel.Properties["Power"] = "2.4";
        fresnel.Properties["Bias"] = "0.08";

        var noiseChannel = graph.AddNode(NodeKind.SplitColor, 960, 180);
        var intensity = graph.AddNode(NodeKind.Multiply, 1240, 360);
        intensity.Properties["Type"] = "Float1";
        var shellRamp = graph.AddNode(NodeKind.ColorRamp, 1520, 260);
        shellRamp.Properties["StartR"] = "0.12";
        shellRamp.Properties["StartG"] = "0.18";
        shellRamp.Properties["StartB"] = "0.32";
        shellRamp.Properties["StartA"] = "1.0";
        shellRamp.Properties["EndR"] = "0.95";
        shellRamp.Properties["EndG"] = "0.98";
        shellRamp.Properties["EndB"] = "1.0";
        shellRamp.Properties["EndA"] = "1.0";
        shellRamp.Properties["Start"] = "0.10";
        shellRamp.Properties["End"] = "0.92";

        var shellBrightness = graph.AddNode(NodeKind.Scalar, 1520, 460);
        shellBrightness.Properties["Value"] = "1.6";
        var brightColor = graph.AddNode(NodeKind.Multiply, 1800, 320);
        brightColor.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 2080, 320);
        output.Properties["AlphaMode"] = "AlphaInput";
        output.Properties["CullMode"] = "CW";
        output.Properties["ZEnable"] = "True";
        output.Properties["AlphaTestEnable"] = "False";
        output.Properties["AlphaBlendEnable"] = "True";
        output.Properties["SrcBlend"] = "SRCALPHA";
        output.Properties["DestBlend"] = "INVSRCALPHA";
        output.Properties["BlendOp"] = "ADD";
        output.Properties["ZWriteEnable"] = "True";

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = noisePanner.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = time.Id, SourcePin = "Value", TargetNodeId = noisePanner.Id, TargetPin = "Time" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noisePanner.Id, SourcePin = "UV", TargetNodeId = noiseTexture.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseTexture.Id, SourcePin = "Color", TargetNodeId = noiseChannel.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = worldNormal.Id, SourcePin = "Value", TargetNodeId = fresnel.Id, TargetPin = "Normal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = viewDirection.Id, SourcePin = "Value", TargetNodeId = fresnel.Id, TargetPin = "ViewDir" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseChannel.Id, SourcePin = "R", TargetNodeId = intensity.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = fresnel.Id, SourcePin = "Result", TargetNodeId = intensity.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = intensity.Id, SourcePin = "Result", TargetNodeId = shellRamp.Id, TargetPin = "T" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellRamp.Id, SourcePin = "Color", TargetNodeId = brightColor.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellBrightness.Id, SourcePin = "Value", TargetNodeId = brightColor.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = brightColor.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = intensity.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "Alpha" });

        return graph;
    }

    private static NodeGraph CreateAuraShellVertexSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 60, 420, 180, "参考：Live Outline_MME/Outline.fx\nVertex 阶段负责生成外层光壳，并叠加大尺度波动与噪波抖动。", 0.16f, 0.46f, 0.74f);
        AddTemplateFrame(graph, 40, 300, 1720, 920, "光壳形变说明\nOffsetAlongNormal 生成基础外壳；VertexWave 提供分层呼吸感；NoiseDisplace 增加不稳定的能量闪烁。", 0.20f, 0.36f, 0.60f);

        var shellColor = graph.AddNode(NodeKind.Color, 80, 120);
        shellColor.Properties["R"] = "0.15";
        shellColor.Properties["G"] = "0.92";
        shellColor.Properties["B"] = "1.0";
        shellColor.Properties["A"] = "1.0";

        var localPosition = graph.AddNode(NodeKind.LocalPosition, 80, 360);
        var localNormal = graph.AddNode(NodeKind.LocalNormal, 80, 520);
        var heightMask = graph.AddNode(NodeKind.AxisMask, 360, 360);
        heightMask.Properties["AxisX"] = "0.0";
        heightMask.Properties["AxisY"] = "1.0";
        heightMask.Properties["AxisZ"] = "0.0";
        heightMask.Properties["Min"] = "0.0";
        heightMask.Properties["Max"] = "12.0";

        var baseAmount = graph.AddNode(NodeKind.Scalar, 360, 560);
        baseAmount.Properties["Value"] = "0.08";
        var shellAmount = graph.AddNode(NodeKind.Multiply, 640, 520);
        shellAmount.Properties["Type"] = "Float1";
        var shellOffset = graph.AddNode(NodeKind.OffsetAlongNormal, 920, 500);

        var wave = graph.AddNode(NodeKind.VertexWave, 920, 720);
        wave.Properties["Amplitude"] = "0.028";
        wave.Properties["Frequency"] = "4.5";
        wave.Properties["Speed"] = "3.2";
        wave.Properties["Phase"] = "0.0";
        wave.Properties["AxisX"] = "0.0";
        wave.Properties["AxisY"] = "1.0";
        wave.Properties["AxisZ"] = "0.0";

        var noise = graph.AddNode(NodeKind.NoiseDisplace, 920, 940);
        noise.Properties["Amplitude"] = "0.018";
        noise.Properties["Scale"] = "7.0";
        noise.Properties["Speed"] = "1.8";
        noise.Properties["Phase"] = "0.0";

        var offsetSum = graph.AddNode(NodeKind.Add, 1220, 700);
        offsetSum.Properties["Type"] = "Float4";
        var finalOffset = graph.AddNode(NodeKind.Add, 1500, 620);
        finalOffset.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 1800, 240);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellColor.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = heightMask.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = heightMask.Id, SourcePin = "Mask", TargetNodeId = shellAmount.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = baseAmount.Id, SourcePin = "Value", TargetNodeId = shellAmount.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localNormal.Id, SourcePin = "Value", TargetNodeId = shellOffset.Id, TargetPin = "Normal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellAmount.Id, SourcePin = "Result", TargetNodeId = shellOffset.Id, TargetPin = "Amount" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = wave.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localNormal.Id, SourcePin = "Value", TargetNodeId = wave.Id, TargetPin = "Normal" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = noise.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localNormal.Id, SourcePin = "Value", TargetNodeId = noise.Id, TargetPin = "Normal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = heightMask.Id, SourcePin = "Mask", TargetNodeId = noise.Id, TargetPin = "Mask" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellOffset.Id, SourcePin = "Offset", TargetNodeId = offsetSum.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = wave.Id, SourcePin = "Offset", TargetNodeId = offsetSum.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = offsetSum.Id, SourcePin = "Result", TargetNodeId = finalOffset.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noise.Id, SourcePin = "Offset", TargetNodeId = finalOffset.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = finalOffset.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "VertexOffset" });

        return graph;
    }

    private static NodeGraph CreateTattooFlameShellPixelSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 20, 760, 120, "使用方式\n本模板用于复制出来的外壳层或附件层，不建议直接替换本体材质。\n推荐搭配：本体使用“纹身本体”，外壳使用“纹身火焰外壳”。", 0.62f, 0.34f, 0.16f);
        AddTemplateFrame(graph, 40, 60, 520, 180, "参考：Live Outline_MME/Mask.fx\n默认已移除独立 mask，仅保留两路滚动 noise 来制造不稳定的边缘破碎。", 0.46f, 0.34f, 0.18f);
        AddTemplateFrame(graph, 600, 80, 1840, 560, "火焰外壳着色说明\n两路 Noise 共同决定火焰强度；ColorRamp 把强度映射成亮青白色火焰核心。", 0.52f, 0.26f, 0.14f);

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 220);
        var time = graph.AddNode(NodeKind.Time, 80, 420);

        var noisePannerA = graph.AddNode(NodeKind.Panner, 360, 300);
        noisePannerA.Properties["SpeedU"] = "0.10";
        noisePannerA.Properties["SpeedV"] = "-0.46";
        var noiseTextureA = graph.AddNode(NodeKind.ExternalTexture, 660, 280);
        noiseTextureA.Properties["ResourceName"] = "noise.png";
        noiseTextureA.Properties["SourcePath"] = FindLiveOutlineAssetPath("noise.png");
        noiseTextureA.Properties["ColorSpace"] = "Linear";
        noiseTextureA.Properties["AddressMode"] = "Wrap";
        noiseTextureA.Properties["FilterMode"] = "Linear";

        var noisePannerB = graph.AddNode(NodeKind.Panner, 360, 520);
        noisePannerB.Properties["SpeedU"] = "-0.06";
        noisePannerB.Properties["SpeedV"] = "-0.22";
        var noiseTextureB = graph.AddNode(NodeKind.ExternalTexture, 660, 500);
        noiseTextureB.Properties["ResourceName"] = "noise.png";
        noiseTextureB.Properties["SourcePath"] = FindLiveOutlineAssetPath("noise.png");
        noiseTextureB.Properties["ColorSpace"] = "Linear";
        noiseTextureB.Properties["AddressMode"] = "Wrap";
        noiseTextureB.Properties["FilterMode"] = "Linear";

        var noiseChannelA = graph.AddNode(NodeKind.SplitColor, 960, 280);
        var noiseChannelB = graph.AddNode(NodeKind.SplitColor, 960, 500);
        var noiseMix = graph.AddNode(NodeKind.Multiply, 1240, 380);
        noiseMix.Properties["Type"] = "Float1";
        var flameRamp = graph.AddNode(NodeKind.ColorRamp, 1800, 220);
        flameRamp.Properties["StartR"] = "0.03";
        flameRamp.Properties["StartG"] = "0.08";
        flameRamp.Properties["StartB"] = "0.18";
        flameRamp.Properties["StartA"] = "1.0";
        flameRamp.Properties["EndR"] = "0.84";
        flameRamp.Properties["EndG"] = "1.0";
        flameRamp.Properties["EndB"] = "0.92";
        flameRamp.Properties["EndA"] = "1.0";
        flameRamp.Properties["Start"] = "0.18";
        flameRamp.Properties["End"] = "0.96";

        var brightness = graph.AddNode(NodeKind.Scalar, 1800, 420);
        brightness.Properties["Value"] = "2.6";
        var brightColor = graph.AddNode(NodeKind.Multiply, 2080, 320);
        brightColor.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 2360, 320);
        output.Properties["AlphaMode"] = "AlphaInput";
        output.Properties["CullMode"] = "NONE";
        output.Properties["ZEnable"] = "True";
        output.Properties["AlphaTestEnable"] = "False";
        output.Properties["AlphaBlendEnable"] = "True";
        output.Properties["SrcBlend"] = "SRCALPHA";
        output.Properties["DestBlend"] = "ONE";
        output.Properties["BlendOp"] = "ADD";
        output.Properties["ZWriteEnable"] = "False";

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = noisePannerA.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = time.Id, SourcePin = "Value", TargetNodeId = noisePannerA.Id, TargetPin = "Time" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noisePannerA.Id, SourcePin = "UV", TargetNodeId = noiseTextureA.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = noisePannerB.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = time.Id, SourcePin = "Value", TargetNodeId = noisePannerB.Id, TargetPin = "Time" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noisePannerB.Id, SourcePin = "UV", TargetNodeId = noiseTextureB.Id, TargetPin = "UV" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseTextureA.Id, SourcePin = "Color", TargetNodeId = noiseChannelA.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseTextureB.Id, SourcePin = "Color", TargetNodeId = noiseChannelB.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseChannelA.Id, SourcePin = "R", TargetNodeId = noiseMix.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseChannelB.Id, SourcePin = "R", TargetNodeId = noiseMix.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseMix.Id, SourcePin = "Result", TargetNodeId = flameRamp.Id, TargetPin = "T" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = flameRamp.Id, SourcePin = "Color", TargetNodeId = brightColor.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = brightness.Id, SourcePin = "Value", TargetNodeId = brightColor.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = brightColor.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseMix.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "Alpha" });

        return graph;
    }

    private static NodeGraph CreateTattooBaseBodySampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 20, 760, 120, "使用方式\n本模板用于模型本体材质。\n如果要做接近参考 MME 的双层效果，请再给复制出来的外壳层使用“纹身火焰外壳”。", 0.40f, 0.46f, 0.18f);
        AddTemplateFrame(graph, 40, 60, 560, 180, "参考：Live Outline_MME/Mask.fx 本体通道\nObjectTexture 保留本体颜色；mask + 滚动 noise 负责注入动态纹身发光。", 0.30f, 0.44f, 0.18f);
        AddTemplateFrame(graph, 40, 260, 2180, 560, "本体着色说明\nMaterialTexture 提供本体颜色；Mask.png 限制生效区域；Noise.png 向上滚动；ColorRamp 把噪波变成发光配色。", 0.36f, 0.32f, 0.12f);

        var texCoord = graph.AddNode(NodeKind.TexCoord, 80, 340);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 140);
        materialTexture.Properties["ColorSpace"] = "Color";

        var maskTexture = graph.AddNode(NodeKind.ExternalTexture, 360, 340);
        maskTexture.Properties["ResourceName"] = "mask.png";
        maskTexture.Properties["SourcePath"] = FindLiveOutlineAssetPath("mask.png");
        maskTexture.Properties["ColorSpace"] = "Linear";
        maskTexture.Properties["AddressMode"] = "Wrap";
        maskTexture.Properties["FilterMode"] = "Linear";

        var time = graph.AddNode(NodeKind.Time, 80, 560);
        var noisePanner = graph.AddNode(NodeKind.Panner, 360, 560);
        noisePanner.Properties["SpeedU"] = "0.0";
        noisePanner.Properties["SpeedV"] = "-0.30";
        var noiseTexture = graph.AddNode(NodeKind.ExternalTexture, 660, 540);
        noiseTexture.Properties["ResourceName"] = "noise.png";
        noiseTexture.Properties["SourcePath"] = FindLiveOutlineAssetPath("noise.png");
        noiseTexture.Properties["ColorSpace"] = "Linear";
        noiseTexture.Properties["AddressMode"] = "Wrap";
        noiseTexture.Properties["FilterMode"] = "Linear";

        var maskSplit = graph.AddNode(NodeKind.SplitColor, 940, 340);
        var noiseSplit = graph.AddNode(NodeKind.SplitColor, 940, 540);
        var energyMask = graph.AddNode(NodeKind.Multiply, 1220, 440);
        energyMask.Properties["Type"] = "Float1";

        var energyRamp = graph.AddNode(NodeKind.ColorRamp, 1500, 320);
        energyRamp.Properties["StartR"] = "0.00";
        energyRamp.Properties["StartG"] = "0.00";
        energyRamp.Properties["StartB"] = "0.00";
        energyRamp.Properties["StartA"] = "0.0";
        energyRamp.Properties["EndR"] = "0.80";
        energyRamp.Properties["EndG"] = "1.00";
        energyRamp.Properties["EndB"] = "0.90";
        energyRamp.Properties["EndA"] = "0.0";
        energyRamp.Properties["Start"] = "0.10";
        energyRamp.Properties["End"] = "0.90";

        var energyBoost = graph.AddNode(NodeKind.Scalar, 1500, 520);
        energyBoost.Properties["Value"] = "0.75";
        var boostedEnergy = graph.AddNode(NodeKind.Multiply, 1780, 420);
        boostedEnergy.Properties["Type"] = "Float4";
        var finalColor = graph.AddNode(NodeKind.Add, 2060, 300);
        finalColor.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 2340, 300);
        output.Properties["AlphaMode"] = "AlphaInput";
        output.Properties["CullMode"] = "NONE";
        output.Properties["ZEnable"] = "True";
        output.Properties["AlphaTestEnable"] = "False";
        output.Properties["AlphaBlendEnable"] = "True";
        output.Properties["SrcBlend"] = "SRCALPHA";
        output.Properties["DestBlend"] = "INVSRCALPHA";
        output.Properties["BlendOp"] = "ADD";
        output.Properties["ZWriteEnable"] = "True";
        output.Properties["AlphaClipThreshold"] = "0.10";

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = materialTexture.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = maskTexture.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = texCoord.Id, SourcePin = "UV", TargetNodeId = noisePanner.Id, TargetPin = "UV" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = time.Id, SourcePin = "Value", TargetNodeId = noisePanner.Id, TargetPin = "Time" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noisePanner.Id, SourcePin = "UV", TargetNodeId = noiseTexture.Id, TargetPin = "UV" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = maskTexture.Id, SourcePin = "Color", TargetNodeId = maskSplit.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseTexture.Id, SourcePin = "Color", TargetNodeId = noiseSplit.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = maskSplit.Id, SourcePin = "R", TargetNodeId = energyMask.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = noiseSplit.Id, SourcePin = "R", TargetNodeId = energyMask.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = energyMask.Id, SourcePin = "Result", TargetNodeId = energyRamp.Id, TargetPin = "T" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = energyRamp.Id, SourcePin = "Color", TargetNodeId = boostedEnergy.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = energyBoost.Id, SourcePin = "Value", TargetNodeId = boostedEnergy.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = materialTexture.Id, SourcePin = "Color", TargetNodeId = finalColor.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = boostedEnergy.Id, SourcePin = "Result", TargetNodeId = finalColor.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = finalColor.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = materialTexture.Id, SourcePin = "A", TargetNodeId = output.Id, TargetPin = "Alpha" });

        return graph;
    }

    private static NodeGraph CreateTattooFlameShellVertexSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 60, 420, 180, "参考：Live Outline_MME/Mask.fx\nVertex 阶段主要负责生成基础火焰外壳；大部分火焰动态仍来自 Pixel 噪波。", 0.56f, 0.32f, 0.14f);
        AddTemplateFrame(graph, 40, 300, 1520, 980, "火焰外壳形变说明\n基础法线外扩负责厚度；NoiseDisplace 破坏轮廓；VertexWave 提供轻微上涌脉动。", 0.50f, 0.24f, 0.10f);

        var shellColor = graph.AddNode(NodeKind.Color, 80, 120);
        shellColor.Properties["R"] = "0.16";
        shellColor.Properties["G"] = "0.86";
        shellColor.Properties["B"] = "0.92";
        shellColor.Properties["A"] = "1.0";

        var localPosition = graph.AddNode(NodeKind.LocalPosition, 80, 360);
        var localNormal = graph.AddNode(NodeKind.LocalNormal, 80, 500);
        var shellAmount = graph.AddNode(NodeKind.Scalar, 360, 620);
        shellAmount.Properties["Value"] = "0.10";
        var shellOffset = graph.AddNode(NodeKind.OffsetAlongNormal, 640, 500);

        var flameNoise = graph.AddNode(NodeKind.NoiseDisplace, 640, 760);
        flameNoise.Properties["Amplitude"] = "0.018";
        flameNoise.Properties["Scale"] = "6.5";
        flameNoise.Properties["Speed"] = "1.5";
        flameNoise.Properties["Phase"] = "0.0";

        var wave = graph.AddNode(NodeKind.VertexWave, 640, 980);
        wave.Properties["Amplitude"] = "0.010";
        wave.Properties["Frequency"] = "3.5";
        wave.Properties["Speed"] = "4.2";
        wave.Properties["Phase"] = "0.0";
        wave.Properties["AxisX"] = "0.0";
        wave.Properties["AxisY"] = "1.0";
        wave.Properties["AxisZ"] = "0.0";

        var offsetSum = graph.AddNode(NodeKind.Add, 940, 640);
        offsetSum.Properties["Type"] = "Float4";
        var finalOffset = graph.AddNode(NodeKind.Add, 1220, 600);
        finalOffset.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 1520, 240);

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellColor.Id, SourcePin = "Color", TargetNodeId = output.Id, TargetPin = "Color" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localNormal.Id, SourcePin = "Value", TargetNodeId = shellOffset.Id, TargetPin = "Normal" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellAmount.Id, SourcePin = "Value", TargetNodeId = shellOffset.Id, TargetPin = "Amount" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = flameNoise.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localNormal.Id, SourcePin = "Value", TargetNodeId = flameNoise.Id, TargetPin = "Normal" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localPosition.Id, SourcePin = "Value", TargetNodeId = wave.Id, TargetPin = "Position" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = localNormal.Id, SourcePin = "Value", TargetNodeId = wave.Id, TargetPin = "Normal" });

        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = shellOffset.Id, SourcePin = "Offset", TargetNodeId = offsetSum.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = flameNoise.Id, SourcePin = "Offset", TargetNodeId = offsetSum.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = offsetSum.Id, SourcePin = "Result", TargetNodeId = finalOffset.Id, TargetPin = "A" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = wave.Id, SourcePin = "Offset", TargetNodeId = finalOffset.Id, TargetPin = "B" });
        graph.AddOrReplaceConnection(new GraphConnection { SourceNodeId = finalOffset.Id, SourcePin = "Result", TargetNodeId = output.Id, TargetPin = "VertexOffset" });

        return graph;
    }

    private static NodeGraph CreateOffsetShadowCaptureSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 40, 620, 170, "通用偏移阴影捕获\n把它挂到需要投影轮廓的材质上。\n不限定前发，身体、衣服、裙摆、披风都可以。", 0.28f, 0.44f, 0.74f);
        AddTemplateFrame(graph, 40, 260, 1180, 980, "控制说明\n(OffscreenOwner).XYZ 控制阴影偏移方向。\n(OffscreenOwner).Rxyz 控制阴影颜色偏移。\n模板默认只保留 XY 偏移，所以它更接近“屏幕向偏移的投影阴影”。", 0.24f, 0.50f, 0.38f);
        AddTemplateFrame(graph, 1260, 260, 1080, 980, "输出说明\nMaterialTexture.A 负责裁切透明边缘。\nLayerSourceOutput 负责写入离屏 RT。\nOutput.VertexOffset 负责把轮廓整体推到目标位置。", 0.56f, 0.36f, 0.30f);

        var texCoord = graph.AddNode(NodeKind.TexCoord, 120, 320);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 400, 320);
        materialTexture.Properties["ColorSpace"] = "Color";

        var ownerOffset = graph.AddNode(NodeKind.ControlObjectPosition, 120, 620);
        ownerOffset.Properties["Name"] = "(OffscreenOwner)";
        ownerOffset.Properties["Item"] = "XYZ";

        var baseDirection = CreateFloat3Node(graph, 400, 580, "-0.05", "-0.05", "0.05");
        var directionScale = CreateFloat3Node(graph, 400, 740, "0.1", "0.1", "0.1");
        var scaledDirection = CreateTypedMathNode(graph, NodeKind.Multiply, 720, 660, "Float3");
        var finalDirection = CreateTypedMathNode(graph, NodeKind.Add, 1020, 660, "Float3");
        var directionSplit = graph.AddNode(NodeKind.SplitXYZ, 1320, 660);
        var zeroScalar = CreateScalarNode(graph, 1320, 840, "0.0");
        var vertexOffset = graph.AddNode(NodeKind.MergeXYZ, 1620, 700);

        var ownerColor = graph.AddNode(NodeKind.ControlObjectPosition, 120, 940);
        ownerColor.Properties["Name"] = "(OffscreenOwner)";
        ownerColor.Properties["Item"] = "Rxyz";

        var baseColor = CreateFloat3Node(graph, 400, 900, "0.9", "0.7", "0.6");
        var colorScale = CreateFloat3Node(graph, 400, 1060, "0.1", "0.1", "0.1");
        var scaledColor = CreateTypedMathNode(graph, NodeKind.Multiply, 720, 980, "Float3");
        var finalColor3 = CreateTypedMathNode(graph, NodeKind.Add, 1020, 980, "Float3");
        var colorSplit = graph.AddNode(NodeKind.SplitXYZ, 1320, 980);
        var oneScalar = CreateScalarNode(graph, 1320, 1160, "1.0");
        var finalColor4 = graph.AddNode(NodeKind.MergeXYZW, 1620, 1020);

        var layerOutput = graph.AddNode(NodeKind.LayerSourceOutput, 1940, 540);
        layerOutput.Properties["AlphaThreshold"] = "0.001";

        var output = graph.AddNode(NodeKind.Output, 2240, 540);
        output.Properties["AlphaMode"] = "ColorAlpha";

        Connect(graph, texCoord, "UV", materialTexture, "UV");

        Connect(graph, ownerOffset, "Value", scaledDirection, "A");
        Connect(graph, directionScale, "Value", scaledDirection, "B");
        Connect(graph, baseDirection, "Value", finalDirection, "A");
        Connect(graph, scaledDirection, "Result", finalDirection, "B");
        Connect(graph, finalDirection, "Result", directionSplit, "Value");
        Connect(graph, directionSplit, "X", vertexOffset, "X");
        Connect(graph, directionSplit, "Y", vertexOffset, "Y");
        Connect(graph, zeroScalar, "Value", vertexOffset, "Z");

        Connect(graph, ownerColor, "Value", scaledColor, "A");
        Connect(graph, colorScale, "Value", scaledColor, "B");
        Connect(graph, baseColor, "Value", finalColor3, "A");
        Connect(graph, scaledColor, "Result", finalColor3, "B");
        Connect(graph, finalColor3, "Result", colorSplit, "Value");
        Connect(graph, colorSplit, "X", finalColor4, "X");
        Connect(graph, colorSplit, "Y", finalColor4, "Y");
        Connect(graph, colorSplit, "Z", finalColor4, "Z");
        Connect(graph, oneScalar, "Value", finalColor4, "W");

        Connect(graph, finalColor4, "Result", layerOutput, "Color");
        Connect(graph, materialTexture, "A", layerOutput, "AlphaMask");
        Connect(graph, layerOutput, "Color", output, "Color");
        Connect(graph, vertexOffset, "Result", output, "VertexOffset");

        return graph;
    }

    private static NodeGraph CreateOffsetShadowMaskSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 40, 620, 170, "偏移阴影遮罩\n把它挂到不希望被偏移阴影覆盖的物体或材质上。", 0.52f, 0.40f, 0.22f);
        AddTemplateFrame(graph, 40, 260, 1380, 360, "工作方式\n输出白色到遮罩 RT。\n合成阶段读取 Alpha，把偏移阴影从这些区域扣掉。", 0.38f, 0.38f, 0.38f);

        var texCoord = graph.AddNode(NodeKind.TexCoord, 120, 360);
        var materialTexture = graph.AddNode(NodeKind.MaterialTexture, 400, 360);
        materialTexture.Properties["ColorSpace"] = "Color";

        var white = graph.AddNode(NodeKind.Color, 720, 320);
        white.Properties["R"] = "1.0";
        white.Properties["G"] = "1.0";
        white.Properties["B"] = "1.0";
        white.Properties["A"] = "1.0";

        var layerOutput = graph.AddNode(NodeKind.LayerSourceOutput, 1040, 360);
        layerOutput.Properties["AlphaThreshold"] = "0.001";

        var output = graph.AddNode(NodeKind.Output, 1340, 360);
        output.Properties["AlphaMode"] = "ColorAlpha";

        Connect(graph, texCoord, "UV", materialTexture, "UV");
        Connect(graph, white, "Color", layerOutput, "Color");
        Connect(graph, materialTexture, "A", layerOutput, "AlphaMask");
        Connect(graph, layerOutput, "Color", output, "Color");

        return graph;
    }

    private static NodeGraph CreateOffsetShadowCaptureVertexSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        AddTemplateFrame(graph, 40, 40, 680, 180, "Offset Shadow Capture Vertex\n这里只负责偏移轮廓，不负责颜色。导出时会自动作为 Vertex 图参与编译。", 0.30f, 0.44f, 0.74f);

        var ownerOffset = graph.AddNode(NodeKind.ControlObjectPosition, 120, 280);
        ownerOffset.Properties["Name"] = "(OffscreenOwner)";
        ownerOffset.Properties["Item"] = "XYZ";

        var baseDirection = CreateFloat3Node(graph, 420, 240, "-0.05", "-0.05", "0.05");
        var directionScale = CreateFloat3Node(graph, 420, 400, "0.1", "0.1", "0.1");
        var scaledDirection = CreateTypedMathNode(graph, NodeKind.Multiply, 740, 320, "Float3");
        var finalDirection = CreateTypedMathNode(graph, NodeKind.Add, 1040, 320, "Float3");
        var directionSplit = graph.AddNode(NodeKind.SplitXYZ, 1340, 320);
        var zeroScalar = CreateScalarNode(graph, 1340, 500, "0.0");
        var vertexOffset = graph.AddNode(NodeKind.MergeXYZ, 1640, 360);
        var output = graph.AddNode(NodeKind.Output, 1940, 360);

        Connect(graph, ownerOffset, "Value", scaledDirection, "A");
        Connect(graph, directionScale, "Value", scaledDirection, "B");
        Connect(graph, baseDirection, "Value", finalDirection, "A");
        Connect(graph, scaledDirection, "Result", finalDirection, "B");
        Connect(graph, finalDirection, "Result", directionSplit, "Value");
        Connect(graph, directionSplit, "X", vertexOffset, "X");
        Connect(graph, directionSplit, "Y", vertexOffset, "Y");
        Connect(graph, zeroScalar, "Value", vertexOffset, "Z");
        Connect(graph, vertexOffset, "Result", output, "VertexOffset");

        return graph;
    }

    private static GraphNode AddTemplateFrame(NodeGraph graph, float x, float y, float width, float height, string title, float r, float g, float b)
    {
        var frame = graph.AddNode(NodeKind.Frame, x, y);
        frame.Properties["Title"] = title;
        frame.Properties["Width"] = width.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["Height"] = height.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["TintR"] = r.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["TintG"] = g.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["TintB"] = b.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        frame.Properties["Opacity"] = "0.16";
        return frame;
    }

    private static string FindLiveOutlineAssetPath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "SOME NEW MME", "Live Outline_MME", "Live Outline_MME", fileName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SOME NEW MME", "Live Outline_MME", "Live Outline_MME", fileName),
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return string.Empty;
    }

    private static NodeGraph CreateTransformVectorSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var localNormal = graph.AddNode(NodeKind.LocalNormal, 80, 180);
        var transformVector = graph.AddNode(NodeKind.TransformVector, 360, 180);
        transformVector.Properties["VectorType"] = "Normal";
        transformVector.Properties["SourceSpace"] = "Local";
        transformVector.Properties["TargetSpace"] = "View";
        transformVector.Properties["NormalizeOutput"] = "True";

        var half = graph.AddNode(NodeKind.Float4Value, 660, 80);
        half.Properties["X"] = "0.5";
        half.Properties["Y"] = "0.5";
        half.Properties["Z"] = "0.5";
        half.Properties["W"] = "0.0";

        var bias = graph.AddNode(NodeKind.Float4Value, 660, 280);
        bias.Properties["X"] = "0.5";
        bias.Properties["Y"] = "0.5";
        bias.Properties["Z"] = "0.5";
        bias.Properties["W"] = "0.0";

        var multiply = graph.AddNode(NodeKind.Multiply, 960, 150);
        multiply.Properties["Type"] = "Float4";
        var add = graph.AddNode(NodeKind.Add, 1240, 150);
        add.Properties["Type"] = "Float4";
        var saturate = graph.AddNode(NodeKind.Saturate, 1520, 150);
        saturate.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 1800, 150);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = localNormal.Id,
            SourcePin = "Value",
            TargetNodeId = transformVector.Id,
            TargetPin = "Vector",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = transformVector.Id,
            SourcePin = "Value",
            TargetNodeId = multiply.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = half.Id,
            SourcePin = "Value",
            TargetNodeId = multiply.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = multiply.Id,
            SourcePin = "Result",
            TargetNodeId = add.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = bias.Id,
            SourcePin = "Value",
            TargetNodeId = add.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = add.Id,
            SourcePin = "Result",
            TargetNodeId = saturate.Id,
            TargetPin = "Color",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = saturate.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateTransformPositionSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var localPosition = graph.AddNode(NodeKind.LocalPosition, 80, 180);
        var transformPosition = graph.AddNode(NodeKind.TransformPosition, 360, 180);
        transformPosition.Properties["SourceSpace"] = "Local";
        transformPosition.Properties["TargetSpace"] = "View";

        var scale = graph.AddNode(NodeKind.Float4Value, 660, 80);
        scale.Properties["X"] = "0.08";
        scale.Properties["Y"] = "-0.08";
        scale.Properties["Z"] = "-0.08";
        scale.Properties["W"] = "0.0";

        var bias = graph.AddNode(NodeKind.Float4Value, 660, 280);
        bias.Properties["X"] = "0.5";
        bias.Properties["Y"] = "0.5";
        bias.Properties["Z"] = "0.5";
        bias.Properties["W"] = "0.0";

        var multiply = graph.AddNode(NodeKind.Multiply, 960, 150);
        multiply.Properties["Type"] = "Float4";
        var add = graph.AddNode(NodeKind.Add, 1240, 150);
        add.Properties["Type"] = "Float4";
        var saturate = graph.AddNode(NodeKind.Saturate, 1520, 150);
        saturate.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 1800, 150);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = localPosition.Id,
            SourcePin = "Value",
            TargetNodeId = transformPosition.Id,
            TargetPin = "Position",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = transformPosition.Id,
            SourcePin = "Value",
            TargetNodeId = multiply.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = scale.Id,
            SourcePin = "Value",
            TargetNodeId = multiply.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = multiply.Id,
            SourcePin = "Result",
            TargetNodeId = add.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = bias.Id,
            SourcePin = "Value",
            TargetNodeId = add.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = add.Id,
            SourcePin = "Result",
            TargetNodeId = saturate.Id,
            TargetPin = "Color",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = saturate.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateTextureCoordinateSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var coords = graph.AddNode(NodeKind.TextureCoordinate, 80, 120);
        coords.Properties["Source"] = "MainUV";

        var half = graph.AddNode(NodeKind.Float4Value, 380, 120);
        half.Properties["X"] = "0.5";
        half.Properties["Y"] = "0.5";
        half.Properties["Z"] = "0.5";
        half.Properties["W"] = "0.0";

        var bias = graph.AddNode(NodeKind.Float4Value, 380, 260);
        bias.Properties["X"] = "0.5";
        bias.Properties["Y"] = "0.5";
        bias.Properties["Z"] = "0.5";
        bias.Properties["W"] = "0.0";

        var multiply = graph.AddNode(NodeKind.Multiply, 700, 150);
        multiply.Properties["Type"] = "Float4";
        var add = graph.AddNode(NodeKind.Add, 980, 150);
        add.Properties["Type"] = "Float4";
        var saturate = graph.AddNode(NodeKind.Saturate, 1260, 150);
        saturate.Properties["Type"] = "Float4";
        var output = graph.AddNode(NodeKind.Output, 1540, 150);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = coords.Id,
            SourcePin = "Reflection",
            TargetNodeId = multiply.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = half.Id,
            SourcePin = "Value",
            TargetNodeId = multiply.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = multiply.Id,
            SourcePin = "Result",
            TargetNodeId = add.Id,
            TargetPin = "A",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = bias.Id,
            SourcePin = "Value",
            TargetNodeId = add.Id,
            TargetPin = "B",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = add.Id,
            SourcePin = "Result",
            TargetNodeId = saturate.Id,
            TargetPin = "Color",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = saturate.Id,
            SourcePin = "Result",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateFakeEnvReflectionBasicSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var coords = graph.AddNode(NodeKind.TextureCoordinate, 80, 90);
        coords.Properties["Source"] = "MainUV";

        var cameraData = graph.AddNode(NodeKind.CameraData, 80, 250);
        var baseTexture = graph.AddNode(NodeKind.MaterialTexture, 380, 80);
        var strength = graph.AddNode(NodeKind.Scalar, 380, 240);
        strength.Properties["Value"] = "0.35";
        var tint = graph.AddNode(NodeKind.Color, 380, 360);
        tint.Properties["R"] = "1.0";
        tint.Properties["G"] = "0.96";
        tint.Properties["B"] = "0.92";
        tint.Properties["A"] = "1.0";
        var fakeReflection = graph.AddNode(NodeKind.FakeEnvReflection, 760, 150);
        fakeReflection.Properties["ResourceName"] = "studio_env.png";
        fakeReflection.Properties["AddressMode"] = "Wrap";
        fakeReflection.Properties["FilterMode"] = "Linear";
        fakeReflection.Properties["BlendMode"] = "Add";
        fakeReflection.Properties["ProjectionMode"] = "Reflect";
        fakeReflection.Properties["HybridMix"] = "0.0";
        fakeReflection.Properties["MaskMode"] = "MaskFresnel";
        fakeReflection.Properties["Roughness"] = "0.0";
        fakeReflection.Properties["Strength"] = "0.35";
        fakeReflection.Properties["Fresnel"] = "1.0";
        fakeReflection.Properties["FresnelPower"] = "5.0";
        fakeReflection.Properties["RimMin"] = "0.18";
        fakeReflection.Properties["RimMax"] = "0.82";

        var output = graph.AddNode(NodeKind.Output, 1120, 150);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = coords.Id,
            SourcePin = "UV",
            TargetNodeId = baseTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = baseTexture.Id,
            SourcePin = "Color",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "BaseColor",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = strength.Id,
            SourcePin = "Value",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "Strength",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = tint.Id,
            SourcePin = "Color",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "Tint",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = coords.Id,
            SourcePin = "Normal",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "Normal",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = cameraData.Id,
            SourcePin = "ViewVector",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "ViewDir",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = fakeReflection.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateMatCapReflectionSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var coords = graph.AddNode(NodeKind.TextureCoordinate, 80, 90);
        coords.Properties["Source"] = "MainUV";

        var baseTexture = graph.AddNode(NodeKind.MaterialTexture, 360, 80);
        var matCapUv = graph.AddNode(NodeKind.MatCapUv, 80, 250);
        var matCapTexture = graph.AddNode(NodeKind.ExternalTexture, 360, 230);
        matCapTexture.Properties["ResourceName"] = "matcap_reflection.png";
        matCapTexture.Properties["TextureMode"] = "Static";
        matCapTexture.Properties["AddressMode"] = "Clamp";
        matCapTexture.Properties["FilterMode"] = "Linear";
        var maskTexture = graph.AddNode(NodeKind.ExternalTexture, 360, 390);
        maskTexture.Properties["ResourceName"] = "matcap_mask.png";
        maskTexture.Properties["TextureMode"] = "Static";
        maskTexture.Properties["AddressMode"] = "Wrap";
        maskTexture.Properties["FilterMode"] = "Linear";
        maskTexture.Properties["ColorSpace"] = "NonColor";
        var splitMask = graph.AddNode(NodeKind.SplitColor, 640, 410);
        var blend = graph.AddNode(NodeKind.MatCapBlendMode, 900, 170);
        blend.Properties["BlendMode"] = "Add";
        blend.Properties["Alpha"] = "1.0";
        blend.Properties["TintR"] = "1.0";
        blend.Properties["TintG"] = "0.96";
        blend.Properties["TintB"] = "0.92";
        blend.Properties["Intensity"] = "1.0";
        var output = graph.AddNode(NodeKind.Output, 1240, 170);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = coords.Id,
            SourcePin = "UV",
            TargetNodeId = baseTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = matCapUv.Id,
            SourcePin = "UV",
            TargetNodeId = matCapTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = coords.Id,
            SourcePin = "UV",
            TargetNodeId = maskTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = baseTexture.Id,
            SourcePin = "Color",
            TargetNodeId = blend.Id,
            TargetPin = "Base",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = matCapTexture.Id,
            SourcePin = "Color",
            TargetNodeId = blend.Id,
            TargetPin = "MatCap",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = maskTexture.Id,
            SourcePin = "Color",
            TargetNodeId = splitMask.Id,
            TargetPin = "Color",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = splitMask.Id,
            SourcePin = "R",
            TargetNodeId = blend.Id,
            TargetPin = "Mask",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = blend.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateFakeEnvReflectionHdriSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var coords = graph.AddNode(NodeKind.TextureCoordinate, 80, 90);
        coords.Properties["Source"] = "MainUV";

        var cameraData = graph.AddNode(NodeKind.CameraData, 80, 250);
        var baseTexture = graph.AddNode(NodeKind.MaterialTexture, 380, 80);
        var maskTexture = graph.AddNode(NodeKind.ExternalTexture, 380, 230);
        maskTexture.Properties["ResourceName"] = "reflection_mask.png";
        maskTexture.Properties["TextureMode"] = "Static";
        maskTexture.Properties["AddressMode"] = "Wrap";
        maskTexture.Properties["FilterMode"] = "Linear";
        maskTexture.Properties["ColorSpace"] = "NonColor";
        var splitMask = graph.AddNode(NodeKind.SplitColor, 650, 250);
        var strength = graph.AddNode(NodeKind.Scalar, 380, 390);
        strength.Properties["Value"] = "0.35";
        var tint = graph.AddNode(NodeKind.Color, 380, 510);
        tint.Properties["R"] = "1.0";
        tint.Properties["G"] = "0.96";
        tint.Properties["B"] = "0.92";
        tint.Properties["A"] = "1.0";
        var fakeReflection = graph.AddNode(NodeKind.FakeEnvReflection, 900, 170);
        fakeReflection.Properties["ResourceName"] = "studio_env.png";
        fakeReflection.Properties["AddressMode"] = "Wrap";
        fakeReflection.Properties["FilterMode"] = "Linear";
        fakeReflection.Properties["BlendMode"] = "Add";
        fakeReflection.Properties["ProjectionMode"] = "Reflect";
        fakeReflection.Properties["HybridMix"] = "0.0";
        fakeReflection.Properties["MaskMode"] = "MaskRimFresnel";
        fakeReflection.Properties["Roughness"] = "0.05";
        fakeReflection.Properties["Strength"] = "0.35";
        fakeReflection.Properties["Fresnel"] = "1.0";
        fakeReflection.Properties["FresnelPower"] = "5.0";
        fakeReflection.Properties["RimMin"] = "0.12";
        fakeReflection.Properties["RimMax"] = "0.82";

        var output = graph.AddNode(NodeKind.Output, 1260, 170);

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = coords.Id,
            SourcePin = "UV",
            TargetNodeId = baseTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = baseTexture.Id,
            SourcePin = "Color",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "BaseColor",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = coords.Id,
            SourcePin = "UV",
            TargetNodeId = maskTexture.Id,
            TargetPin = "UV",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = maskTexture.Id,
            SourcePin = "Color",
            TargetNodeId = splitMask.Id,
            TargetPin = "Color",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = splitMask.Id,
            SourcePin = "R",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "Mask",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = strength.Id,
            SourcePin = "Value",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "Strength",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = tint.Id,
            SourcePin = "Color",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "Tint",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = coords.Id,
            SourcePin = "Normal",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "Normal",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = cameraData.Id,
            SourcePin = "ViewVector",
            TargetNodeId = fakeReflection.Id,
            TargetPin = "ViewDir",
        });

        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = fakeReflection.Id,
            SourcePin = "Color",
            TargetNodeId = output.Id,
            TargetPin = "Color",
        });

        return graph;
    }

    private static NodeGraph CreateMatcapAtalsSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var uv = graph.AddNode(NodeKind.TexCoord, 80, 120);
        var baseTexture = graph.AddNode(NodeKind.MaterialTexture, 340, 40);

        var idMap = graph.AddNode(NodeKind.ExternalTexture, 340, 220);
        ConfigureExternalTextureNode(idMap, "girl007a_body01_a.png", "Clamp", "Point", "NonColor");

        var materialMap = graph.AddNode(NodeKind.ExternalTexture, 340, 400);
        ConfigureExternalTextureNode(materialMap, "girl007a_body01_r.png", "Clamp", "Linear", "NonColor");

        var worldNormal = graph.AddNode(NodeKind.WorldNormal, 80, 600);
        var viewDirection = graph.AddNode(NodeKind.ViewDirection, 80, 780);
        var matcapLightDir = CreateFloat4Node(graph, 340, 620, "0.0", "1.0", "0.0", "0.0");

        var materialRBase = CreateScalarNode(graph, 860, 80, "0.0");
        var materialLodScale = CreateScalarNode(graph, 860, 160, "1.0");
        var lodScale = CreateScalarNode(graph, 860, 240, "1.0");
        var matcapDiffuseScale = CreateScalarNode(graph, 860, 320, "1.0");
        var matcapScale = CreateScalarNode(graph, 860, 400, "1.0");
        var matcapExpR = CreateScalarNode(graph, 860, 480, "1.0");
        var matcapScaleR = CreateScalarNode(graph, 860, 560, "1.0");
        var matcapExpB = CreateScalarNode(graph, 860, 640, "1.0");
        var matcapScaleB = CreateScalarNode(graph, 860, 720, "8.0");
        var halfScalar = CreateScalarNode(graph, 860, 800, "0.5");
        var lodBaseValue = CreateScalarNode(graph, 860, 880, "6.0");
        var logScaleValue = CreateScalarNode(graph, 860, 960, "1.2");
        var maxMipLevel = CreateScalarNode(graph, 860, 1040, "8.0");
        var grayBaseValue = CreateScalarNode(graph, 860, 1120, "0.08");
        var fresnelExponentScale = CreateScalarNode(graph, 860, 1200, "-9.28");
        var fresnelDiffuseScale = CreateScalarNode(graph, 860, 1280, "50.0");
        var softSpecScale = CreateScalarNode(graph, 860, 1360, "0.3");
        var zeroScalar = CreateScalarNode(graph, 860, 1440, "0.0");
        var fresnelNegScale = CreateScalarNode(graph, 860, 1520, "-1.04");
        var fresnelPosScale = CreateScalarNode(graph, 860, 1600, "1.04");
        var grayWeights = CreateFloat4Node(graph, 1140, 1120, "0.299", "0.587", "0.114", "0.0");
        var fresnelRatioMul = CreateFloat4Node(graph, 1140, 1280, "-1.0", "-0.0275", "-0.572", "0.022");
        var fresnelRatioAdd = CreateFloat4Node(graph, 1140, 1360, "1.0", "0.0425", "1.04", "-0.04");

        var xDirCross = graph.AddNode(NodeKind.Cross, 620, 640);
        var xDirNormalize = graph.AddNode(NodeKind.Normalize, 880, 640);
        var yDirCross = graph.AddNode(NodeKind.Cross, 1140, 760);
        var yDirNormalize = graph.AddNode(NodeKind.Normalize, 1400, 760);
        var matcapUvX = graph.AddNode(NodeKind.Dot, 1140, 560);
        var matcapUvY = graph.AddNode(NodeKind.Dot, 1660, 760);
        var matcapUvBasis = graph.AddNode(NodeKind.AppendFloat2, 1920, 660);
        var matcapUvHalf = CreateTypedMathNode(graph, NodeKind.Multiply, 2180, 660, "Float2");
        var matcapUvBias = CreateTypedMathNode(graph, NodeKind.Add, 2440, 660, "Float2");
        var matcapUvLocal = CreateTypedMathNode(graph, NodeKind.Saturate, 2700, 660, "Float2");

        var materialRAdd = CreateTypedMathNode(graph, NodeKind.Add, 1400, 80, "Float1");
        var materialRSaturate = CreateTypedMathNode(graph, NodeKind.Saturate, 1660, 80, "Float1");
        var lodStrength = CreateTypedMathNode(graph, NodeKind.Multiply, 1400, 160, "Float1");
        var lodNormalized = CreateTypedMathNode(graph, NodeKind.Saturate, 1660, 160, "Float1");
        var lodLog = graph.AddNode(NodeKind.Logarithm, 1920, 160);
        var lodLogSplit = graph.AddNode(NodeKind.SplitXYZW, 2180, 160);
        var lodLogScaled = CreateTypedMathNode(graph, NodeKind.Multiply, 2440, 160, "Float1");
        var lodBias = CreateTypedMathNode(graph, NodeKind.Add, 2700, 160, "Float1");
        var lodValue = CreateTypedMathNode(graph, NodeKind.Multiply, 2960, 160, "Float1");
        var mipControl = CreateTypedMathNode(graph, NodeKind.Divide, 3220, 160, "Float1");

        var diffuseGray = graph.AddNode(NodeKind.Dot, 1400, 1120);
        var diffuseBase = CreateTypedMathNode(graph, NodeKind.Multiply, 1400, 1200, "Float1");
        var diffuseGrayLerp = CreateTypedMathNode(graph, NodeKind.Lerp, 1660, 1160, "Float1");
        var diffuseGrayColor = graph.AddNode(NodeKind.ComposeColor, 1920, 1160);
        var diffuseLightnessColor = CreateTypedMathNode(graph, NodeKind.Lerp, 2180, 1160, "Float4");
        var diffuseLightnessSplit = graph.AddNode(NodeKind.SplitColor, 2440, 1160);

        var vdotn = graph.AddNode(NodeKind.Dot, 1400, 1480);
        var fresnelInput = CreateTypedMathNode(graph, NodeKind.Multiply, 1660, 1480, "Float1");
        var fresnelExp = graph.AddNode(NodeKind.Exponent, 1920, 1480);
        var fresnelExpSplit = graph.AddNode(NodeKind.SplitXYZW, 2180, 1480);
        var fresnelRatioWeighted = CreateTypedMathNode(graph, NodeKind.Multiply, 1660, 1320, "Float4");
        var fresnelRatio = CreateTypedMathNode(graph, NodeKind.Add, 1920, 1320, "Float4");
        var fresnelRatioSplit = graph.AddNode(NodeKind.SplitXYZW, 2180, 1320);
        var fresnelRatioXSquared = CreateTypedMathNode(graph, NodeKind.Power, 2440, 1280, "Float1");
        fresnelRatioXSquared.Properties["Exponent"] = "2.0";
        var fresnelRatioMin = CreateTypedMathNode(graph, NodeKind.Min, 2700, 1280, "Float1");
        var fresnelRatioShared = CreateTypedMathNode(graph, NodeKind.Multiply, 2960, 1280, "Float1");
        var fresnelXScale = CreateTypedMathNode(graph, NodeKind.Multiply, 3220, 1220, "Float1");
        var fresnelYScale = CreateTypedMathNode(graph, NodeKind.Multiply, 3220, 1340, "Float1");
        var fresnelXBase = CreateTypedMathNode(graph, NodeKind.Add, 3480, 1220, "Float1");
        var fresnelYBase = CreateTypedMathNode(graph, NodeKind.Add, 3480, 1340, "Float1");
        var fresnelX = CreateTypedMathNode(graph, NodeKind.Add, 3740, 1220, "Float1");
        var fresnelY = CreateTypedMathNode(graph, NodeKind.Add, 3740, 1340, "Float1");
        var fresnelDiffuseMask = CreateTypedMathNode(graph, NodeKind.Multiply, 2700, 1480, "Float1");
        var fresnelDiffuseMaskSaturate = CreateTypedMathNode(graph, NodeKind.Saturate, 2960, 1480, "Float1");
        var fresnelYWeighted = CreateTypedMathNode(graph, NodeKind.Multiply, 3220, 1480, "Float1");

        var matcapWeightBase = CreateTypedMathNode(graph, NodeKind.Multiply, 4000, 1180, "Float4");
        var matcapWeight = CreateTypedMathNode(graph, NodeKind.Add, 4260, 1180, "Float4");

        var matcapAtlasR = graph.AddNode(NodeKind.MatCapAtlasSample, 3480, 420);
        ConfigureMatCapAtlasNode(matcapAtlasR, "MatCapALtals1.png");
        var matcapAtlasB = graph.AddNode(NodeKind.MatCapAtlasSample, 3480, 680);
        ConfigureMatCapAtlasNode(matcapAtlasB, "MatCapALtals1.png");
        var matcapAtlasRPower = CreateTypedMathNode(graph, NodeKind.Power, 3740, 420, "Float4");
        var matcapAtlasRScaled = CreateTypedMathNode(graph, NodeKind.Multiply, 4000, 420, "Float4");
        var matcapAtlasBPower = CreateTypedMathNode(graph, NodeKind.Power, 3740, 680, "Float4");
        var matcapAtlasBScaled = CreateTypedMathNode(graph, NodeKind.Multiply, 4000, 680, "Float4");
        var matcapAtlasBMasked = CreateTypedMathNode(graph, NodeKind.Multiply, 4260, 680, "Float4");
        var matcapAtlasCombined = CreateTypedMathNode(graph, NodeKind.Add, 4520, 540, "Float4");

        var matcapColor = CreateTypedMathNode(graph, NodeKind.Multiply, 4780, 720, "Float4");
        var softSpecColor = CreateTypedMathNode(graph, NodeKind.Multiply, 5040, 600, "Float4");
        var finalSpecular = CreateTypedMathNode(graph, NodeKind.Lerp, 5300, 660, "Float4");
        var finalSpecularSplit = graph.AddNode(NodeKind.SplitColor, 5560, 660);
        var finalSpecularColor = graph.AddNode(NodeKind.ComposeColor, 5820, 660);
        var finalColor = CreateTypedMathNode(graph, NodeKind.Add, 6080, 660, "Float4");
        var output = graph.AddNode(NodeKind.Output, 6340, 660);

        Connect(graph, uv, "UV", baseTexture, "UV");
        Connect(graph, uv, "UV", idMap, "UV");
        Connect(graph, uv, "UV", materialMap, "UV");

        Connect(graph, viewDirection, "Value", xDirCross, "A");
        Connect(graph, matcapLightDir, "Value", xDirCross, "B");
        Connect(graph, xDirCross, "Result", xDirNormalize, "Value");
        Connect(graph, viewDirection, "Value", yDirCross, "A");
        Connect(graph, xDirNormalize, "Result", yDirCross, "B");
        Connect(graph, yDirCross, "Result", yDirNormalize, "Value");
        Connect(graph, xDirNormalize, "Result", matcapUvX, "A");
        Connect(graph, worldNormal, "Value", matcapUvX, "B");
        Connect(graph, yDirNormalize, "Result", matcapUvY, "A");
        Connect(graph, worldNormal, "Value", matcapUvY, "B");
        Connect(graph, matcapUvX, "Result", matcapUvBasis, "X");
        Connect(graph, matcapUvY, "Result", matcapUvBasis, "Y");
        Connect(graph, matcapUvBasis, "Result", matcapUvHalf, "A");
        Connect(graph, halfScalar, "Value", matcapUvHalf, "B");
        Connect(graph, matcapUvHalf, "Result", matcapUvBias, "A");
        Connect(graph, halfScalar, "Value", matcapUvBias, "B");
        Connect(graph, matcapUvBias, "Result", matcapUvLocal, "Color");

        Connect(graph, materialMap, "R", materialRAdd, "A");
        Connect(graph, materialRBase, "Value", materialRAdd, "B");
        Connect(graph, materialRAdd, "Result", materialRSaturate, "Color");

        Connect(graph, materialMap, "G", lodStrength, "A");
        Connect(graph, materialLodScale, "Value", lodStrength, "B");
        Connect(graph, lodStrength, "Result", lodNormalized, "Color");
        Connect(graph, lodNormalized, "Result", lodLog, "Value");
        Connect(graph, lodLog, "Result", lodLogSplit, "Value");
        Connect(graph, lodLogSplit, "X", lodLogScaled, "A");
        Connect(graph, logScaleValue, "Value", lodLogScaled, "B");
        Connect(graph, lodLogScaled, "Result", lodBias, "A");
        Connect(graph, lodBaseValue, "Value", lodBias, "B");
        Connect(graph, lodBias, "Result", lodValue, "A");
        Connect(graph, lodScale, "Value", lodValue, "B");
        Connect(graph, lodValue, "Result", mipControl, "A");
        Connect(graph, maxMipLevel, "Value", mipControl, "B");

        Connect(graph, baseTexture, "Color", diffuseGray, "A");
        Connect(graph, grayWeights, "Value", diffuseGray, "B");
        Connect(graph, grayBaseValue, "Value", diffuseBase, "A");
        Connect(graph, matcapDiffuseScale, "Value", diffuseBase, "B");
        Connect(graph, diffuseBase, "Result", diffuseGrayLerp, "A");
        Connect(graph, diffuseGray, "Result", diffuseGrayLerp, "B");
        Connect(graph, materialRSaturate, "Result", diffuseGrayLerp, "T");
        Connect(graph, diffuseGrayLerp, "Result", diffuseGrayColor, "R");
        Connect(graph, diffuseGrayLerp, "Result", diffuseGrayColor, "G");
        Connect(graph, diffuseGrayLerp, "Result", diffuseGrayColor, "B");
        Connect(graph, zeroScalar, "Value", diffuseGrayColor, "A");
        Connect(graph, diffuseGrayColor, "Color", diffuseLightnessColor, "A");
        Connect(graph, baseTexture, "Color", diffuseLightnessColor, "B");
        Connect(graph, materialRSaturate, "Result", diffuseLightnessColor, "T");
        Connect(graph, diffuseLightnessColor, "Result", diffuseLightnessSplit, "Color");

        Connect(graph, viewDirection, "Value", vdotn, "A");
        Connect(graph, worldNormal, "Value", vdotn, "B");
        Connect(graph, vdotn, "Result", fresnelInput, "A");
        Connect(graph, fresnelExponentScale, "Value", fresnelInput, "B");
        Connect(graph, fresnelInput, "Result", fresnelExp, "Value");
        Connect(graph, fresnelExp, "Result", fresnelExpSplit, "Value");
        Connect(graph, fresnelRatioMul, "Value", fresnelRatioWeighted, "A");
        Connect(graph, lodNormalized, "Result", fresnelRatioWeighted, "B");
        Connect(graph, fresnelRatioWeighted, "Result", fresnelRatio, "A");
        Connect(graph, fresnelRatioAdd, "Value", fresnelRatio, "B");
        Connect(graph, fresnelRatio, "Result", fresnelRatioSplit, "Value");
        Connect(graph, fresnelRatioSplit, "X", fresnelRatioXSquared, "Value");
        Connect(graph, fresnelRatioXSquared, "Result", fresnelRatioMin, "A");
        Connect(graph, fresnelExpSplit, "X", fresnelRatioMin, "B");
        Connect(graph, fresnelRatioMin, "Result", fresnelRatioShared, "A");
        Connect(graph, fresnelRatioSplit, "X", fresnelRatioShared, "B");
        Connect(graph, fresnelRatioShared, "Result", fresnelXScale, "A");
        Connect(graph, fresnelNegScale, "Value", fresnelXScale, "B");
        Connect(graph, fresnelRatioShared, "Result", fresnelYScale, "A");
        Connect(graph, fresnelPosScale, "Value", fresnelYScale, "B");
        Connect(graph, fresnelXScale, "Result", fresnelXBase, "A");
        Connect(graph, fresnelRatioSplit, "Y", fresnelXBase, "B");
        Connect(graph, fresnelXBase, "Result", fresnelX, "A");
        Connect(graph, fresnelRatioSplit, "Z", fresnelX, "B");
        Connect(graph, fresnelYScale, "Result", fresnelYBase, "A");
        Connect(graph, fresnelRatioSplit, "Y", fresnelYBase, "B");
        Connect(graph, fresnelYBase, "Result", fresnelY, "A");
        Connect(graph, fresnelRatioSplit, "W", fresnelY, "B");
        Connect(graph, diffuseLightnessSplit, "G", fresnelDiffuseMask, "A");
        Connect(graph, fresnelDiffuseScale, "Value", fresnelDiffuseMask, "B");
        Connect(graph, fresnelDiffuseMask, "Result", fresnelDiffuseMaskSaturate, "Color");
        Connect(graph, fresnelY, "Result", fresnelYWeighted, "A");
        Connect(graph, fresnelDiffuseMaskSaturate, "Result", fresnelYWeighted, "B");

        Connect(graph, diffuseLightnessColor, "Result", matcapWeightBase, "A");
        Connect(graph, fresnelX, "Result", matcapWeightBase, "B");
        Connect(graph, matcapWeightBase, "Result", matcapWeight, "A");
        Connect(graph, fresnelYWeighted, "Result", matcapWeight, "B");

        Connect(graph, matcapUvLocal, "Result", matcapAtlasR, "UV");
        Connect(graph, matcapUvLocal, "Result", matcapAtlasB, "UV");
        Connect(graph, idMap, "R", matcapAtlasR, "Index");
        Connect(graph, idMap, "B", matcapAtlasB, "Index");
        Connect(graph, mipControl, "Result", matcapAtlasR, "MipControl");
        Connect(graph, mipControl, "Result", matcapAtlasB, "MipControl");
        Connect(graph, matcapAtlasR, "Color", matcapAtlasRPower, "Value");
        Connect(graph, matcapExpR, "Value", matcapAtlasRPower, "Exponent");
        Connect(graph, matcapAtlasRPower, "Result", matcapAtlasRScaled, "A");
        Connect(graph, matcapScaleR, "Value", matcapAtlasRScaled, "B");
        Connect(graph, matcapAtlasB, "Color", matcapAtlasBPower, "Value");
        Connect(graph, matcapExpB, "Value", matcapAtlasBPower, "Exponent");
        Connect(graph, matcapAtlasBPower, "Result", matcapAtlasBScaled, "A");
        Connect(graph, matcapScaleB, "Value", matcapAtlasBScaled, "B");
        Connect(graph, matcapAtlasBScaled, "Result", matcapAtlasBMasked, "A");
        Connect(graph, idMap, "G", matcapAtlasBMasked, "B");
        Connect(graph, matcapAtlasRScaled, "Result", matcapAtlasCombined, "A");
        Connect(graph, matcapAtlasBMasked, "Result", matcapAtlasCombined, "B");

        Connect(graph, matcapAtlasCombined, "Result", matcapColor, "A");
        Connect(graph, matcapWeight, "Result", matcapColor, "B");
        Connect(graph, matcapColor, "Result", softSpecColor, "A");
        Connect(graph, softSpecScale, "Value", softSpecColor, "B");
        Connect(graph, softSpecColor, "Result", finalSpecular, "A");
        Connect(graph, matcapColor, "Result", finalSpecular, "B");
        Connect(graph, matcapScale, "Value", finalSpecular, "T");
        Connect(graph, finalSpecular, "Result", finalSpecularSplit, "Color");
        Connect(graph, finalSpecularSplit, "R", finalSpecularColor, "R");
        Connect(graph, finalSpecularSplit, "G", finalSpecularColor, "G");
        Connect(graph, finalSpecularSplit, "B", finalSpecularColor, "B");
        Connect(graph, zeroScalar, "Value", finalSpecularColor, "A");
        Connect(graph, baseTexture, "Color", finalColor, "A");
        Connect(graph, finalSpecularColor, "Color", finalColor, "B");
        Connect(graph, finalColor, "Result", output, "Color");
        Connect(graph, baseTexture, "A", output, "Alpha");

        return graph;
    }

    private static NodeGraph CreateGenericRampSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var normal = graph.AddNode(NodeKind.WorldNormal, 100, 120);
        var factor = graph.AddNode(NodeKind.HalfLambert, 360, 120);
        var rowMask = CreateScalarNode(graph, 360, 260, "0.0");
        var ramp = graph.AddNode(NodeKind.GenericRampSample, 620, 160);
        ramp.Properties["ResourceName"] = "ramp.png";
        ramp.Properties["RowCount"] = "8";
        ramp.Properties["RowOffset"] = "0";
        ramp.Properties["FlipY"] = "True";
        var output = graph.AddNode(NodeKind.Output, 900, 160);

        Connect(graph, normal, "Value", factor, "Normal");
        Connect(graph, factor, "Result", ramp, "Factor");
        Connect(graph, rowMask, "Value", ramp, "RowMask");
        Connect(graph, ramp, "Color", output, "Color");

        return graph;
    }

    private static NodeGraph CreateGenshinRampSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var normal = graph.AddNode(NodeKind.WorldNormal, 100, 100);
        var factor = graph.AddNode(NodeKind.HalfLambert, 360, 100);
        var regionMask = CreateScalarNode(graph, 360, 240, "0.0");
        var coolWarmMix = CreateScalarNode(graph, 360, 380, "0.0");
        var ramp = graph.AddNode(NodeKind.GenshinRamp, 640, 180);
        ramp.Properties["ResourceName"] = "genshin_ramp.png";
        ramp.Properties["WarmRegionCount"] = "5";
        ramp.Properties["RowOffset"] = "0";
        ramp.Properties["FlipY"] = "False";
        var output = graph.AddNode(NodeKind.Output, 930, 180);

        Connect(graph, normal, "Value", factor, "Normal");
        Connect(graph, factor, "Result", ramp, "Factor");
        Connect(graph, regionMask, "Value", ramp, "RegionMask");
        Connect(graph, coolWarmMix, "Value", ramp, "CoolWarmMix");
        Connect(graph, ramp, "Color", output, "Color");

        return graph;
    }

    private static NodeGraph CreateSnowBreakRampSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var normal = graph.AddNode(NodeKind.WorldNormal, 100, 120);
        var factor = graph.AddNode(NodeKind.HalfLambert, 360, 120);
        var rowMask = CreateScalarNode(graph, 360, 260, "0.0");
        var ramp = graph.AddNode(NodeKind.SnowBreakRamp, 620, 160);
        ramp.Properties["ResourceName"] = "snowbreak_ramp.png";
        ramp.Properties["RowCount"] = "8";
        ramp.Properties["RowOffset"] = "0";
        ramp.Properties["FlipY"] = "True";
        var output = graph.AddNode(NodeKind.Output, 900, 160);

        Connect(graph, normal, "Value", factor, "Normal");
        Connect(graph, factor, "Result", ramp, "Factor");
        Connect(graph, rowMask, "Value", ramp, "RowMask");
        Connect(graph, ramp, "Color", output, "Color");

        return graph;
    }

    private static NodeGraph CreateStarRailRampSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var normal = graph.AddNode(NodeKind.WorldNormal, 100, 120);
        var factor = graph.AddNode(NodeKind.HalfLambert, 360, 120);
        var rowMask = CreateScalarNode(graph, 360, 260, "0.0");
        var coolWarmMix = CreateScalarNode(graph, 360, 400, "0.0");
        var coolRamp = graph.AddNode(NodeKind.GenericRampSample, 660, 140);
        coolRamp.Properties["ResourceName"] = "starrail_cool_ramp.png";
        coolRamp.Properties["RowCount"] = "8";
        coolRamp.Properties["RowOffset"] = "0";
        coolRamp.Properties["FlipY"] = "True";
        var warmRamp = graph.AddNode(NodeKind.GenericRampSample, 660, 360);
        warmRamp.Properties["ResourceName"] = "starrail_warm_ramp.png";
        warmRamp.Properties["RowCount"] = "8";
        warmRamp.Properties["RowOffset"] = "0";
        warmRamp.Properties["FlipY"] = "True";
        var blend = CreateTypedMathNode(graph, NodeKind.Lerp, 960, 250, "Float4");
        var output = graph.AddNode(NodeKind.Output, 1240, 250);

        Connect(graph, normal, "Value", factor, "Normal");
        Connect(graph, factor, "Result", coolRamp, "Factor");
        Connect(graph, factor, "Result", warmRamp, "Factor");
        Connect(graph, rowMask, "Value", coolRamp, "RowMask");
        Connect(graph, rowMask, "Value", warmRamp, "RowMask");
        Connect(graph, coolRamp, "Color", blend, "A");
        Connect(graph, warmRamp, "Color", blend, "B");
        Connect(graph, coolWarmMix, "Value", blend, "T");
        Connect(graph, blend, "Result", output, "Color");

        return graph;
    }

    private static NodeGraph CreateSkinPreintegratedLutSampleCore()
    {
        var graph = new NodeGraph
        {
            WorkspaceMode = GraphWorkspaceMode.ObjectMaterial,
        };

        var normal = graph.AddNode(NodeKind.WorldNormal, 100, 120);
        var halfLambert = graph.AddNode(NodeKind.HalfLambert, 360, 120);
        var curvature = CreateScalarNode(graph, 360, 260, "0.5");
        var lut = graph.AddNode(NodeKind.SkinPreintegratedLut, 660, 160);
        lut.Properties["ResourceName"] = "preintegrated_skin_brdf.png";
        var strength = CreateScalarNode(graph, 660, 360, "1.0");
        var skinTint = graph.AddNode(NodeKind.Color, 660, 480);
        skinTint.Properties["R"] = "1.0";
        skinTint.Properties["G"] = "0.78";
        skinTint.Properties["B"] = "0.72";
        skinTint.Properties["A"] = "1.0";
        var tinted = CreateTypedMathNode(graph, NodeKind.Multiply, 960, 160, "Float4");
        var scaled = CreateTypedMathNode(graph, NodeKind.Multiply, 1240, 160, "Float4");
        var output = graph.AddNode(NodeKind.Output, 1500, 160);

        Connect(graph, normal, "Value", halfLambert, "Normal");
        Connect(graph, halfLambert, "Result", lut, "HalfLambert");
        Connect(graph, curvature, "Value", lut, "Curvature");
        Connect(graph, lut, "Color", tinted, "A");
        Connect(graph, skinTint, "Color", tinted, "B");
        Connect(graph, tinted, "Result", scaled, "A");
        Connect(graph, strength, "Value", scaled, "B");
        Connect(graph, scaled, "Result", output, "Color");

        return graph;
    }

    private static GraphNode CreateSnowBreakAtlasUvChain(
        NodeGraph graph,
        float originX,
        float originY,
        GraphNode localAtlasUv,
        GraphNode splitId,
        string idPin,
        GraphNode atlasCellCount,
        GraphNode atlasCellCountMinusOne,
        GraphNode atlasColumns,
        GraphNode atlasInvCols,
        GraphNode atlasInvRows,
        GraphNode atlasInnerSizeX,
        GraphNode atlasInnerSizeY,
        GraphNode atlasPadding)
    {
        var indexScaled = CreateTypedMathNode(graph, NodeKind.Multiply, originX, originY, "Float1");
        var roundedIndex = graph.AddNode(NodeKind.Round, originX + 260, originY);
        var clampedIndex = CreateTypedMathNode(graph, NodeKind.Min, originX + 520, originY, "Float1");
        var rowDivide = CreateTypedMathNode(graph, NodeKind.Divide, originX + 780, originY, "Float1");
        var rowTruncate = graph.AddNode(NodeKind.Truncate, originX + 1040, originY);
        var rowTimesCols = CreateTypedMathNode(graph, NodeKind.Multiply, originX + 1300, originY, "Float1");
        var colIndex = CreateTypedMathNode(graph, NodeKind.Subtract, originX + 1560, originY, "Float1");
        var offsetBaseX = CreateTypedMathNode(graph, NodeKind.Multiply, originX + 1820, originY, "Float1");
        var offsetBaseY = CreateTypedMathNode(graph, NodeKind.Multiply, originX + 2080, originY, "Float1");

        var splitLocalUv = graph.AddNode(NodeKind.SplitXY, originX + 780, originY + 170);
        var scaledLocalX = CreateTypedMathNode(graph, NodeKind.Multiply, originX + 1040, originY + 120, "Float1");
        var scaledLocalY = CreateTypedMathNode(graph, NodeKind.Multiply, originX + 1040, originY + 220, "Float1");
        var offsetLocalX = CreateTypedMathNode(graph, NodeKind.Add, originX + 1300, originY + 120, "Float1");
        var offsetLocalY = CreateTypedMathNode(graph, NodeKind.Add, originX + 1300, originY + 220, "Float1");
        var paddedX = CreateTypedMathNode(graph, NodeKind.Add, originX + 1560, originY + 120, "Float1");
        var paddedY = CreateTypedMathNode(graph, NodeKind.Add, originX + 1560, originY + 220, "Float1");
        var oneMinusY = CreateTypedMathNode(graph, NodeKind.OneMinus, originX + 1820, originY + 220, "Float1");
        var finalUv = graph.AddNode(NodeKind.AppendFloat2, originX + 2080, originY + 170);

        Connect(graph, splitId, idPin, indexScaled, "A");
        Connect(graph, atlasCellCount, "Result", indexScaled, "B");
        Connect(graph, indexScaled, "Result", roundedIndex, "Value");
        Connect(graph, roundedIndex, "Result", clampedIndex, "A");
        Connect(graph, atlasCellCountMinusOne, "Result", clampedIndex, "B");
        Connect(graph, clampedIndex, "Result", rowDivide, "A");
        Connect(graph, atlasColumns, "Value", rowDivide, "B");
        Connect(graph, rowDivide, "Result", rowTruncate, "Value");
        Connect(graph, rowTruncate, "Result", rowTimesCols, "A");
        Connect(graph, atlasColumns, "Value", rowTimesCols, "B");
        Connect(graph, clampedIndex, "Result", colIndex, "A");
        Connect(graph, rowTimesCols, "Result", colIndex, "B");
        Connect(graph, colIndex, "Result", offsetBaseX, "A");
        Connect(graph, atlasInvCols, "Result", offsetBaseX, "B");
        Connect(graph, rowTruncate, "Result", offsetBaseY, "A");
        Connect(graph, atlasInvRows, "Result", offsetBaseY, "B");

        Connect(graph, localAtlasUv, "Result", splitLocalUv, "Value");
        Connect(graph, splitLocalUv, "X", scaledLocalX, "A");
        Connect(graph, atlasInnerSizeX, "Result", scaledLocalX, "B");
        Connect(graph, splitLocalUv, "Y", scaledLocalY, "A");
        Connect(graph, atlasInnerSizeY, "Result", scaledLocalY, "B");
        Connect(graph, scaledLocalX, "Result", offsetLocalX, "A");
        Connect(graph, offsetBaseX, "Result", offsetLocalX, "B");
        Connect(graph, scaledLocalY, "Result", offsetLocalY, "A");
        Connect(graph, offsetBaseY, "Result", offsetLocalY, "B");
        Connect(graph, offsetLocalX, "Result", paddedX, "A");
        Connect(graph, atlasPadding, "Value", paddedX, "B");
        Connect(graph, offsetLocalY, "Result", paddedY, "A");
        Connect(graph, atlasPadding, "Value", paddedY, "B");
        Connect(graph, paddedY, "Result", oneMinusY, "Color");
        Connect(graph, paddedX, "Result", finalUv, "X");
        Connect(graph, oneMinusY, "Result", finalUv, "Y");

        return finalUv;
    }

    private static GraphNode CreateScalarNode(NodeGraph graph, float x, float y, string value)
    {
        var node = graph.AddNode(NodeKind.Scalar, x, y);
        node.Properties["Value"] = value;
        return node;
    }

    private static GraphNode CreateFloat2Node(NodeGraph graph, float x, float y, string valueX, string valueY)
    {
        var node = graph.AddNode(NodeKind.Float2Value, x, y);
        node.Properties["X"] = valueX;
        node.Properties["Y"] = valueY;
        return node;
    }

    private static GraphNode CreateFloat3Node(NodeGraph graph, float x, float y, string valueX, string valueY, string valueZ)
    {
        var node = graph.AddNode(NodeKind.Float3Value, x, y);
        node.Properties["X"] = valueX;
        node.Properties["Y"] = valueY;
        node.Properties["Z"] = valueZ;
        return node;
    }

    private static GraphNode CreateFloat4Node(NodeGraph graph, float x, float y, string valueX, string valueY, string valueZ, string valueW)
    {
        var node = graph.AddNode(NodeKind.Float4Value, x, y);
        node.Properties["X"] = valueX;
        node.Properties["Y"] = valueY;
        node.Properties["Z"] = valueZ;
        node.Properties["W"] = valueW;
        return node;
    }

    private static GraphNode CreateTypedMathNode(NodeGraph graph, NodeKind kind, float x, float y, string type)
    {
        var node = graph.AddNode(kind, x, y);
        node.Properties["Type"] = type;
        return node;
    }

    private static void ConfigureExternalTextureNode(
        GraphNode node,
        string resourceName,
        string addressMode,
        string filterMode,
        string colorSpace,
        string textureMode = "Static",
        string mipLevels = "1")
    {
        node.Properties[ResourceNodeProperties.ResourceName] = resourceName;
        node.Properties[ResourceNodeProperties.TextureMode] = textureMode;
        node.Properties["AddressMode"] = addressMode;
        node.Properties["FilterMode"] = filterMode;
        node.Properties["ColorSpace"] = colorSpace;
        node.Properties[ResourceNodeProperties.MipLevels] = mipLevels;
    }

    private static void ConfigureMatCapAtlasNode(GraphNode node, string resourceName)
    {
        ConfigureExternalTextureNode(node, resourceName, "Clamp", "Linear", "NonColor", "Static", "0");
        node.Properties["IndexMode"] = "Mask";
        node.Properties["AtlasColumns"] = "4.0";
        node.Properties["AtlasRows"] = "4.0";
        node.Properties["Padding"] = "0.0025";
        node.Properties["ManualMipLevel"] = "0";
        node.Properties["MaxMipLevel"] = "8.0";
        node.Properties["FlipY"] = "True";
    }

    private static void Connect(NodeGraph graph, GraphNode source, string sourcePin, GraphNode target, string targetPin)
    {
        graph.AddOrReplaceConnection(new GraphConnection
        {
            SourceNodeId = source.Id,
            SourcePin = sourcePin,
            TargetNodeId = target.Id,
            TargetPin = targetPin,
        });
    }

}
