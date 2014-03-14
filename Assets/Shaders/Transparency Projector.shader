Shader "Transparency Projector" {	


Properties
	{_MainTex ("Cookie", 2D) = "" {TexGen ObjectLinear} }

// Keep the color already on-screen,
// but replace the alpha.
SubShader
{		
    Tags {"Queue"="Transparent" "IgnoreProjector"="False" "RenderType"="Transparent"}
	Blend Zero SrcColor
	
	Pass 
	{        		
		SetTexture [_MainTex] 
		{
			ConstantColor (1,1,1)
			Combine constant, texture
			Matrix [_Projector]
		}
	}   
}


}