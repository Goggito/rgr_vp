using Avalonia.Controls;
using LogicSimulator.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LogicSimulator.Views.Shapes {
    public partial class FlipFlop: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 9;

        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;
        protected override int[][] Sides => new int[][] {
            System.Array.Empty<int>(),
            new int[] { 0, 0, 0 },
            new int[] { 1, 1, 1 },
            System.Array.Empty<int>()
        };

        protected override void Init() => InitializeComponent();

        

        private readonly bool[] prev = new bool[3];
        private readonly bool[] out_d = new bool[3];

        public void Brain(ref bool[] ins, ref bool[] outs) {
            for (int i = 0; i < 3; i++) {
                if (prev[i] && !ins[i]) out_d[i] = !out_d[i];
                outs[i] = out_d[i];
                prev[i] = ins[i];
            }
        }

        /*
         * Кастомный экспорт и импорт
         */

        public override Dictionary<string, object> ExtraExport() =>
            new() {
                ["state"] = string.Join('.', prev.Select(x => x ? '1' : '0')) + "." +
                string.Join('.', out_d.Select(x => x ? '1' : '0'))
            };

        public override void ExtraImport(string key, object extra) {
            if (key != "state") { Log.Write(key + "-запись элемента не поддерживается"); return; }
            if (extra is not string @state) { Log.Write("Неверный тип state-записи элемента: " + extra); return; }
            var arr = @state.Split('.');
            prev[0] = arr[0] == "1";
            prev[1] = arr[1] == "1";
            prev[2] = arr[2] == "1";
            out_d[0] = arr[3] == "1";
            out_d[1] = arr[4] == "1";
            out_d[2] = arr[5] == "1";
        }
    }
}
