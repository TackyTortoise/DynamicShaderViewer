float4x4 gWorld : WORLD;
float4x4 gWorldViewProj : WORLDVIEWPROJECTION; 
float4x4 gMatrixViewInverse : VIEWINVERSE;
float3 gLightDirection = float3(-0.577f, -0.577f, 0.577f);
float4 gDiffuseColor = float4(1,1,1,1);

struct VS_INPUT{
	float3 pos : POSITION;
	float3 normal : NORMAL;
};
struct VS_OUTPUT{
	float4 pos : SV_POSITION;
	float3 normal : NORMAL;
};

DepthStencilState EnableDepth
{
	DepthEnable = TRUE;
	DepthWriteMask = ALL;
};

RasterizerState BackCulling
{
	CullMode = BACK;
};

VS_OUTPUT VS(VS_INPUT input){
	VS_OUTPUT output;
	// Step 1:	convert position into float4 and multiply with matWorldViewProj
	output.pos = mul ( float4(input.pos,1.0f), gWorldViewProj );
	// Step 2:	rotate the normal: NO TRANSLATION
	//			this is achieved by clipping the 4x4 to a 3x3 matrix, 
	//			thus removing the postion row of the matrix
	output.normal = normalize(mul(input.normal, (float3x3)gWorld));
	return output;
}

float4 PS(VS_OUTPUT input) : SV_TARGET{

	float3 color_rgb = gDiffuseColor.rgb;
	float color_a = gDiffuseColor.a;
	
	//HalfLambert Diffuse :)
	float diffuseStrength = dot(input.normal, -gLightDirection);
	diffuseStrength = saturate(diffuseStrength);
	color_rgb = color_rgb * diffuseStrength;
	
	return float4( color_rgb , color_a );
}

technique10 Default
{
pass P0
    {
		SetRasterizerState(BackCulling);
		SetDepthStencilState(EnableDepth, 0);

        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}