// -----------------------------------------------------------------------------
// A shader which wraps an input texture using four selected points.
// -----------------------------------------------------------------------------

PROTOGAME_DECLARE_TEXTURE(Texture);

float4x4 World;
float4x4 View;
float4x4 Projection;

float2 TopLeft;
float2 TopRight;
float2 BottomLeft;
float2 BottomRight;
float Alpha;

struct VertexShaderInput
{
	float4 Position : PROTOGAME_POSITION;
	float2 TexCoord : PROTOGAME_TEXCOORD(0);
};

struct VertexShaderOutput
{
	float4 Position : PROTOGAME_POSITION;
	float2 TexCoord : PROTOGAME_TEXCOORD(0);
};

struct PixelShaderOutput
{
	float4 Color : PROTOGAME_TARGET(0);
};

VertexShaderOutput DefaultVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.TexCoord = input.TexCoord;

	return output;
}

PixelShaderOutput DefaultPixelShader(VertexShaderOutput input)
{
	PixelShaderOutput output;

	// Warp the input position.
	float2 inputPosition;

	float2 fromTopLeftToTopRight = TopRight - TopLeft;
	float2 fromBottomLeftToBottomRight = BottomRight - BottomLeft;

	float2 topAnchor = TopLeft + fromTopLeftToTopRight * input.TexCoord.x;
	float2 bottomAnchor = BottomLeft + fromBottomLeftToBottomRight * input.TexCoord.x;
	float2 warpedPosition = lerp(
		topAnchor,
		bottomAnchor,
		input.TexCoord.y);

	inputPosition = warpedPosition;

	output.Color = PROTOGAME_SAMPLE_TEXTURE(Texture, inputPosition);
	output.Color *= Alpha;

	return output;
}

technique
{
	pass
	{
		VertexShader = compile PROTOGAME_VERTEX_HIGH_SHADER DefaultVertexShader();
		PixelShader = compile PROTOGAME_PIXEL_HIGH_SHADER DefaultPixelShader();
	}
}