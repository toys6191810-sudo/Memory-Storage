namespace Memory_Storage;

public sealed class GeometryBackgroundView : GraphicsView, IDrawable
{
    private readonly GeometryShape[] shapes =
    [
        new(0.10f, 0.18f, 72, 0.20f, GeometryShapeKind.Triangle),
        new(0.82f, 0.16f, 88, 0.34f, GeometryShapeKind.Hexagon),
        new(0.18f, 0.78f, 96, 0.46f, GeometryShapeKind.Square),
        new(0.76f, 0.72f, 70, 0.62f, GeometryShapeKind.Triangle),
        new(0.48f, 0.14f, 48, 0.80f, GeometryShapeKind.Line),
        new(0.58f, 0.84f, 110, 0.12f, GeometryShapeKind.Hexagon),
        new(0.30f, 0.34f, 46, 0.58f, GeometryShapeKind.Line),
        new(0.92f, 0.48f, 54, 0.26f, GeometryShapeKind.Square)
    ];

    public GeometryBackgroundView()
    {
        Drawable = this;
        InputTransparent = true;
    }

    public double AnimationProgress { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;

        for (var index = 0; index < shapes.Length; index++)
        {
            var shape = shapes[index];
            var wave = Math.Sin(AnimationProgress + shape.Phase * Math.Tau);
            var drift = Math.Cos(AnimationProgress * 0.72 + shape.Phase * Math.Tau);
            var x = dirtyRect.Width * shape.X + (float)(wave * 14);
            var y = dirtyRect.Height * shape.Y + (float)(drift * 12);
            var rotation = (float)((AnimationProgress * 18 + shape.Phase * 220) % 360);
            var alpha = AppUi.IsDarkMode ? 0.16f : 0.18f;

            canvas.SaveState();
            canvas.Translate(x, y);
            canvas.Rotate(rotation);
            canvas.StrokeSize = 2.2f;
            canvas.StrokeColor = AppUi.Primary.WithAlpha(alpha);
            canvas.FillColor = AppUi.Primary.WithAlpha(alpha * 0.18f);

            switch (shape.Kind)
            {
                case GeometryShapeKind.Triangle:
                    DrawTriangle(canvas, shape.Size);
                    break;
                case GeometryShapeKind.Hexagon:
                    DrawHexagon(canvas, shape.Size);
                    break;
                case GeometryShapeKind.Square:
                    DrawSquare(canvas, shape.Size);
                    break;
                default:
                    DrawLine(canvas, shape.Size);
                    break;
            }

            canvas.RestoreState();
        }

        DrawConnectorLines(canvas, dirtyRect);
    }

    private static void DrawTriangle(ICanvas canvas, float size)
    {
        var path = new PathF();
        path.MoveTo(0, -size * 0.48f);
        path.LineTo(size * 0.46f, size * 0.35f);
        path.LineTo(-size * 0.46f, size * 0.35f);
        path.Close();
        canvas.FillPath(path);
        canvas.DrawPath(path);
    }

    private static void DrawHexagon(ICanvas canvas, float size)
    {
        var radius = size * 0.48f;
        var path = new PathF();

        for (var index = 0; index < 6; index++)
        {
            var angle = MathF.PI / 3 * index;
            var x = MathF.Cos(angle) * radius;
            var y = MathF.Sin(angle) * radius;

            if (index == 0)
            {
                path.MoveTo(x, y);
            }
            else
            {
                path.LineTo(x, y);
            }
        }

        path.Close();
        canvas.FillPath(path);
        canvas.DrawPath(path);
    }

    private static void DrawSquare(ICanvas canvas, float size)
    {
        var rect = new RectF(-size * 0.42f, -size * 0.42f, size * 0.84f, size * 0.84f);
        canvas.FillRoundedRectangle(rect, 10);
        canvas.DrawRoundedRectangle(rect, 10);
    }

    private static void DrawLine(ICanvas canvas, float size)
    {
        canvas.DrawLine(-size * 0.5f, 0, size * 0.5f, 0);
        canvas.DrawLine(0, -size * 0.25f, 0, size * 0.25f);
    }

    private static void DrawConnectorLines(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeSize = 1.4f;
        canvas.StrokeColor = AppUi.Primary.WithAlpha(AppUi.IsDarkMode ? 0.08f : 0.10f);
        canvas.DrawLine(dirtyRect.Width * 0.14f, dirtyRect.Height * 0.22f, dirtyRect.Width * 0.34f, dirtyRect.Height * 0.36f);
        canvas.DrawLine(dirtyRect.Width * 0.66f, dirtyRect.Height * 0.76f, dirtyRect.Width * 0.86f, dirtyRect.Height * 0.54f);
        canvas.DrawLine(dirtyRect.Width * 0.50f, dirtyRect.Height * 0.18f, dirtyRect.Width * 0.78f, dirtyRect.Height * 0.22f);
    }

    private readonly record struct GeometryShape(float X, float Y, float Size, float Phase, GeometryShapeKind Kind);

    private enum GeometryShapeKind
    {
        Triangle,
        Hexagon,
        Square,
        Line
    }
}
