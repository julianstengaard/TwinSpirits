// Change this string to move shader to new location
Shader "Custom/Unlit/Transparent Colored (behind)" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    
    SubShader {
        Tags {
			"Queue"="Transparent-1"
			"IgnoreProjector"="True" 
			"RenderType"="Transparent"
		}
        LOD 100
    
        ZWrite On
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