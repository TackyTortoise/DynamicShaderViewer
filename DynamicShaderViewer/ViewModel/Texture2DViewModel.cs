using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DynamicShaderViewer.ShaderInfo;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using SharpDX.Direct3D10;

namespace DynamicShaderViewer.ViewModel
{
    class Texture2DViewModel : ViewModelBase
    {
        public Texture2DViewModel()
        {
            ShaderViewPort.OnShaderReload += ForceShaderUpdate;
        }

        private Device1 _device;
        public void SetDevice(Device1 d)
        {
            _device = d;

            var t = ShaderViewPort.Shader.Effect.GetVariableByName(ShaderName).AsShaderResource();
            t.SetResource(new ShaderResourceView(_device, ContentValue));
        }
        
        private string _fileName = @"./Textures/TestTexture.png";

        public string FileName
        {
            get { return _fileName; }
            set { Set(ref _fileName, value, "FileName"); }
        }

        public string ShaderName { get; set; } = "DefaultName";
            
        private Texture2D _contentValue = null;
        public Texture2D ContentValue { get { return _contentValue; } set { Set(ref _contentValue, value, "ContentValue"); } }

        private RelayCommand _loadTextureCommand;

        public RelayCommand LoadTextureCommand
        {
            get
            {
                return _loadTextureCommand ?? (_loadTextureCommand = new RelayCommand(() =>
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    string baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                    ofd.InitialDirectory = $@"{baseDirectory}Textures";
                    ofd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

                    if (ofd.ShowDialog() == true)
                    {
                        Texture2D oldTex = ContentValue;
                        try
                        {
                            ContentValue = Resource.FromFile<Texture2D>(_device, ofd.FileName);
                        }
                        catch (Exception ex)
                        {
                            ContentValue = oldTex;
                            MessageBox.Show($"Failed to load texture {ofd.FileName}, exception: {ex.Message}");
                            return;
                        }

                        FileName = ofd.FileName;

                        var t = ShaderViewPort.Shader.Effect.GetVariableByName(ShaderName).AsShaderResource();
                        t.SetResource(new ShaderResourceView(_device, ContentValue));
                    }
                }));
            }
        }

        private RelayCommand _reloadTextureCommand;

        public RelayCommand ReloadTextureCommand
        {
            get
            {
                return _reloadTextureCommand ?? (_reloadTextureCommand = new RelayCommand(() =>
                {
                    if (_device == null)
                        return;
                    ContentValue = Resource.FromFile<Texture2D>(_device, _fileName);
                    var t = ShaderViewPort.Shader.Effect.GetVariableByName(ShaderName).AsShaderResource();
                    t?.SetResource(new ShaderResourceView(_device, ContentValue));
                }));
            }
        }

        public void ForceShaderUpdate()
        {
            ReloadTextureCommand.Execute(null);
        }
    }
}
