using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;
using DynamicShaderViewer.ShaderInfo;
using DynamicShaderViewer.ShaderParser;
using DynamicShaderViewer.ViewModel;
using DynamicShaderViewer.Views;
using Microsoft.Win32;

namespace DynamicShaderViewer.Helper
{
    [Serializable]
    [XmlRoot (ElementName = "Holder")]
    public struct SerializeHolder<T>
    {
        public SerializeHolder(string name, T value)
        {
            Name = name;
            Value = value;
        }
        [XmlElement (ElementName = "shadername")]
        public string Name { get; set; }
        [XmlElement (ElementName = "value")]
        public T Value { get; set; }
    }

    [Serializable]
    [XmlRoot (ElementName = "SerializedParameters")]
    public class ParameterSerializer
    {
        private XmlSerializer XmlSer = new XmlSerializer(typeof(ParameterSerializer));

        public string ShaderName { get; set; }

        public List<SerializeHolder<bool>> BoolList { get; set; } = new List<SerializeHolder<bool>>();
        public List<SerializeHolder<Float2>> Float2List { get; set; } = new List<SerializeHolder<Float2>>();
        public List<SerializeHolder<Float3>> Float3List { get; set; } = new List<SerializeHolder<Float3>>();
        public List<SerializeHolder<Float4>> Float4List { get; set; } = new List<SerializeHolder<Float4>>();
        public List<SerializeHolder<float>> FloatList { get; set; } = new List<SerializeHolder<float>>();
        public List<SerializeHolder<int>> IntList { get; set; } = new List<SerializeHolder<int>>();
        public List<SerializeHolder<string>> Texture2DList { get; set; } = new List<SerializeHolder<string>>();

        public void SerializeParameters(UIElementCollection parameterGrid, string shaderName)
        {
            ShaderName = shaderName;

            ClearLists();

            foreach (var p in parameterGrid)
            {
                if (p is BoolView)
                {
                    var vm = (p as BoolView).DataContext as BoolViewModel;
                    BoolList.Add(new SerializeHolder<bool>(vm.ShaderName, vm.ContentValue));
                }
                else if (p is IntView)
                {
                    var vm = (p as IntView).DataContext as IntViewModel;
                    IntList.Add(new SerializeHolder<int>(vm.ShaderName, vm.ContentValue));
                }
                else if (p is FloatView)
                {
                    var vm = (p as FloatView).DataContext as FloatViewModel;
                    FloatList.Add(new SerializeHolder<float>(vm.ShaderName, vm.ContentValue));
                }
                else if (p is Float2View)
                {
                    var vm = (p as Float2View).DataContext as Float2ViewModel;
                    Float2List.Add(new SerializeHolder<Float2>(vm.ShaderName, vm.ContentValue));
                }
                else if (p is Float3View)
                {
                    var vm = (p as Float3View).DataContext as Float3ViewModel;
                    Float3List.Add(new SerializeHolder<Float3>(vm.ShaderName, vm.ContentValue));
                }
                else if (p is Float4View)
                {
                    var vm = (p as Float4View).DataContext as Float4ViewModel;
                    Float4List.Add(new SerializeHolder<Float4>(vm.ShaderName, vm.ContentValue));
                }
                else if (p is Texture2DView)
                {
                    var vm = (p as Texture2DView).DataContext as Texture2DViewModel;
                    Texture2DList.Add(new SerializeHolder<string>(vm.ShaderName, vm.FileName));
                }
            }

            var ofd = new SaveFileDialog();
            string baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            ofd.InitialDirectory = baseDirectory;
            ofd.DefaultExt = ".xml";
            ofd.Filter = "XML files (.xml)|*.xml";
            if (ofd.ShowDialog() == true)
            {
                using (var f = File.Create(ofd.FileName))
                {
                    XmlSer.Serialize(f, this);
                }
            }
        }

        public string DeSerializeParameters(List<Parameter> oldParameters)
        {
            var ofd = new OpenFileDialog();
            string baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            ofd.InitialDirectory = baseDirectory;
            ofd.Filter = "XML files (.xml)|*.xml";

            if (ofd.ShowDialog() == true)
            {
                if (ofd.FileName == string.Empty)
                    return string.Empty;
                UndoRedoStack.Reset();
                using (var f = File.OpenRead(ofd.FileName))
                {
                    var loadedParameters = XmlSer.Deserialize(f) as ParameterSerializer;
                    foreach (var b in loadedParameters.BoolList)
                    {
                        var p = oldParameters.Find(x => x.ShaderName == b.Name);
                        if (p != null) p.DefaultValue = b.Value.ToString();
                    }
                    foreach (var i in loadedParameters.IntList)
                    {
                        var p = oldParameters.Find(x => x.ShaderName == i.Name);
                        if (p != null) p.DefaultValue = i.Value.ToString();
                    }
                    foreach (var fl in loadedParameters.FloatList)
                    {
                        var p = oldParameters.Find(x => x.ShaderName == fl.Name);
                        if (p != null) p.DefaultValue = fl.Value.ToString(CultureInfo.InvariantCulture);
                    }
                    foreach (var fl in loadedParameters.Float2List)
                    {
                        var p = oldParameters.Find(x => x.ShaderName == fl.Name);
                        if (p != null) p.DefaultValue = $"{fl.Value.X.ToString(CultureInfo.InvariantCulture)},{fl.Value.Y.ToString(CultureInfo.InvariantCulture)}";
                    }
                    foreach (var fl in loadedParameters.Float3List)
                    {
                        var p = oldParameters.Find(x => x.ShaderName == fl.Name);
                        if (p != null) p.DefaultValue = $"{fl.Value.X.ToString(CultureInfo.InvariantCulture)},{fl.Value.Y.ToString(CultureInfo.InvariantCulture)},{fl.Value.Z.ToString(CultureInfo.InvariantCulture)}";
                    }
                    foreach (var fl in loadedParameters.Float4List)
                    {
                        var p = oldParameters.Find(x => x.ShaderName == fl.Name);
                        if (p != null) p.DefaultValue = $"{fl.Value.X.ToString(CultureInfo.InvariantCulture)},{fl.Value.Y.ToString(CultureInfo.InvariantCulture)},{fl.Value.Z.ToString(CultureInfo.InvariantCulture)},{fl.Value.W.ToString(CultureInfo.InvariantCulture)}";
                    }
                    foreach (var t in loadedParameters.Texture2DList)
                    {
                        var p = oldParameters.Find(x => x.ShaderName == t.Name);
                        if (p != null) p.DefaultValue = t.Value;
                    }
                    
                    return loadedParameters.ShaderName;
                }
            }
            return string.Empty;
        }

        private void ClearLists()
        {
            BoolList.Clear();
            IntList.Clear();
            FloatList.Clear();
            Float2List.Clear();
            Float3List.Clear();
            Float4List.Clear();
        }
    }
}
