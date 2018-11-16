using System.Collections.Generic;
using sparkvial_app.rows;

namespace sparkvial_app.nodes {
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
}
