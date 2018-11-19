using System.Collections.Generic;
using libsv;
using sparkvial.rows;
using sparkvial.tapes;

namespace sparkvial.nodes {
    public class SensorNode : BaseNode {
        public string unit;
        public bool Updated { get; private set; }
        public SourceTape tape;

        public SensorNode(string name, string type, string unit, SourceTape tape, Graph parentGraph) {
            var outputRow = new OutputRow(name, type, this);
            Rows = new List<BaseRow> { outputRow };
            this.unit = unit;
            this.parentGraph = parentGraph;
            this.tape = tape;
            outputRow.outputTape = tape;
        }
    }
}
