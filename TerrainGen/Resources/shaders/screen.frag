#version 330 core
out vec4 FragColor;
  
in vec2 TexCoords;

uniform sampler2DMS screenColor;
uniform sampler2DMS screenDepth;
uniform sampler2D random;
uniform int width;
uniform int height;

vec4 mtexture(sampler2DMS s, vec2 coords)
{
	ivec2 vpCoords = ivec2(width, height);
	vpCoords.x = int(vpCoords.x * coords.x); 
	vpCoords.y = int(vpCoords.y * coords.y);

	vec4 sample1 = texelFetch(s, vpCoords, 0);
	vec4 sample2 = texelFetch(s, vpCoords, 1);
	vec4 sample3 = texelFetch(s, vpCoords, 2);
	vec4 sample4 = texelFetch(s, vpCoords, 3);
	return (sample1 + sample2 + sample3 + sample4) / 4.0f;
}

vec3 normal_from_depth(float depth, vec2 texcoords) {
  
  const vec2 offset1 = vec2(0.0,0.001);
  const vec2 offset2 = vec2(0.001,0.0);
  
  float depth1 = mtexture(screenDepth, texcoords + offset1).r;
  float depth2 = mtexture(screenDepth, texcoords + offset2).r;
  
  vec3 p1 = vec3(offset1, depth1 - depth);
  vec3 p2 = vec3(offset2, depth2 - depth);
  
  vec3 normal = cross(p1, p2);
  normal.z = -normal.z;
  
  return normalize(normal);
}

float rand(vec2 co)
{
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

float ps_ssao()
{   
  const float total_strength = 1.0;
  const float base = 0.2;
  
  const float area = 0.0075;
  const float falloff = 0.000001;  
  const float radius = 0.0002;
  
  const int samples = 16;
  const vec3 sample_sphere[samples] = vec3[samples](
      vec3( 0.5381, 0.1856,-0.4319), vec3( 0.1379, 0.2486, 0.4430),
      vec3( 0.3371, 0.5679,-0.0057), vec3(-0.6999,-0.0451,-0.0019),
      vec3( 0.0689,-0.1598,-0.8547), vec3( 0.0560, 0.0069,-0.1843),
      vec3(-0.0146, 0.1402, 0.0762), vec3( 0.0100,-0.1924,-0.0344),
      vec3(-0.3577,-0.5301,-0.4358), vec3(-0.3169, 0.1063, 0.0158),
      vec3( 0.0103,-0.5869, 0.0046), vec3(-0.0897,-0.4940, 0.3287),
      vec3( 0.7119,-0.0154,-0.0918), vec3(-0.0533, 0.0596,-0.5411),
      vec3( 0.0352,-0.0631, 0.5460), vec3(-0.4776, 0.2847,-0.0271)
  );
  
  vec3 random = normalize(texture(random, TexCoords * 4.0).rgb);
  
  float depth = mtexture(screenDepth, TexCoords).r;
 
  vec3 position = vec3(TexCoords, depth);
  vec3 normal = normal_from_depth(depth, TexCoords);
  
  float radius_depth = radius/depth;
  float occlusion = 0.0;
  for(int i=0; i < samples; i++) {
  
    vec3 ray = radius_depth * reflect(sample_sphere[i], random);
    vec3 hemi_ray = position + sign(dot(ray,normal)) * ray;
    
    float occ_depth = mtexture(screenDepth, clamp(hemi_ray.xy, 0., 1.)).r;
    float difference = depth - occ_depth;
    
    occlusion += step(falloff, difference) * (1.0-smoothstep(falloff, area, difference));
  }
  
  float ao = 1.0 - total_strength * occlusion * (1.0 / samples);
  return clamp(ao + base, 0., 1.);
}

void main()
{ 
	vec4 color = mtexture(screenColor, TexCoords);
	vec4 depth = mtexture(screenDepth, TexCoords);

	FragColor = vec4(color.rgb * ps_ssao(), color.a);
}