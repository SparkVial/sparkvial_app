using System;
using System.Collections.Generic;
using System.Linq;
using libsv;
using MoreLinq;
using SkiaSharp;
using sparkvial.nodes;
using sparkvial.tapes;
using sparkvial_app;

namespace sparkvial.rows {
    public class ChartRow : BaseRow {
        public CacheTape tape = new CacheTape(null) {
            maxEntries = 100
        };

        public override float Height => GraphStyle.ChartHeight;
        public override float MinWidth => GraphStyle.ChartMinWidth;

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
            var tapeSnapshot = tape.ToArray();
            var filtered = tapeSnapshot.Where(smp => smp.values.Count == 1 && smp.values[0] is FloatField)
                                       .Select(smp => Tuple.Create(smp.timestamp, (smp.values[0] as FloatField).value));

            if (filtered.Count() != 0) {
                var minimumTs = filtered.Min(f => f.Item1) - 0.2f;
                var maximumTs = filtered.Max(f => f.Item1) + 0.2f;

                if (maximumTs - minimumTs < 2) {
                    maximumTs += 2;
                }

                var minimumFound = filtered.Select((f, i) => Tuple.Create(i, f)).MinBy(f => f.Item2.Item2).First();
                var minimum = minimumFound.Item2.Item2;
                var minimumIdx = minimumFound.Item1;

                var maximumFound = filtered.Select((f, i) => Tuple.Create(i, f)).MaxBy(f => f.Item2.Item2).First();
                var maximum = maximumFound.Item2.Item2;
                var maximumIdx = maximumFound.Item1;

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
                //var dataEnum = tapeSnapshot.GetEnumerator();
                //dataEnum.MoveNext();
                //var curr = dataEnum.Current;
                //dataEnum.MoveNext();
                //var next = dataEnum.Current;
                for (var i = 0; i < tapeSnapshot.Count() - 1; i++) {
                    var x1Pos = (tapeSnapshot[i].timestamp - minimumTs) / Math.Abs(maximumTs - minimumTs);
                    var y1Pos = ((tapeSnapshot[i].values[0] as FloatField).value - minimum) / Math.Abs(maximum - minimum);
                    var x2Pos = (tapeSnapshot[i+1].timestamp - minimumTs) / Math.Abs(maximumTs - minimumTs);
                    var y2Pos = ((tapeSnapshot[i+1].values[0] as FloatField).value - minimum) / Math.Abs(maximum - minimum);
                    canvas.DrawLine(
                        new SKPoint(x + width * x1Pos, y + GraphStyle.ChartHeight * (1 - y1Pos)),
                        new SKPoint(x + width * x2Pos, y + GraphStyle.ChartHeight * (1 - y2Pos)),
                        GraphStyle.ChartLinePaint // FIXME: Faded
                    );

                    //dataEnum.MoveNext();
                    //curr = next;
                    //next = dataEnum.Current;
                }

                // Draw data points
                {
                    var i = 0;
                    foreach (var smp in tape) {
                        var xPos = (smp.timestamp - minimumTs) / Math.Abs(maximumTs - minimumTs);
                        var yPos = ((smp.values[0] as FloatField).value - minimum) / Math.Abs(maximum - minimum);
                        canvas.DrawCircle(
                            new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos)),
                            2,
                            GraphStyle.ChartBoldLinePaint // FIXME: Faded
                        );

                        if (i == maximumIdx) {
                            canvas.DrawText($"{maximum}", new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos) - 5), GraphStyle.ChartCenteredShadowLinePaint);
                            canvas.DrawText($"{maximum}", new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos) - 5), GraphStyle.ChartCenteredBoldLinePaint);
                        } else if (i == minimumIdx) {
                            canvas.DrawText($"{minimum}", new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos) + 16), GraphStyle.ChartCenteredShadowLinePaint);
                            canvas.DrawText($"{minimum}", new SKPoint(x + width * xPos, y + GraphStyle.ChartHeight * (1 - yPos) + 16), GraphStyle.ChartCenteredBoldLinePaint);
                        }
                        i++;
                    }
                }
            } else {
                canvas.DrawText("No data", new SKPoint(x + width / 2, y + GraphStyle.ChartHeight / 2), GraphStyle.ChartCenteredBoldLinePaint);
            }
        }
    }
}
