using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using LogicSimulator.Models;
using LogicSimulator.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class Switch: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 5;

        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;
        protected override int[][] Sides => new int[][] {
            Array.Empty<int>(),
            Array.Empty<int>(),
            new int[] { 1 },
            Array.Empty<int>()
        };

        protected override void Init() => InitializeComponent();

        /*
         * Мозги
         */

        bool my_state = false;
        Point? press_pos;

        // Данная схема работает гораздо быстрее, чем событие Tapped ;'-} Из-за того, что не обрабатывается дополнительно DoubleTapped, что гасит второй Tapped + некоторые задержки
        private static Point GetPos(PointerEventArgs e) {
            if (e.Source is not Control src) return new();
            while ((string?) src.Tag != "scene" && src.Parent != null) src = (Control) src.Parent;
            return e.GetCurrentPoint(src).Position;
        }
        private void Press(object? sender, PointerPressedEventArgs e) {
            if (e.Source == border) press_pos = GetPos(e);
        }
        private void Release(object? sender, PointerReleasedEventArgs e) {
            if (e.Source != border) return;
            if (press_pos == null || GetPos(e).Hypot((Point) press_pos) > 5) return;
            press_pos = null;

            my_state = !my_state;
            border.Background = new SolidColorBrush(Color.Parse(my_state ? "#7d1414" : "#d32f2e"));
        }

        public void Brain(ref bool[] ins, ref bool[] outs) => outs[0] = my_state;

        /*
         * Кастомный экспорт и импорт
         */

        public override Dictionary<string, object> ExtraExport() => new() { ["state"] = my_state };

        public override void ExtraImport(string key, object extra) {
            if (key != "state") { Log.Write(key + "-запись элемента не поддерживается"); return; }
            if (extra is not bool @state) { Log.Write("Неверный тип state-записи элемента: " + extra); return; }
            my_state = @state;
            if (my_state) border.Background = new SolidColorBrush(Color.Parse("#7d1414"));
        }

        /*
         * Для тестирования
         */

        public void SetState(bool state) {
            my_state = state;
            border.Background = new SolidColorBrush(Color.Parse(state ? "#7d1414" : "#d32f2e"));
        }
    }
}
