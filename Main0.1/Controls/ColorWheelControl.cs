using System.Drawing.Drawing2D;

namespace RayMmdNodeEditor.Controls;

public sealed class ColorWheelControl : Control
{
    private const int OuterPadding = 8;
    private const int RingThickness = 18;

    private Bitmap? _ringBitmap;
    private Bitmap? _svBitmap;
    private HsvColor _hsv = new(0f, 1f, 1f);
    private bool _draggingHue;
    private bool _draggingSv;

    public event EventHandler? SelectedColorChanged;

    public ColorWheelControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable, true);
        Size = new Size(220, 220);
        MinimumSize = new Size(180, 180);
        BackColor = Color.FromArgb(45, 48, 54);
    }

    public Color SelectedColor
    {
        get => _hsv.ToColor();
        set
        {
            _hsv = HsvColor.FromColor(value);
            RebuildSvBitmap();
            Invalidate();
        }
    }

    public HsvColor SelectedHsv
    {
        get => _hsv;
        set
        {
            _hsv = value;
            RebuildSvBitmap();
            Invalidate();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ringBitmap?.Dispose();
            _svBitmap?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RebuildRingBitmap();
        RebuildSvBitmap();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(BackColor);

        var ringBounds = GetRingBounds();
        var squareBounds = GetSvSquareBounds();

        if (_ringBitmap is null)
        {
            RebuildRingBitmap();
        }

        if (_svBitmap is null)
        {
            RebuildSvBitmap();
        }

        if (_ringBitmap is not null)
        {
            e.Graphics.DrawImage(_ringBitmap, ringBounds);
        }

        if (_svBitmap is not null)
        {
            e.Graphics.DrawImage(_svBitmap, squareBounds);
            using var squarePen = new Pen(Color.FromArgb(28, 28, 28));
            e.Graphics.DrawRectangle(squarePen, squareBounds);
        }

        DrawHueMarker(e.Graphics, ringBounds);
        DrawSvMarker(e.Graphics, squareBounds);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();

        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        if (TryUpdateHue(e.Location))
        {
            _draggingHue = true;
            return;
        }

        if (TryUpdateSv(e.Location))
        {
            _draggingSv = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_draggingHue)
        {
            TryUpdateHue(e.Location);
        }
        else if (_draggingSv)
        {
            TryUpdateSv(e.Location);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _draggingHue = false;
        _draggingSv = false;
    }

    private bool TryUpdateHue(Point point)
    {
        var ringBounds = GetRingBounds();
        var center = new PointF(ringBounds.Left + (ringBounds.Width / 2f), ringBounds.Top + (ringBounds.Height / 2f));
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;
        var distance = MathF.Sqrt((dx * dx) + (dy * dy));
        var outerRadius = ringBounds.Width / 2f;
        var innerRadius = outerRadius - RingThickness;

        if (distance < innerRadius || distance > outerRadius)
        {
            return false;
        }

        var angle = MathF.Atan2(-dy, dx) * 180f / MathF.PI;
        var hue = (angle + 360f) % 360f;
        _hsv = new HsvColor(hue, _hsv.Saturation, _hsv.Value);
        RebuildSvBitmap();
        Invalidate();
        SelectedColorChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private bool TryUpdateSv(Point point)
    {
        var squareBounds = GetSvSquareBounds();
        if (!squareBounds.Contains(point))
        {
            return false;
        }

        var saturation = (point.X - squareBounds.Left) / (float)Math.Max(1, squareBounds.Width - 1);
        var value = 1f - ((point.Y - squareBounds.Top) / (float)Math.Max(1, squareBounds.Height - 1));
        _hsv = new HsvColor(_hsv.Hue, saturation, value);
        Invalidate();
        SelectedColorChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private void RebuildRingBitmap()
    {
        _ringBitmap?.Dispose();
        _ringBitmap = null;

        var ringBounds = GetRingBounds();
        if (ringBounds.Width <= 0 || ringBounds.Height <= 0)
        {
            return;
        }

        var bitmap = new Bitmap(ringBounds.Width, ringBounds.Height);
        var center = new PointF(bitmap.Width / 2f, bitmap.Height / 2f);
        var outerRadius = bitmap.Width / 2f;
        var innerRadius = outerRadius - RingThickness;

        // The outer ring encodes hue; the inner square handles saturation and value.
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var dx = x - center.X;
                var dy = y - center.Y;
                var distance = MathF.Sqrt((dx * dx) + (dy * dy));
                if (distance < innerRadius || distance > outerRadius)
                {
                    bitmap.SetPixel(x, y, Color.Transparent);
                    continue;
                }

                var angle = MathF.Atan2(-dy, dx) * 180f / MathF.PI;
                var hue = (angle + 360f) % 360f;
                bitmap.SetPixel(x, y, new HsvColor(hue, 1f, 1f).ToColor());
            }
        }

        _ringBitmap = bitmap;
    }

    private void RebuildSvBitmap()
    {
        _svBitmap?.Dispose();
        _svBitmap = null;

        var squareBounds = GetSvSquareBounds();
        if (squareBounds.Width <= 0 || squareBounds.Height <= 0)
        {
            return;
        }

        var bitmap = new Bitmap(squareBounds.Width, squareBounds.Height);

        // Horizontal axis is saturation; vertical axis is value.
        for (var y = 0; y < bitmap.Height; y++)
        {
            var value = 1f - (y / (float)Math.Max(1, bitmap.Height - 1));
            for (var x = 0; x < bitmap.Width; x++)
            {
                var saturation = x / (float)Math.Max(1, bitmap.Width - 1);
                bitmap.SetPixel(x, y, new HsvColor(_hsv.Hue, saturation, value).ToColor());
            }
        }

        _svBitmap = bitmap;
    }

    private Rectangle GetRingBounds()
    {
        var size = Math.Min(Width, Height) - (OuterPadding * 2);
        size = Math.Max(size, 0);
        var x = (Width - size) / 2;
        var y = (Height - size) / 2;
        return new Rectangle(x, y, size, size);
    }

    private Rectangle GetSvSquareBounds()
    {
        var ringBounds = GetRingBounds();
        var outerRadius = ringBounds.Width / 2f;
        var innerRadius = Math.Max(outerRadius - RingThickness - 4f, 1f);
        var side = (int)Math.Floor((innerRadius * MathF.Sqrt(2f)) - 10f);
        side = Math.Max(side, 20);
        var x = ringBounds.Left + ((ringBounds.Width - side) / 2);
        var y = ringBounds.Top + ((ringBounds.Height - side) / 2);
        return new Rectangle(x, y, side, side);
    }

    private void DrawHueMarker(Graphics graphics, Rectangle ringBounds)
    {
        var center = new PointF(ringBounds.Left + (ringBounds.Width / 2f), ringBounds.Top + (ringBounds.Height / 2f));
        var radius = (ringBounds.Width / 2f) - (RingThickness / 2f);
        var radians = (_hsv.Hue * MathF.PI) / 180f;
        var point = new PointF(
            center.X + (MathF.Cos(radians) * radius),
            center.Y - (MathF.Sin(radians) * radius));

        using var outerPen = new Pen(Color.Black, 3f);
        using var innerPen = new Pen(Color.White, 1.5f);
        graphics.DrawEllipse(outerPen, point.X - 7f, point.Y - 7f, 14f, 14f);
        graphics.DrawEllipse(innerPen, point.X - 7f, point.Y - 7f, 14f, 14f);
    }

    private void DrawSvMarker(Graphics graphics, Rectangle squareBounds)
    {
        var point = new PointF(
            squareBounds.Left + (_hsv.Saturation * squareBounds.Width),
            squareBounds.Top + ((1f - _hsv.Value) * squareBounds.Height));

        using var outerPen = new Pen(Color.Black, 3f);
        using var innerPen = new Pen(Color.White, 1.5f);
        graphics.DrawEllipse(outerPen, point.X - 6f, point.Y - 6f, 12f, 12f);
        graphics.DrawEllipse(innerPen, point.X - 6f, point.Y - 6f, 12f, 12f);
    }
}
