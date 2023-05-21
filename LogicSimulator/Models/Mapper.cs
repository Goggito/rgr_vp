using Avalonia.Controls;
using Avalonia;
using LogicSimulator.ViewModels;
using LogicSimulator.Views.Shapes;
using System;
using System.Collections.Generic;
using DynamicData;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.LogicalTree;
using System.Linq;
using Button = LogicSimulator.Views.Shapes.Button;
using Avalonia.Input;

namespace LogicSimulator.Models {
    public class Mapper {
        readonly Line marker = new() { Tag = "Marker", ZIndex = 2, IsVisible = false, Stroke = Brushes.YellowGreen, StrokeThickness = 3 };
        readonly Rectangle marker2 = new() { Tag = "Marker", Classes = new("anim"), ZIndex = 2, IsVisible = false, Stroke = Brushes.MediumAquamarine, StrokeThickness = 3 };
        
        public Line Marker { get => marker; }
        public Rectangle Marker2 { get => marker2; }

        public readonly Simulator sim = new(); 

        public Canvas canv = new();

        /*
         * Маркер
         */

        private IGate? marked_item;
        private JoinedItems? marked_line;

        private void UpdateMarker() {
            marker2.IsVisible = marked_item != null || marked_line != null;

            if (marked_item != null) {
                var bound = marked_item.GetBounds();
                marker2.Margin = new(bound.X, bound.Y);
                marker2.Width = bound.Width;
                marker2.Height = bound.Height;
                marked_line = null;
            }

            if (marked_line != null) {
                var line = marked_line.line;
                var A = line.StartPoint;
                var B = line.EndPoint;
                marker2.Margin = new(Math.Min(A.X, B.X), Math.Min(A.Y, B.Y));
                marker2.Width = Math.Abs(A.X - B.X);
                marker2.Height = Math.Abs(A.Y - B.Y);
            }
        }

        /*
         * Выборка элементов
         */

        private int selected_item = 0;
        public int SelectedItem { get => selected_item; set => selected_item = value; }

        private static IGate CreateItem(int n) {
            return n switch {
                0 => new AND_2(),
                1 => new OR_2(),
                2 => new NOT(),
                3 => new XOR_2(),
                4 => new SuM(),
                5 => new Switch(),
                6 => new Button(),
                7 => new LightBulb(),
                8 => new NAND_2(),
                9 => new FlipFlop(),
                10 => new OR_8(),
                11 => new AND_8(),
                _ => new AND_2(),
            };
        }

        public IGate[] item_types = Enumerable.Range(0, 12).Select(CreateItem).ToArray();

        public IGate GenSelectedItem() => CreateItem(selected_item);

        /*
         * Хранилище
         */

        readonly List<IGate> items = new();
        // readonly MatrixTransform general_transform = new() { Matrix = new(1.0, 0.0, 0.0, 1.0, 0, 0) };
        // Canvas? itemer;
        private void AddToMap(IControl item) {
            /*if (itemer == null) { Снова мимо :///
                itemer = new Canvas();
                var layout = new LayoutTransformControl() {
                    LayoutTransform = general_transform,
                    Child = itemer,
                };
                canv.Children.Add(layout);
            }
            itemer.Children.Add(item);*/
            canv.Children.Add(item);
        }

        public void AddItem(IGate item) {
            items.Add(item);
            sim.AddItem(item);
            AddToMap(item.GetSelf());
        }
        public void RemoveItem(IGate item) {
            if (marked_item != null) {
                marked_item = null;
                UpdateMarker();
            }
            if (marked_line != null && item.ContainsJoin(marked_line)) {
                marked_line = null;
                UpdateMarker();
            }

            items.Remove(item);
            sim.RemoveItem(item);

            item.ClearJoins();
            ((Control) item).Remove();
        }
        public void RemoveAll() {
            foreach (var item in items.ToArray()) RemoveItem(item);
            sim.Clear();
        }

        private void SaveAllPoses() {
            foreach (var item in items) item.SavePose();
        }

        /*
         * Определение режима перемещения
         */

        int mode = 0;
        /*
         *    Режимы:
         * 0 - ничего не делает
         * 1 - двигаем камеру
         * 2 - двигаем элемент
         * 3 - тянем элемент
         * 4 - вышвыриваем элемент
         * 5 - тянем линию от входа (In)
         * 6 - тянем линию от выхода (Out)
         * 7 - тянем линию от узла (IO)
         * 8 - тянем уже существующее соединение - переподключаем
        */

