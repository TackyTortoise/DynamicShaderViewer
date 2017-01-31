float4x4 gWorld;
float4x4 gWorldViewProj; 

float3 gLightDirection = float3(-0.577f, -0.577f, 0.577f);

struct VS_INPUT{
	float3 pos : POSITION;
	float4 color : COLOR;
	float3 normal : NORMAL;
};
struct VS_OUTPUT{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
	float3 normal : NORMAL;
};

RasterizerState Solid
{
	FillMode = SOLID;
	CullMode = NONE;
};


//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_OUTPUT VS(VS_INPUT input){
	VS_OUTPUT output;
	// Step 1:	convert position into float4 and multiply with matWorldViewProj
	output.pos = mul ( float4(input.pos,1.0f), gWorldViewProj );
	// Step 2:	rotate the normal: NO TRANSLATION
	//			this is achieved by clipping the 4x4 to a 3x3 matrix, 
	//			thus removing the postion row of the matrix
	output.normal = mul(normalize(input.normal).xyz, (float3x3)gWorld);
	// Step3:	Just copy the color
	output.color=input.color;
	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(VS_OUTPUT input) : SV_TARGET{

	//calculate the  fraction of the color
	//lightdir is opposite to normal so multiply by -1
	//use max to prevent a negative color value
	//split into rgb and a to prevent transparency by dot product
	float3 color_rgb= input.color.rgb;
	float color_a = input.color.a;
	//use dot product to calc part of color
	color_rgb = color_rgb * saturate(dot(input.normal, -gLightDirection ));
	//join rgb and a to fill output struct
	
	return float4( color_rgb , color_a );
}

//--------------------------------------------------------------------------------------
// Technique
//--------------------------------------------------------------------------------------
technique10 PosCol3DTech
{
    pass P0
    {
		SetRasterizerState(Solid);
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}

