using System;
using System.Linq;
using SkiaScene.TouchManipulation;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using sparkvial_app.nodes;
using TouchTracking;
using Xamarin.Forms;

namespace sparkvial_app {
    public class GraphEditor : SKCanvasView {
        float canvasScale = 1;
        //BaseNode currentInteractingNode = null;
        //int currentInteractingHandle = -1;
        //SKPoint currentInteractPos = new SKPoint();
        private string log = "";
        TouchGestureRecognizer gestures = new TouchGestureRecognizer();

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
            gestures.OnTap += OnTap;
            gestures.OnPan += OnPan;
            gestures.OnPinch += OnPinch;
        }

        class TapInfo {
            public readonly SKPoint pos;
            public readonly BaseNode node;
            public readonly int handle;
            public readonly bool isHandleTip;

            public TapInfo(SKPoint pos, BaseNode node, int handle, bool isHandleTip) {
                this.pos = pos;
                this.node = node;
                this.handle = handle;
                this.isHandleTip = isHandleTip;
            }
        }

        public void OnInteract(object sender, TouchActionEventArgs args) {
            var loc = new SKPoint(args.Location.X, args.Location.Y);
            Graph.panPos = new SKPoint(loc.X / canvasScale, loc.Y / canvasScale);
            if (args.Type == TouchActionType.Released) {
                OnTapUp(sender, args);
            } else if (args.Type == TouchActionType.Pressed) {
                OnTapDown(sender, args);
            }
            try {
                gestures.ProcessTouchEvent(args.Id, args.Type, loc);
            } catch (Exception) { }
            InvalidateSurface();
        }

        //TapInfo GetTapInfo(SKPoint pos) {
        //    var vpPos = new SKPoint(pos.X / canvasScale + OffsetX, pos.Y / canvasScale + OffsetY);
        //    for (int n = Graph.nodes.Count; n != 0; n--) {
        //        var node = Graph.nodes[n-1] as BaseNode;
        //        var w = node.Width;
        //        var h = node.Height;
        //        if (vpPos.X >= node.x &&
        //            vpPos.X <= node.x + w &&
        //            vpPos.Y >= node.y &&
        //            vpPos.Y <= node.y + h) {

        //            // Hit node
        //            for (int i = 1; i <= node.Rows.Count; i++) {
        //                if (vpPos.Y <= node.y + (i * GraphStyle.RowHeight)) {

        //                    // Got the hit handle
        //                    if (node.Rows[i - 1] is InputRow &&
        //                        vpPos.X <= node.x + GraphStyle.HandleWidth) {

        //                        // Hit input handle tip
        //                        return new TapInfo(vpPos, node, i - 1, true);
        //                    } else if (node.Rows[i - 1] is OutputRow &&
        //                        vpPos.X >= node.x + w - GraphStyle.HandleWidth) {

        //                        // Hit output handle tip
        //                        return new TapInfo(vpPos, node, i - 1, true);
        //                    } else {
        //                        return new TapInfo(vpPos, node, i - 1, false);
        //                    }
        //                }
        //            }
        //            return new TapInfo(vpPos, node, 0, false);
        //        }
        //    }
        //    return new TapInfo(vpPos, null, 0, false);
        //}

