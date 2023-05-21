using LogicSimulator.ViewModels;
using LogicSimulator.Views.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicSimulator.Models {
    public class Meta {
        public IGate? item;
        public int[] ins;
        public int[] outs;
        public bool[] i_buf;
        public bool[] o_buf;

        public Meta(IGate item, int out_id) {
            this.item = item;
            ins = Enumerable.Repeat(0, item.CountIns).ToArray();
            outs = Enumerable.Range(out_id, item.CountOuts).ToArray();
            i_buf = Enumerable.Repeat(false, item.CountIns).ToArray();
            o_buf = Enumerable.Repeat(false, item.CountOuts).ToArray();
        }

        public void Print() {
            Log.Write("Элемент: " + item + " | Ins: " + Utils.Obj2json(ins) + " | Outs: " + Utils.Obj2json(outs));
        }
    }


    public class Simulator {
        public Simulator() {
            Start();
        }

        private Task? task;
        private bool stop_sim = false;
        public bool lock_sim = false;
        public void Start() {
            if (task != null || lock_sim) return;
            stop_sim = false;
            task = Task.Run(async () => {
                for (; ; ) {
                    await Task.Delay(1000 / 1000);
                    
                    try { Tick(); } catch (Exception e) { Log.Write("Logical crush: " + e); continue; }

                    if (stop_sim) return;
                }
            });
        }
        public void Stop() {
            if (task == null) return;
            stop_sim = true;
            task.GetAwaiter().GetResult();
            task = null;
        }



        List<bool> outs = new() { false };
        List<bool> outs2 = new() { false };
        readonly List<Meta> items = new();
        readonly Dictionary<IGate, Meta> ids = new();

        public void AddItem(IGate item) {
            Stop();

            int out_id = outs.Count;
            for (int i = 0; i < item.CountOuts; i++) {
                outs.Add(false);
                outs2.Add(false);
            }

            // int id = items.Count;
            Meta meta = new(item, out_id);
            items.Add(meta);
            ids.Add(item, meta);

            Start();
            // meta.Print();
        }

        public void RemoveItem(IGate item) {
            Stop();

            Meta meta = ids[item];
            meta.item = null;
            foreach (var i in Enumerable.Range(0, meta.outs.Length)) {
                int n = meta.outs[i];
                outs[n] = outs2[n] = false;
            }
            ids.Remove(item);

            Start();
        }

        private void Tick() {
            foreach (var meta in items) {
                var item = meta.item;
                if (item == null) continue;

                item.LogicUpdate(ids, meta);

                int[] i_n = meta.ins, o_n = meta.outs;
                bool[] ib = meta.i_buf, ob = meta.o_buf;

                for (int i = 0; i < ib.Length; i++) ib[i] = outs[i_n[i]];
                item.Brain(ref ib, ref ob);
                for (int i = 0; i < ob.Length; i++) {
                    bool res = ob[i];
                    outs2[o_n[i]] = res;
                    item.SetJoinColor(i, res);
                }
            }

            (outs2, outs) = (outs, outs2); 

            if (comparative_test_mode) {
                prev_state = cur_state;
                cur_state = Export();
            }
        }

        public void Clean() {
            int n = 0;
            int[] arr = Enumerable.Repeat(-1, outs.Count).ToArray();
            StringBuilder sb = new();
            sb.Append('0');
            foreach (var meta in items)
                if (meta.item != null)
                    foreach (var @out in meta.outs) {
                        arr[@out] = ++n;
                        sb.Append(outs[@out] ? '1' : '0');
                    }
            arr[0] = 0;
            foreach (var meta in items) {
                meta.outs = meta.outs.Select(x => arr[x]).ToArray();
                meta.ins = meta.ins.Select(x => arr[x]).ToArray();
            }
            Import(sb.ToString());
        }
        public string Export() => string.Join("", outs.Select(x => x ? '1' : '0'));
        public void Import(string state) {
            if (state.Length == 0) state = "0";
            outs = state.Select(x => x == '1').ToList();
            outs2 = outs.ToList(); // clone
        }
        public void Clear() {
            outs = new() { false };
            outs2 = new() { false };
            items.Clear();
            ids.Clear();
        }

        public void TopSecretPublicTickMethod() => Tick();

        // Для комплесного решения:

        public Switch[] GetSwitches() => items.Select(x => x.item).OfType<Switch>().ToArray();
        public LightBulb[] GetLightBulbs() => items.Select(x => x.item).OfType<LightBulb>().ToArray();

       

        private bool comparative_test_mode = false;
        private string prev_state = "0";
        private string cur_state = "0";

        public bool ComparativeTestMode {
            get => comparative_test_mode;
            set {
                comparative_test_mode = value;
                if (value) prev_state = cur_state = Export();
            }
        }

        public bool SomethingHasChanged => prev_state != cur_state;
    }
}
