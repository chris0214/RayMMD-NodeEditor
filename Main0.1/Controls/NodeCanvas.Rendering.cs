using System.Drawing.Drawing2D;
using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Controls;

public sealed partial class NodeCanvas
{
    private void DrawGridCore(Graphics graphics)
    {
        graphics.Clear(BackColor);

        var minorSpacing = Math.Max(8f, 20f * _zoom);
        var majorSpacing = 100f * _zoom;

        using var minorPen = new Pen(Color.FromArgb(56, 58, 66));
        using var majorPen = new Pen(Color.FromArgb(68, 72, 82));

        var minorStartX = Mod(_panOffset.X, minorSpacing);
        var minorStartY = Mod(_panOffset.Y, minorSpacing);
        for (var x = minorStartX; x < Width; x += minorSpacing)
        {
            graphics.DrawLine(minorPen, x, 0f, x, Height);
        }

        for (var y = minorStartY; y < Height; y += minorSpacing)
        {
            graphics.DrawLine(minorPen, 0f, y, Width, y);
        }

        var majorStartX = Mod(_panOffset.X, majorSpacing);
        var majorStartY = Mod(_panOffset.Y, majorSpacing);
        for (var x = majorStartX; x < Width; x += majorSpacing)
        {
            graphics.DrawLine(majorPen, x, 0f, x, Height);
        }

        for (var y = majorStartY; y < Height; y += majorSpacing)
        {
            graphics.DrawLine(majorPen, 0f, y, Width, y);
        }

        DrawBackgroundWatermark(graphics);
    }

    private void DrawBackgroundWatermarkCore(Graphics graphics)
    {
        var state = graphics.Save();
        graphics.TranslateTransform(Width * 0.5f, Height * 0.5f);
        graphics.RotateTransform(-18f);

        using var watermarkFont = new Font("Segoe UI", 44f, FontStyle.Bold, GraphicsUnit.Pixel);
        using var shadowBrush = new SolidBrush(Color.FromArgb(10, 0, 0, 0));
        using var textBrush = new SolidBrush(Color.FromArgb(18, 120, 168, 240));
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };

        var layout = new RectangleF(-720f, -90f, 1440f, 180f);
        graphics.DrawString(BackgroundWatermarkText, watermarkFont, shadowBrush, new RectangleF(layout.X + 3f, layout.Y + 3f, layout.Width, layout.Height), format);
        graphics.DrawString(BackgroundWatermarkText, watermarkFont, textBrush, layout, format);

