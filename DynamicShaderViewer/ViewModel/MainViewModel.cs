using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DynamicShaderViewer.Helper;
using DynamicShaderViewer.ShaderInfo;
using DynamicShaderViewer.ShaderParser;
using DynamicShaderViewer.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SharpDX.Direct3D10;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SharpDX;

namespace DynamicShaderViewer.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ParameterSerializer ParameterSerialize = new ParameterSerializer();
        public ShaderViewPort ViewPort { get; set; } = new ShaderViewPort();
        public HLSLParser ShaderParser = new HLSLParser();
        private string _shaderName;

        private RelayCommand _loadShaderCommand;

        public RelayCommand LoadShaderCommand
        {
            get
            {
                return _loadShaderCommand ?? (_loadShaderCommand = new RelayCommand(() =>
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    string baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                    ofd.InitialDirectory = $@"{baseDirectory}Shaders";
                    ofd.Filter = "Effect Files (.fx)|*.fx";

                    if (ofd.ShowDialog() == true)
                    {
                        if (!ShaderParser.ParseFile(ofd.FileName))
                        {
                            MessageBox.Show("Failed to get any parameters from shader");
                            ShaderParser.ParameterList.Clear();
                        }

                        _shaderName = ofd.FileName;


                        if (ViewPort.LoadShader(_shaderName))
                            UpdateParameterUI();
                    }
                }));
            }
        }

        private RelayCommand _loadModelCommand;

        public RelayCommand LoadModelCommand
        {
            get
            {
                return _loadModelCommand ?? (_loadModelCommand = new RelayCommand(() =>
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    string baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                    ofd.InitialDirectory = $@"{baseDirectory}Models";

                    if (ofd.ShowDialog() == true)
                        ViewPort.LoadModel(ofd.FileName);
                }));
            }
        }

        private RelayCommand _undoCommand;

        public RelayCommand UndoCommand
        {
            get
            {
                return _undoCommand ?? (_undoCommand = new RelayCommand(() =>
                           {
                               UndoRedoStack.Undo(); ;
                           }
                       ));
            }
        }

        private RelayCommand _redoCommand;

        public RelayCommand RedoCommand
        {
            get
            {
                return _redoCommand ?? (_redoCommand = new RelayCommand(() =>
                           {
                               UndoRedoStack.Redo();
                           }
                       ));
            }
        }

        private RelayCommand _serializeParameters;

        public RelayCommand SerializeParameters
        {
            get
            {
                return _serializeParameters ?? (_serializeParameters = new RelayCommand(() =>
                           {
                               ParameterSerialize.SerializeParameters(ParameterGrid.Children, ViewPort.ShaderFileName);
                           }
                       ));
            }
        }

        private RelayCommand _deserializeParameters;

        public RelayCommand DeserializeParameters
        {
            get
            {
                return _deserializeParameters ?? (_deserializeParameters = new RelayCommand(() =>
                           {
                               try
                               {
                                   var oldList = new List<Parameter>(ShaderParser.ParameterList);
                                   var r = ParameterSerialize.DeSerializeParameters(ShaderParser.ParameterList);
                                   if (r != ViewPort.ShaderFileName && r != string.Empty)
                                       MessageBox.Show(
                                           "Warning, loading parameters from shader different from the one currently loaded, will try to set common parameters!");
                                   if (r == string.Empty)
                                       ShaderParser.ParameterList = oldList;
                                   UpdateParameterUI();
                                   ViewPort.ReloadShader.Execute(null);
                               }
                               catch (Exception ex)
                               {
                                   MessageBox.Show($"Failed do load xml file, Exception: {ex.Message}");
                               }
                           }
                       ));
            }
        }

        public Grid ParameterGrid { get; set; } = new Grid();
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                ShaderParser.ParseFile(@"C:\Users\diete\Desktop\3DAE\Tool Development\DynamicShaderViewer\DynamicShaderViewer\bin\Debug\Shaders\PosNormTex3D.fx");
                UpdateParameterUI();
            }
            else
            {
                ShaderParser.ParseFile(@"./Shaders/PosColNorm3D.fx");
                UpdateParameterUI();
            }
        }

        void UpdateParameterUI()
        {
            ParameterGrid.Children.Clear();
            ParameterGrid.RowDefinitions.Clear();
            int useRow = 0;

            foreach (var parameter in ShaderParser.ParameterList)
            {
                //Boolean
                if (parameter.Type == typeof(bool))
                {
                    var rd = new RowDefinition();
                    rd.Height = new GridLength(50);
                    ParameterGrid.RowDefinitions.Add(rd);

                    var boolView = new BoolView();
                    var boolDc = new BoolViewModel();
                    boolDc.ShaderName = parameter.ShaderName;

                    if (parameter.DefaultValue != string.Empty)
                        boolDc.ContentValue = bool.Parse(parameter.DefaultValue);

                    boolView.DataContext = boolDc;
                    boolView.SetValue(Grid.RowProperty, useRow);
                    ParameterGrid.Children.Add(boolView);

                    ++useRow;
                }

                //Int
                else if (parameter.Type == typeof(int))
                {
                    var rd = new RowDefinition();
                    rd.Height = new GridLength(50);
                    ParameterGrid.RowDefinitions.Add(rd);

                    var intView = new IntView();
                    var intDc = new IntViewModel();
                    intDc.ShaderName = parameter.ShaderName;

                    if (parameter.DefaultValue != string.Empty)
                        intDc.ContentValue = int.Parse(parameter.DefaultValue);

                    intView.DataContext = intDc;
                    intView.SetValue(Grid.RowProperty, useRow);
                    ParameterGrid.Children.Add(intView);

                    ++useRow;
                }

                //Float
                if (parameter.Type == typeof(float))
                {
                    var rd = new RowDefinition();
                    rd.Height = new GridLength(50);
                    ParameterGrid.RowDefinitions.Add(rd);

                    var floatView = new FloatView();
                    var floatViewDc = new FloatViewModel();
                    floatViewDc.ShaderName = parameter.ShaderName;

                    if (parameter.DefaultValue != string.Empty)
                        floatViewDc.ContentValue = float.Parse(parameter.DefaultValue, CultureInfo.InvariantCulture);

                    floatView.DataContext = floatViewDc;
                    floatView.SetValue(Grid.RowProperty, useRow);
                    ParameterGrid.Children.Add(floatView);

                    ++useRow;
                }

                //Float2
                else if (parameter.Type == typeof(Float2))
                {
                    var rd = new RowDefinition();
                    rd.Height = new GridLength(75);
                    ParameterGrid.RowDefinitions.Add(rd);

                    var float2View = new Float2View();
                    var float2Dc = new Float2ViewModel();
                    float2Dc.ShaderName = parameter.ShaderName;

                    if (parameter.DefaultValue != string.Empty)
                        float2Dc.ContentValue = new Float2(parameter.DefaultValue);

                    float2View.DataContext = float2Dc;
                    float2View.SetValue(Grid.RowProperty, useRow);
                    ParameterGrid.Children.Add(float2View);

                    ++useRow;
                }

                //Float3
                else if (parameter.Type == typeof(Float3))
                {
                    var rd = new RowDefinition();
                    rd.Height = new GridLength(75);
                    ParameterGrid.RowDefinitions.Add(rd);

                    var float3View = new Float3View();
                    var float3Dc = new Float3ViewModel();
                    float3Dc.ShaderName = parameter.ShaderName;

                    if (parameter.DefaultValue != string.Empty)
                        float3Dc.ContentValue = new Float3(parameter.DefaultValue);

                    float3View.DataContext = float3Dc;
                    float3View.SetValue(Grid.RowProperty, useRow);
                    ParameterGrid.Children.Add(float3View);

                    ++useRow;
                }

                //Float4
                else if (parameter.Type == typeof(Float4))
                {
                    var rd = new RowDefinition();
                    rd.Height = new GridLength(75);
                    ParameterGrid.RowDefinitions.Add(rd);

                    var float4View = new Float4View();
                    var float4Dc = new Float4ViewModel();
                    float4Dc.ShaderName = parameter.ShaderName;

                    if (parameter.DefaultValue != string.Empty)
                        float4Dc.ContentValue = new Float4(parameter.DefaultValue);

                    float4View.DataContext = float4Dc;
                    float4View.SetValue(Grid.RowProperty, useRow);
                    ParameterGrid.Children.Add(float4View);

                    ++useRow;
                }

                //Texture2D
                else if (parameter.Type == typeof(Texture2D))
                {
                    var rd = new RowDefinition();
                    rd.Height = new GridLength(75);
                    ParameterGrid.RowDefinitions.Add(rd);

                    var textureView = new Texture2DView();
                    var textureDc = new Texture2DViewModel();
                    textureDc.ShaderName = parameter.ShaderName;

                    if (parameter.DefaultValue == String.Empty)
                    {
                        textureDc.ContentValue = Resource.FromFile<Texture2D>(ViewPort.GetDevice(),
                            @"./Textures/TestTexture.png");
                        textureDc.FileName = @"./Textures/TestTexture.png";
                    }
                    else
                    {
                        textureDc.ContentValue = Resource.FromFile<Texture2D>(ViewPort.GetDevice(),
                            parameter.DefaultValue);
                        textureDc.FileName = parameter.DefaultValue;
                    }

                    textureDc.SetDevice(ViewPort.GetDevice());

                    textureView.DataContext = textureDc;
                    textureView.SetValue(Grid.RowProperty, useRow);
                    ParameterGrid.Children.Add(textureView);

                    ++useRow;
                }
            }

            UndoRedoStack.Reset();
        }
    }
}