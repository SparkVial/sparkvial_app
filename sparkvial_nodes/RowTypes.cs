using System;
using System.Collections.Generic;
using SkiaSharp;

namespace sparkvial_app.nodes {
    public static class RowTypes {
        public readonly static List<Type> list = new List<Type>() { };
    }

    public abstract class BaseRow {
        public virtual float MinWidth => 0;
        public virtual float Height => 0;
        public BaseNode parentNode;
        public static bool CommonFade = false;

        public virtual SKPoint ConnectorOffset(float width) => new SKPoint(0, 0);

        public virtual void Render(float x, float y, float width, bool curveTop, bool curveBottom, SKCanvas canvas) {
            throw new NotImplementedException("Please define a rendering function for your NodeRow");
        }

        public virtual void OnTapDown(float x, float y) {
            Graph.currentlyInteractingNode = parentNode;
        }

        public virtual void OnTapUp(float x, float y) { }
    }


    public abstract class ConnectorRow : BaseRow {
        public string name;
        public string type;
        public override float MinWidth => (2 * GraphStyle.HandleHorizontalPadding) + GraphStyle.HandleWidth + GraphStyle.TextPaint.MeasureText(name);
        public virtual bool IsInConnector(float x, float y, float width) => false;
        public virtual bool TypeFade { get; set; }

        public virtual bool ConnectionFilter(GraphConnection conn) {
            // If tapped a handle, remove all it's connections
            if (conn.source == conn.sink && conn.sourceRow == conn.sinkRow) {
                (conn.source as BaseNode).parentGraph.connections.RemoveAll(c => c.source == conn.source && c.sourceRow == conn.sourceRow);
                (conn.source as BaseNode).parentGraph.connections.RemoveAll(c => c.sink == conn.sink && c.sinkRow == conn.sinkRow);
                return false;
            }

            // Make sure inputs can't be connected to inputs and vice versa
            if ((conn.source as BaseNode).Rows[conn.sourceRow].GetType()
                == (conn.sink as BaseNode).Rows[conn.sinkRow].GetType())
                return false;

            // Make sure we aren't creating a simple loop

            // TODO: Make sure we aren't creating a loop
            return conn.source != conn.sink;
        }

        public override void OnTapDown(float x, float y) {
            Graph.currentlyInteractingNode = parentNode;
            if (IsInConnector(x, y, parentNode.Width)) {
                Graph.currentlyInteractingRow = parentNode.Rows.FindIndex(r => r == this);
                TypeFade = true;
                CommonFade = true;
            }
        }

        public override void OnTapUp(float x, float y) {
            if (IsInConnector(x, y, parentNode.Width) && Graph.currentlyInteractingNode != null && Graph.currentlyInteractingRow != -1) {
                var newConnection = new GraphConnection() {
                    source = parentNode,
                    sourceRow = parentNode.Rows.FindIndex(r => r == this),
                    sink = Graph.currentlyInteractingNode,
                    sinkRow = Graph.currentlyInteractingRow
                };

                if (!parentNode.parentGraph.connections.Exists(c => c.Equals(newConnection)) && ConnectionFilter(newConnection)) {
                    parentNode.parentGraph.connections.Add(newConnection);
                }
            }
        }
    }

    public class InputRow : ConnectorRow {
        public static new bool Fade = false;

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

    public class OutputRow : ConnectorRow {
        public static new bool Fade = false;

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

    public class ChartRow : BaseRow {
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
            //var minimum = (parentNode as )
        }
    }
}
