// Change this string to move shader to new location
Shader "Custom/Unlit/Transparent Colored" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    
    SubShader {
        Tags {
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent"
		}
        LOD 100
    
		ZTest Always
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass {
            Lighting Off
            SetTexture [_MainTex] { 
                constantColor[_Color]
                combine constant * texture
            } 
        }
    }
}