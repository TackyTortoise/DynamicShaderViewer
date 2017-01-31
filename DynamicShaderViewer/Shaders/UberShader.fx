

//GLOBAL MATRICES
//***************
// The World View Projection Matrix
float4x4 m_MatrixWorldViewProj : WORLDVIEWPROJECTION;
// The ViewInverse Matrix - the third row contains the camera position!
float4x4 m_MatrixViewInv : VIEWINVERSE;
// The World Matrix
float4x4 m_MatrixWorld : WORLD;

//STATES
//******
RasterizerState BackCulling 
{ 
	CullMode = BACK; 
};

BlendState EnableBlending 
{     
	BlendEnable[0] = TRUE;
	SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
};

//SAMPLER STATES
//**************
SamplerState m_TextureSampler
{
	Filter = MIN_MAG_MIP_LINEAR;
 	AddressU = WRAP;
	AddressV = WRAP;
	AddressW = WRAP;
};

//LIGHT
//*****
float3 gLightDirection = float3(0.577f, 0.577f, 0.577f);

//DIFFUSE
//*******
bool m_bDiffuseTexture = false;

float4 m_ColorDiffuse = float4(1,1,1,1);

Texture2D m_TextureDiffuse;

//SPECULAR
//********
float4 m_ColorSpecular = float4(1,1,1,1);

Texture2D m_TextureSpecularLevel;

bool m_bSpecularLevelTexture = false;

int m_Shininess = 15;

//AMBIENT
//*******
float4 m_ColorAmbient = float4(0,0,0,1);

float m_AmbientIntensity = 0.0f;

//NORMAL MAPPING
//**************
bool m_FlipGreenChannel = false;

bool m_bNormalMapping = false;

Texture2D m_TextureNormal;
//ENVIRONMENT MAPPING
//*******************
TextureCube m_CubeEnvironment;

bool m_bEnvironmentMapping = false;

float m_ReflectionStrength = 0.0f;

float m_RefractionStrength = 0.0f;

float m_RefractionIndex = 0.3f;

//OPACITY
//***************
float m_Opacity = 1.0f;

bool m_bOpacityMap = false;

Texture2D m_TextureOpacity;


//SPECULAR MODELS
//***************
bool m_SpecularBlinn = false;

bool m_SpecularPhong = false;

//FRESNEL FALLOFF
//***************
bool m_bFresnelFallOff = false;


float4 m_ColorFresnel = float4(1,1,1,1);

float m_FresnelPower = 1.0f;

float m_FresnelMultiplier = 1.0;

float m_FresnelHardness = 0;

//VS IN & OUT
//***********
struct VS_Input
{
	float3 Position: POSITION;
	float3 Normal: NORMAL;
	float3 Tangent: TANGENT;
	float2 TexCoord: TEXCOORD0;
};

struct VS_Output
{
	float4 Position: SV_POSITION;
	float4 WorldPosition: COLOR0;
	float3 Normal: NORMAL;
	float3 Tangent: TANGENT;
	float2 TexCoord: TEXCOORD0;
};

float3 CalculateSpecularBlinn(float3 viewDirection, float3 normal, float2 texCoord)
{
	float3 hn = normalize(viewDirection + gLightDirection);
	float specularStrength = dot(-normal, hn);
	specularStrength = saturate(specularStrength);
	specularStrength = pow(specularStrength,m_Shininess);
	float3 specColor =  m_ColorSpecular.xyz * specularStrength;
	
	//Use a Texture to control the specular level?
	if(m_bSpecularLevelTexture)
		specColor*= m_TextureSpecularLevel.Sample(m_TextureSampler, texCoord).r;
		
	return specColor;
}

float3 CalculateSpecularPhong(float3 viewDirection, float3 normal, float2 texCoord)
{
	float3 reflectedVector = reflect(gLightDirection, normal);
	float specularStrength = dot(-viewDirection, reflectedVector);
	specularStrength = saturate(specularStrength);
	specularStrength = pow(specularStrength,m_Shininess);
	float3 specColor =  m_ColorSpecular.xyz * specularStrength;
	
	//Use a Texture to control the specular level?
	if(m_bSpecularLevelTexture)
		specColor*= m_TextureSpecularLevel.Sample(m_TextureSampler, texCoord).r;
		
	return specColor;
}

float3 CalculateSpecular(float3 viewDirection, float3 normal, float2 texCoord)
{
	float3 specColor = float3(0,0,0);
	
	if (m_SpecularBlinn)
		specColor += CalculateSpecularBlinn(viewDirection, normal, texCoord);
		
	if (m_SpecularPhong)
		specColor += CalculateSpecularPhong(viewDirection, normal, texCoord);
				
	return specColor;
}

float3 CalculateNormal(float3 tangent, float3 normal, float2 texCoord)
{
	float3 newNormal = normal;
	
	if(m_bNormalMapping)
	{
		float3 binormal = cross(tangent, normal);
		binormal = normalize(binormal);
		if (m_FlipGreenChannel)
		{
			binormal = -binormal;
		}
		float3x3 localAxis = float3x3(tangent, binormal, normal);
	
		float3 sampledNormal = 2.0f * m_TextureNormal.Sample(m_TextureSampler,texCoord).rgb - 1.0f;
		newNormal =  mul(sampledNormal, localAxis);
	}
	
	return newNormal;
}

