using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DynamicShaderViewer.ShaderParser;

namespace DynamicShaderViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Float3 f = new Float3("float3(1,2,3)");

            //var parser = new HLSLParser();
            //parser.ParseFile("./Shaders/UberShader.fx");
            //StringBuilder sb = new StringBuilder();
            //foreach (var parameter in parser.ParameterList)
            //{
            //    sb.Append(parameter);
            //    sb.Append("\n");
            //}
            //File.WriteAllText("output.txt", sb.ToString());
        }
    }
}
