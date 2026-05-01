using RayMmdNodeEditor.Graph;
using RayMmdNodeEditor.Services;

namespace RayMmdNodeEditor.Controls;

public sealed partial class NodeCanvas
{
    private const float InlineRowHeight = 28f;
    private const float InlineRowSpacing = 6f;
    private const float InlineSectionTopPadding = 10f;
    private const float InlineSectionBottomPadding = 8f;
    private const float InlineLabelWidth = 58f;
    private const float InlineValueWidth = 60f;
    private const float InlineEditorCornerRadius = 8f;

    private readonly TextBox _inlineTextBox = new();
    private Keys? _activeQuickSpawnKey;
    private ToolStripDropDown? _inlineEditorDropDown;
    private NumericDragState? _numericDragState;
    private InlineTextEditState? _inlineTextEditState;
    private static readonly IReadOnlyDictionary<Keys, NodeKind> QuickSpawnNodeMap = new Dictionary<Keys, NodeKind>
    {
        [Keys.D1] = NodeKind.Scalar,
        [Keys.D2] = NodeKind.Float2Value,
        [Keys.D3] = NodeKind.Float3Value,
        [Keys.D4] = NodeKind.Float4Value,
        [Keys.NumPad1] = NodeKind.Scalar,
        [Keys.NumPad2] = NodeKind.Float2Value,
        [Keys.NumPad3] = NodeKind.Float3Value,
        [Keys.NumPad4] = NodeKind.Float4Value,
        [Keys.C] = NodeKind.Color,
        [Keys.A] = NodeKind.Add,
        [Keys.M] = NodeKind.Multiply,
        [Keys.L] = NodeKind.Lerp,
    };

    private enum InlineEditorVisualKind
    {
        Numeric,
        VectorButton,
        ColorButton,
    }

    private sealed record InlineEditorDescriptor(
        string Label,
        GraphValueType ValueType,
        InlineEditorVisualKind VisualKind,
        float Minimum,
        float Maximum,
        float Step,
        float[] DefaultValues,
        string[]? PropertyKeys = null,
        string? StorageKey = null,
        string? BoundPin = null);

    private sealed record InlineEditorLayout(
        GraphNode Node,
        InlineEditorDescriptor Descriptor,
        RectangleF RowBounds,
        RectangleF EditorBounds,
        RectangleF SwatchBounds);

    private sealed class NumericDragState
    {
        public NumericDragState(GraphNode node, InlineEditorLayout layout)
        {
            Node = node;
            Layout = layout;
        }

        public GraphNode Node { get; }
        public InlineEditorLayout Layout { get; }
        public bool UndoCaptured { get; set; }
        public bool HasChanges { get; set; }
    }

    private sealed record InlineTextEditState(GraphNode Node, InlineEditorLayout Layout);

    private void InitializeInlineEditorHost()
    {
        _inlineTextBox.Visible = false;
        _inlineTextBox.BorderStyle = BorderStyle.FixedSingle;
        EditorTheme.StyleTextBox(_inlineTextBox);
        _inlineTextBox.KeyDown += InlineTextBoxOnKeyDown;
        _inlineTextBox.Leave += InlineTextBoxOnLeave;
        Controls.Add(_inlineTextBox);
    }

    private void ClearInlineEditorState(bool commitTextEditor)
    {
        if (commitTextEditor)
        {
            if (!CommitPendingInlineEditors(showValidationErrors: false))
            {
                CancelInlineTextEditor();
            }
        }
        else
        {
            CancelInlineTextEditor();
        }

        CloseInlineEditorDropDown();
        _numericDragState = null;
    }

    public bool CommitPendingInlineEditors(bool showValidationErrors)
    {
        if (_inlineTextEditState is null || !_inlineTextBox.Visible)
        {
            return true;
        }

        var descriptor = _inlineTextEditState.Layout.Descriptor;
        if (descriptor.VisualKind != InlineEditorVisualKind.Numeric)
        {
            _inlineTextBox.Visible = false;
            _inlineTextEditState = null;
            return true;
        }

        var text = _inlineTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            text = FloatParser.Format(descriptor.DefaultValues.FirstOrDefault());
        }

