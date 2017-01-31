using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DynamicShaderViewer.Helper;
using DynamicShaderViewer.ShaderInfo;
using DynamicShaderViewer.ShaderParser;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SharpDX;
using SharpDX.Mathematics.Interop;

namespace DynamicShaderViewer.ViewModel
{
    public class Float3ViewModel : ViewModelBase
    {
        public Float3ViewModel()
        {
            ShaderViewPort.OnShaderReload += ForceShaderUpdate;
        }
        public string ShaderName { get; set; } = "DefaultName";

        private Float3 _contentValue = new Float3(0.0f, 0.0f, 0.0f);

        private bool _normalizing;
        public Float3 ContentValue
        {
            get
            {
                return _contentValue;
            }
            set
            {
                Set(ref _contentValue, value, "ContentValue");

                if (ShaderViewPort.Shader != null)
                    UpdateShaderValuesCommand.Execute(null);
            }
        }

        private float _xValue;

        public float XValue
        {
            get { return _contentValue.X; }
            set
            {
                if (!UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => XValue, (v) => XValue = v, _contentValue.X, _normalizing ? 3 : 1));
                ContentValue = new Float3(value, ContentValue.Y, ContentValue.Z);
                
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
                    UndoRedoStack.AddTracker(new Tracker<float>(() => YValue, (v) => YValue = v, _contentValue.Y, _normalizing ? 3 : 1));
                ContentValue = new Float3(ContentValue.X, value, ContentValue.Z);
                
                Set(ref _yValue, value, "YValue");
            }
        }

        private float _zValue;

        public float ZValue
        {
            get { return _contentValue.Z; }
            set
            {
                if (!UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => ZValue, (v) => ZValue = v, _contentValue.Z, _normalizing ? 3 : 1));
                ContentValue = new Float3(ContentValue.X, ContentValue.Y, value);
                
                Set(ref _zValue, value, "ZValue");
            }
        }

        private RelayCommand _normalizeValues;

        public RelayCommand NormalizeValue
        {
            get
            {
                return _normalizeValues ?? (_normalizeValues = new RelayCommand(() =>
                    {
                        float length = (float)Math.Sqrt(XValue * XValue + YValue * YValue +
                                                         ZValue * ZValue);

                        _normalizing = true;
                        XValue = _contentValue.X / length;
                        YValue = _contentValue.Y / length;
                        ZValue = _contentValue.Z / length;
                        _normalizing = false;
                        UpdateShaderValuesCommand.Execute(null);
                    }
                ));
            }
        }

        private RelayCommand _updateShaderValuesCommand;

        public RelayCommand UpdateShaderValuesCommand
        {
            get
            {
                return _updateShaderValuesCommand ?? (_updateShaderValuesCommand = new RelayCommand(() =>
                {
                    var t = ShaderViewPort.Shader.Effect.GetVariableByName(ShaderName).AsVector();
                    t?.Set(new RawVector3(XValue, YValue, ZValue));
                }));
            }
        }

        public void ForceShaderUpdate()
        {
            UpdateShaderValuesCommand.Execute(null);
        }
    }
}
