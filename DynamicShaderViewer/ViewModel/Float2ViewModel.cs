using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicShaderViewer.Helper;
using DynamicShaderViewer.ShaderInfo;
using DynamicShaderViewer.ShaderParser;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SharpDX.Mathematics.Interop;

namespace DynamicShaderViewer.ViewModel
{
    class Float2ViewModel : ViewModelBase
    {
        public Float2ViewModel()
        {
            ShaderViewPort.OnShaderReload += ForceShaderUpdate;
        }
        public string ShaderName { get; set; } = "DefaultName";

        private Float2 _contentValue = new Float2(0.0f, 0.0f);
        public Float2 ContentValue { get { return _contentValue; } set { Set(ref _contentValue, value, "ContentValue"); } }

        private RelayCommand _updateShaderValuesCommand;

        private float _xValue;

        public float XValue
        {
            get { return _contentValue.X; }
            set
            {
                if (!UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => XValue, (v) => XValue = v, _contentValue.X));

                ContentValue = new Float2(value, ContentValue.Y);
                Set(ref _xValue, value, "XValue");
            }
        }

        private float _yValue;

        public float YValue
        {
            get { return _contentValue.Y; }
            set
            {
                if (!UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => YValue, (v) => YValue = v, _contentValue.Y));

                ContentValue = new Float2(ContentValue.X, value);
                Set(ref _yValue, value, "YValue");
            }
        }
        
        public RelayCommand UpdateShaderValuesCommand
        {
            get
            {
                return _updateShaderValuesCommand ?? (_updateShaderValuesCommand = new RelayCommand(() =>
                {
                    var t = ShaderViewPort.Shader.Effect.GetVariableByName(ShaderName).AsVector();
                    t?.Set(new RawVector2(XValue, YValue));
                }));
            }
        }

        public void ForceShaderUpdate()
        {
            UpdateShaderValuesCommand.Execute(null);
        }
    }
}
