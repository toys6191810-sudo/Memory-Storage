namespace Memory_Storage;

public class EyeToggleView : GraphicsView, IDrawable
{
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen),
        typeof(bool),
        typeof(EyeToggleView),
        false,
        propertyChanged: (bindable, _, _) => ((EyeToggleView)bindable).Invalidate());

    public EyeToggleView()
    {
        Drawable = this;
        WidthRequest = 24;
        HeightRequest = 18;
        InputTransparent = true;
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;

        var stroke = Color.FromArgb("#FF6A00");
        var centerY = dirtyRect.Center.Y;
        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var left = dirtyRect.Left + 2.5f;
        var right = dirtyRect.Right - 2.5f;

        canvas.StrokeColor = stroke;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        if (IsOpen)
        {
            var top = dirtyRect.Top + height * 0.22f;
            var bottom = dirtyRect.Bottom - height * 0.22f;
            var eye = new PathF();
            eye.MoveTo(left, centerY);
            eye.CurveTo(width * 0.25f, top, width * 0.75f, top, right, centerY);
            eye.CurveTo(width * 0.75f, bottom, width * 0.25f, bottom, left, centerY);
            eye.Close();

            canvas.StrokeSize = 2.0f;
            canvas.FillColor = Colors.Transparent;
            canvas.DrawPath(eye);
            canvas.FillColor = stroke;
            canvas.FillCircle(dirtyRect.Center.X, centerY, 2.6f);
            return;
        }

        var lashTop = dirtyRect.Top + height * 0.33f;
        var lidY = dirtyRect.Top + height * 0.50f;
        var lashBottom = dirtyRect.Bottom - height * 0.22f;
        var lid = new PathF();
        lid.MoveTo(left + 1.5f, lidY);
        lid.CurveTo(width * 0.30f, lashBottom, width * 0.70f, lashBottom, right - 1.5f, lidY);

        canvas.StrokeSize = 2.0f;
        canvas.DrawPath(lid);

        canvas.StrokeSize = 1.7f;
        canvas.DrawLine(width * 0.30f, lidY + 2.2f, width * 0.24f, lashTop + 2.2f);
        canvas.DrawLine(width * 0.50f, lidY + 3.4f, width * 0.50f, lashTop);
        canvas.DrawLine(width * 0.70f, lidY + 2.2f, width * 0.76f, lashTop + 2.2f);
    }
}
