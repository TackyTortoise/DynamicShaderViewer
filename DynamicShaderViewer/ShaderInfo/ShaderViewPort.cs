using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DirectxImageControl;
using DynamicShaderViewer.ViewModel;
using GalaSoft.MvvmLight.CommandWpf;
using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Device1 = SharpDX.Direct3D10.Device1;

namespace DynamicShaderViewer.ShaderInfo
{
    public class ShaderViewPort : IDX10Viewport
    {
        private Device1 _device;
        private RenderTargetView _renderTargetView;
        private DX10RenderCanvas _renderControl;

        public static event Action OnShaderReload;

        private bool _upright = false;

        private float _modelScale = .5f;

        public AssimpModel Model { get; set; }
        public static IEffect Shader { get; set; }

        private float _modelRotation;
        
        private string _shaderPath;

        public string ShaderFileName => Path.GetFileName(_shaderPath);

        private bool _isInitialized = false;

        private string _modelPath = string.Empty;

        private RelayCommand<MouseWheelEventArgs> _scrollCommand;

        public RelayCommand<MouseWheelEventArgs> ScrollCommand
        {
            get
            {
                return _scrollCommand ?? (_scrollCommand = new RelayCommand<MouseWheelEventArgs>((MouseWheelEventArgs x) =>
                {
                    _modelScale += x.Delta / 2000f;
                    if (_modelScale < .1f)
                        _modelScale = .1f;
                    if (_modelScale > 10f)
                        _modelScale = 10f;
                }));
            }
        }

        private RelayCommand _flipModelCommand;

        public RelayCommand FlipModelCommand
        {
            get
            {
                return _flipModelCommand ?? (_flipModelCommand = new RelayCommand(() =>
                {
                    _upright = !_upright;
                }));
            }
        }

        private int _turnDirection = 1;

        private RelayCommand _switchRotationCommand;

        public RelayCommand SwitchRotationCommand
        {
            get
            {
                return _switchRotationCommand ?? (_switchRotationCommand = new RelayCommand(() =>
                {
                    _turnDirection = -_turnDirection;
                    _pauseRotation = false;
                }));
            }
        }

        private bool _pauseRotation = false;
        private RelayCommand _switchPauseCommand;

        public RelayCommand SwitchPauseCommand
        {
            get
            {
                return _switchPauseCommand ?? (_switchPauseCommand = new RelayCommand(() =>
                           {
                               _pauseRotation = !_pauseRotation;
                           }
                       ));
            }
        }

        private RelayCommand _reloadShader;

        public RelayCommand ReloadShader
        {
            get
            {
                return _reloadShader ?? (_reloadShader = new RelayCommand(() =>
                           {
                               LoadShader(_shaderPath);
                               OnShaderReload?.Invoke();
                           }
                       ));
            }
        }

        private RelayCommand _reloadModel;

        public RelayCommand ReloadModel
        {
            get
            {
                return _reloadModel ?? (_reloadModel = new RelayCommand(() =>
                           {
                               LoadModel(_modelPath);
                           }
                       ));
            }
        }

        public void Initialize(Device1 device, RenderTargetView renderTarget, DX10RenderCanvas canvasControl)
        {
            if (_isInitialized)
                return;
            _device = device;
            _renderTargetView = renderTarget;
            _renderControl = canvasControl;

            if (Shader == null)
                LoadShader(@"./Shaders/PosColNorm3D.fx");

            if (Model == null)
                LoadModel(@"./Models/teapot.fbx");

            _isInitialized = true;
        }

        public void LoadModel(string fileName)
        {
            var oldModel = Model;
            var mn = _modelPath;
            Model = new AssimpModel(fileName);
            try
            {
                Model.Create(_device, Shader as GenericShader);
                _modelPath = fileName;
            }
            catch (Exception ex)
            {
                Model = oldModel;
                _modelPath = mn;
                MessageBox.Show($"Failed to load model, Exception: {ex.Message}");
            }
        }

        public bool LoadShader(string fileName)
        {
            var oldshader = Shader;
            var oldshaderpath = _shaderPath;
            Shader = new GenericShader(fileName);
            try
            {
                Shader.Create(_device);
                _shaderPath = fileName;
                Update(0f);
                if (_modelPath != string.Empty)
                    LoadModel(_modelPath);
            }
            catch (Exception ex)
            {
                Shader = oldshader;
                _shaderPath = oldshaderpath;
                if (_modelPath != string.Empty)
                    LoadModel(_modelPath);
                Update(0f);
                MessageBox.Show($"Failed to load shader, Exception: {ex.Message}");
                return false;
            }
            return true;
        }

        public void Deinitialize()
        {

        }

        public void Update(float deltaT)
        {
            if (Model != null && Shader != null)
            {
                if (!_pauseRotation)
                    _modelRotation += MathUtil.PiOverFour * deltaT * _turnDirection;

                var worldMat = Matrix.Identity;
                worldMat *= Matrix.Scaling(_modelScale);
                if (!_upright)
                    worldMat *= Matrix.RotationX(-MathUtil.PiOverTwo);
                worldMat *= Matrix.RotationY(_modelRotation);

                var viewMat = Matrix.LookAtLH(new Vector3(0, 50, -100), Vector3.Zero, Vector3.UnitY);
                var projMat = Matrix.PerspectiveFovLH(MathUtil.PiOverFour, (float)_renderControl.ActualWidth / (float)_renderControl.ActualHeight, 0.1f, 1000f);

                Shader.SetWorld(worldMat);
                Shader.SetWorldViewProjection(worldMat * viewMat * projMat);
            }
        }

        public void Render(float deltaT)
        {
            if (_device == null)
                return;

            if (Model != null && Shader != null)
            {
                _device.InputAssembler.InputLayout = Shader.InputLayout;
                _device.InputAssembler.PrimitiveTopology = Model.PrimitiveTopology;
                _device.InputAssembler.SetIndexBuffer(Model.IndexBuffer, Format.R32_UInt, 0);
                _device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(Model.VertexBuffer, Model.VertexStride, 0));

                for (int i = 0; i < Shader.Technique.Description.PassCount; ++i)
                {
                    Shader.Technique.GetPassByIndex(i).Apply();
                    _device.DrawIndexed(Model.IndexCount, 0, 0);
                }
            }
        }

        public Device1 GetDevice()
        {
            return _device;
        }
    }
}
