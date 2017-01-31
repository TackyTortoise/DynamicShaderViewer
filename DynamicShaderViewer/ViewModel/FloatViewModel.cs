using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using DynamicShaderViewer.Helper;
using DynamicShaderViewer.ShaderInfo;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace DynamicShaderViewer.ViewModel
{
    class FloatViewModel : ViewModelBase
    {
        public FloatViewModel()
        {
            ShaderViewPort.OnShaderReload += ForceShaderUpdate;
        }
        public string ShaderName { get; set; } = "DefaultName";
        private float _contentValue;

        public float ContentValue
        {
            get { return _contentValue; }
            set
            {
                if (!UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => ContentValue, (v) => ContentValue = v, _contentValue));
                Set(ref _contentValue, value, "ContentValue");
                if (ShaderViewPort.Shader != null)
                    UpdateShaderValuesCommand.Execute(null);
            }
        }

        private RelayCommand _updateShaderValuesCommand;
        public RelayCommand UpdateShaderValuesCommand
        {
            get
            {
                return _updateShaderValuesCommand ?? (_updateShaderValuesCommand = new RelayCommand(() =>
                {
                    var t = ShaderViewPort.Shader.Effect.GetVariableByName(ShaderName).AsScalar();
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
