using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Assimp;
using DynamicShaderViewer.Helper;
using DynamicShaderViewer.ShaderParser;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;
using Buffer = SharpDX.Direct3D10.Buffer;

namespace DynamicShaderViewer.ShaderInfo
{
    public class AssimpModel
    {
        public PrimitiveTopology PrimitiveTopology { get; set; }
        public int VertexStride { get; set; }
        public int IndexCount { get; set; }
        public Buffer IndexBuffer { get; set; }
        public Buffer VertexBuffer { get; set; }

        private string _fileName;
        public AssimpModel(string fileName)
        {
            _fileName = fileName;
        }
        public void Create(Device1 device, GenericShader effect)
        {
            PrimitiveTopology = PrimitiveTopology.TriangleList;
            VertexStride = effect.VertexStride;


            List<int> indices = new List<int>();

            var importer = new AssimpContext();
            Scene scene = importer.ImportFile(_fileName, PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.Triangulate);
            
            if (!importer.IsImportFormatSupported(Path.GetExtension(_fileName)))
            {
                throw new Exception("File Format not supported");
            }

            long vertCount = 0;
            foreach (var model in scene.Meshes)
            {
                vertCount += model.VertexCount;
            }
            var verts = new float[VertexStride * vertCount];

            int meshOffset = 0;
            foreach (var model in scene.Meshes)
            {
                for (var i = 0; i < model.VertexCount; ++i)
                {
                    var pos = model.Vertices[i];
                    var nor = model.Normals[i];
                    var uv = model.TextureCoordinateChannels[0][i];
                    uv.Y = -uv.Y;
                    var col = model.HasVertexColors(0) ? model.VertexColorChannels[0][i] : new Color4D(1, 0, 1);
                    var tan = model.Tangents[i];

                    var inputOffset = 0;
                    foreach (var inputParam in effect.InputParameters)
                    {
                        if (inputParam.SemanticName == "POSITION")
                        {
                            Array.Copy(pos.ToArray(), 0, verts, i * (VertexStride / sizeof(float)) + inputOffset + meshOffset, 3);
                            inputOffset += 3;
                        }
                        else if (inputParam.SemanticName == "NORMAL")
                        {
                            Array.Copy(nor.ToArray(), 0, verts, i * (VertexStride / sizeof(float)) + inputOffset + meshOffset, 3);
                            inputOffset += 3;
                        }
                        else if (inputParam.SemanticName == "COLOR")
                        {
                            Array.Copy(col.ToArray(), 0, verts, i * (VertexStride / sizeof(float)) + inputOffset + meshOffset, 4);
                            inputOffset += 4;
                        }
                        else if (inputParam.SemanticName == "TEXCOORD" || inputParam.SemanticName == "TEXCOORD0")
                        {
                            Array.Copy(uv.ToArray(), 0, verts, i * (VertexStride / sizeof(float)) + inputOffset + meshOffset, 2);
                            inputOffset += 2;
                        }
                        else if (inputParam.SemanticName == "TANGENT")
                        {
                            Array.Copy(tan.ToArray(), 0, verts, i * (VertexStride / sizeof(float)) + inputOffset + meshOffset, 3);
                            inputOffset += 3;
                        }
                        else
                        {
                            MessageBox.Show("AssimpModel::Create() > Unsupported Semantic type! ({inputParam.SemanticName})");
                        }
                    }
                }

                meshOffset += model.VertexCount * VertexStride;

                indices.AddRange(model.GetIndices().ToList());
            }

            IndexCount = indices.Count;

            BufferDescription bd = new BufferDescription(
                (int)(verts.Length),
                ResourceUsage.Immutable,
                BindFlags.VertexBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None);
            VertexBuffer = new Buffer(device, DataStream.Create(verts, false, false), bd);

            bd = new BufferDescription(
                sizeof(int) * IndexCount,
                ResourceUsage.Immutable,
                BindFlags.IndexBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None);
            IndexBuffer = new Buffer(device, DataStream.Create(indices.ToArray(), false, false), bd);
        }
    }
}
