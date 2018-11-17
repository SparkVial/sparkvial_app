using SkiaSharp;
using sparkvial.nodes;
using sparkvial_app;

namespace sparkvial.rows {
    public class InputRow : ConnectorRow {
        public static bool Fade = false;

        public override SKPoint ConnectorOffset(float width) => new SKPoint(0.5f * GraphStyle.HandleWidth, 0.5f * GraphStyle.RowHeight);

        public InputRow(string name, string type, BaseNode parentNode) {
            this.name = name;
            this.type = type;
            this.parentNode = parentNode;
        }

        public override float Height => GraphStyle.RowHeight;
        public override bool TypeFade { get => Fade; set => Fade = value; }
        public override bool IsInConnector(float x, float y, float width) => x < GraphStyle.HandleWidth;

        public override bool ConnectionFilter(GraphConnection conn) {
            var tests = base.ConnectionFilter(conn);
            return tests;
        }

        public override void Render(float x, float y, float width, bool curveTop, bool curveBottom, SKCanvas canvas) {
            PaintUtils.DrawRoundRect(
                canvas,
                x,
                y,
                GraphStyle.HandleWidth,
                GraphStyle.RowHeight,
                curveTop ? GraphStyle.CornerRadius : 0,
                0,
                0,
                curveBottom ? GraphStyle.CornerRadius : 0,
                Fade ? GraphStyle.FadedInputHandlePaint : GraphStyle.InputHandlePaint
            );
            PaintUtils.DrawRoundRect(
                canvas,
                x + GraphStyle.HandleWidth,
                y,
                width - GraphStyle.HandleWidth,
                GraphStyle.RowHeight,
                0,
                curveTop ? GraphStyle.CornerRadius : 0,
                curveBottom ? GraphStyle.CornerRadius : 0,
                0,
                Fade ? GraphStyle.FadedInputPaint : GraphStyle.InputPaint
            );
            canvas.DrawText(
                name,
                x + GraphStyle.HandleWidth + GraphStyle.HandleHorizontalPadding,
                y + GraphStyle.TextSize / 2.823f + GraphStyle.RowHeight / 2,
                Fade ? GraphStyle.FadedTextPaint : GraphStyle.TextPaint
            );
            if (!curveTop) {
                canvas.DrawRect(x, y, width, 1, Fade ? GraphStyle.FadedInputSepPaint : GraphStyle.InputSepPaint);
            }
        }
    }
}
