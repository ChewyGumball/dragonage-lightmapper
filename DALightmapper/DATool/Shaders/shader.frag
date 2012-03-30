uniform sampler2D diffuseTexture;
void main()
{
	//gl_FragColor = vec4(0.4,0.4,0.8, 1.0);
	gl_FragColor = texture2D(diffuseTexture, gl_TexCoord[0].st);
}