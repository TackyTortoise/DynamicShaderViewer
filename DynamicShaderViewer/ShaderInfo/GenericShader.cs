using System;
using System.CodeDom;
using System.Collections.Generic;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using Format = SharpDX.DXGI.Format;

namespace DynamicShaderViewer.ShaderInfo
{
    public class GenericShader : IEffect
    {
        public string FxFileName;
        public EffectTechnique Technique { get; set; }
        public Effect Effect { get; set; }
        public InputLayout InputLayout { get; set; }

        public InputElement[] InputParameters { get; set; }
        public int VertexStride;

        public GenericShader(string fxFileName)
        {
            FxFileName = fxFileName;
        }
        public void Create(Device1 device)
        {
            CompilationResult shaderByteCode = ShaderBytecode.CompileFromFile(FxFileName, "fx_4_0", ShaderFlags.None);
            if (shaderByteCode.Bytecode == null)
            {
                throw new Exception($"Failed to compile shader, message: {shaderByteCode.Message}");
            }
            InputParameters = CreateInputLayout(device);
            Effect = new Effect(device, shaderByteCode);
            Technique = Effect.GetTechniqueByIndex(0);

            var pass = Technique.GetPassByIndex(0);
            InputLayout = new InputLayout(device, pass.Description.Signature, InputParameters);
        }

        public void SetWorld(Matrix world)
        {
            try
            {
                var w = Effect?.GetVariableByName("gWorld");
                if (w.IsValid)
                    w.AsMatrix().SetMatrix(world);
                else Effect?.GetVariableBySemantic("WORLD").AsMatrix().SetMatrix(world);
            }
            catch (Exception ex)
            {
                throw new Exception("No world matrix found in shader! Please use name \"gWorld\" or semantic \"WORLD\"");
            }
        }

        public void SetWorldViewProjection(Matrix wvp)
        {
            try
            {
                var w = Effect?.GetVariableByName("gWorldViewProj");
                if (w.IsValid)
                    w.AsMatrix().SetMatrix(wvp);
                else Effect?.GetVariableBySemantic("WORLDVIEWPROJECTION").AsMatrix().SetMatrix(wvp);
            }
            catch (Exception ex)
            {
                throw new Exception("No worldviewprojection matrix found in shader! Please use name \"gWorldViewProj\" or semantic \"WORLDVIEWPROJECTION\"");
            }
        }

        public void SetLightDirection(Vector3 dir)
        {
            var l = Effect?.GetVariableByName("gLightDirection");
            l?.AsMatrix().SetMatrix(dir);
        }

        public InputElement[] CreateInputLayout(Device1 device)
        {
            VertexStride = 0;
            //create input layout
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile(FxFileName, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            if (vertexShaderByteCode.Bytecode == null)
                throw new Exception("Could not find vertex shader, please rename it to \"VS\"");
            var vs = new VertexShader(device, vertexShaderByteCode);

            List<InputElement> inputElements = new List<InputElement>();
            ShaderReflection r = new ShaderReflection(vertexShaderByteCode);
            var d = r.Description;
            var ip = d.InputParameters;
            for (int i = 0; i < ip; ++i)
            {
                var ipd = r.GetInputParameterDescription(i);
                Format frmt;
                //dxgi format
                if ((ipd.UsageMask & RegisterComponentMaskFlags.ComponentX) != 0)
                {
                    if ((ipd.UsageMask & RegisterComponentMaskFlags.ComponentY) != 0)
                    {
                        if ((ipd.UsageMask & RegisterComponentMaskFlags.ComponentZ) != 0)
                        {
                            if ((ipd.UsageMask & RegisterComponentMaskFlags.ComponentW) != 0)
                            {
                                switch (ipd.ComponentType)
                                {
                                    case RegisterComponentType.UInt32:
                                        frmt = Format.R32G32B32A32_UInt;
                                        VertexStride += 4 * sizeof(uint);
                                        break;
                                    case RegisterComponentType.SInt32:
                                        frmt = Format.R32G32B32A32_SInt;
                                        VertexStride += 4 * sizeof(int);
                                        break;
                                    case RegisterComponentType.Float32:
                                        frmt = Format.R32G32B32A32_Float;
                                        VertexStride += 4 * sizeof(float);
                                        break;
                                    default:
                                        throw new Exception("Invalid Component Type when creating input layout");
                                }
                            }
                            else
                            {
                                switch (ipd.ComponentType)
                                {
                                    case RegisterComponentType.UInt32:
                                        frmt = Format.R32G32B32_UInt;
                                        VertexStride += 3 * sizeof(uint);
                                        break;
                                    case RegisterComponentType.SInt32:
                                        frmt = Format.R32G32B32_SInt;
                                        VertexStride += 3 * sizeof(int);
                                        break;
                                    case RegisterComponentType.Float32:
                                        frmt = Format.R32G32B32_Float;
                                        VertexStride += 3 * sizeof(float);
                                        break;
                                    default:
                                        throw new Exception("Invalid Component Type when creating input layout");
                                }
                            }
                        }
                        else
                        {
                            switch (ipd.ComponentType)
                            {
                                case RegisterComponentType.UInt32:
                                    frmt = Format.R32G32_UInt;
                                    VertexStride += 2 * sizeof(uint);
                                    break;
                                case RegisterComponentType.SInt32:
                                    frmt = Format.R32G32_SInt;
                                    VertexStride += 2 * sizeof(int);
                                    break;
                                case RegisterComponentType.Float32:
                                    frmt = Format.R32G32_Float;
                                    VertexStride += 2 * sizeof(float);
                                    break;
                                default:
                                    throw new Exception("Invalid Component Type when creating input layout");
                            }
                        }
                    }
                    else
                    {
                        switch (ipd.ComponentType)
                        {
                            case RegisterComponentType.UInt32:
                                frmt = Format.R32_UInt;
                                VertexStride += sizeof(uint);
                                break;
                            case RegisterComponentType.SInt32:
                                frmt = Format.R32_SInt;
                                VertexStride += sizeof(int);
                                break;
                            case RegisterComponentType.Float32:
                                frmt = Format.R32_Float;
                                VertexStride += sizeof(float);
                                break;
                            default:
                                throw new Exception("Invalid Component Type when creating input layout");
                        }
                    }
                }
                else
                {
                    throw new Exception("Invalid format for input parameter");
                }
                inputElements.Add(new InputElement(ipd.SemanticName, ipd.SemanticIndex, frmt, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0));
            }
            return inputElements.ToArray();
        }
    }
}
