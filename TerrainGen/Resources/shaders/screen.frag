#version 330 core
out vec4 FragColor;
  
in vec2 TexCoords;

uniform sampler2DMS screenColor;
uniform sampler2DMS screenUi;
uniform int width;
uniform int height;
uniform int samples;
uniform int samplesUi;

vec4 mtexture(sampler2DMS s, vec2 coords, int samp)
{
	ivec2 vpCoords = ivec2(width, height);
	vpCoords.x = int(vpCoords.x * coords.x);
	vpCoords.y = int(vpCoords.y * coords.y);

	vec4 avg = vec4(0);
	for (int i = 0; i < samp; i++)
	{
		avg += texelFetch(s, vpCoords, i);
	}
	return avg / float(samp);
}

float linearDepth(float depthSample)
{
	const float zNear = 1;
	const float zFar = 1024;
    depthSample = 2.0 * depthSample - 1.0;
    float zLinear = 2.0 * zNear * zFar / (zFar + zNear - depthSample * (zFar - zNear));
    return zLinear;
}

void main()
{ 
	vec4 color = mtexture(screenColor, TexCoords, samples);
	vec4 ui = mtexture(screenUi, TexCoords, samplesUi);

	FragColor = ui + (1 - ui.a) * color;
}