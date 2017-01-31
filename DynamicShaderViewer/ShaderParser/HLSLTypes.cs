using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DynamicShaderViewer.Annotations;
using GalaSoft.MvvmLight;
using SharpDX.Direct3D9;

namespace DynamicShaderViewer.ShaderParser
{
    [Serializable]
    [XmlRoot (ElementName = "float2")]
    public struct Float2
    {
        public Float2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Float2(string dValue)
        {
            X = 0;
            Y = 0;
            SetDefaultValue(dValue);
        }

        [XmlAttribute (AttributeName = "xvalue")]
        public float X { get; set; }
        [XmlAttribute(AttributeName = "yvalue")]
        public float Y { get; set; }

        public void SetDefaultValue(string dValue)
        {
            string pattern = @"[-+]?[0-9]*\.?[0-9]+(?:f)?,[-+]?[0-9]*\.?[0-9]+(?:f)?";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(dValue);
            if (matches.Count == 0)
            {
                throw new ArgumentException("Default value of Float2 is incorrect");
            }

            string[] v = matches[0].ToString().Split(',');
            X = float.Parse(v[0], CultureInfo.InvariantCulture);
            Y = float.Parse(v[1], CultureInfo.InvariantCulture);
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "float3")]
    public struct Float3
    {
        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Float3(string dValue)
        {
            X = 0;
            Y = 0;
            Z = 0;
            SetDefaultValue(dValue);
        }
        [XmlAttribute(AttributeName = "xvalue")]
        public float X { get; set; }
        [XmlAttribute(AttributeName = "yvalue")]
        public float Y { get; set; }
        [XmlAttribute(AttributeName = "zvalue")]
        public float Z { get; set; }
        public void SetDefaultValue(string dValue)
        {
            string pattern = @"[-+]?[0-9]*\.?[0-9]+(?:f)?,[-+]?[0-9]*\.?[0-9]+(?:f)?,[-+]?[0-9]*\.?[0-9]+(?:f)?";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(dValue);
            if (matches.Count == 0)
            {
                throw new ArgumentException("Default value of Float3 is incorrect");
            }

            string[] v = matches[0].ToString().Split(',');

            /*rgx = new Regex("f");
            for (int i = 0; i < v.Length; ++i)
            {
                v[i] = rgx.Replace(v[i], "");
            }*/

            X = float.Parse(v[0], CultureInfo.InvariantCulture);
            Y = float.Parse(v[1], CultureInfo.InvariantCulture);
            Z = float.Parse(v[2], CultureInfo.InvariantCulture);
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "float3")]
    public struct Float4
    {
        public Float4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Float4(string dValue)
        {
            X = 0;
            Y = 0;
            Z = 0;
            W = 0;
            SetDefaultValue(dValue);
        }
        [XmlAttribute(AttributeName = "xvalue")]
        public float X { get; set; }
        [XmlAttribute(AttributeName = "yvalue")]
        public float Y { get; set; }
        [XmlAttribute(AttributeName = "zvalue")]
        public float Z { get; set; }
        [XmlAttribute(AttributeName = "wvalue")]
        public float W { get; set; }

        public void SetDefaultValue(string dValue)
        {
            string pattern = @"[-+]?[0-9]*\.?[0-9]+(?:f)?,[-+]?[0-9]*\.?[0-9]+(?:f)?,[-+]?[0-9]*\.?[0-9]+(?:f)?,[-+]?[0-9]*\.?[0-9]+(?:f)?";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(dValue);
            if (matches.Count == 0)
            {
                throw new ArgumentException("Default value of Float4 is incorrect");
            }

            string[] v = matches[0].ToString().Split(',');

            /*rgx = new Regex("f");
            for (int i = 0; i < v.Length; ++i)
            {
                v[i] = rgx.Replace(v[i], "");
            }*/

            X = float.Parse(v[0], CultureInfo.InvariantCulture);
            Y = float.Parse(v[1], CultureInfo.InvariantCulture);
            Z = float.Parse(v[2], CultureInfo.InvariantCulture);
            W = float.Parse(v[3], CultureInfo.InvariantCulture);
        }
    }
}