        private static int CalcMode(string? tag) {
            if (tag == null) return 0;
            return tag switch {
                "Scene" => 1,
                "Body" => 2,
                "Resizer" => 3,
                "Deleter" => 4,
                "In" => 5,
                "Out" => 6,
                "IO" => 7,
                "Join" => 8,
                "Pin" or _ => 0,
            };
        }
        private void UpdateMode(Control item) => mode = CalcMode((string?) item.Tag);
        
        private static bool IsMode(Control item, string[] mods) {
            var name = (string?) item.Tag;
            if (name == null) return false;
            return mods.IndexOf(name) != -1;
        }

        private static UserControl? GetUC(Control item) {
            while (item.Parent != null) {
                if (item is UserControl @UC) return @UC;
                item = (Control) item.Parent;
            }
            return null;
        }
        private static IGate? GetGate(Control item) {
            var UC = GetUC(item);
            if (UC is IGate @gate) return @gate;
            return null;
        }

        /*
         * Обработка мыши
         */

        Point moved_pos;
        IGate? moved_item;
        Point item_old_pos;
        Size item_old_size;

        Ellipse? marker_circle;
        Distantor? start_dist;
        int marker_mode;

        Line? old_join;
        bool join_start;
        bool delete_join = false;

        public bool lock_self_connect = true;

        public void Press(Control item, Point pos) {
            // Log.Write("PointerPressed: " + item.GetType().Name + " pos: " + pos);

            UpdateMode(item);
            // Log.Write("new_mode: " + mode);

            moved_pos = pos;
            moved_item = GetGate(item);
            tapped = true;
            if (moved_item != null) item_old_pos = moved_item.GetPos();

            switch (mode) {
            case 1:
                SaveAllPoses();
                break;
            case 3:
                if (moved_item == null) break;
                item_old_size = moved_item.GetBodySize();
                break;
            case 5 or 6 or 7:
                if (marker_circle == null) break;
                var gate = GetGate(marker_circle) ?? throw new Exception("ERROR"); 
                start_dist = gate.GetPin(marker_circle);

                var circle_pos = start_dist.GetPos();
                marker.StartPoint = marker.EndPoint = circle_pos;
                marker.IsVisible = true;
                marker_mode = mode;
                break;
            case 8:
                if (item is not Line @join) break;
                JoinedItems.arrow_to_join.TryGetValue(@join, out var @join2);
                if (@join2 == null) break;

                if (marked_line == @join2) {
                    marked_line = null;
                    UpdateMarker();
                }

                var dist_a = @join.StartPoint.Hypot(pos);
                var dist_b = @join.EndPoint.Hypot(pos);
                join_start = dist_a > dist_b;
                old_join = @join;

                marker.StartPoint = join_start ? @join.StartPoint : pos;
                marker.EndPoint = join_start ? pos : @join.EndPoint;
                marker_mode = CalcMode(join_start ? @join2.A.tag : @join2.B.tag);

                marker.IsVisible = true;
                @join.IsVisible = false;
                break;
            }

            Move(item, pos);
        }

