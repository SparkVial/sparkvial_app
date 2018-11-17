using System;
using System.Collections.Generic;
using sparkvial.tapes;
using sparkvial.rows;

namespace sparkvial.nodes {
    public class ChartNode : BaseNode {
        public string unit = "";
        public CacheTape tape;

        public ChartNode(Graph parentGraph) {
            var inputRow = new InputRow("Chart", "Number", this);
            var chartRow = new ChartRow();
            Rows = new List<BaseRow> {
                inputRow,
                chartRow
            };
            this.parentGraph = parentGraph;
            tape = chartRow.tape;
            inputRow.inputTape = tape;
        }
    }
}
