using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace sparkvial_app.nodes {
    public static class GraphStyle {
        public static float ChartMinWidth = 360;
        public static float ChartHeight = 200;
        public static float HandleWidth = 40;
        public static float RowHeight = 40;
        public static float TextSize = 24;
        public static float CornerRadius = 10;
        public static float HandleHorizontalPadding = 18;
        public static int FadeOpacity = 150;
        public static int FadeBlur = 6;
        public static int TextFadeBlur = 2;
        public static float GridlineDistance = 60;

        public static readonly SKPaint InputPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(0, 162, 232).ToSKColor(),
            IsAntialias = true
        };

        public static readonly SKPaint InputSepPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(130, 218, 255).ToSKColor(),
            IsAntialias = true
        };

        public static readonly SKPaint InputHandlePaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(40, 190, 255).ToSKColor(),
            IsAntialias = true
        };

        public static readonly SKPaint OutputPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(255, 127, 39).ToSKColor(),
            IsAntialias = true
        };

        public static readonly SKPaint OutputSepPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(214, 104, 5).ToSKColor(),
            IsAntialias = true
        };

        public static readonly SKPaint OutputHandlePaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(255, 168, 111).ToSKColor(),
            IsAntialias = true
        };

        public static readonly SKPaint ChartPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(88, 88, 88).ToSKColor(),
            IsAntialias = true
        };

        public static readonly SKPaint ChartLinePaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(225, 225, 225).ToSKColor(),
            StrokeWidth = 2,
            IsAntialias = true
        };

        public static readonly SKPaint ChartFaintLinePaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(180, 180, 180).ToSKColor(),
            StrokeWidth = 1.5f,
            TextSize = 14,
            IsAntialias = true
        };

        public static readonly SKPaint ChartBoldLinePaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(245, 245, 245).ToSKColor(),
            StrokeWidth = 3,
            TextSize = 14,
            IsAntialias = true
        };

        public static readonly SKPaint ChartCenteredBoldLinePaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(245, 245, 245).ToSKColor(),
            StrokeWidth = 3,
            TextSize = 14,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };

        public static readonly SKPaint ChartShadowLinePaint = new SKPaint {
            Style = SKPaintStyle.StrokeAndFill,
            Color = Color.FromRgb(88, 88, 88).ToSKColor(),
            StrokeWidth = 5,
            TextSize = 14,
            IsAntialias = true
        };

        public static readonly SKPaint ChartCenteredShadowLinePaint = new SKPaint {
            Style = SKPaintStyle.StrokeAndFill,
            Color = Color.FromRgb(88, 88, 88).ToSKColor(),
            StrokeWidth = 5,
            TextSize = 14,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };

        public static readonly SKPaint FadedInputPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgba(0, 162, 232, FadeOpacity).ToSKColor(),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, FadeBlur),
            IsAntialias = true
        };

        public static readonly SKPaint FadedInputSepPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgba(130, 218, 255, FadeOpacity).ToSKColor(),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, FadeBlur),
            IsAntialias = true
        };

        public static readonly SKPaint FadedInputHandlePaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgba(40, 190, 255, FadeOpacity).ToSKColor(),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, FadeBlur),
            IsAntialias = true
        };

        public static readonly SKPaint FadedOutputPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgba(255, 127, 39, FadeOpacity).ToSKColor(),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, FadeBlur),
            IsAntialias = true
        };

        public static readonly SKPaint FadedOutputSepPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgba(214, 104, 5, FadeOpacity).ToSKColor(),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, FadeBlur),
            IsAntialias = true
        };

        public static readonly SKPaint FadedOutputHandlePaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgba(255, 168, 111, FadeOpacity).ToSKColor(),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, FadeBlur),
            IsAntialias = true
        };

        public static readonly SKPaint FadedChartPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(88, 88, 88).ToSKColor(),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, FadeBlur),
            IsAntialias = true
        };

        public static readonly SKPaint FadedChartLinePaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(225, 225, 225).ToSKColor(),
            StrokeWidth = 2,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, FadeBlur),
            IsAntialias = true
        };

        public static readonly SKPaint ConnectionPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(163, 73, 164).ToSKColor(),
            StrokeWidth = 5,
            IsAntialias = true
        };

        public static readonly SKPaint TextPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(255, 255, 255).ToSKColor(),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left,
            TextSize = TextSize,
            Typeface = SKTypeface.FromFamilyName("Segoe UI"),
        };

        public static readonly SKPaint FadedTextPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(255, 255, 255).ToSKColor(),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left,
            TextSize = TextSize,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, TextFadeBlur)
        };

        public static readonly SKPaint VisibleTextPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(127, 127, 127).ToSKColor(),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left,
            TextSize = 12,
            Typeface = SKTypeface.FromFamilyName("Segoe UI")
        };

        public static readonly SKPaint GridPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgb(200, 200, 200).ToSKColor(),
            StrokeWidth = 2,
            IsAntialias = true
        };

        public static readonly SKPaint ShadowPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgba(0, 0, 0, 120).ToSKColor(),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10),
            IsAntialias = true
        };
    }
}