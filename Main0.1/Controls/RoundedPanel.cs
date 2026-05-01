using System.Drawing.Drawing2D;

namespace RayMmdNodeEditor.Controls;

public sealed class RoundedPanel : Panel
{
    private int _cornerRadius = 16;

    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(4, value);
            UpdatePanelRegion();
            Invalidate();
        }
    }

    public Color BorderColor { get; set; } = EditorTheme.BorderSoft;

    public float BorderThickness { get; set; } = 1f;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        BackColor = EditorTheme.Panel;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdatePanelRegion();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = CreateRoundedRectangle(bounds, CornerRadius);
        using var backgroundBrush = new SolidBrush(BackColor);
        using var borderPen = new Pen(BorderColor, BorderThickness);
        e.Graphics.FillPath(backgroundBrush, path);
        e.Graphics.DrawPath(borderPen, path);
        base.OnPaint(e);
    }

    private void UpdatePanelRegion()
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        using var path = CreateRoundedRectangle(new Rectangle(0, 0, Width, Height), CornerRadius);
        Region = new Region(path);
    }

    private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, float radius)
    {
        var safeRadius = Math.Max(1f, Math.Min(radius, Math.Min(bounds.Width, bounds.Height) * 0.5f));
        var diameter = safeRadius * 2f;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
