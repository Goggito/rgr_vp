using Avalonia.Controls;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class SuM: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 4;

        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;
        protected override int[][] Sides => new int[][] {
            System.Array.Empty<int>(),
            new int[] { 0, 0, 0 },
            new int[] { 1, 1 },
            System.Array.Empty<int>()
        };

        protected override void Init() => InitializeComponent();

        /*
         * Мозги
         */

        public void Brain(ref bool[] ins, ref bool[] outs) {
            int count = (ins[0] ? 1 : 0) + (ins[1] ? 1 : 0) + (ins[2] ? 1 : 0);
            outs[0] = (count & 1) != 0;
            outs[1] = (count & 2) != 0;
        }
    }
}
