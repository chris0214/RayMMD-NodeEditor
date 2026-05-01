using System.Drawing.Drawing2D;

namespace RayMmdNodeEditor.Controls;

public sealed class GradientSliderControl : Control
{
    private const int MarkerWidth = 8;
    private bool _dragging;
    private float _value;

    public event EventHandler? ValueChanged;

    public GradientSliderControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable, true);
        Height = 16;
        MinimumSize = new Size(80, 16);
        BackColor = EditorTheme.Panel;
    }

    public GradientSliderKind Kind { get; set; } = GradientSliderKind.Hue;

    public Color BaseColor { get; set; } = Color.White;

    public float Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_value - clamped) < 0.0001f)
            {
                return;
            }

            _value = clamped;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var barRect = new RectangleF(4f, 4f, Width - 8f, Height - 8f);
        using var path = CreateRoundedRectangle(barRect, 5f);
        using var brush = CreateBrush(barRect);
        using var borderPen = new Pen(Color.FromArgb(92, 16, 18, 24), 1f);
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(borderPen, path);

        var markerX = barRect.Left + (barRect.Width * _value);
        var markerRect = new RectangleF(markerX - (MarkerWidth / 2f), 1f, MarkerWidth, Height - 2f);
        using var markerBrush = new SolidBrush(Color.FromArgb(240, 248, 250, 252));
        using var markerPen = new Pen(Color.FromArgb(120, 16, 18, 24), 1f);
        e.Graphics.FillRectangle(markerBrush, markerRect);
        e.Graphics.DrawRectangle(markerPen, markerRect.X, markerRect.Y, markerRect.Width, markerRect.Height);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        Focus();
        _dragging = true;
        UpdateValueFromPoint(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging)
        {
            UpdateValueFromPoint(e.Location);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _dragging = false;
    }

    private void UpdateValueFromPoint(Point point)
    {
        var width = Math.Max(1f, Width - 8f);
        Value = Math.Clamp((point.X - 4f) / width, 0f, 1f);
    }

    private Brush CreateBrush(RectangleF rect)
    {
        return Kind switch
        {
            GradientSliderKind.Alpha => new LinearGradientBrush(
                rect,
                Color.FromArgb(0, BaseColor),
                Color.FromArgb(255, BaseColor),
                LinearGradientMode.Horizontal),
            _ => CreateHueBrush(rect),
        };
    }

    private static Brush CreateHueBrush(RectangleF rect)
    {
        var brush = new LinearGradientBrush(rect, Color.Red, Color.Red, LinearGradientMode.Horizontal);
        var blend = new ColorBlend
        {
            Positions = [0f, 1f / 6f, 2f / 6f, 3f / 6f, 4f / 6f, 5f / 6f, 1f],
            Colors =
            [
                Color.Red,
                Color.Yellow,
                Color.Lime,
                Color.Cyan,
                Color.Blue,
                Color.Magenta,
                Color.Red,
            ],
        };
        brush.InterpolationColors = blend;
        return brush;
    }

    private static GraphicsPath CreateRoundedRectangle(RectangleF rect, float radius)
    {
        var diameter = radius * 2f;
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180f, 90f);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270f, 90f);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0f, 90f);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90f, 90f);
        path.CloseFigure();
        return path;
    }
}

public enum GradientSliderKind
{
    Hue,
    Alpha,
}
