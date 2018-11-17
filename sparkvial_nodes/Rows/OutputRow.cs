using SkiaSharp;
using sparkvial.rows;
using sparkvial_app;

namespace sparkvial.nodes {
    public class OutputRow : ConnectorRow {
        public static bool Fade = false;

        public override SKPoint ConnectorOffset(float width) => new SKPoint(width - 0.5f * GraphStyle.HandleWidth, 0.5f * GraphStyle.RowHeight);

        public OutputRow(string name, string type, BaseNode parentNode) {
            this.name = name;
            this.type = type;
            this.parentNode = parentNode;
        }

        public override float Height => GraphStyle.RowHeight;
        public override bool TypeFade { get => Fade; set => Fade = value; }
        public override bool IsInConnector(float x, float y, float width) => x > width - GraphStyle.HandleWidth;

        public override void Render(float x, float y, float width, bool curveTop, bool curveBottom, SKCanvas canvas) {
            PaintUtils.DrawRoundRect(
                canvas,
                x,
                y,
                width - GraphStyle.HandleWidth,
                GraphStyle.RowHeight,
                curveTop ? GraphStyle.CornerRadius : 0,
                0,
                0,
                curveBottom ? GraphStyle.CornerRadius : 0,
                Fade ? GraphStyle.FadedOutputPaint : GraphStyle.OutputPaint
            );
            PaintUtils.DrawRoundRect(
                canvas,
                x + width - GraphStyle.HandleWidth,
                y,
                GraphStyle.HandleWidth,
                GraphStyle.RowHeight,
                0,
                curveTop ? GraphStyle.CornerRadius : 0,
                curveBottom ? GraphStyle.CornerRadius : 0,
                0,
                Fade ? GraphStyle.FadedOutputHandlePaint : GraphStyle.OutputHandlePaint
            );
            var txtWidth = GraphStyle.TextPaint.MeasureText(name);
            canvas.DrawText(
                name,
                x + width - GraphStyle.HandleWidth - GraphStyle.HandleHorizontalPadding - txtWidth,
                y + GraphStyle.TextSize / 2.823f + GraphStyle.RowHeight / 2,
                Fade ? GraphStyle.FadedTextPaint : GraphStyle.TextPaint
            );
            if (!curveTop) {
                canvas.DrawRect(x, y, width, 1, Fade ? GraphStyle.FadedOutputSepPaint : GraphStyle.OutputSepPaint);
            }
        }
    }
}
