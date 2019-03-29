#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vert;

uniform mat4 m;
uniform mat4 v;
uniform mat4 p;
  
void main()
{
  // Output position of the vertex, in clip space : MVP * position
  mat4 MVP = p*v*m;
  gl_Position =  MVP * vec4(vert,1);
}