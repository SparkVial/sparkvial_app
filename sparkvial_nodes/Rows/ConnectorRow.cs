using sparkvial.nodes;
using sparkvial.tapes;

namespace sparkvial.rows {
    public abstract class ConnectorRow : BaseRow {
        public string name;
        public string type;
        public override float MinWidth => (2 * GraphStyle.HandleHorizontalPadding) + GraphStyle.HandleWidth + GraphStyle.TextPaint.MeasureText(name);
        public virtual bool IsInConnector(float x, float y, float width) => false;
        public virtual bool TypeFade { get; set; }
        public IdentityTape inputTape;
        public Tape outputTape;

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

                var newConnection = new GraphConnection();
                if (this is OutputRow) {
                    newConnection.source = parentNode;
                    newConnection.sourceRow = parentNode.Rows.FindIndex(r => r == this);
                    newConnection.sink = Graph.currentlyInteractingNode;
                    newConnection.sinkRow = Graph.currentlyInteractingRow;
                } else if (this is InputRow) {
                    newConnection.source = Graph.currentlyInteractingNode;
                    newConnection.sourceRow = Graph.currentlyInteractingRow;
                    newConnection.sink = parentNode;
                    newConnection.sinkRow = parentNode.Rows.FindIndex(r => r == this);
                }

                if (!parentNode.parentGraph.connections.Exists(c => c.Equals(newConnection)) && ConnectionFilter(newConnection)) {
                    parentNode.parentGraph.connections.Add(newConnection);
                }
            }
        }
    }
}
