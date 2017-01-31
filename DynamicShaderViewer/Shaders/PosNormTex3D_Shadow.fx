float4x4 gWorld : WORLD;
float4x4 gWorldViewProj : WORLDVIEWPROJECTION; 
float4x4 gLightViewProj;
float3 gLightDirection = float3(-0.577f, -0.577f, 0.577f);
float2 shadowMapSize = float2(1080, 720);
float gshadowMapBias = .0002f;

Texture2D gDiffuseMap;
Texture2D gShadowMap;

SamplerState samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;// or Mirror or Clamp or Border
    AddressV = Wrap;// or Mirror or Clamp or Border
};

/*SamplerState pointSampler
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};*/

SamplerComparisonState cmpSampler
{
   // sampler state
   Filter = COMPARISON_MIN_MAG_MIP_LINEAR;
   AddressU = MIRROR;
   AddressV = MIRROR;
 
   // sampler comparison state
   ComparisonFunc = LESS_EQUAL;
};

RasterizerState Solid
{
	FillMode = SOLID;
	CullMode = FRONT;
};

struct VS_INPUT{
	float3 pos : POSITION;
	float3 normal : NORMAL;
	float2 texCoord : TEXCOORD;
};
struct VS_OUTPUT{
	float4 pos : SV_POSITION;
	float3 normal : NORMAL;
	float2 texCoord : TEXCOORD;
	float4 lpos : TEXCOORD1;
};

DepthStencilState EnableDepth
{
	DepthEnable = TRUE;
	DepthWriteMask = ALL;
};

RasterizerState NoCulling
{
	CullMode = NONE;
	//FillMode = WIREFRAME;
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
	output.texCoord = input.texCoord;
	
	// storing worldspace projected to light clipping space
	output.lpos = mul(float4(input.pos, 1.0f), mul(gWorld, gLightViewProj));
	
	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float2 texOffset( int u, int v )
{
    return float2( u * 1.0f/shadowMapSize.x, v * 1.0f/shadowMapSize.y );
}

float4 PS(VS_OUTPUT input) : SV_TARGET
{
	input.lpos.xyz /= input.lpos.w;
	
	//float shadowMultiplier = 1.f;
	
	if( input.lpos.x < -1.0f || input.lpos.x > 1.0f ||
        input.lpos.y < -1.0f || input.lpos.y > 1.0f ||
        input.lpos.z < 0.0f  || input.lpos.z > 1.0f ) return float4(1,0,1,1); // return ambient out of light frustrum
		
	//clip space to texture space
	input.lpos.x = input.lpos.x/2 + 0.5;
    input.lpos.y = input.lpos.y/-2 + 0.5;
	
	//shadowMapBias	
	input.lpos.z -= gshadowMapBias; 
	
	//PCF sampling for shadow map
    float sum = 0;
	
	//perform PCF filtering on a 4 x 4 texel neighborhood
    for (float y = -1.5; y <= 1.5; y += 1.0)
    {
        for (float x = -1.5; x <= 1.5; x += 1.0)
        {
            sum += gShadowMap.SampleCmpLevelZero( cmpSampler, input.lpos.xy + texOffset(x,y), input.lpos.z );
        }
    }
	
	float shadowFactor = sum / 16.0;
	
	//"seetrough"
	if (shadowFactor < .5f) shadowFactor = .5f;
	
	//float shadowFactor = gShadowMap.SampleCmpLevelZero( cmpSampler, input.lpos.xy, input.lpos.z );
	
	//float shadowMapDepth = gShadowMap.Sample(pointSampler, input.lpos.xy).r;
	//if ( shadowMapDepth < input.lpos.z) shadowMultiplier = .5f;

	float4 diffuseColor = gDiffuseMap.Sample( samLinear,input.texCoord );
	float3 color_rgb= diffuseColor.rgb;
	float color_a = diffuseColor.a;
	
	//HalfLambert Diffuse :)
	float diffuseStrength = dot(input.normal, -gLightDirection);
	diffuseStrength = diffuseStrength * 0.5 + 0.5;
	diffuseStrength = saturate(diffuseStrength);
	color_rgb = color_rgb * diffuseStrength;// * shadowMultiplier;
	color_rgb *= shadowFactor;
	return float4( color_rgb , color_a );
}

//--------------------------------------------------------------------------------------
// Technique
//--------------------------------------------------------------------------------------
technique10 Default
{
    pass P0
    {
		SetRasterizerState(NoCulling);
		SetDepthStencilState(EnableDepth, 0);

		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}

