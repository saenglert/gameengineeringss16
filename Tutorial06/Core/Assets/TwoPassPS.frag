#ifdef GL_ES
    precision highp float;
#endif

uniform vec4 linecolor;

void main()
{
    gl_FragColor = linecolor;
}    