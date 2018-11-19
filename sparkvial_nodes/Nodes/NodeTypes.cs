using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using sparkvial.rows;

namespace sparkvial.nodes {
    public static class NodeTypes {
        public static readonly List<Type> list = new List<Type>() { };
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
}
