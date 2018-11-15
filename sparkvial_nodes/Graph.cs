using System.Collections.Generic;
using SkiaSharp;

namespace sparkvial_app.nodes {
    public class Graph {
        public List<object> nodes = new List<object>();
        public List<GraphConnection> connections = new List<GraphConnection>();

        public static BaseNode currentlyInteractingNode = null;
        public static int currentlyInteractingRow = -1;
        public static SKPoint panPos = new SKPoint();
    }
    
    public class GraphConnection {
        public object source;
        public int sourceRow;
        public object sink;
        public int sinkRow;

        public override bool Equals(object obj) {
            if (obj is GraphConnection c) {
                return source == c.source &&
                    sourceRow == c.sourceRow &&
                    sink == c.sink &&
                    sinkRow == c.sinkRow;
            }
            return false;
        }

        public override int GetHashCode() {
            var hashCode = -1143406828;
            hashCode = hashCode * -1521134295 + (source as BaseNode).GetHashCode();
            hashCode = hashCode * -1521134295 + sourceRow.GetHashCode();
            hashCode = hashCode * -1521134295 + (sink as BaseNode).GetHashCode();
            hashCode = hashCode * -1521134295 + sinkRow.GetHashCode();
            return hashCode;
        }
    }

    public enum GraphNodeHandleType {
        Input = 0,
        Output = 1
    }
}
