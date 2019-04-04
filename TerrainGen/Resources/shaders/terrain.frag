#version 330 core

uniform vec3 lightPos;
uniform vec3 tint;
uniform int samples;
uniform sampler2D random;

in vec3 fragPos;
in vec4 fragColor;
in vec3 fragNormal;

out vec4 color;

void main()
{
	vec2 resolution = vec2(512., 512.);

    vec3 norm = normalize(fragNormal);
    vec3 lightDir = normalize(lightPos - fragPos);  
    float diffuse = max(dot(norm, lightDir), 0.0);
    float ambient = 0.3;
	
    // Look up noise from texture
    vec4 noise = texture(random, gl_FragCoord.xy / resolution.yy);
    
    color = vec4(fragColor.rgb * tint * clamp(ambient + diffuse, 0, 1), 1.);

    color += vec4(vec3((noise.r - 0.5) / 255.0), 0.0);
}