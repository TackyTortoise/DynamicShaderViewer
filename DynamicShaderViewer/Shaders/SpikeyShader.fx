//************
// VARIABLES *
//************
cbuffer cbPerObject
{
	float4x4 m_MatrixWorldViewProj : WORLDVIEWPROJECTION;
	float4x4 m_MatrixWorld : WORLD;
	float3 m_LightDir = {0.2f,-1.0f,0.2f};
}

RasterizerState FrontCulling 
{ 
	CullMode = NONE; 
};

SamplerState samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;// of Mirror of Clamp of Border
    AddressV = Wrap;// of Mirror of Clamp of Border
};

Texture2D m_TextureDiffuse;

//**********
// STRUCTS *
//**********
struct VS_DATA
{
	float3 Position : POSITION;
	float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD;
};

struct GS_DATA
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD0;
};

//****************
// VERTEX SHADER *
//****************
VS_DATA VS(VS_DATA vsData)
{
	//Step 1.
	//Delete this transformation code and just return the VS_DATA parameter (vsData)
	//Don't forget to change the return type!

	/*GS_DATA temp = (GS_DATA)0;
	temp.Position = mul(float4(vsData.Position,1),m_MatrixWorldViewProj);
	temp.Normal = mul(vsData.Normal, (float3x3)m_MatrixWorld);
	temp.TexCoord = vsData.TexCoord;*/

	return vsData;
}

//******************
// GEOMETRY SHADER *
//******************

void CreateVertex(inout TriangleStream<GS_DATA> triStream, float3 pos, float3 normal, float2 texCoord)
{
	//Step 1. Create a GS_DATA object
	GS_DATA output = (GS_DATA)0;
	//Step 2. Transform the position using the WVP Matrix and assign it to (GS_DATA object).Position (Keep in mind: float3 -> float4)
	output.Position = mul(float4(pos, 1), m_MatrixWorldViewProj);
	//Step 3. Transform the normal using the World Matrix and assign it to (GS_DATA object).Normal (Only Rotation, No translation!)
	output.Normal = mul(normal, (float3x3)m_MatrixWorld);
	//Step 4. Assign texCoord to (GS_DATA object).TexCoord
	output.TexCoord = texCoord;
	//Step 5. Append (GS_DATA object) to the TriangleStream parameter (TriangleStream::Append(...))
	triStream.Append(output);
}


[maxvertexcount(6)]
void SpikeGenerator(triangle VS_DATA vertices[3], inout TriangleStream<GS_DATA> triStream)
{
	//Use these variable names
	float3 basePoint, top, left, right, spikeNormal;

	//Step 1. Calculate The basePoint
	basePoint = (vertices[0].Position + vertices[1].Position + vertices[2].Position) / 3; 
	//Step 2. Calculate The normal of the basePoint
	float3 basePointNormal = (vertices[0].Normal + vertices[1].Normal + vertices[2].Normal) / 3;
	//Step 3. Calculate The Spike's Top Position
	top = basePoint + (8 * basePointNormal);
	//Step 4. Calculate The Left And Right Positions
	float3 spikeDirection = (vertices[2].Position - vertices[0].Position) * 0.1f;
	left = basePoint - spikeDirection;
	right = basePoint + spikeDirection;
	//Step 5. Calculate The Normal of the spike
	float3 leftTop = top - left;
	float3 rightTop = top - right;
	spikeNormal = cross(leftTop, rightTop);

	//Step 6. Create The Vertices [Complete code in CreateVertex(...)]
	
	//Create Existing Geometry
	CreateVertex(triStream,vertices[0].Position,vertices[0].Normal,vertices[0].TexCoord);
	CreateVertex(triStream,vertices[1].Position,vertices[1].Normal,vertices[1].TexCoord);
	CreateVertex(triStream,vertices[2].Position,vertices[2].Normal,vertices[2].TexCoord);

	//Restart the strip so we can add another (independent) triangle!
	triStream.RestartStrip();

	//Create Spike Geometry
	CreateVertex(triStream, top, spikeNormal, float2(0,0));
	CreateVertex(triStream, left, spikeNormal, float2(0,0));
	CreateVertex(triStream, right, spikeNormal, float2(0,0));
}

//***************
// PIXEL SHADER *
//***************
float4 MainPS(GS_DATA input) : SV_TARGET 
{
	input.Normal=-normalize(input.Normal);
	float alpha = m_TextureDiffuse.Sample(samLinear,input.TexCoord).a;
	float3 color = m_TextureDiffuse.Sample( samLinear,input.TexCoord ).rgb;
	float s = max(dot(m_LightDir,input.Normal), 0.4f);

	return float4(color*s,alpha);
}


//*************
// TECHNIQUES *
//*************
technique10 techSpikey 
{
	pass p0 
	{
		SetRasterizerState(FrontCulling);	
		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(CompileShader(gs_4_0, SpikeGenerator()));
		SetPixelShader(CompileShader(ps_4_0, MainPS()));
	}
}
/*
technique10 DefaultTechnique 
{
	pass p0 {
		SetRasterizerState(FrontCulling);	
		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, MainPS()));
	}
}*/