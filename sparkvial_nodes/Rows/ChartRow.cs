using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using sparkvial_app.nodes;

namespace sparkvial_app.rows {
    public class ChartRow : BaseRow {
        public Queue<Tuple<float, float>> data;

        public override float Height => GraphStyle.ChartHeight;
        public override float MinWidth => GraphStyle.ChartMinWidth;

        public ChartRow(Queue<Tuple<float, float>> data) {
            this.data = data;
        }

        public override void OnTapDown(float x, float y) {
            Graph.currentlyInteractingNode = parentNode;
        }

        public override void Render(float x, float y, float width, bool curveTop, bool curveBottom, SKCanvas canvas) {
            PaintUtils.DrawRoundRect(
                canvas,
                x,
                y,
                width,
                GraphStyle.ChartHeight,
                curveTop ? GraphStyle.CornerRadius : 0,
                curveTop ? GraphStyle.CornerRadius : 0,
                curveBottom ? GraphStyle.CornerRadius : 0,
                curveBottom ? GraphStyle.CornerRadius : 0,
                CommonFade ? GraphStyle.FadedChartPaint : GraphStyle.ChartPaint // FIXME: Faded
            );
            var justTimes = data.Select(p => p.Item1);
            var justData = data.Select(p => p.Item2);
            var minimumTs = justTimes.Min() - 0.2f;
            var maximumTs = justTimes.Max() + 0.2f;
            var dataArr = data.ToArray();

            if (maximumTs - minimumTs < 2) {
                maximumTs += 2;
            }

            var minimum = justData.Min();
            var maximum = justData.Max();
            var minimumIdx = Array.FindIndex(dataArr, d => d.Item2 == minimum);
            var maximumIdx = Array.FindIndex(dataArr, d => d.Item2 == maximum);
            minimum = Math.Min(-0.2f, minimum - 0.5f);
            maximum = Math.Max(maximum + 0.5f, 0.2f);

            // Draw the zero line
            if (minimum <= 0 && maximum > 0) {
                var zeroHeight = Math.Abs(minimum) / Math.Abs(maximum - minimum);
                canvas.DrawLine(
                    new SKPoint(x, y + GraphStyle.ChartHeight * (1 - zeroHeight)),
                    new SKPoint(x + width, y + GraphStyle.ChartHeight * (1 - zeroHeight)),
                    GraphStyle.ChartFaintLinePaint
                );
                canvas.DrawText("0", new SKPoint(x + 9, y + GraphStyle.ChartHeight * (1 - zeroHeight) - 7), GraphStyle.ChartFaintLinePaint);
            }

            // Draw data lines
            for (var i = 0; i < dataArr.Count() - 1; i++) {
                var x1Pos = (dataArr[i].Item1 - minimumTs) / Math.Abs(maximumTs - minimumTs);
                var y1Pos = (dataArr[i].Item2 - minimum) / Math.Abs(maximum - minimum);
                var x2Pos = (dataArr[i+1].Item1 - minimumTs) / Math.Abs(maximumTs - minimumTs);
                var y2Pos = (dataArr[i+1].Item2 - minimum) / Math.Abs(maximum - minimum);
                canvas.DrawLine(
                    new SKPoint(x + width * x1Pos, y + GraphStyle.ChartHeight * (1 - y1Pos)),
                    new SKPoint(x + width * x2Pos, y + GraphStyle.ChartHeight * (1 - y2Pos)),
                    GraphStyle.ChartLinePaint // FIXME: Faded
                );
            }

            // Draw data points
            for (var i = 0; i < dataArr.Count(); i++) {
                var xPos = (dataArr[i].Item1 - minimumTs) / Math.Abs(maximumTs - minimumTs);
                var yPos = (dataArr[i].Item2 - minimum) / Math.Abs(maximum - minimum);
                canvas.DrawCircle(
                    new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos)),
                    2,
                    GraphStyle.ChartBoldLinePaint // FIXME: Faded
                );

                if (i == maximumIdx) {
                    canvas.DrawText($"{dataArr[maximumIdx].Item2}", new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos) - 5), GraphStyle.ChartCenteredShadowLinePaint);
                    canvas.DrawText($"{dataArr[maximumIdx].Item2}", new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos) - 5), GraphStyle.ChartCenteredBoldLinePaint);
                } else if (i == minimumIdx) {
                    canvas.DrawText($"{dataArr[minimumIdx].Item2}", new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos) + 16), GraphStyle.ChartCenteredShadowLinePaint);
                    canvas.DrawText($"{dataArr[minimumIdx].Item2}", new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos) + 16), GraphStyle.ChartCenteredBoldLinePaint);
                }
            }
        }
    }
}