        graphics.Restore(state);
    }

    private void DrawConnectionsCore(Graphics graphics)
    {
        foreach (var connection in _graph.Connections)
        {
            if (!TryGetConnectionPoints(connection, out var from, out var to, out var valueType))
            {
                continue;
            }

            DrawBezier(graphics, from, to, GetPinColor(valueType), 3.5f);
        }
    }

    private void DrawPendingConnectionCore(Graphics graphics)
    {
        if (_pendingConnection is null)
        {
            return;
        }

        var pending = _pendingConnection.Value;
        var endPoint = _snapPin?.Center ?? _mouseCanvasPoint;
        if (pending.IsInput)
        {
            DrawBezier(graphics, endPoint, pending.StartPoint, GetPinColor(pending.Type), 2.8f);
        }
        else
        {
            DrawBezier(graphics, pending.StartPoint, endPoint, GetPinColor(pending.Type), 2.8f);
        }
    }

    private void DrawSelectionRectangleCore(Graphics graphics)
    {
        if (!_isMarqueeSelecting || _marqueeBounds.Width < 0.5f || _marqueeBounds.Height < 0.5f)
        {
            return;
        }

        using var fillBrush = new SolidBrush(Color.FromArgb(48, 86, 164, 255));
        using var borderPen = new Pen(Color.FromArgb(180, 120, 188, 255), 1.2f);
        graphics.FillRectangle(fillBrush, _marqueeBounds.X, _marqueeBounds.Y, _marqueeBounds.Width, _marqueeBounds.Height);
        graphics.DrawRectangle(borderPen, _marqueeBounds.X, _marqueeBounds.Y, _marqueeBounds.Width, _marqueeBounds.Height);
    }

    private void DrawCutPathCore(Graphics graphics)
    {
        if (!_isCuttingConnections || _cutPath.Count < 2)
        {
            return;
        }

        using var glowPen = new Pen(Color.FromArgb(80, 255, 120, 120), 5.2f)
        {
            DashStyle = DashStyle.Dash,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };
        using var mainPen = new Pen(Color.FromArgb(235, 255, 155, 155), 2.2f)
        {
            DashStyle = DashStyle.Dash,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };

        graphics.DrawLines(glowPen, _cutPath.ToArray());
        graphics.DrawLines(mainPen, _cutPath.ToArray());
    }

    private void DrawNodesCore(Graphics graphics)
    {
        foreach (var node in _graph.Nodes.Where(IsFrameNode))
        {
            DrawNode(graphics, node);
        }

        foreach (var node in _graph.Nodes.Where(node => !IsFrameNode(node)))
        {
            DrawNode(graphics, node);
        }
    }

    private static IReadOnlyList<NodePinDefinition> GetVisiblePinsCore(GraphNode node, bool input)
    {
        if (node.Kind == NodeKind.Group)
        {
            var customPins = input ? node.CustomInputs : node.CustomOutputs;
            if (customPins.Count > 0)
            {
                return customPins;
            }
        }

        var definition = NodeRegistry.Get(node.Kind);
        var pins = input ? definition.Inputs : definition.Outputs;

        if (node.Kind != NodeKind.ShadowRampColor || !input)
        {
            return pins;
        }

        var stopCount = 1;
        if (node.Properties.TryGetValue("StopCount", out var rawStopCount) &&
            int.TryParse(rawStopCount, out var parsedStopCount))
        {
            stopCount = Math.Clamp(parsedStopCount, 1, 3);
        }

        return pins
            .Where(pin => pin.Name switch
            {
                "Mid1Color" => stopCount >= 1,
                "Mid2Color" => stopCount >= 2,
                "Mid3Color" => stopCount >= 3,
                _ => true,
            })
            .ToList();
    }

    private void DrawNodeCore(Graphics graphics, GraphNode node)
    {
        if (node.Kind == NodeKind.Frame)
        {
            DrawFrameNode(graphics, node);
            return;
        }

        if (node.Kind == NodeKind.Group)
        {
            DrawGroupNode(graphics, node);
            return;
        }

        if (node.Kind == NodeKind.Reroute)
        {
            DrawRerouteNode(graphics, node);
            return;
        }

        var definition = NodeRegistry.Get(node.Kind);
        var visibleInputs = GetVisiblePins(node, true);
        var visibleOutputs = GetVisiblePins(node, false);
        var bounds = GetNodeBounds(node);
        var selected = _selectedNodeIds.Contains(node.Id);
        var primarySelected = _selectedNode?.Id == node.Id;
        var hovered = _hoverNode?.Id == node.Id || (_hoverPin?.Node.Id == node.Id);

        var accent = definition.AccentColor;
        var bodyColor = Color.FromArgb(43, 46, 54);
        var headerTop = BlendColor(accent, Color.White, 0.16f);
        var headerBottom = BlendColor(accent, Color.Black, 0.24f);
        var borderColor = primarySelected
            ? Color.FromArgb(255, 235, 148)
            : selected
                ? Color.FromArgb(170, 206, 235, 255)
                : hovered
                    ? BlendColor(accent, Color.White, 0.46f)
                    : BlendColor(accent, Color.White, 0.24f);
        using var shadowBrush = new SolidBrush(Color.FromArgb(70, 0, 0, 0));
        using var bodyBrush = new SolidBrush(bodyColor);
        using var borderPen = new Pen(borderColor, primarySelected ? 2.8f : selected ? 2.4f : hovered ? 1.8f : 1.4f);
        using var innerBorderPen = new Pen(Color.FromArgb(70, 255, 255, 255), 1f);
        using var titleBrush = new SolidBrush(Color.FromArgb(248, 248, 248));
        using var textBrush = new SolidBrush(Color.FromArgb(225, 228, 234));
        using var titleFont = new Font(Font.FontFamily, 9.2f, FontStyle.Bold);
        using var smallFont = new Font(Font.FontFamily, 8.5f, FontStyle.Regular);
        using var badgeFont = new Font(Font.FontFamily, 7.4f, FontStyle.Bold);
        using var pinOutlinePen = new Pen(Color.FromArgb(120, 12, 14, 18), 1.1f);
        using var titleFormat = new StringFormat
        {
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap,
        };

        var shadowBounds = new RectangleF(bounds.X + 5f, bounds.Y + 8f, bounds.Width, bounds.Height);
        using var shadowPath = CreateRoundedRectangle(shadowBounds, NodeCornerRadius);
        using var nodePath = CreateRoundedRectangle(bounds, NodeCornerRadius);
        using var headerPath = CreateTopRoundedRectangle(new RectangleF(bounds.X, bounds.Y, bounds.Width, HeaderHeight), NodeCornerRadius);
        using var headerBrush = new LinearGradientBrush(
            new PointF(bounds.Left, bounds.Top),
            new PointF(bounds.Left, bounds.Top + HeaderHeight),
            headerTop,
            headerBottom);
        using var accentGlowBrush = new SolidBrush(Color.FromArgb(primarySelected ? 74 : selected ? 54 : hovered ? 42 : 32, accent));

        graphics.FillPath(shadowBrush, shadowPath);
        graphics.FillPath(bodyBrush, nodePath);
        graphics.FillPath(accentGlowBrush, nodePath);
        graphics.FillPath(headerBrush, headerPath);

        graphics.DrawPath(borderPen, nodePath);
        graphics.DrawPath(innerBorderPen, nodePath);
        var badgeText = _nodeBadgeProvider?.Invoke(node);
        var badgeWidth = 0f;
        if (!string.IsNullOrWhiteSpace(badgeText))
        {
            badgeWidth = Math.Min(82f, graphics.MeasureString(badgeText, badgeFont).Width + 16f);
            var badgeBounds = new RectangleF(bounds.Right - badgeWidth - 12f, bounds.Y + 8f, badgeWidth, 18f);
            using var badgeBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
            using var badgeTextBrush = new SolidBrush(Color.FromArgb(230, 245, 248, 252));
            using var badgePath = CreateRoundedRectangle(badgeBounds, 7f);
            graphics.FillPath(badgeBrush, badgePath);
            using var badgeFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
            graphics.DrawString(badgeText, badgeFont, badgeTextBrush, badgeBounds, badgeFormat);
        }

        var titleBounds = new RectangleF(bounds.X + 18f, bounds.Y + 7f, bounds.Width - 36f - badgeWidth - 8f, HeaderHeight - 12f);
        graphics.DrawString(GetNodeHeaderTitle(node, definition), titleFont, titleBrush, titleBounds, titleFormat);

        foreach (var pin in visibleInputs)
        {
            var center = GetPinCenter(node, pin, true);
            using var pinBrush = new SolidBrush(GetPinColor(pin.Type));
            var hoveredPin = _hoverPin is { } hover && hover.Node.Id == node.Id && hover.Pin.Name == pin.Name && hover.IsInput;
            var snappedPin = _snapPin is { } snap && snap.Node.Id == node.Id && snap.Pin.Name == pin.Name && snap.IsInput;
            var radius = snappedPin ? PinRadius + 2.2f : hoveredPin ? PinRadius + 1.2f : PinRadius;
            using var pinGlowBrush = new SolidBrush(Color.FromArgb(snappedPin ? 110 : hoveredPin ? 72 : 0, GetPinColor(pin.Type)));
            if (snappedPin || hoveredPin)
            {
                graphics.FillEllipse(pinGlowBrush, center.X - radius - 3f, center.Y - radius - 3f, (radius + 3f) * 2f, (radius + 3f) * 2f);
            }

            graphics.FillEllipse(pinBrush, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
            graphics.DrawEllipse(pinOutlinePen, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
            var pinLabel = UiTextHelper.PinLabel(pin);
            graphics.DrawString(pinLabel, smallFont, textBrush, bounds.X + 18f, center.Y - 8f);
        }

        foreach (var pin in visibleOutputs)
        {
            var center = GetPinCenter(node, pin, false);
            using var pinBrush = new SolidBrush(GetPinColor(pin.Type));
            var hoveredPin = _hoverPin is { } hover && hover.Node.Id == node.Id && hover.Pin.Name == pin.Name && !hover.IsInput;
            var snappedPin = _snapPin is { } snap && snap.Node.Id == node.Id && snap.Pin.Name == pin.Name && !snap.IsInput;
            var radius = snappedPin ? PinRadius + 2.2f : hoveredPin ? PinRadius + 1.2f : PinRadius;
            using var pinGlowBrush = new SolidBrush(Color.FromArgb(snappedPin ? 110 : hoveredPin ? 72 : 0, GetPinColor(pin.Type)));
            if (snappedPin || hoveredPin)
            {
                graphics.FillEllipse(pinGlowBrush, center.X - radius - 3f, center.Y - radius - 3f, (radius + 3f) * 2f, (radius + 3f) * 2f);
            }

            graphics.FillEllipse(pinBrush, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
            graphics.DrawEllipse(pinOutlinePen, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);

            var pinLabel = UiTextHelper.PinLabel(pin);
            var textSize = graphics.MeasureString(pinLabel, smallFont);
            graphics.DrawString(pinLabel, smallFont, textBrush, bounds.Right - textSize.Width - 18f, center.Y - 8f);
        }

        DrawInlineEditors(graphics, node, bounds, accent);
    }

    private void DrawFrameNode(Graphics graphics, GraphNode node)
    {
        var bounds = GetNodeBounds(node);
        var selected = _selectedNodeIds.Contains(node.Id);
        var hovered = _hoverNode?.Id == node.Id;
        var tint = Color.FromArgb(
            255,
            ClampByte(ReadNodeFloat(node, "TintR", 0.36f)),
            ClampByte(ReadNodeFloat(node, "TintG", 0.52f)),
            ClampByte(ReadNodeFloat(node, "TintB", 0.86f)));
        var opacity = Math.Clamp(ReadNodeFloat(node, "Opacity", 0.16f), 0.04f, 0.55f);
        var borderColor = selected
            ? Color.FromArgb(255, 235, 148)
            : hovered
                ? BlendColor(tint, Color.White, 0.35f)
                : BlendColor(tint, Color.Black, 0.18f);
        using var fillBrush = new SolidBrush(Color.FromArgb((int)Math.Round(255 * opacity), tint));
        using var headerBrush = new SolidBrush(Color.FromArgb((int)Math.Round(255 * Math.Min(opacity + 0.08f, 0.70f)), BlendColor(tint, Color.White, 0.12f)));
        using var borderPen = new Pen(borderColor, selected ? 2.1f : 1.4f) { DashStyle = DashStyle.Dash };
        using var titleBrush = new SolidBrush(Color.FromArgb(240, 245, 250));
        using var titleFont = new Font(Font.FontFamily, 9f, FontStyle.Bold);

        graphics.FillRectangle(fillBrush, bounds.X, bounds.Y + FrameHeaderHeight, bounds.Width, Math.Max(0f, bounds.Height - FrameHeaderHeight));
        graphics.FillRectangle(headerBrush, bounds.X, bounds.Y, bounds.Width, FrameHeaderHeight);
        graphics.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        graphics.DrawString(
            node.Properties.TryGetValue("Title", out var title) && !string.IsNullOrWhiteSpace(title) ? title : "Frame",
            titleFont,
            titleBrush,
            bounds.X + 12f,
            bounds.Y + 7f);
    }

    private void DrawRerouteNode(Graphics graphics, GraphNode node)
    {
        var bounds = GetNodeBounds(node);
        var selected = _selectedNodeIds.Contains(node.Id);
        var hovered = _hoverNode?.Id == node.Id || (_hoverPin?.Node.Id == node.Id);
        var fillColor = Color.FromArgb(58, 62, 72);
        var borderColor = selected
            ? Color.FromArgb(255, 235, 148)
            : hovered
                ? Color.FromArgb(170, 206, 235, 255)
                : Color.FromArgb(116, 124, 138);
        using var fillBrush = new SolidBrush(fillColor);
        using var borderPen = new Pen(borderColor, selected ? 2.0f : 1.4f);
        var ellipseBounds = new RectangleF(bounds.X + 3f, bounds.Y + 2f, bounds.Width - 6f, bounds.Height - 4f);
        graphics.FillEllipse(fillBrush, ellipseBounds);
        graphics.DrawEllipse(borderPen, ellipseBounds);
    }

    private void DrawGroupNode(Graphics graphics, GraphNode node)
    {
        var definition = NodeRegistry.Get(node.Kind);
        var visibleInputs = GetVisiblePins(node, true);
        var visibleOutputs = GetVisiblePins(node, false);
        var bounds = GetNodeBounds(node);
        var selected = _selectedNodeIds.Contains(node.Id);
        var primarySelected = _selectedNode?.Id == node.Id;
        var hovered = _hoverNode?.Id == node.Id || (_hoverPin?.Node.Id == node.Id);

        var accent = Color.FromArgb(96, 156, 118);
        var bodyColor = Color.FromArgb(47, 52, 49);
        var headerTop = BlendColor(accent, Color.White, 0.14f);
        var headerBottom = BlendColor(accent, Color.Black, 0.18f);
        var borderColor = primarySelected
            ? Color.FromArgb(255, 235, 148)
            : selected
                ? Color.FromArgb(170, 206, 235, 255)
                : hovered
                    ? BlendColor(accent, Color.White, 0.46f)
                    : BlendColor(accent, Color.White, 0.24f);
        using var shadowBrush = new SolidBrush(Color.FromArgb(70, 0, 0, 0));
        using var bodyBrush = new SolidBrush(bodyColor);
        using var borderPen = new Pen(borderColor, primarySelected ? 2.8f : selected ? 2.4f : hovered ? 1.8f : 1.4f);
        using var innerBorderPen = new Pen(Color.FromArgb(70, 255, 255, 255), 1f);
        using var titleBrush = new SolidBrush(Color.FromArgb(248, 248, 248));
        using var textBrush = new SolidBrush(Color.FromArgb(220, 228, 222));
        using var mutedTextBrush = new SolidBrush(Color.FromArgb(170, 194, 182));
        using var titleFont = new Font(Font.FontFamily, 9.2f, FontStyle.Bold);
        using var smallFont = new Font(Font.FontFamily, 8.3f, FontStyle.Regular);
        using var badgeFont = new Font(Font.FontFamily, 7.4f, FontStyle.Bold);
        using var pinOutlinePen = new Pen(Color.FromArgb(120, 12, 14, 18), 1.1f);
        using var titleFormat = new StringFormat
        {
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap,
        };

        var shadowBounds = new RectangleF(bounds.X + 5f, bounds.Y + 8f, bounds.Width, bounds.Height);
        using var shadowPath = CreateRoundedRectangle(shadowBounds, NodeCornerRadius);
        using var nodePath = CreateRoundedRectangle(bounds, NodeCornerRadius);
        using var headerPath = CreateTopRoundedRectangle(new RectangleF(bounds.X, bounds.Y, bounds.Width, HeaderHeight), NodeCornerRadius);
        using var headerBrush = new LinearGradientBrush(
            new PointF(bounds.Left, bounds.Top),
            new PointF(bounds.Left, bounds.Top + HeaderHeight),
            headerTop,
            headerBottom);
        using var accentStripBrush = new SolidBrush(Color.FromArgb(196, accent));
        using var badgeBrush = new SolidBrush(Color.FromArgb(52, 20, 28, 22));
        using var badgeTextBrush = new SolidBrush(Color.FromArgb(235, 220, 238, 224));
        using var openBrush = new SolidBrush(Color.FromArgb(42, 255, 255, 255));
        using var openTextBrush = new SolidBrush(Color.FromArgb(228, 244, 248, 245));
        using var dividerPen = new Pen(Color.FromArgb(52, 255, 255, 255), 1f);

        graphics.FillPath(shadowBrush, shadowPath);
        graphics.FillPath(bodyBrush, nodePath);
        graphics.FillPath(headerBrush, headerPath);
        graphics.DrawPath(borderPen, nodePath);
        graphics.DrawPath(innerBorderPen, nodePath);
        graphics.FillRectangle(accentStripBrush, bounds.X + 12f, bounds.Y + 12f, 4f, bounds.Height - 24f);

        var badgeRect = new RectangleF(bounds.X + 20f, bounds.Y + 9f, 52f, 18f);
        var openRect = new RectangleF(bounds.Right - 68f, bounds.Y + 9f, 48f, 18f);
        using var badgePath = CreateRoundedRectangle(badgeRect, 8f);
        using var openPath = CreateRoundedRectangle(openRect, 8f);
        graphics.FillPath(badgeBrush, badgePath);
        graphics.FillPath(openBrush, openPath);
        graphics.DrawString("GROUP", badgeFont, badgeTextBrush, badgeRect.X + 6f, badgeRect.Y + 3f);
        graphics.DrawString("Open", badgeFont, openTextBrush, openRect.X + 9f, openRect.Y + 3f);

        var titleBounds = new RectangleF(bounds.X + 80f, bounds.Y + 7f, bounds.Width - 156f, HeaderHeight - 12f);
        graphics.DrawString(GetNodeHeaderTitle(node, definition), titleFont, titleBrush, titleBounds, titleFormat);

        var firstInput = visibleInputs.Count > 0 ? UiTextHelper.PinLabel(visibleInputs[0]) : "-";
        var firstOutput = visibleOutputs.Count > 0 ? UiTextHelper.PinLabel(visibleOutputs[0]) : "-";
        var infoText = $"{visibleInputs.Count} in / {visibleOutputs.Count} out  •  {firstInput} -> {firstOutput}";
        _ = infoText;
        graphics.DrawString($"{visibleInputs.Count} in   {visibleOutputs.Count} out", smallFont, textBrush, bounds.X + 20f, bounds.Y + HeaderHeight + 10f);
        graphics.DrawLine(dividerPen, bounds.X + 20f, bounds.Y + HeaderHeight + 30f, bounds.Right - 20f, bounds.Y + HeaderHeight + 30f);

        foreach (var pin in visibleInputs)
        {
            var center = GetPinCenter(node, pin, true);
            using var pinBrush = new SolidBrush(GetPinColor(pin.Type));
            graphics.FillEllipse(pinBrush, center.X - PinRadius, center.Y - PinRadius, PinRadius * 2f, PinRadius * 2f);
            graphics.DrawEllipse(pinOutlinePen, center.X - PinRadius, center.Y - PinRadius, PinRadius * 2f, PinRadius * 2f);
            graphics.DrawString(UiTextHelper.PinLabel(pin), smallFont, textBrush, bounds.X + 20f, center.Y - 8f);
        }

        foreach (var pin in visibleOutputs)
        {
            var center = GetPinCenter(node, pin, false);
            using var pinBrush = new SolidBrush(GetPinColor(pin.Type));
            graphics.FillEllipse(pinBrush, center.X - PinRadius, center.Y - PinRadius, PinRadius * 2f, PinRadius * 2f);
            graphics.DrawEllipse(pinOutlinePen, center.X - PinRadius, center.Y - PinRadius, PinRadius * 2f, PinRadius * 2f);
            var pinLabel = UiTextHelper.PinLabel(pin);
            var textSize = graphics.MeasureString(pinLabel, smallFont);
            graphics.DrawString(pinLabel, smallFont, textBrush, bounds.Right - textSize.Width - 20f, center.Y - 8f);
        }
    }

    private static void DrawBezierCore(Graphics graphics, PointF from, PointF to, Color color, float width)
    {
        var distance = Math.Max(70f, Math.Abs(to.X - from.X) * 0.48f);
        var cp1 = new PointF(from.X + distance, from.Y);
        var cp2 = new PointF(to.X - distance, to.Y);

        using var glowPen = new Pen(Color.FromArgb(54, color), width + 3f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };
        using var mainPen = new Pen(color, width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };

        graphics.DrawBezier(glowPen, from, cp1, cp2, to);
        graphics.DrawBezier(mainPen, from, cp1, cp2, to);
    }

    private void DrawViewportInfoCore(Graphics graphics)
    {
        using var backBrush = new SolidBrush(Color.FromArgb(96, 22, 24, 28));
        using var textBrush = new SolidBrush(Color.FromArgb(220, 220, 220));
        using var smallFont = new Font(Font.FontFamily, 8f, FontStyle.Regular);

        if (!string.IsNullOrWhiteSpace(_filterDescription))
        {
            var breadcrumb = _filterDescription.Trim();
            var crumbSize = graphics.MeasureString(breadcrumb, smallFont);
            var crumbRect = new RectangleF(10f, 10f, crumbSize.Width + 16f, crumbSize.Height + 8f);
            graphics.FillRectangle(backBrush, crumbRect);
            graphics.DrawString(breadcrumb, smallFont, textBrush, crumbRect.X + 8f, crumbRect.Y + 4f);
        }

        var selectedCount = _selectedNodeIds.Count;
        var selectionInfo = selectedCount switch
        {
            0 => LocalizationService.Get("status.selection.none", "No selection"),
            1 => LocalizationService.Get("status.selection.one", "1 node selected"),
            _ => LocalizationService.Format("status.selection.many", "{0} nodes selected", selectedCount),
        };
        var historyInfo = LocalizationService.Format("status.history", "Undo {0} / Redo {1}", _undoStack.Count, _redoStack.Count);
        var info = LocalizationService.Format("status.viewport", "Zoom {0}%  {1}  {2}", Math.Round(_zoom * 100f), selectionInfo, historyInfo);

        var size = graphics.MeasureString(info, smallFont);
        var rect = new RectangleF(10f, Height - size.Height - 10f, size.Width + 14f, size.Height + 6f);
        graphics.FillRectangle(backBrush, rect);
        graphics.DrawString(info, smallFont, textBrush, rect.X + 7f, rect.Y + 3f);
    }

    private RectangleF GetNodeBoundsCore(GraphNode node)
    {
        if (node.Kind == NodeKind.Frame)
        {
            return GetFrameBounds(node);
        }

        if (node.Kind == NodeKind.Reroute)
        {
            return new RectangleF(node.X, node.Y, RerouteWidth, RerouteHeight);
        }

        if (node.Kind == NodeKind.Group)
        {
            var groupPinRows = Math.Max(Math.Max(GetVisiblePins(node, true).Count, GetVisiblePins(node, false).Count), 1);
            var groupBodyHeight = 38f + (groupPinRows * PinRowHeight) + 12f;
            return new RectangleF(node.X, node.Y, GroupNodeWidth, HeaderHeight + groupBodyHeight);
        }

        var pinRows = Math.Max(Math.Max(GetVisiblePins(node, true).Count, GetVisiblePins(node, false).Count), 1);
        var bodyHeight = 18f + (pinRows * PinRowHeight) + 12f + GetInlineEditorExtraHeight(node);
        return new RectangleF(node.X, node.Y, NodeWidth, HeaderHeight + bodyHeight);
    }

    private static string GetNodeHeaderTitleCore(GraphNode node, NodeDefinition definition)
    {
        if (node.Kind == NodeKind.Group &&
            node.Properties.TryGetValue("GroupName", out var groupName) &&
            !string.IsNullOrWhiteSpace(groupName))
        {
            return groupName.Trim();
        }

        if (node.Kind is NodeKind.ExternalTexture or NodeKind.MatCapAtlasSample or NodeKind.EmissiveTexture or NodeKind.NormalMap or NodeKind.TriplanarBoxmap or NodeKind.PreIntegratedFGD or NodeKind.GenshinRamp or NodeKind.SnowBreakRamp or NodeKind.GenericRampSample or NodeKind.SkinPreintegratedLut or NodeKind.FakeEnvReflection)
        {
            if (ResourceNodeProperties.TryGetDisplayResourceName(node, out var resourceName))
            {
                var fileStem = Path.GetFileNameWithoutExtension(resourceName.Trim());
                if (!string.IsNullOrWhiteSpace(fileStem))
                {
                    return fileStem;
                }
            }

            if (node.Properties.TryGetValue(ResourceNodeProperties.SourcePath, out var sourcePath) &&
                !string.IsNullOrWhiteSpace(sourcePath))
            {
                var fileStem = Path.GetFileNameWithoutExtension(sourcePath.Trim());
                if (!string.IsNullOrWhiteSpace(fileStem))
                {
                    return fileStem;
                }
            }
        }

        return UiTextHelper.NodeTitle(definition);
    }

    private static int ClampByte(float normalizedValue)
    {
        return Math.Clamp((int)Math.Round(normalizedValue * 255f), 0, 255);
    }
}