float3 CalculateDiffuse(float3 normal, float2 texCoord)
{
	float3 diffColor = m_ColorDiffuse.rgb;
	
	float diffuseStrength = dot(-normal, gLightDirection);
	diffuseStrength = saturate(diffuseStrength);
	diffColor *= diffuseStrength;
	
	//Use a texture to define the diffuse color?
	if(m_bDiffuseTexture)
		diffColor *= m_TextureDiffuse.Sample(m_TextureSampler, texCoord).rgb;
		
	return diffColor;
}

float3 CalculateFresnelFalloff(float3 normal, float3 viewDirection, float3 environmentColor)
{
	if(m_bFresnelFallOff)
	{
		float fresnel = saturate(abs(dot(normal,viewDirection)));
		fresnel = 1 - fresnel;
		fresnel = pow(fresnel,m_FresnelPower);
		fresnel = saturate(fresnel)*m_FresnelMultiplier;
		
		float fresnelMask =1.0f;
		fresnelMask = 1 - saturate(dot(float3(0.0,1.0,0.0),normal));
		fresnelMask = pow(fresnelMask, m_FresnelHardness);
		fresnel *= fresnelMask;
		
		if(m_bEnvironmentMapping)
			return environmentColor.rgb*fresnel;
		else
			return m_ColorFresnel.rgb*fresnel;
	}
	
	if(m_bEnvironmentMapping)
		return environmentColor;
	
	return float3(0,0,0);
}

float3 CalculateEnvironment(float3 viewDirection, float3 normal)
{
	float3 environmentColor = float3(0,0,0);
	if(m_bEnvironmentMapping)
	{
		float3 reflectedVector = reflect(viewDirection,normal);
		float3 refractedVector = refract(viewDirection,normal,m_RefractionIndex);
		environmentColor = (m_CubeEnvironment.Sample(m_TextureSampler,reflectedVector).rgb * m_ReflectionStrength)
							+(m_CubeEnvironment.Sample(m_TextureSampler,refractedVector).rgb * m_RefractionStrength);	
	}	
	
	return environmentColor;
}

float CalculateOpacity(float2 texCoord)
{
	float opacity = m_Opacity;
	if (m_bOpacityMap)
		opacity *= m_TextureOpacity.Sample(m_TextureSampler, texCoord).r;
	
	return opacity;
}

// The main vertex shader
VS_Output VS(VS_Input input) {
	
	VS_Output output = (VS_Output)0;
	
	output.Position = mul(float4(input.Position, 1.0), m_MatrixWorldViewProj);
	output.WorldPosition = mul(float4(input.Position,1.0), m_MatrixWorld);
	output.Normal = mul(input.Normal, (float3x3)m_MatrixWorld);
	output.Tangent = mul(input.Tangent, (float3x3)m_MatrixWorld);
	output.TexCoord = input.TexCoord;
	
	return output;
}

// The main pixel shader
float4 MainPS(VS_Output input) : SV_TARGET {
	// NORMALIZE
	input.Normal = normalize(input.Normal);
	input.Tangent = normalize(input.Tangent);
	
	float3 viewDirection = normalize(input.WorldPosition.xyz - m_MatrixViewInv[3].xyz);
	
	//NORMAL
	float3 newNormal = CalculateNormal(input.Tangent, input.Normal, input.TexCoord);
		
	//SPECULAR
	float3 specColor = CalculateSpecular(viewDirection, newNormal, input.TexCoord);
		
	//DIFFUSE
	float3 diffColor = CalculateDiffuse(newNormal, input.TexCoord);
		
	//AMBIENT
	float3 ambientColor = m_ColorAmbient.rgb * m_AmbientIntensity;
	
	//ENVIRONMENT MAPPING
	float3 environmentColor = CalculateEnvironment(viewDirection, newNormal);
	
	//FRESNEL FALLOFF
	environmentColor = CalculateFresnelFalloff(newNormal, viewDirection, environmentColor);
		
	//FINAL COLOR CALCULATION
	float3 finalColor = diffColor + specColor + environmentColor + ambientColor;
	
	//OPACITY
	float opacity = CalculateOpacity(input.TexCoord);
	
	return float4(finalColor,opacity);
}

// Default Technique
/*technique10 WithAlphaBlending {
	pass p0 {
		SetRasterizerState(BackCulling);
		SetBlendState(EnableBlending,float4(0.0f, 0.0f, 0.0f, 0.0f), 0xFFFFFFFF);
		SetVertexShader(CompileShader(vs_4_0, MainVS()));
		SetGeometryShader( NULL );
		SetPixelShader(CompileShader(ps_4_0, MainPS()));
	}
}*/

// Default Technique
technique10 WithoutAlphaBlending {
	pass p0 {
		SetRasterizerState(BackCulling);
		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader( NULL );
		SetPixelShader(CompileShader(ps_4_0, MainPS()));
	}
}