        public void FixItem(ref Control res, Point pos, IEnumerable<ILogical> items) {
            foreach (var logic in items) {
                var item = (Control) logic;
                var tb = item.TransformedBounds;
                if (tb != null && tb.Value.Bounds.TransformToAABB(tb.Value.Transform).Contains(pos) && (string?) item.Tag != "Join") res = item; 
                FixItem(ref res, pos, item.GetLogicalChildren());
            }
        }
        public void Move(Control item, Point pos, bool use_fix = true) {
            // Log.Write("PointerMoved: " + item.GetType().Name + " pos: " + pos);

            if (use_fix && (mode == 5 || mode == 6 || mode == 7 || mode == 8)) {
                var tb = canv.TransformedBounds;
                if (tb != null) {
                    item = new Canvas() { Tag = "Scene" };
                    var bounds = tb.Value.Bounds.TransformToAABB(tb.Value.Transform);
                    FixItem(ref item, pos + bounds.TopLeft, canv.Children);
                    // Log.Write("tag: " + item.Tag);
                }
            }

            string[] mods = new[] { "In", "Out", "IO" };
            var tag = (string?) item.Tag;
            if (IsMode(item, mods) && item is Ellipse @ellipse
                && !(marker_mode == 5 && tag == "In" || marker_mode == 6 && tag == "Out" ||
                lock_self_connect && moved_item == GetGate(item))) {

                if (marker_circle != null && marker_circle != @ellipse) { 
                    marker_circle.Fill = new SolidColorBrush(Color.Parse("#0000"));
                    marker_circle.Stroke = Brushes.Gray;
                }
                marker_circle = @ellipse;
                @ellipse.Fill = Brushes.Lime;
                @ellipse.Stroke = Brushes.Green;
            } else if (marker_circle != null) {
                marker_circle.Fill = new SolidColorBrush(Color.Parse("#0000"));
                marker_circle.Stroke = Brushes.Gray;
                marker_circle = null;
            }

            if (mode == 8) delete_join = (string?) item.Tag == "Deleter";

           



            var delta = pos - moved_pos;
            if (delta.X == 0 && delta.Y == 0) return;

            if (Math.Pow(delta.X, 2) + Math.Pow(delta.Y, 2) > 9) tapped = false;

            switch (mode) {
            case 1:
                foreach (var item_ in items) {
                    var pose = item_.GetPose();
                    item_.Move(pose + delta, true);
                }
                UpdateMarker();
                break;
            case 2:
                if (moved_item == null) break;
                var new_pos = item_old_pos + delta;
                moved_item.Move(new_pos);
                UpdateMarker();
                break;
            case 3:
                if (moved_item == null) break;
                var new_size = item_old_size + new Size(delta.X, delta.Y);
                moved_item.Resize(new_size);
                UpdateMarker();
                break;
            case 5 or 6 or 7:
                var end_pos = marker_circle == null ? pos : marker_circle.Center(canv);
                marker.EndPoint = end_pos;
                break;
            case 8:
                if (old_join == null) break;
                var p = marker_circle == null ? pos : marker_circle.Center(canv);
                if (join_start) marker.EndPoint = p;
                else marker.StartPoint = p;
                break;
            }
        }

        public bool tapped = false; 
        public Point tap_pos; 

        public int Release(Control item, Point pos, bool use_fix = true) {
            Move(item, pos, use_fix);
            // Log.Write("PointerReleased: " + item.GetType().Name + " pos: " + pos);

            switch (mode) {
            case 5 or 6 or 7:
                if (start_dist == null) break;
                if (marker_circle != null) {
                    var gate = GetGate(marker_circle) ?? throw new Exception("ERROR"); 
                    var end_dist = gate.GetPin(marker_circle);
                    // Log.Write("Стартовый элемент: " + start_dist.parent + " (" + start_dist.GetPos() + ")");
                    // Log.Write("Конечный  элемент: " + end_dist.parent   + " (" + end_dist.GetPos()   + ")");
                    var newy = new JoinedItems(start_dist, end_dist);
                    AddToMap(newy.line);
                }
                marker.IsVisible = false;
                marker_mode = 0;
                break;
            case 8:
                if (old_join == null) break;
                JoinedItems.arrow_to_join.TryGetValue(old_join, out var @join);
                if (marker_circle != null && @join != null) {
                    var gate = GetGate(marker_circle) ?? throw new Exception("ERROR"); 
                    var p = gate.GetPin(marker_circle);
                    @join.Delete();

                    var newy = join_start ? new JoinedItems(@join.A, p) : new JoinedItems(p, @join.B);
                    AddToMap(newy.line);
                } else old_join.IsVisible = true;

                marker.IsVisible = false;
                marker_mode = 0;
                old_join = null;

                if (delete_join) @join?.Delete();
                delete_join = false;
                break;
            }

            if (tapped) Tapped(item, pos);

            int res_mode = mode;
            mode = 0;
            moved_item = null;
            return res_mode;
        }

        private void Tapped(Control item, Point pos) {
            // Log.Write("Tapped: " + item.GetType().Name + " pos: " + pos);
            tap_pos = pos;

            switch (mode) {
            /* case 4:
                if (moved_item != null) RemoveItem(moved_item);
                break; */
            case 2 or 8:
                if (item is Line @line) {
                    if (!JoinedItems.arrow_to_join.TryGetValue(@line, out var @join)) break;
                    marked_item = null;
                    marked_line = @join;
                    UpdateMarker();
                    break;
                }

                if (moved_item == null) break;

                marked_item = moved_item;
                UpdateMarker();
                break;
            }
        }

