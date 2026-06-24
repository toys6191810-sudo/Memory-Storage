namespace Memory_Storage;

public class AvatarIconView : GraphicsView, IDrawable
{
    public AvatarIconView()
    {
        Drawable = this;
        WidthRequest = 28;
        HeightRequest = 28;
        InputTransparent = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;
        canvas.StrokeColor = AppUi.Primary;
        canvas.FillColor = Colors.Transparent;
        canvas.StrokeSize = 2.4f;

        var cx = dirtyRect.Center.X;
        var headY = dirtyRect.Top + dirtyRect.Height * 0.34f;
        canvas.DrawCircle(cx, headY, dirtyRect.Width * 0.16f);

        var shoulders = new PathF();
        shoulders.MoveTo(dirtyRect.Left + dirtyRect.Width * 0.22f, dirtyRect.Bottom - dirtyRect.Height * 0.18f);
        shoulders.CurveTo(
            dirtyRect.Left + dirtyRect.Width * 0.30f,
            dirtyRect.Top + dirtyRect.Height * 0.64f,
            dirtyRect.Left + dirtyRect.Width * 0.70f,
            dirtyRect.Top + dirtyRect.Height * 0.64f,
            dirtyRect.Right - dirtyRect.Width * 0.22f,
            dirtyRect.Bottom - dirtyRect.Height * 0.18f);
        canvas.DrawPath(shoulders);
    }
}

public class SettingsIconView : GraphicsView, IDrawable
{
    public SettingsIconView()
    {
        Drawable = this;
        WidthRequest = 28;
        HeightRequest = 28;
        InputTransparent = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;
        canvas.StrokeColor = AppUi.Primary;
        canvas.FillColor = Colors.Transparent;
        canvas.StrokeSize = 2.3f;
        canvas.StrokeLineCap = LineCap.Round;

        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var toothInner = dirtyRect.Width * 0.26f;
        var toothOuter = dirtyRect.Width * 0.41f;

        for (var i = 0; i < 8; i++)
        {
            var angle = MathF.PI * 2 * i / 8;
            var x1 = cx + MathF.Cos(angle) * toothInner;
            var y1 = cy + MathF.Sin(angle) * toothInner;
            var x2 = cx + MathF.Cos(angle) * toothOuter;
            var y2 = cy + MathF.Sin(angle) * toothOuter;
            canvas.DrawLine(x1, y1, x2, y2);
        }

        canvas.StrokeSize = 2.4f;
        canvas.DrawCircle(cx, cy, dirtyRect.Width * 0.24f);
        canvas.FillColor = AppUi.Primary;
        canvas.FillCircle(cx, cy, dirtyRect.Width * 0.08f);
    }
}
