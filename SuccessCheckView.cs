namespace Memory_Storage;

public class SuccessCheckView : GraphicsView, IDrawable
{
    public SuccessCheckView()
    {
        Drawable = this;
        WidthRequest = 92;
        HeightRequest = 92;
        InputTransparent = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;

        var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) * 0.46f;
        canvas.FillColor = Color.FromArgb("#D8FBE4");
        canvas.FillCircle(dirtyRect.Center.X, dirtyRect.Center.Y, radius);

        canvas.StrokeColor = Color.FromArgb("#14974F");
        canvas.StrokeSize = 5.2f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        var check = new PathF();
        check.MoveTo(dirtyRect.Left + dirtyRect.Width * 0.32f, dirtyRect.Top + dirtyRect.Height * 0.55f);
        check.LineTo(dirtyRect.Left + dirtyRect.Width * 0.45f, dirtyRect.Top + dirtyRect.Height * 0.68f);
        check.LineTo(dirtyRect.Left + dirtyRect.Width * 0.70f, dirtyRect.Top + dirtyRect.Height * 0.34f);
        canvas.DrawPath(check);
    }
}