        public void WheelMove(Control item, double move, Point pos) {
            // Log.Write("WheelMoved: " + item.GetType().Name + " delta: " + (move > 0 ? 1 : -1));
            int mode = CalcMode((string?) item.Tag);
            double scale = move > 0 ? 1.1 : 1 / 1.1;
            double inv_scale = 1 / scale;

            switch (mode) {
            case 1:
                foreach (var gate in items) {
                    gate.ChangeScale(scale, true);

                    var item_pos = gate.GetPos();
                    var delta = item_pos - pos;
                    delta *= scale;
                    var new_pos = delta + pos;
                    gate.Move(new_pos, false);
                }
                UpdateMarker();
                break;
            case 2:
                var gate2 = GetGate(item);
                if (gate2 == null) return;
                gate2.ChangeScale(inv_scale);
                UpdateMarker();
                break;
            }
        }

        public void KeyPressed(Control _, Key key) {
            // Log.Write("KeyPressed: " + item.GetType().Name + " key: " + key);
            switch (key) {
            case Key.Up:
            case Key.Left:
            case Key.Right:
            case Key.Down:
                int dx = key == Key.Left ? -1 : key == Key.Right ? 1 : 0;
                int dy = key == Key.Up ? -1 : key == Key.Down ? 1 : 0;
                marked_item?.Move(marked_item.GetPos() + new Point(dx * 10, dy * 10));
                UpdateMarker();
                break;
            case Key.Delete:
                if (marked_item != null) RemoveItem(marked_item);
                if (marked_line != null) {
                    marked_line.Delete();
                    marked_line = null;
                    UpdateMarker();
                }
                break;
            }
        }


        /*
         * Экспорт и импорт
         */

        public readonly FileHandler filer = new();
        public Scheme? current_scheme;

        public void Export() {
            if (current_scheme == null) return;

            var arr = items.Select(x => x.Export()).ToArray();

            Dictionary<IGate, int> item_to_num = new();
            int n = 0;
            foreach (var item in items) item_to_num.Add(item, n++);
            List<object[]> joins = new();
            foreach (var item in items) joins.Add(item.ExportJoins(item_to_num));

            sim.Clean();
            string states = sim.Export();

            try { current_scheme.Update(arr, joins.ToArray(), states); }
            catch (Exception e) { Log.Write("Save error:\n" + e); }

            /* Log.Write("Items: " + Utils.Obj2json(arr));
            Log.Write("Joins: " + Utils.Obj2json(joins));
            Log.Write("States: " + Utils.Obj2json(states)); */
        }

        public void ImportScheme(bool start = true) {
            if (current_scheme == null) return;

            sim.Stop();
            sim.lock_sim = true;

            RemoveAll();

            List<IGate> list = new();
            foreach (var item in current_scheme.items) {
                if (item is not Dictionary<string, object> @dict) { Log.Write("Не верный тип элемента: " + item); continue; }

                if (!@dict.TryGetValue("id", out var @value)) { Log.Write("id элемента не обнаружен"); continue; }
                if (@value is not int @id) { Log.Write("Неверный тип id: " + @value); continue; }
                var newy = CreateItem(@id);

                newy.Import(@dict);
                AddItem(newy);
                list.Add(newy);
            }
            var items_arr = list.ToArray();

            List<JoinedItems> joinz = new();
            foreach (var obj in current_scheme.joins) {
                object[] join;
                if (obj is List<object> @j) join = @j.ToArray();
                else if (obj is object[] @j2) join = @j2;
                else { Log.Write("Одно из соединений не того типа: " + obj + " " + Utils.Obj2json(obj)); continue; }
                if (join.Length != 6 ||
                    join[0] is not int @num_a || join[1] is not int @pin_a || join[2] is not string @tag_a ||
                    join[3] is not int @num_b || join[4] is not int @pin_b || join[5] is not string @tag_b) { Log.Write("Содержимое списка соединения ошибочно"); continue; }

                var newy = new JoinedItems(new(items_arr[@num_a], @pin_a, tag_a), new(items_arr[@num_b], @pin_b, tag_b));
                AddToMap(newy.line);
                joinz.Add(newy);
            }

            foreach (var join in joinz) join.Update();

            sim.Import(current_scheme.states);
            sim.lock_sim = false;
            if (start) sim.Start();
        }
    }
}
