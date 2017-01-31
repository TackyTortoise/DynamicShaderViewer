using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicShaderViewer.ShaderParser
{
    public class Parameter
    {
        public Parameter(Type type, string name, string value = "")
        {
            Type = type;
            ShaderName = name.Last() == ';' ? name.Substring(0, name.Length - 1) : name;
            DefaultValue = value;
        }

        public Type Type;
        public String ShaderName;
        public string DefaultValue;

        public override string ToString()
        {
            return $"{Type} {ShaderName} = {DefaultValue}";
        }
    }
}
