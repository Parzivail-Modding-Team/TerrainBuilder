#version 330 core

uniform vec3 lightPos;
uniform vec3 tint;
uniform int samples;

in vec3 fragPos;
in vec4 fragColor;
in vec3 fragNormal;

out vec4 color;

void main()
{
    vec3 norm = normalize(fragNormal);
    vec3 lightDir = normalize(lightPos - fragPos);  
    float diffuse = max(dot(norm, lightDir), 0.0);
    float ambient = 0.3;
    
    color = vec4(fragColor.rgb * tint * clamp(ambient + diffuse, 0, 1), 1.);
}