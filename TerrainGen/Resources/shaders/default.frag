#version 330 core

uniform vec3 tint;

in vec3 fragPos;
in vec4 fragColor;
in vec3 fragNormal;

out vec4 color;

void main()
{
    vec3 lightPos = vec3(1000, 1000, 1000);

    vec3 norm = normalize(fragNormal);
    vec3 lightDir = normalize(lightPos - fragPos);  
    float diffuse = max(dot(norm, lightDir), 0.0);
    float ambient = 0.3;
    
    color = vec4(fragColor.rgb * tint * (ambient + diffuse), 1.);
}