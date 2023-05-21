using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.Collections.Generic;

namespace LogicSimulator.Models {
    public class JoinedItems {
        public static readonly Dictionary<Line, JoinedItems> arrow_to_join = new();

        public JoinedItems(Distantor a, Distantor b) {
            A = a; B = b; Update();
            a.parent.AddJoin(this);
            if (a.parent != b.parent) b.parent.AddJoin(this);
            arrow_to_join[line] = this;
        }
        public Distantor A { get; set; }
        public Distantor B { get; set; }
        public Line line = new() { Tag = "Join", ZIndex = 2, Stroke = Brushes.DarkGray, StrokeThickness = 3 };

        public void Update() {
            line.StartPoint = A.GetPos();
            line.EndPoint = B.GetPos();
        }
        public void Delete() {
            arrow_to_join.Remove(line);
            line.Remove();
            A.parent.RemoveJoin(this);
            B.parent.RemoveJoin(this);
        }
    }
}
