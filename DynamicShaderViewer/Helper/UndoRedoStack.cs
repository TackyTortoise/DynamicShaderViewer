using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DynamicShaderViewer.ShaderParser;
using SharpDX.Direct3D9;

namespace DynamicShaderViewer.Helper
{
    public class Tracker<T>
    {
        public Func<T> Get;
        public Action<T> Set;
        public Tracker(Func<T> @get, Action<T> @set, T value, int batchSize = 1)
        {
            Get = @get;
            Set = @set;
            MyValue = value;
            BatchSize = batchSize;
        }

        public T Value
        {
            get { return Get(); }
            set { Set(value); }
        }
        public T MyValue;
        public int BatchSize;
    }
    public class UndoRedoStack
    {
        private static Stack<Tracker<float>> _undoStack = new Stack<Tracker<float>>();
        private static Stack<Tracker<float>> _redoStack = new Stack<Tracker<float>>();

        public static bool BusyUndo = false;
        public static int RedoBatchSize = 1;
        public static void AddTracker(Tracker<float> t)
        {
            _undoStack.Push(t);
            _redoStack.Clear();
        }

        public static void ResetUndo(int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                _undoStack.Pop();
            }
        }

        public static void Undo()
        {
            if (_undoStack.Count <= 0)
                return;
            var c = _undoStack.Peek().BatchSize;
            for (int i = 0; i < c; ++i)
            {
                BusyUndo = true;
                Tracker<float> cmd = _undoStack.Pop();
                _redoStack.Push(new Tracker<float>(cmd.Get, cmd.Set, cmd.Value, cmd.BatchSize));
                ExecuteTracker(ref cmd);
                BusyUndo = false;
            }
        }

        public static void Redo()
        {
            if (_redoStack.Count <= 0)
                return;
            var c = _redoStack.Peek().BatchSize;
            RedoBatchSize = c;
            for (int i = 0; i < c; ++i)
            {
                Tracker<float> cmd = _redoStack.Pop();
                Stack<Tracker<float>> sc = new Stack<Tracker<float>>(_redoStack);
                ExecuteTracker(ref cmd);
                _redoStack = new Stack<Tracker<float>>(sc);
            }
            RedoBatchSize = 1;
        }

        private static void ExecuteTracker(ref Tracker<float> cmd)
        {
            cmd.Value = cmd.MyValue;
        }

        public static void Reset()
        {
            _undoStack = new Stack<Tracker<float>>();
            _redoStack = new Stack<Tracker<float>>();
        }
    }
}
