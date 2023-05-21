using Avalonia;
using LogicSimulator.Views.Shapes;

namespace LogicSimulator.Models {
    public class Distantor {
        public readonly int num;
        public IGate parent;
        public readonly string tag;

        public Distantor(IGate parent, int n, string tag) { 
            this.parent = parent;
            num = n; 
            this.tag = tag;
        }

        public Point GetPos() => parent.GetPinPos(num);
    }
}
