using SkiaSharp;

namespace sparkvial_app {
    static class PaintUtils {
        public static void DrawRoundRect(SKCanvas canvas, float x, float y, float w, float h, float r1, float r2, float r3, float r4, SKPaint paint) {
            var rr = new SKRoundRect();
            rr.SetRectRadii(new SKRect() {
                Left = x + w,
                Top = y,
                Right = x,
                Bottom = y + h
            }, new SKPoint[] { new SKPoint(r1, r1), new SKPoint(r2, r2), new SKPoint(r3, r3), new SKPoint(r4, r4) });
            canvas.DrawRoundRect(rr, paint);
        }
    }
}
