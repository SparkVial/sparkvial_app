using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkiaSharp;

namespace sparkvial.nodes {
    public static class ExtensionMethods {
        public static bool Exists<T>(this ObservableCollection<T> collection, Predicate<T> match) {
            if (match == null) {
                throw new ArgumentNullException("match");
            }

            foreach (var elem in collection) {
                if (match(elem))
                    return true;
            }
            return false;
        }

        public static int RemoveAll<T>(this ObservableCollection<T> collection, Predicate<T> match) {
            if (match == null) {
                throw new ArgumentNullException("match");
            }

            var toRemove = collection.Where(entity => match(entity)).ToList();
            toRemove.ForEach(entity => collection.Remove(entity));
            return toRemove.Count;
        }
    }

    public class Graph {
        public List<object> nodes = new List<object>();
        public ObservableCollection<GraphConnection> connections = new ObservableCollection<GraphConnection>();

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
