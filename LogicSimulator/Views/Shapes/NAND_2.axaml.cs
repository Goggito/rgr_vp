using Avalonia;
using Avalonia.Controls;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class NAND_2: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 8;

        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;
        protected override int[][] Sides => new int[][] {
            System.Array.Empty<int>(),
            new int[] { 0, 0 },
            new int[] { 1 },
            System.Array.Empty<int>()
        };

        protected override void Init() => InitializeComponent();

        /*
         * Обработка размеров внутренностей
         */

        public double InvertorSize => EllipseSize / 2;
        public double InvertorStrokeSize => EllipseStrokeSize / 2;
        public Thickness InvertorMargin => new(width + BaseFraction * 2 - InvertorSize / 2, 0, 0, 0);

        /*
         * Мозги
         */

        public void Brain(ref bool[] ins, ref bool[] outs) => outs[0] = !(ins[0] && ins[1]);
    }
}
