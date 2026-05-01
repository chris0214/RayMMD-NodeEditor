namespace RayMmdNodeEditor.Controls;

public sealed class ColorEditorControl : UserControl
{
    private readonly TableLayoutPanel _layout = new();
    private readonly Panel _wheelPanel = new();
    private readonly TableLayoutPanel _rightPanel = new();
    private readonly ColorWheelControl _colorWheel = new();
    private readonly TrackBar _alphaBar = new();
    private readonly Panel _previewPanel = new();
    private readonly NumericUpDown _redInput = CreateNumberInput();
    private readonly NumericUpDown _greenInput = CreateNumberInput();
    private readonly NumericUpDown _blueInput = CreateNumberInput();
    private readonly NumericUpDown _alphaInput = CreateNumberInput();
    private readonly Label _tipLabel;
    private bool _updatingUi;

    public event EventHandler? SelectedColorChanged;

    public ColorEditorControl()
    {
        Dock = DockStyle.Top;
        AutoSize = true;
        BackColor = EditorTheme.Panel;

        _layout.Dock = DockStyle.Top;
        _layout.AutoSize = true;
        _layout.ColumnCount = 2;
        _layout.Padding = new Padding(0, 12, 0, 0);
        Controls.Add(_layout);

        _wheelPanel.Dock = DockStyle.Fill;
        _wheelPanel.Padding = new Padding(0, 0, 12, 0);
        _wheelPanel.AutoSize = true;
        _colorWheel.Dock = DockStyle.Top;
        _wheelPanel.Controls.Add(_colorWheel);

        _rightPanel.Dock = DockStyle.Fill;
        _rightPanel.AutoSize = true;
        _rightPanel.RowCount = 4;
        _rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));
        _rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 72f));
        _rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var previewBox = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Color Preview",
        };
        _previewPanel.Dock = DockStyle.Fill;
        _previewPanel.BorderStyle = BorderStyle.FixedSingle;
        previewBox.Controls.Add(_previewPanel);
        _rightPanel.Controls.Add(previewBox, 0, 0);

        var alphaBox = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Alpha",
        };
        _alphaBar.Dock = DockStyle.Fill;
        _alphaBar.Minimum = 0;
        _alphaBar.Maximum = 1000;
        _alphaBar.TickFrequency = 100;
        _alphaBar.ValueChanged += (_, _) =>
        {
            if (_updatingUi)
            {
                return;
            }

            var rgb = _colorWheel.SelectedColor;
            ApplyColor(Color.FromArgb(CurrentAlphaByte, rgb.R, rgb.G, rgb.B));
        };
        alphaBox.Controls.Add(_alphaBar);
        _rightPanel.Controls.Add(alphaBox, 0, 1);

        _rightPanel.Controls.Add(BuildNumberTable(), 0, 2);
        _tipLabel = BuildTipLabel();
        _rightPanel.Controls.Add(_tipLabel, 0, 3);

        _colorWheel.SelectedColorChanged += (_, _) =>
        {
            if (_updatingUi)
            {
                return;
            }

            var rgb = _colorWheel.SelectedColor;
            ApplyColor(Color.FromArgb(CurrentAlphaByte, rgb.R, rgb.G, rgb.B));
        };

        SelectedColor = Color.White;
        EditorTheme.ApplyThemeRecursive(this);
        UpdateResponsiveLayout();
    }

    public Color SelectedColor { get; private set; }

    private int CurrentAlphaByte => Math.Clamp((int)Math.Round(_alphaBar.Value / 1000d * 255d), 0, 255);

    public void SetSelectedColor(Color color)
    {
        ApplyColor(color, force: true, raiseEvent: false);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateResponsiveLayout();
    }

    private Control BuildNumberTable()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        AddNumberRow(table, "Red", _redInput, 0);
        AddNumberRow(table, "Green", _greenInput, 1);
        AddNumberRow(table, "Blue", _blueInput, 2);
        AddNumberRow(table, "Alpha", _alphaInput, 3);

        _redInput.ValueChanged += (_, _) => ApplyFromNumbers();
        _greenInput.ValueChanged += (_, _) => ApplyFromNumbers();
        _blueInput.ValueChanged += (_, _) => ApplyFromNumbers();
        _alphaInput.ValueChanged += (_, _) => ApplyFromNumbers();

        return table;
    }

    private Label BuildTipLabel()
    {
        return new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0),
            Text = "The wheel controls hue. The square controls saturation and value. Use the numeric fields for precise input.",
        };
    }

    private void UpdateResponsiveLayout()
    {
        if (IsDisposed)
        {
            return;
        }

        _layout.SuspendLayout();
        try
        {
            _layout.Controls.Clear();
            _layout.ColumnStyles.Clear();
            _layout.RowStyles.Clear();

            var availableWidth = Math.Max(Width - Padding.Horizontal, 0);
            var narrow = availableWidth > 0 && availableWidth < 470;
            var wheelSize = narrow
                ? Math.Clamp(availableWidth - 24, 180, 220)
                : 220;
            _colorWheel.Size = new Size(wheelSize, wheelSize);

            if (narrow)
            {
                _layout.ColumnCount = 1;
                _layout.RowCount = 2;
                _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                _wheelPanel.Padding = new Padding(0, 0, 0, 12);
                _layout.Controls.Add(_wheelPanel, 0, 0);
                _layout.Controls.Add(_rightPanel, 0, 1);
            }
            else
            {
                _layout.ColumnCount = 2;
                _layout.RowCount = 1;
                _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, wheelSize + 12f));
                _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                _wheelPanel.Padding = new Padding(0, 0, 12, 0);
                _layout.Controls.Add(_wheelPanel, 0, 0);
                _layout.Controls.Add(_rightPanel, 1, 0);
            }

            var tipWidth = narrow
                ? Math.Max(availableWidth - 24, 160)
                : Math.Max(availableWidth - (wheelSize + 40), 160);
            _tipLabel.MaximumSize = new Size(tipWidth, 0);
        }
        finally
        {
            _layout.ResumeLayout();
        }
    }

    private void ApplyFromNumbers()
    {
        if (_updatingUi)
        {
            return;
        }

        var red = (int)Math.Round((double)_redInput.Value * 255d);
        var green = (int)Math.Round((double)_greenInput.Value * 255d);
        var blue = (int)Math.Round((double)_blueInput.Value * 255d);
        var alpha = (int)Math.Round((double)_alphaInput.Value * 255d);
        ApplyColor(Color.FromArgb(alpha, red, green, blue));
    }

    private void ApplyColor(Color color, bool force = false, bool raiseEvent = true)
    {
        if (!force && color.ToArgb() == SelectedColor.ToArgb())
        {
            return;
        }

        SelectedColor = color;
        _updatingUi = true;

        try
        {
            // Update all linked controls in one pass to avoid feedback loops.
            _previewPanel.BackColor = color;
            _colorWheel.SelectedColor = Color.FromArgb(255, color.R, color.G, color.B);
            _alphaBar.Value = Math.Clamp((int)Math.Round(color.A / 255d * 1000d), _alphaBar.Minimum, _alphaBar.Maximum);
            _redInput.Value = ClampDecimal(color.R / 255m);
            _greenInput.Value = ClampDecimal(color.G / 255m);
            _blueInput.Value = ClampDecimal(color.B / 255m);
            _alphaInput.Value = ClampDecimal(color.A / 255m);
        }
        finally
        {
            _updatingUi = false;
        }

        if (raiseEvent)
        {
            SelectedColorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static decimal ClampDecimal(decimal value)
    {
        return Math.Min(1m, Math.Max(0m, value));
    }

    private static void AddNumberRow(TableLayoutPanel table, string labelText, Control input, int rowIndex)
    {
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 28,
            Margin = new Padding(0, 0, 8, 8),
        }, 0, rowIndex);
        table.Controls.Add(input, 1, rowIndex);
    }

    private static NumericUpDown CreateNumberInput()
    {
        var input = new NumericUpDown
        {
            Dock = DockStyle.Top,
            DecimalPlaces = 3,
            Increment = 0.01m,
            Minimum = 0m,
            Maximum = 1m,
            Margin = new Padding(0, 0, 0, 8),
            ThousandsSeparator = false,
        };
        EditorTheme.StyleNumeric(input);
        return input;
    }
}
