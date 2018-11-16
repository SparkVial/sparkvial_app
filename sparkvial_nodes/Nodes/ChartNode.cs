using System;
using System.Collections.Generic;
using sparkvial_app.rows;

namespace sparkvial_app.nodes {
    public class ChartNode : BaseNode {
        public string unit = "";
        public Queue<Tuple<float, float>> data;

        public ChartNode(Graph parentGraph, Queue<Tuple<float, float>> data) {
            Rows = new List<BaseRow> {
                new InputRow("Chart", "Number", this),
                new ChartRow(data)
            };
            this.parentGraph = parentGraph;
            this.data = data;
        }
    }
}
