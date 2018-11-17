using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using sparkvial.nodes;
using sparkvial.rows;
using Xamarin.Forms;

namespace sparkvial_app {
    public class GraphEditor : SKCanvasView {
        float canvasScale = 1;
        //BaseNode currentInteractingNode = null;
        //int currentInteractingHandle = -1;
        //SKPoint currentInteractPos = new SKPoint();
        private string log = "Alpha version";
        private SKPoint lastPanPos = new SKPoint(0, 0);
        private SKPoint lastPinchPos = new SKPoint(0, 0);

        #region Properties
        public static readonly BindableProperty GraphProperty = BindableProperty.Create(
            propertyName: "Graph",
            returnType: typeof(Graph),
            declaringType: typeof(GraphEditor),
            defaultValue: new Graph()
        );

        public Graph Graph {
            get { return (Graph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        public static readonly BindableProperty OffsetXProperty = BindableProperty.Create(
            propertyName: "OffsetX",
            returnType: typeof(float),
            declaringType: typeof(GraphEditor),
            defaultValue: 0.0f
        );

        public float OffsetX {
            get { return (float)GetValue(OffsetXProperty); }
            set { SetValue(OffsetXProperty, value); InvalidateSurface(); }
        }

        public static readonly BindableProperty OffsetYProperty = BindableProperty.Create(
            propertyName: "OffsetY",
            returnType: typeof(float),
            declaringType: typeof(GraphEditor),
            defaultValue: 0.0f
        );

        public float OffsetY {
            get { return (float)GetValue(OffsetYProperty); }
            set { SetValue(OffsetYProperty, value); InvalidateSurface(); }
        }

        #endregion

        #region Interactions
        public GraphEditor() {
            Graph = new Graph();
            PaintSurface += Paint;
        }

        public void OnInteract(object sender, SKTouchEventArgs args) {
            var loc = new SKPoint(args.Location.X, args.Location.Y);
            Graph.panPos = new SKPoint(loc.X / canvasScale, loc.Y / canvasScale);
            if (args.ActionType == SKTouchAction.Released) {
                OnTapUp(sender, args);
            } else if (args.ActionType == SKTouchAction.Pressed) {
                OnTapDown(sender, args);
            }
            InvalidateSurface();
        }

        public void OnTapDown(object sender, SKTouchEventArgs args) {
            foreach (var _node in Graph.nodes) {
                var node = _node as BaseNode;
                var x = (args.Location.X / canvasScale) + OffsetX;
                var y = (args.Location.Y / canvasScale) + OffsetY;
                var nx = node.pos.X;
                var ny = node.pos.Y;
                var nw = node.Width;
                var nh = node.Height;
                if (x >= nx && x <= nx + nw && y >= ny && y <= ny + nh) {
                    node.OnTapDown(x - nx, y - ny);
                    InvalidateSurface();
                    break;
                }
            }
        }

        public void OnTapUp(object sender, SKTouchEventArgs args) {
            InputRow.Fade = false;
            OutputRow.Fade = false;
            BaseRow.CommonFade = false;

            foreach (var _node in Graph.nodes) {
                var node = _node as BaseNode;
                var x = (args.Location.X / canvasScale) + OffsetX;
                var y = (args.Location.Y / canvasScale) + OffsetY;
                var nx = node.pos.X;
                var ny = node.pos.Y;
                var nw = node.Width;
                var nh = node.Height;
                if (x >= nx && x <= nx + nw && y >= ny && y <= ny + nh) {
                    node.OnTapUp(x - nx, y - ny);
                    InvalidateSurface();
                    break;
                }
            }
            Graph.currentlyInteractingNode = null;
            Graph.currentlyInteractingRow = -1;
            //log = $"{Graph.connections.Count}";
            
            InvalidateSurface();
        }

        //public void OnTap(object sender, TapEventArgs args) { 
        //}

        public void OnPan(object sender, PanUpdatedEventArgs e) {
            if (e.StatusType == GestureStatus.Started) {

            } else if (e.StatusType == GestureStatus.Running) {
                if (Graph.currentlyInteractingNode == null) {
                    var dx = lastPanPos.X - (float)e.TotalX;
                    var dy = lastPanPos.Y - (float)e.TotalY;
                    OffsetX += dx / canvasScale;
                    OffsetY += dy / canvasScale;
                    //OffsetX = newOffset.X;
                    //OffsetY = newOffset.Y;
                } else {
                    var oldPos = lastPanPos;
                    oldPos = new SKPoint(oldPos.X / canvasScale, oldPos.Y / canvasScale);
                    var newPos = new SKPoint((float)e.TotalX, (float)e.TotalY);
                    newPos = new SKPoint(newPos.X / canvasScale, newPos.Y / canvasScale);
                    Graph.currentlyInteractingNode.OnPan(oldPos, newPos);
                }
            } else {
                lastPanPos = new SKPoint(0, 0);
            }
            lastPanPos = new SKPoint((float)e.TotalX, (float)e.TotalY);
        }

        public void OnPinch(object sender, PinchGestureUpdatedEventArgs e) {
            var midX = (float)e.ScaleOrigin.X * (float)Width;
            var midY = (float)e.ScaleOrigin.Y * (float)Height;
            if (e.Status == GestureStatus.Started) {
            } else if (e.Status == GestureStatus.Running) {
                canvasScale *= (float)e.Scale;
                if (canvasScale < 0.2f) {
                    canvasScale = 0.2f;
                } else if (canvasScale > 2.5f) {
                    canvasScale = 2.5f;
                } else {
                    var dx = lastPinchPos.X - midX;
                    var dy = lastPinchPos.Y - midY;
                    dx += midX * ((float)e.Scale - 1);
                    dy += midY * ((float)e.Scale - 1);

                    //log = $"{dx}, {dy}";
                    OffsetX += dx / canvasScale;
                    OffsetY += dy / canvasScale;
                }
            } else {
                return;
            }
            InvalidateSurface();
            lastPinchPos = new SKPoint(midX, midY);
        }

        #endregion

        void OnNewConnection(GraphConnection conn) {

        }

        void Paint(object sender, SKPaintSurfaceEventArgs args) {
            var canvas = args.Surface.Canvas;
            canvas.Clear();
            canvas.Scale(canvasScale);

            // Draw grid
            var actualGridlineDistance = (float) (GraphStyle.GridlineDistance / Math.Pow(4, Math.Floor(Math.Log(canvasScale) / Math.Log(4))));
            for (float i = -OffsetX % actualGridlineDistance; i < args.Info.Width / canvasScale; i += actualGridlineDistance) {
                canvas.DrawLine(new SKPoint(i, 0), new SKPoint(i, args.Info.Height / canvasScale), GraphStyle.GridPaint);
            }
            for (float i = -OffsetY % actualGridlineDistance; i < args.Info.Height / canvasScale; i += actualGridlineDistance) {
                canvas.DrawLine(new SKPoint(0, i), new SKPoint(args.Info.Width / canvasScale, i), GraphStyle.GridPaint);
            }

            // Draw nodes
            foreach (var _node in Graph.nodes) {
                var node = _node as BaseNode;
                node.Render(node.pos.X - OffsetX, node.pos.Y - OffsetY, canvas);
            }

            // Draw connections
            foreach (var conn in Graph.connections) {
                SKPoint DrawNodeConnector(BaseNode node, int rowID) {
                    var row = node.Rows[rowID];
                    var connOfset = row.ConnectorOffset(node.Width);
                    var rowHeight = 0.0f;
                    for (int i = 0; i < rowID; i++) {
                        rowHeight += node.Rows[i].Height;
                    }
                    var x = node.pos.X + connOfset.X;
                    var y = node.pos.Y + rowHeight + connOfset.Y;
                    var p = new SKPoint(x - OffsetX, y - OffsetY);
                    canvas.DrawCircle(p, GraphStyle.RowHeight * 0.4f * 0.5f, GraphStyle.ConnectionPaint);
                    return p;
                }

                var p1 = DrawNodeConnector(conn.source as BaseNode, conn.sourceRow);
                var p2 = DrawNodeConnector(conn.sink as BaseNode, conn.sinkRow);

                canvas.DrawLine(p1, p2, GraphStyle.ConnectionPaint);
            }

            // Draw new connection
            if (Graph.currentlyInteractingNode != null && Graph.currentlyInteractingRow != -1) {
                var rowHeight = 0.0f;
                for (int i = 0; i < Graph.currentlyInteractingRow; i++) {
                    rowHeight += Graph.currentlyInteractingNode.Rows[i].Height;
                }
                var p1 = Graph.currentlyInteractingNode.Rows[Graph.currentlyInteractingRow].ConnectorOffset(Graph.currentlyInteractingNode.Width)
                    + Graph.currentlyInteractingNode.pos
                    + new SKPoint(-OffsetX, rowHeight - OffsetY);
                var p2 = Graph.panPos;
                canvas.DrawCircle(p1, GraphStyle.RowHeight * 0.4f * 0.5f, GraphStyle.ConnectionPaint);
                canvas.DrawCircle(p2, GraphStyle.RowHeight * 0.4f * 0.5f, GraphStyle.ConnectionPaint);
                canvas.DrawLine(p1, p2, GraphStyle.ConnectionPaint);
            }

            // Draw debug text
            canvas.DrawText(log, 30, 30, GraphStyle.VisibleTextPaint);
        }


        void PaintNode(BaseNode node, float x, float y, SKCanvas canvas) {
            
        }
    }
}