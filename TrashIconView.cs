namespace Memory_Storage;

public sealed class TrashIconView : GraphicsView, IDrawable
{
    public TrashIconView()
    {
        Drawable = this;
        WidthRequest = 24;
        HeightRequest = 24;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var ink = AppUi.IsDarkMode ? Colors.White : Color.FromArgb("#111816");
        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var cx = width / 2f;

        canvas.StrokeColor = ink;
        canvas.StrokeSize = 2.4f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.FillColor = Colors.Transparent;

        canvas.DrawLine(width * 0.22f, height * 0.28f, width * 0.78f, height * 0.28f);
        canvas.DrawLine(width * 0.38f, height * 0.18f, width * 0.62f, height * 0.18f);
        canvas.DrawLine(width * 0.44f, height * 0.18f, width * 0.47f, height * 0.12f);
        canvas.DrawLine(width * 0.56f, height * 0.18f, width * 0.53f, height * 0.12f);

        var body = new PathF();
        body.MoveTo(width * 0.30f, height * 0.34f);
        body.LineTo(width * 0.36f, height * 0.84f);
        body.QuadTo(cx, height * 0.92f, width * 0.64f, height * 0.84f);
        body.LineTo(width * 0.70f, height * 0.34f);
        canvas.DrawPath(body);

        canvas.DrawLine(width * 0.43f, height * 0.43f, width * 0.45f, height * 0.76f);
        canvas.DrawLine(width * 0.57f, height * 0.43f, width * 0.55f, height * 0.76f);
    }
}
