using System;
using System.Collections.Generic;
using SkiaSharp;
using sparkvial.nodes;

namespace sparkvial.rows {
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
}