        void OnTapDown(object sender, TouchActionEventArgs args) {
            //var p = new SKPoint(args.Location.X, args.Location.Y);
            //var info = GetTapInfo(p);
            //if (info.node != null) {
            //    currentInteractingNode = info.node;
            //    if (info.isHandleTip) {
            //        currentInteractingHandle = info.handle;
            //        if (info.node.Rows[info.handle] == GraphNodeHandleType.Input) {
            //            fadeInputs = true;
            //        } else {
            //            fadeOutputs = true;
            //        }
            //        currentInteractPos = p;
            //    }
            //}

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
                    break;
                }
            }
        }

        void OnTapUp(object sender, TouchActionEventArgs args) {
            //currentInteractingNode = null;
            //currentInteractingHandle = -1;
            //fadeOutputs = false;
            //fadeInputs = false;
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
                    break;
                }
            }
            Graph.currentlyInteractingNode = null;
            Graph.currentlyInteractingRow = -1;
            log = $"{Graph.connections.Count}";
        }

        void OnTap(object sender, TapEventArgs args) {
            
        }

        void OnPan(object sender, PanEventArgs args) {
            var dx = args.PreviousPoint.X - args.NewPoint.X;
            var dy = args.PreviousPoint.Y - args.NewPoint.Y;

            if (args.TouchActionType == TouchActionType.Moved) {
                if (Graph.currentlyInteractingNode == null) {
                    OffsetX += dx / canvasScale;
                    OffsetY += dy / canvasScale;
                } else {
                    var oldPos = args.PreviousPoint;
                    oldPos = new SKPoint(oldPos.X / canvasScale, oldPos.Y / canvasScale);
                    var newPos = args.NewPoint;
                    newPos = new SKPoint(newPos.X / canvasScale, newPos.Y / canvasScale);
                    Graph.currentlyInteractingNode.OnPan(oldPos, newPos);
                }
            }
        }

        void OnPinch(object sender, PinchEventArgs args) {
            var oldDistance = Math.Sqrt(Math.Pow(args.PreviousPoint.X - args.PivotPoint.X, 2) + Math.Pow(args.PreviousPoint.Y - args.PivotPoint.Y, 2));
            var newDistance = Math.Sqrt(Math.Pow(args.NewPoint.X - args.PivotPoint.X, 2) + Math.Pow(args.NewPoint.Y - args.PivotPoint.Y, 2));
            var oldMiddlePoint = new SKPoint((args.PivotPoint.X + args.PreviousPoint.X) / 2, (args.PivotPoint.Y + args.PreviousPoint.Y) / 2);
            var newMiddlePoint = new SKPoint((args.PivotPoint.X + args.NewPoint.X) / 2, (args.PivotPoint.Y + args.NewPoint.Y) / 2);
            var middlePointDelta1 = new SKPoint(
                newMiddlePoint.X * (float)(newDistance / oldDistance),
                newMiddlePoint.Y * (float)(newDistance / oldDistance)
            ) - newMiddlePoint;
            var middlePointDelta2 = oldMiddlePoint - newMiddlePoint;
            canvasScale *= (float)(newDistance / oldDistance);
            if (canvasScale < 0.2f) {
                canvasScale = 0.2f;
            } else if (canvasScale > 2.5f) {
                canvasScale = 2.5f;
            } else {
                OffsetX += middlePointDelta1.X / canvasScale;
                OffsetY += middlePointDelta1.Y / canvasScale;
            }
            OffsetX += middlePointDelta2.X / canvasScale;
            OffsetY += middlePointDelta2.Y / canvasScale;
            InvalidateSurface();
        }

        #endregion

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

                //var x = currentInteractingNode.x + 0.5 * GraphStyle.HandleWidth;
                //var y = currentInteractingNode.y + (currentInteractingHandle + 0.5) * GraphStyle.RowHeight;
                //if ((currentInteractingNode.type as GraphNodeType).handles[currentInteractingHandle].type == GraphNodeHandleType.Output) {
                //    x += CalculateNodeWidth(currentInteractingNode) - GraphStyle.HandleWidth;
                //}
                //var p = new SKPoint((float)x - OffsetX, (float)y - OffsetY);
                //var interactPosScaled = new SKPoint(currentInteractPos.X / canvasScale, currentInteractPos.Y / canvasScale);
                //canvas.DrawCircle(interactPosScaled, GraphStyle.RowHeight * 0.4f * 0.5f, GraphStyle.ConnectionPaint);

            }

            // Draw debug text
            canvas.DrawText(log, 30, 30, GraphStyle.VisibleTextPaint);
        }


        void PaintNode(BaseNode node, float x, float y, SKCanvas canvas) {
            
        }
    }
}