using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpDX.Direct3D10;

namespace DynamicShaderViewer.ShaderParser
{
    

    public class HLSLParser
    {
        public List<Parameter> ParameterList = new List<Parameter>();

        public bool ParseFile(string path)
        {
            ParameterList.Clear();

            var rgx = new Regex("f");

            var reader = File.OpenText(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Replace("\t", "");
                string[] items = line.Split(' ');
                var itemIndex = 0;
                if (items.Contains("struct"))
                    break;
                foreach (var item in items)
                {
                    string dValue = "";

                    if (items.Length > 3)
                    {
                        for (int i = 3; i < items.Length; ++i)
                            dValue += items[i];
                        if (dValue.Length > 1)
                            dValue = dValue.Substring(0, dValue.Length - 1);
                    }

                    if (item.Equals("int"))
                    {
                        ParameterList.Add(new Parameter(typeof(int), items[itemIndex + 1], dValue));
                    }

                    //Types of Floats
                    if (item.Equals("float"))
                    {
                        dValue = rgx.Replace(dValue, "");
                        ParameterList.Add(new Parameter(typeof(float), items[itemIndex + 1], dValue));
                    }

                    else if (item.Equals("float2"))
                    {
                        dValue = rgx.Replace(dValue, "");
                        ParameterList.Add(new Parameter(typeof(Float2), items[itemIndex + 1], dValue));
                    }

                    else if (item.Equals("float3"))
                    {
                        dValue = rgx.Replace(dValue, "");
                        ParameterList.Add(new Parameter(typeof(Float3), items[itemIndex + 1], dValue));
                    }

                    else if (item.Equals("float4"))
                    {
                        dValue = rgx.Replace(dValue, "");
                        ParameterList.Add(new Parameter(typeof(Float4), items[itemIndex + 1], dValue));
                    }

                    else if (item.Equals("bool"))
                    {
                        ParameterList.Add(new Parameter(typeof(bool), items[itemIndex + 1], dValue));
                    }

                    //Textures
                    else if (item.Equals("Texture2D"))
                    {
                        ParameterList.Add(new Parameter(typeof(Texture2D), items[itemIndex + 1], dValue));
                    }

                    ++itemIndex;
                }
            }

            ParameterList = ParameterList.OrderBy((x) => x.Type.ToString()).ToList();

            if (ParameterList.Count > 0)
                return true;
            else
                return false;
        }
    }
}
