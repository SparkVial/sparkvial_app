using System.Collections.Generic;
using sparkvial_app.rows;

namespace sparkvial_app.nodes {
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
