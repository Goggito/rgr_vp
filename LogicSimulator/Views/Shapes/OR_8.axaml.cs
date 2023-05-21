using Avalonia.Controls;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class OR_8: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 10;

        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;
        protected override int[][] Sides => new int[][] {
            System.Array.Empty<int>(),
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new int[] { 1 },
            System.Array.Empty<int>()
        };

        protected override void Init() => InitializeComponent();

        /*
         * Мозги
         */

        public void Brain(ref bool[] ins, ref bool[] outs) => outs[0] = ins[0] || ins[1] || ins[2] || ins[3] || ins[4] || ins[5] || ins[6] || ins[7];
    }
}
