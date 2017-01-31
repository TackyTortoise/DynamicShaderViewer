float4x4 gWorld : WORLD;
float4x4 gWorldViewProj : WORLDVIEWPROJECTION; 
float4x4 gMatrixViewInverse : VIEWINVERSE;
float3 gLightDirection = float3(-0.577f, -0.577f, 0.577f);
float4 gDiffuseColor = float4(1,1,1,1);
float gCellWidth = .3f;

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

RasterizerState NoCulling
{
	CullMode = BACK;
};

RasterizerState FrontCulling
{
	CullMode = Front;
};

BlendState EnableBlending
{
	BlendEnable[0] = TRUE;
	SrcBlend = SRC_ALPHA;
	DestBlend = INV_SRC_ALPHA;
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
	output.normal = normalize(mul(input.normal, (float3x3)gWorld));
	// Step3:	Just copy the color
	//output.color = gDiffuseColor;
	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(VS_OUTPUT input) : SV_TARGET{

	float3 color_rgb = gDiffuseColor.rgb;
	float color_a = gDiffuseColor.a;

	float intensity = dot(normalize(-gLightDirection), input.normal);
	if (intensity < 0)
		intensity = 0;

	//clamp at different levels
	if (intensity > 0.95)
		color_rgb = color_rgb;
	else if (intensity > 0.5)
		color_rgb = .7f * color_rgb;
	else if (intensity > 0.05)
		color_rgb = .35f * color_rgb;
	else
		color_rgb = .1f * color_rgb;
	
	//HalfLambert Diffuse :)
	float diffuseStrength = dot(input.normal, -gLightDirection);
	diffuseStrength = diffuseStrength * 0.5 + 0.5;
	diffuseStrength = saturate(diffuseStrength);
	color_rgb = color_rgb * diffuseStrength;
	
	return float4( color_rgb , color_a );
}

VS_OUTPUT OutlineVertexShader(VS_INPUT input)
{
	VS_OUTPUT output = (VS_OUTPUT) 0;
 
    // Calculate where the vertex ought to be.  This line is equivalent
    // to the transformations in the CelVertexShader.
	float4 original = mul(float4(input.pos, 1.0f), gWorldViewProj);
 
    // Calculates the normal of the vertex like it ought to be.
	float4 normal = mul(float4(input.normal, 1.0f), gWorldViewProj);
 
    // Take the correct "original" location and translate the vertex a little
    // bit in the direction of the normal to draw a slightly expanded object.
    // Later, we will draw over most of this with the right color, except the expanded
    // part, which will leave the outline that we want.
	//output.pos = original + (mul(gCellWidth, float4(input.normal,1.f)));
	output.pos = mul(float4(input.pos + gCellWidth * input.normal, 1.f), gWorldViewProj);
 
	return output;
}

float4 OutlinePixelShader(VS_OUTPUT input) : SV_TARGET
{
	return float4(0,0,0,1);
}

//--------------------------------------------------------------------------------------
// Technique
//--------------------------------------------------------------------------------------
technique10 Default
{
	pass P0
	{
		SetRasterizerState(FrontCulling);
		SetDepthStencilState(EnableDepth, 0);
		SetVertexShader(CompileShader(vs_4_0, OutlineVertexShader()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, OutlinePixelShader()));
	}

    pass P1
    {
		SetRasterizerState(NoCulling);
		SetDepthStencilState(EnableDepth, 0);

        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}

