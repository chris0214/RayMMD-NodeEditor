namespace RayMmdNodeEditor.Controls;

public sealed class InlineColorPickerControl : UserControl
{
    private readonly ColorWheelControl _colorWheel = new();
    private readonly Panel _previewPanel = new();
    private readonly GradientSliderControl _hueSlider = new() { Kind = GradientSliderKind.Hue };
    private readonly GradientSliderControl _alphaSlider = new() { Kind = GradientSliderKind.Alpha };
    private bool _updatingUi;

    public event EventHandler? SelectedColorChanged;

    public InlineColorPickerControl()
    {
        Size = new Size(252, 316);
        MinimumSize = Size;
        MaximumSize = Size;
        BackColor = EditorTheme.Panel;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10),
            BackColor = EditorTheme.Panel,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18f));
        Controls.Add(layout);

        _colorWheel.Dock = DockStyle.Fill;
        _colorWheel.Margin = new Padding(0);
        layout.Controls.Add(_colorWheel, 0, 0);

        _previewPanel.Dock = DockStyle.Fill;
        _previewPanel.Margin = new Padding(0, 8, 0, 6);
        _previewPanel.BorderStyle = BorderStyle.FixedSingle;
        layout.Controls.Add(_previewPanel, 0, 1);

        _hueSlider.Dock = DockStyle.Fill;
        _hueSlider.Margin = new Padding(0, 0, 0, 6);
        layout.Controls.Add(_hueSlider, 0, 2);

        _alphaSlider.Dock = DockStyle.Fill;
        _alphaSlider.Margin = new Padding(0);
        layout.Controls.Add(_alphaSlider, 0, 3);

        _colorWheel.SelectedColorChanged += (_, _) =>
        {
            if (_updatingUi)
            {
                return;
            }

            var rgb = _colorWheel.SelectedColor;
            ApplyColor(Color.FromArgb(CurrentAlphaByte, rgb.R, rgb.G, rgb.B));
        };

        _hueSlider.ValueChanged += (_, _) =>
        {
            if (_updatingUi)
            {
                return;
            }

            var hsv = _colorWheel.SelectedHsv;
            _colorWheel.SelectedHsv = new HsvColor(_hueSlider.Value * 360f, hsv.Saturation, hsv.Value);
            var rgb = _colorWheel.SelectedColor;
            ApplyColor(Color.FromArgb(CurrentAlphaByte, rgb.R, rgb.G, rgb.B));
        };

        _alphaSlider.ValueChanged += (_, _) =>
        {
            if (_updatingUi)
            {
                return;
            }

            var rgb = _colorWheel.SelectedColor;
            ApplyColor(Color.FromArgb(CurrentAlphaByte, rgb.R, rgb.G, rgb.B));
        };

        SetSelectedColor(Color.White);
        EditorTheme.ApplyThemeRecursive(this);
    }

    public Color SelectedColor { get; private set; }

    private int CurrentAlphaByte => Math.Clamp((int)Math.Round(_alphaSlider.Value * 255d), 0, 255);

    public void SetSelectedColor(Color color)
    {
        ApplyColor(color, true);
    }

    private void ApplyColor(Color color, bool force = false)
    {
        if (!force && color.ToArgb() == SelectedColor.ToArgb())
        {
            return;
        }

        SelectedColor = color;
        _updatingUi = true;

        try
        {
            _previewPanel.BackColor = color;
            var hsv = HsvColor.FromColor(Color.FromArgb(255, color.R, color.G, color.B));
            _colorWheel.SelectedHsv = hsv;
            _hueSlider.Value = hsv.Hue / 360f;
            _alphaSlider.BaseColor = Color.FromArgb(255, color.R, color.G, color.B);
            _alphaSlider.Value = color.A / 255f;
        }
        finally
        {
            _updatingUi = false;
        }

        SelectedColorChanged?.Invoke(this, EventArgs.Empty);
    }
}
