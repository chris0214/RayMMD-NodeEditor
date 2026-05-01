using System.Drawing.Drawing2D;

namespace RayMmdNodeEditor.Controls;

public sealed class RoundedTabControl : TabControl
{
    public RoundedTabControl()
    {
        DrawMode = TabDrawMode.OwnerDrawFixed;
        SizeMode = TabSizeMode.Fixed;
        ItemSize = new Size(136, 34);
        Padding = new Point(18, 6);
        Font = new Font("Segoe UI", 9f, FontStyle.Regular);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        if (e.Control is TabPage page)
        {
            page.BackColor = EditorTheme.WindowBack;
            page.ForeColor = EditorTheme.TextPrimary;
        }
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var tabBounds = GetTabRect(e.Index);
        tabBounds.Inflate(-4, -3);

        var selected = SelectedIndex == e.Index;
        using var path = CreateRoundedTabPath(tabBounds, 11f);
        using var fillBrush = new SolidBrush(selected ? EditorTheme.PanelRaised : EditorTheme.Chrome);
        using var borderPen = new Pen(selected ? EditorTheme.Accent : EditorTheme.BorderSoft, selected ? 1.8f : 1.1f);
        using var textBrush = new SolidBrush(selected ? EditorTheme.TextPrimary : EditorTheme.TextSecondary);
        using var textFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap,
        };

        graphics.FillPath(fillBrush, path);
        graphics.DrawPath(borderPen, path);
        graphics.DrawString(TabPages[e.Index].Text, Font, textBrush, tabBounds, textFormat);
    }

    private static GraphicsPath CreateRoundedTabPath(Rectangle bounds, float radius)
    {
        var diameter = radius * 2f;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
