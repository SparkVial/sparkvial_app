using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace sparkvial_app.nodes {
    public static class NodeTypes {
        public readonly static List<Type> list = new List<Type>() { };
    }

    public abstract class BaseNode {
        public virtual List<BaseRow> Rows { get; set; } = new List<BaseRow>();
        public string comment = "Default comment";
        public SKPoint pos = new SKPoint();
        public Graph parentGraph;

        public virtual float Width {
            get {
                return Rows.Select(h => h.MinWidth).Max();
            }
        }

        public virtual float Height => Rows.Select(r => r.Height).Sum();

        public void Render(float x, float y, SKCanvas canvas) {
            var width = Width;
            var currentInd = 0;

            // Draw shadow
            canvas.DrawRect(
                x + 5,
                y + 15,
                width - 10,
                Height - 10,
                GraphStyle.ShadowPaint
            );

            // Draw each row
            foreach (var row in Rows) {
                var curveTop = currentInd == 0;
                var curveBottom = currentInd == Rows.Count - 1;

                row.Render(x, y, width, curveTop, curveBottom, canvas);
                y += row.Height;
                currentInd++;
            }
        }

        public virtual void OnTapDown(float x, float y) {
            var h = 0.0f;
            for (int i = 0; i < Rows.Count; i++) {
                var prevH = h;
                h += Rows[i].Height;
                if (y <= h) {
                    Rows[i].OnTapDown(x, y - prevH);
                    break;
                }
            }
        }

        public virtual void OnTapUp(float x, float y) {
            var h = 0.0f;
            for (int i = 0; i < Rows.Count; i++) {
                var prevH = h;
                h += Rows[i].Height;
                if (y <= h) {
                    Rows[i].OnTapUp(x, y - prevH);
                    break;
                }
            }
        }

        public virtual void OnPan(SKPoint oldPos, SKPoint newPos) {
            var dx = oldPos.X - newPos.X;
            var dy = oldPos.Y - newPos.Y;
            if (Graph.currentlyInteractingRow == -1) {
                Graph.currentlyInteractingNode.pos.X -= dx;
                Graph.currentlyInteractingNode.pos.Y -= dy;
            }
        }
    }


    public class ChartNode : BaseNode {
        public string unit = "";
        public Queue<float> data;

        public ChartNode(Graph parentGraph) {
            Rows = new List<BaseRow> {
                new InputRow("Chart", "Number", this),
                new ChartRow()
            };
            this.parentGraph = parentGraph;
        }
    }

    public class SensorNode : BaseNode {
        public string unit;

        public SensorNode(string name, string type, string unit, Graph parentGraph) {
            Rows = new List<BaseRow> {
                new OutputRow(name, type, this),
            };
            this.unit = unit;
            this.parentGraph = parentGraph;
        }
    }

    public class AddNode : BaseNode {
        public AddNode(Graph parentGraph) {
            Rows = new List<BaseRow> {
                new InputRow("A", "Number", this),
                new InputRow("B", "Number", this),
                new OutputRow("A + B", "Number", this)
            };
            this.parentGraph = parentGraph;
        }
    };
}
