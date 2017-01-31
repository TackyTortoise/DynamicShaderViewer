using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Xsl;
using DynamicShaderViewer.Helper;
using DynamicShaderViewer.ShaderInfo;
using DynamicShaderViewer.ShaderParser;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace DynamicShaderViewer.ViewModel
{
    class Float4ViewModel : ViewModelBase
    {
        public Float4ViewModel()
        {
            XValue = _contentValue.X;
            YValue = _contentValue.Y;
            ZValue = _contentValue.Z;
            WValue = _contentValue.W;

            ShaderViewPort.OnShaderReload += ForceShaderUpdate;
        }
        public string ShaderName { get; set; } = "DefaultName";

        private bool _viaPicker = false;

        private Color _selectedColor;

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { Set(ref _selectedColor, value, "SelectedColor"); }
        }

        private Float4 _contentValue = new Float4(0.0f, 0.0f, 0.0f, 1.0f);

        public Float4 ContentValue
        {
            get { return _contentValue; }
            set
            {
                Set(ref _contentValue, value, "ContentValue");
                if (ShaderViewPort.Shader.Effect.GetVariableByName(ShaderName).AsVector() != null)
                    UpdateShaderValuesCommand.Execute(null);
            }
        }

        private float _xValue;

        public float XValue
        {
            get { return _contentValue.X; }
            set
            {
                if (!_viaPicker && !UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => XValue, (v) => XValue = v, _contentValue.X, UndoRedoStack.RedoBatchSize));
                ContentValue = new Float4(value, ContentValue.Y, ContentValue.Z, ContentValue.W);
                Set(ref _xValue, value, "XValue");
            }
        }

        private float _yValue;

        public float YValue
        {
            get { return _contentValue.Y; }
            set
            {
                if (!_viaPicker && !UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => YValue, (v) => YValue = v, _contentValue.Y, UndoRedoStack.RedoBatchSize));
                ContentValue = new Float4(ContentValue.X, value, ContentValue.Z, ContentValue.W);
                Set(ref _yValue, value, "YValue");
            }
        }

        private float _zValue;

        public float ZValue
        {
            get { return _contentValue.Z; }
            set
            {
                if (!_viaPicker && !UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => ZValue, (v) => ZValue = v, _contentValue.Z, UndoRedoStack.RedoBatchSize));
                ContentValue = new Float4(ContentValue.X, ContentValue.Y, value, ContentValue.W);
                Set(ref _zValue, value, "ZValue");
            }
        }

        private float _wValue;

        public float WValue
        {
            get { return _wValue; }
            set
            {
                if (!_viaPicker && !UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => WValue, (v) => WValue = v, _contentValue.W, UndoRedoStack.RedoBatchSize));
                ContentValue = new Float4(ContentValue.X, ContentValue.Y, ContentValue.Z, value);
                Set(ref _wValue, value, "WValue");
            }
        }

        private bool _enableColorPicker = false;

        public bool EnableColorPicker
        {
            get { return _enableColorPicker; }
            set { Set(ref _enableColorPicker, value, "EnableColorPicker"); }
        }

        private RelayCommand _switchColorPicker;

        public RelayCommand SwitchColorPicker
        {
            get
            {
                return _switchColorPicker ?? (_switchColorPicker = new RelayCommand(() =>
                           {
                               if (!EnableColorPicker && !UndoRedoStack.BusyUndo)
                               {
                                   UndoRedoStack.AddTracker(new Tracker<float>(() => XValue, (v) => XValue = v, _contentValue.X, 3));
                                   UndoRedoStack.AddTracker(new Tracker<float>(() => YValue, (v) => YValue = v, _contentValue.Y, 3));
                                   UndoRedoStack.AddTracker(new Tracker<float>(() => ZValue, (v) => ZValue = v, _contentValue.Z, 3));
                               }
                               EnableColorPicker = !EnableColorPicker;
                           }
                       ));
            }
        }

        private RelayCommand<Float4> _colorPickerChange;

        public RelayCommand<Float4> ColorPickerChange
        {
            get
            {
                return _colorPickerChange ?? (_colorPickerChange = new RelayCommand<Float4>((p) =>
                           {
                               _viaPicker = true;
                               XValue = SelectedColor.R / 255f;
                               YValue = SelectedColor.G / 255f;
                               ZValue = SelectedColor.B / 255f;
                               WValue = _contentValue.W;
                               _viaPicker = false;
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
                    t?.Set(ContentValue);
                }));
            }
        }
        

        public void ForceShaderUpdate()
        {
            UpdateShaderValuesCommand.Execute(null);
        }
    }
}
