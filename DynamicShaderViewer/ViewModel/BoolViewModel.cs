using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicShaderViewer.Helper;
using DynamicShaderViewer.ShaderInfo;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace DynamicShaderViewer.ViewModel
{
    class BoolViewModel :ViewModelBase
    {
        public BoolViewModel()
        {
            ShaderViewPort.OnShaderReload += ForceShaderUpdate;
        }

        public string ShaderName { get; set; } = "DefaultName";
        private bool _contentValue;

        public bool ContentValue
        {
            get { return _contentValue; }
            set
            {
                if (!UndoRedoStack.BusyUndo)
                    UndoRedoStack.AddTracker(new Tracker<float>(() => ContentValue ? 1 : 0, (v) => ContentValue = v > 0, _contentValue ? 1 : 0));
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