        if (!FloatParser.TryParse(text, out var parsedValue))
        {
            if (!showValidationErrors)
            {
                return false;
            }

            MessageBox.Show(
                FindForm(),
                "This inline value needs a valid number.",
                LocalizationService.Get("app.title", "MME Node Editor"));
            _inlineTextBox.Focus();
            _inlineTextBox.SelectAll();
            return false;
        }

        if (descriptor.PropertyKeys is not { Length: > 0 } && string.IsNullOrWhiteSpace(descriptor.StorageKey))
        {
            CancelInlineTextEditor();
            return true;
        }

        var currentValues = ReadInlineEditorValues(_inlineTextEditState.Node, descriptor);
        if (Math.Abs(currentValues[0] - parsedValue) > 0.0001f)
        {
            CaptureUndoState();
            currentValues[0] = parsedValue;
            WriteInlineEditorValues(_inlineTextEditState.Node, descriptor, currentValues);
            CommitExternalChange();
        }

        _inlineTextBox.Visible = false;
        _inlineTextEditState = null;
        return true;
    }

    private void CancelInlineTextEditor()
    {
        _inlineTextBox.Visible = false;
        _inlineTextEditState = null;
    }

    private void CloseInlineEditorDropDown()
    {
        if (_inlineEditorDropDown is null)
        {
            return;
        }

        var dropDown = _inlineEditorDropDown;
        _inlineEditorDropDown = null;
        dropDown.Close(ToolStripDropDownCloseReason.CloseCalled);
        dropDown.Dispose();
    }

    private bool HandleInlineEditorMouseDown(MouseEventArgs e, GraphNode? nodeHit)
    {
        if (nodeHit is null || !TryGetInlineEditorLayout(nodeHit, _mouseCanvasPoint, out var layout))
        {
            return false;
        }

        if (!_selectedNodeIds.Contains(nodeHit.Id))
        {
            SelectSingleNode(nodeHit);
        }

        CloseInlineEditorDropDown();

        switch (layout.Descriptor.VisualKind)
        {
            case InlineEditorVisualKind.Numeric:
                CommitPendingInlineEditors(showValidationErrors: false);
                _numericDragState = new NumericDragState(nodeHit, layout);
                ApplyNumericDragValue(layout, _mouseCanvasPoint);
                return true;
            case InlineEditorVisualKind.ColorButton:
                OpenInlineColorEditor(layout);
                return true;
            case InlineEditorVisualKind.VectorButton:
                OpenInlineVectorEditor(layout);
                return true;
            default:
                return false;
        }
    }

    private bool HandleInlineEditorDoubleClick(MouseEventArgs e, GraphNode? nodeHit)
    {
        if (nodeHit is null || !TryGetInlineEditorLayout(nodeHit, _mouseCanvasPoint, out var layout))
        {
            return false;
        }

        if (layout.Descriptor.VisualKind != InlineEditorVisualKind.Numeric)
        {
            if (layout.Descriptor.VisualKind == InlineEditorVisualKind.ColorButton)
            {
                OpenInlineColorEditor(layout);
                return true;
            }

            if (layout.Descriptor.VisualKind == InlineEditorVisualKind.VectorButton)
            {
                OpenInlineVectorEditor(layout);
                return true;
            }

            return false;
        }

        OpenInlineNumericTextEditor(layout);
        return true;
    }

    private void HandleInlineEditorMouseMove()
    {
        if (_numericDragState is null)
        {
            return;
        }

        ApplyNumericDragValue(_numericDragState.Layout, _mouseCanvasPoint);
    }

    private void EndInlineEditorDrag()
    {
        if (_numericDragState?.HasChanges == true)
        {
            CommitExternalChange();
        }

        _numericDragState = null;
    }

    private void ApplyNumericDragValue(InlineEditorLayout layout, PointF canvasPoint)
    {
        if (layout.Descriptor.Maximum <= layout.Descriptor.Minimum)
        {
            return;
        }

        var normalized = Math.Clamp((canvasPoint.X - layout.EditorBounds.Left) / layout.EditorBounds.Width, 0f, 1f);
        var value = layout.Descriptor.Minimum + ((layout.Descriptor.Maximum - layout.Descriptor.Minimum) * normalized);
        value = RoundToStep(value, layout.Descriptor.Step);

        var currentValues = ReadInlineEditorValues(layout.Node, layout.Descriptor);
        if (Math.Abs(currentValues[0] - value) < 0.0001f)
        {
            return;
        }

        if (_numericDragState is { UndoCaptured: false })
        {
            CaptureUndoState();
            _numericDragState.UndoCaptured = true;
        }

        currentValues[0] = value;
        WriteInlineEditorValues(layout.Node, layout.Descriptor, currentValues);
        if (_numericDragState is not null)
        {
            _numericDragState.HasChanges = true;
        }

        Invalidate();
    }

    private void OpenInlineNumericTextEditor(InlineEditorLayout layout)
    {
        CloseInlineEditorDropDown();
        var values = ReadInlineEditorValues(layout.Node, layout.Descriptor);
        var clientBounds = Rectangle.Round(CanvasRectToClient(layout.EditorBounds));
        clientBounds.Height = Math.Max(clientBounds.Height, 24);

        _inlineTextEditState = new InlineTextEditState(layout.Node, layout);
        _inlineTextBox.Bounds = clientBounds;
        _inlineTextBox.Text = FloatParser.Format(values[0]);
        _inlineTextBox.Visible = true;
        _inlineTextBox.BringToFront();
        _inlineTextBox.Focus();
        _inlineTextBox.SelectAll();
    }

    private void OpenInlineVectorEditor(InlineEditorLayout layout)
    {
        var values = ReadInlineEditorValues(layout.Node, layout.Descriptor);
        var editorPanel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(8),
            BackColor = EditorTheme.Panel,
        };
        editorPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40f));
        editorPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124f));

        var labels = GetVectorComponentLabels(layout.Descriptor.ValueType);
        var capturedUndo = false;

        for (var index = 0; index < labels.Length; index++)
        {
            editorPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            editorPanel.Controls.Add(new Label
            {
                Text = labels[index],
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 8, 8),
            }, 0, index);

            var numeric = new NumericUpDown
            {
                Dock = DockStyle.Top,
                DecimalPlaces = 3,
                Increment = (decimal)Math.Max(layout.Descriptor.Step, 0.01f),
                Minimum = -1000m,
                Maximum = 1000m,
                Margin = new Padding(0, 0, 0, 8),
                Value = ClampDecimal((decimal)values[index], -1000m, 1000m),
            };
            EditorTheme.StyleNumeric(numeric);
            var capturedIndex = index;
            numeric.ValueChanged += (_, _) =>
            {
                if (!capturedUndo)
                {
                    CaptureUndoState();
                    capturedUndo = true;
                }

                values[capturedIndex] = (float)numeric.Value;
                WriteInlineEditorValues(layout.Node, layout.Descriptor, values);
                CommitExternalChange();
            };
            editorPanel.Controls.Add(numeric, 1, index);
        }

        EditorTheme.ApplyThemeRecursive(editorPanel);
        ShowInlineDropDown(layout, editorPanel);
    }

    private void OpenInlineColorEditor(InlineEditorLayout layout)
    {
        var values = ReadInlineEditorValues(layout.Node, layout.Descriptor);
        var editor = new InlineColorPickerControl();
        editor.SetSelectedColor(Color.FromArgb(
            Math.Clamp((int)Math.Round(GetColorComponent(values, 3) * 255f), 0, 255),
            Math.Clamp((int)Math.Round(GetColorComponent(values, 0) * 255f), 0, 255),
            Math.Clamp((int)Math.Round(GetColorComponent(values, 1) * 255f), 0, 255),
            Math.Clamp((int)Math.Round(GetColorComponent(values, 2) * 255f), 0, 255)));

        var capturedUndo = false;
        editor.SelectedColorChanged += (_, _) =>
        {
            if (!capturedUndo)
            {
                CaptureUndoState();
                capturedUndo = true;
            }

            var color = editor.SelectedColor;
            values[0] = color.R / 255f;
            values[1] = color.G / 255f;
            values[2] = color.B / 255f;
            values[3] = color.A / 255f;
            WriteInlineEditorValues(layout.Node, layout.Descriptor, values);
            CommitExternalChange();
        };

        ShowInlineDropDown(layout, editor);
    }

    private void ShowInlineDropDown(InlineEditorLayout layout, Control control)
    {
        CloseInlineEditorDropDown();

        var host = new ToolStripControlHost(control)
        {
            AutoSize = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };

        var dropDown = new ToolStripDropDown
        {
            AutoClose = true,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Renderer = EditorTheme.ToolStripRenderer,
        };
        dropDown.Items.Add(host);
        dropDown.Closed += (_, _) =>
        {
            if (ReferenceEquals(_inlineEditorDropDown, dropDown))
            {
                _inlineEditorDropDown = null;
            }
        };

        control.PerformLayout();
        var size = control.GetPreferredSize(Size.Empty);
        if (size.Width <= 0 || size.Height <= 0)
        {
            size = control.Size;
        }
        if (size.Width <= 0 || size.Height <= 0)
        {
            size = new Size(220, 180);
        }

        host.Size = size;
        control.Size = size;
        _inlineEditorDropDown = dropDown;

        var clientAnchor = CanvasPointToClient(new PointF(layout.EditorBounds.Left, layout.EditorBounds.Bottom + 4f));
        dropDown.Show(this, clientAnchor);
    }

    private RectangleF CanvasRectToClient(RectangleF canvasRect)
    {
        var topLeft = CanvasPointToClient(new PointF(canvasRect.Left, canvasRect.Top));
        var bottomRight = CanvasPointToClient(new PointF(canvasRect.Right, canvasRect.Bottom));
        return RectangleF.FromLTRB(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
    }

    private Point CanvasPointToClient(PointF canvasPoint)
    {
        return new Point(
            (int)Math.Round((_panOffset.X + (canvasPoint.X * _zoom))),
            (int)Math.Round((_panOffset.Y + (canvasPoint.Y * _zoom))));
    }

    private void DrawInlineEditors(Graphics graphics, GraphNode node, RectangleF bounds, Color accent)
    {
        foreach (var layout in GetInlineEditorLayouts(node))
        {
            var values = ReadInlineEditorValues(node, layout.Descriptor);
            using var panelBrush = new SolidBrush(Color.FromArgb(48, 255, 255, 255));
            using var borderPen = new Pen(Color.FromArgb(58, 255, 255, 255), 1f);
            using var textBrush = new SolidBrush(Color.FromArgb(228, 232, 238));
            using var mutedBrush = new SolidBrush(Color.FromArgb(180, 197, 204, 214));
            using var rowPath = CreateRoundedRectangle(layout.EditorBounds, InlineEditorCornerRadius);

            graphics.FillPath(panelBrush, rowPath);
            graphics.DrawPath(borderPen, rowPath);
            graphics.DrawString(layout.Descriptor.Label, Font, mutedBrush, layout.RowBounds.X, layout.RowBounds.Y + 5f);

            switch (layout.Descriptor.VisualKind)
            {
                case InlineEditorVisualKind.Numeric:
                    DrawInlineNumericEditor(graphics, layout, values[0], accent, textBrush);
                    break;
                case InlineEditorVisualKind.ColorButton:
                    DrawInlineColorEditor(graphics, layout, values, mutedBrush);
                    break;
                case InlineEditorVisualKind.VectorButton:
                    DrawInlineVectorEditor(graphics, layout, values, textBrush);
                    break;
            }
        }
    }

    private void DrawInlineNumericEditor(Graphics graphics, InlineEditorLayout layout, float value, Color accent, Brush textBrush)
    {
        var normalized = layout.Descriptor.Maximum <= layout.Descriptor.Minimum
            ? 0f
            : Math.Clamp((value - layout.Descriptor.Minimum) / (layout.Descriptor.Maximum - layout.Descriptor.Minimum), 0f, 1f);
        var fillWidth = Math.Max(0f, (layout.EditorBounds.Width - InlineValueWidth) * normalized);
        var sliderRect = new RectangleF(layout.EditorBounds.X, layout.EditorBounds.Y, layout.EditorBounds.Width - InlineValueWidth - 8f, layout.EditorBounds.Height);
        var valueRect = new RectangleF(sliderRect.Right + 8f, layout.EditorBounds.Y, InlineValueWidth, layout.EditorBounds.Height);

        using var sliderBrush = new SolidBrush(Color.FromArgb(66, 24, 26, 31));
        using var fillBrush = new SolidBrush(Color.FromArgb(150, accent));
        using var valueBrush = new SolidBrush(Color.FromArgb(226, 244, 247, 252));
        using var sliderPath = CreateRoundedRectangle(sliderRect, InlineEditorCornerRadius);
        using var valuePath = CreateRoundedRectangle(valueRect, InlineEditorCornerRadius);

        graphics.FillPath(sliderBrush, sliderPath);
        if (fillWidth > 0.5f)
        {
            using var fillPath = CreateRoundedRectangle(new RectangleF(sliderRect.X, sliderRect.Y, fillWidth, sliderRect.Height), InlineEditorCornerRadius);
            graphics.FillPath(fillBrush, fillPath);
        }

        graphics.FillPath(sliderBrush, valuePath);
        graphics.DrawString(FloatParser.Format(value), Font, valueBrush, valueRect, new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        });
    }

    private void DrawInlineVectorEditor(Graphics graphics, InlineEditorLayout layout, IReadOnlyList<float> values, Brush textBrush)
    {
        var summary = string.Join(", ", values.Select(FloatParser.Format));
        graphics.DrawString(summary, Font, textBrush, layout.EditorBounds, new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
        });
    }

    private void DrawInlineColorEditor(Graphics graphics, InlineEditorLayout layout, IReadOnlyList<float> values, Brush mutedBrush)
    {
        var swatchColor = Color.FromArgb(
            Math.Clamp((int)Math.Round(GetColorComponent(values, 3) * 255f), 0, 255),
            Math.Clamp((int)Math.Round(GetColorComponent(values, 0) * 255f), 0, 255),
            Math.Clamp((int)Math.Round(GetColorComponent(values, 1) * 255f), 0, 255),
            Math.Clamp((int)Math.Round(GetColorComponent(values, 2) * 255f), 0, 255));
        var previewBounds = new RectangleF(layout.EditorBounds.X + 6f, layout.EditorBounds.Y + 4f, layout.EditorBounds.Width - 12f, layout.EditorBounds.Height - 8f);
        using var swatchBrush = new SolidBrush(swatchColor);
        using var swatchBorderPen = new Pen(Color.FromArgb(90, 10, 12, 16), 1f);
        using var previewPath = CreateRoundedRectangle(previewBounds, 6f);

        graphics.FillPath(swatchBrush, previewPath);
        graphics.DrawPath(swatchBorderPen, previewPath);
        graphics.DrawString("色轮", Font, mutedBrush, previewBounds, new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        });
    }

    private IReadOnlyList<InlineEditorLayout> GetInlineEditorLayouts(GraphNode node)
    {
        var descriptors = GetInlineEditorDescriptors(node).ToList();
        if (descriptors.Count == 0)
        {
            return [];
        }

        var pinRows = Math.Max(Math.Max(GetVisiblePins(node, true).Count, GetVisiblePins(node, false).Count), 1);
        var startY = node.Y + HeaderHeight + 18f + (pinRows * PinRowHeight) + InlineSectionTopPadding;
        var layouts = new List<InlineEditorLayout>(descriptors.Count);

        for (var index = 0; index < descriptors.Count; index++)
        {
            var rowY = startY + (index * (InlineRowHeight + InlineRowSpacing));
            var rowBounds = new RectangleF(node.X + 16f, rowY, NodeWidth - 32f, InlineRowHeight);
            var editorBounds = new RectangleF(rowBounds.X + InlineLabelWidth, rowBounds.Y, rowBounds.Width - InlineLabelWidth, rowBounds.Height);
            var swatchBounds = new RectangleF(editorBounds.X + 6f, editorBounds.Y + 4f, editorBounds.Height - 8f, editorBounds.Height - 8f);
            layouts.Add(new InlineEditorLayout(node, descriptors[index], rowBounds, editorBounds, swatchBounds));
        }

        return layouts;
    }

    private IEnumerable<InlineEditorDescriptor> GetInlineEditorDescriptors(GraphNode node)
    {
        var definition = NodeRegistry.Get(node.Kind);

        if (node.Kind == NodeKind.Color)
        {
            yield return CreateColorInlineDescriptor();
            yield break;
        }

        var propertyBackedPins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in definition.Properties)
        {
            if (property.Editor == NodePropertyEditorKind.Hidden)
            {
                continue;
            }

            if (TryCreatePropertyInlineDescriptor(node, definition, property, out var descriptor))
            {
                if (!string.IsNullOrWhiteSpace(descriptor.BoundPin))
                {
                    propertyBackedPins.Add(descriptor.BoundPin);
                }

                yield return descriptor;
            }
        }

        foreach (var pin in GetVisiblePins(node, true))
        {
            var effectivePinType = GetEffectiveInlineInputType(node, pin);
            if (propertyBackedPins.Contains(pin.Name) ||
                _graph.FindInputConnection(node.Id, pin.Name) is not null ||
                !ShouldExposeGenericInlinePin(definition, effectivePinType))
            {
                continue;
            }

            var defaults = GetDefaultInlinePinValues(node.Kind, pin.Name, effectivePinType);
            var (minimum, maximum, step) = GetInlineRange(pin.Name, effectivePinType);
            yield return new InlineEditorDescriptor(
                UiTextHelper.PinLabel(pin),
                effectivePinType,
                SelectInlineVisualKind(pin, effectivePinType),
                minimum,
                maximum,
                step,
                defaults,
                StorageKey: NodeInlineValueCodec.GetStorageKey(pin.Name),
                BoundPin: pin.Name);
        }
    }

    private bool TryCreatePropertyInlineDescriptor(GraphNode node, NodeDefinition definition, NodePropertyDefinition property, out InlineEditorDescriptor descriptor)
    {
        descriptor = null!;

        if (node.Kind == NodeKind.Color && property.Name is "R" or "G" or "B" or "A")
        {
            return false;
        }

        var matchingPin = GetVisiblePins(node, true)
            .FirstOrDefault(pin => string.Equals(pin.Name, property.InlinePin ?? property.Name, StringComparison.OrdinalIgnoreCase));
        if (matchingPin is not null && _graph.FindInputConnection(node.Id, matchingPin.Name) is not null)
        {
            return false;
        }

        var allowInline = property.AllowInlineEditor ||
                          (property.Kind == NodePropertyKind.Float && matchingPin is not null) ||
                          IsImplicitInlineProperty(property.Name);
        if (!allowInline || property.Kind != NodePropertyKind.Float)
        {
            return false;
        }

        var boundPin = matchingPin?.Name;
        var valueType = matchingPin?.Type ?? GraphValueType.Float1;
        if (valueType != GraphValueType.Float1)
        {
            return false;
        }

        var (minimum, maximum, step) = GetInlineRange(property.Name, valueType, property.Minimum, property.Maximum, property.Step);
        descriptor = new InlineEditorDescriptor(
            UiTextHelper.PropertyLabel(property),
            GraphValueType.Float1,
            InlineEditorVisualKind.Numeric,
            minimum,
            maximum,
            step,
            [ReadNodeFloat(node, property.Name, FloatParser.TryParse(property.DefaultValue, out var parsedDefault) ? parsedDefault : 0f)],
            PropertyKeys: [property.Name],
            BoundPin: boundPin);
        return true;
    }

    private InlineEditorDescriptor CreateColorInlineDescriptor()
    {
        return new InlineEditorDescriptor(
            "Color",
            GraphValueType.Float4,
            InlineEditorVisualKind.ColorButton,
            0f,
            1f,
            0.01f,
            [1f, 1f, 1f, 1f],
            PropertyKeys: ["R", "G", "B", "A"]);
    }

    private static bool ShouldExposeGenericInlinePin(NodeDefinition definition, GraphValueType valueType)
    {
        if (definition.Category != NodeCategory.Math)
        {
            return false;
        }

        return valueType is GraphValueType.Float1 or GraphValueType.Float2 or GraphValueType.Float3 or GraphValueType.Float4;
    }

    private static InlineEditorVisualKind SelectInlineVisualKind(NodePinDefinition pin, GraphValueType effectivePinType)
    {
        if (effectivePinType == GraphValueType.Float1)
        {
            return InlineEditorVisualKind.Numeric;
        }

        return IsColorLikeLabel(pin.DisplayName) || IsColorLikeLabel(pin.Name)
            ? InlineEditorVisualKind.ColorButton
            : InlineEditorVisualKind.VectorButton;
    }

    private static (float Minimum, float Maximum, float Step) GetInlineRange(string label, GraphValueType valueType, float? minimum = null, float? maximum = null, float? step = null)
    {
        if (minimum.HasValue && maximum.HasValue)
        {
            return (minimum.Value, maximum.Value, step ?? 0.01f);
        }

        if (valueType != GraphValueType.Float1)
        {
            return (-10f, 10f, 0.01f);
        }

        var normalized = label.Trim();
        if (normalized is "R" or "G" or "B" or "A" or "Opacity" or "Alpha" or "Mask" or "Roughness" or "Metallic" or "Specular" or "Occlusion")
        {
            return (0f, 1f, 0.01f);
        }

        if (normalized is "Exponent" or "Power")
        {
            return (0f, 8f, 0.05f);
        }

        return (-10f, 10f, 0.01f);
    }

    private static float[] GetDefaultInlinePinValues(NodeKind nodeKind, string pinName, GraphValueType valueType)
    {
        var componentCount = NodeInlineValueCodec.GetComponentCount(valueType);
        var defaults = new float[componentCount];

        var fillValue = 0f;
        if (nodeKind is NodeKind.Multiply or NodeKind.Divide)
        {
            fillValue = 1f;
        }
        else if (nodeKind == NodeKind.Lerp && string.Equals(pinName, "B", StringComparison.OrdinalIgnoreCase))
        {
            fillValue = 1f;
        }
        else if (nodeKind == NodeKind.Lerp && string.Equals(pinName, "T", StringComparison.OrdinalIgnoreCase))
        {
            fillValue = 0.5f;
        }
        else if (string.Equals(pinName, "Opacity", StringComparison.OrdinalIgnoreCase))
        {
            fillValue = 1f;
        }

        for (var index = 0; index < componentCount; index++)
        {
            defaults[index] = fillValue;
        }

        return defaults;
    }

    private float GetInlineEditorExtraHeight(GraphNode node)
    {
        var count = GetInlineEditorDescriptors(node).Count();
        if (count == 0)
        {
            return 0f;
        }

        return InlineSectionTopPadding + InlineSectionBottomPadding + (count * InlineRowHeight) + ((count - 1) * InlineRowSpacing);
    }

    private bool TryGetInlineEditorLayout(GraphNode node, PointF canvasPoint, out InlineEditorLayout layout)
    {
        foreach (var candidate in GetInlineEditorLayouts(node))
        {
            if (candidate.EditorBounds.Contains(canvasPoint))
            {
                layout = candidate;
                return true;
            }
        }

        layout = null!;
        return false;
    }

    private float[] ReadInlineEditorValues(GraphNode node, InlineEditorDescriptor descriptor)
    {
        if (descriptor.PropertyKeys is { Length: > 0 })
        {
            var values = new float[descriptor.PropertyKeys.Length];
            for (var index = 0; index < descriptor.PropertyKeys.Length; index++)
            {
                var fallback = index < descriptor.DefaultValues.Length ? descriptor.DefaultValues[index] : 0f;
                values[index] = ReadNodeFloat(node, descriptor.PropertyKeys[index], fallback);
            }

            return values;
        }

        if (!string.IsNullOrWhiteSpace(descriptor.StorageKey) &&
            node.Properties.TryGetValue(descriptor.StorageKey, out var rawValue) &&
            NodeInlineValueCodec.TryParse(rawValue, descriptor.ValueType, out var parsedValues))
        {
            return parsedValues;
        }

        return descriptor.DefaultValues.ToArray();
    }

    private void WriteInlineEditorValues(GraphNode node, InlineEditorDescriptor descriptor, IReadOnlyList<float> values)
    {
        if (descriptor.PropertyKeys is { Length: > 0 })
        {
            for (var index = 0; index < descriptor.PropertyKeys.Length; index++)
            {
                var value = index < values.Count ? values[index] : 0f;
                node.Properties[descriptor.PropertyKeys[index]] = FloatParser.Format(value);
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(descriptor.StorageKey))
        {
            node.Properties[descriptor.StorageKey] = NodeInlineValueCodec.Format(descriptor.ValueType, values);
        }
    }

    private static float RoundToStep(float value, float step)
    {
        if (step <= 0f)
        {
            return value;
        }

        return (float)Math.Round(value / step) * step;
    }

    private static decimal ClampDecimal(decimal value, decimal minimum, decimal maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, value));
    }

    private static string[] GetVectorComponentLabels(GraphValueType valueType)
    {
        return valueType switch
        {
            GraphValueType.Float2 => ["X", "Y"],
            GraphValueType.Float3 => ["X", "Y", "Z"],
            _ => ["X", "Y", "Z", "W"],
        };
    }

    private static bool IsColorLikeLabel(string value)
    {
        return value.Contains("color", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("tint", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("albedo", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("emissive", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsImplicitInlineProperty(string propertyName)
    {
        return propertyName is "Min" or "Max" or "T" or "Edge" or "Exponent" or "Strength" or "Bias" or "Scale";
    }

    private static float GetColorComponent(IReadOnlyList<float> values, int index)
    {
        if (index < values.Count)
        {
            return values[index];
        }

        return index == 3 ? 1f : 0f;
    }

    private static GraphValueType GetEffectiveInlineInputType(GraphNode node, NodePinDefinition pin)
    {
        if (pin.Type != GraphValueType.Float4 ||
            !node.Properties.TryGetValue("Type", out var rawType) ||
            string.IsNullOrWhiteSpace(rawType))
        {
            return pin.Type;
        }

        return rawType.Trim().ToUpperInvariant() switch
        {
            "FLOAT1" => GraphValueType.Float1,
            "FLOAT2" => GraphValueType.Float2,
            "FLOAT3" => GraphValueType.Float3,
            _ => GraphValueType.Float4,
        };
    }

    private void InlineTextBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            if (CommitPendingInlineEditors(showValidationErrors: true))
            {
                Focus();
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            CancelInlineTextEditor();
            Focus();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }

    private void InlineTextBoxOnLeave(object? sender, EventArgs e)
    {
        CommitPendingInlineEditors(showValidationErrors: false);
    }
}
