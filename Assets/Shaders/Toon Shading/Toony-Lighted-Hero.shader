Shader "Toon/Lighted Hero" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {} 
		
		_RimCol ("Rim Colour" , Color) = (1,0,0,1)
        _RimPow ("Rim Power", Float) = 1.0
	}

	SubShader {
		Pass {
			Name "Behind" 
			Tags { "RenderType"="transparent" "Queue" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Greater               // here the check is for the pixel being greater or closer to the camera, in which 
			Cull Back                   // case the model is behind something, so this pass runs
			ZWrite Off
			LOD 200                     

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			    float3 normal : TEXCOORD1;      // Normal needed for rim lighting
			    float3 viewDir : TEXCOORD2;     // as is view direction. 
			};

			sampler2D _MainTex;
			float4 _RimCol;
			float _RimPow;

			float4 _MainTex_ST;

			v2f vert (appdata_tan v)
			{
			    v2f o;
			    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			    o.normal = normalize(v.normal);
			    o.viewDir = normalize(ObjSpaceViewDir(v.vertex));       //this could also be WorldSpaceViewDir, which would 
			    return o;                                               //return the World space view direction. 
			}

			half4 frag (v2f i) : COLOR
			{
			    half Rim = 1 - saturate(dot(normalize(i.viewDir), i.normal));       //Calculates where the model view falloff is        
			                                                                        //for rim lighting.
			    half4 RimOut = _RimCol * pow(Rim, _RimPow);
			    return RimOut;
			}
			ENDCG
		}

		Tags { "RenderType"="Opaque" "Queue" = "Transparent"}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf ToonRamp

		sampler2D _Ramp;

		// custom lighting function that uses a texture ramp based
		// on angle between light direction and normal
		#pragma lighting ToonRamp exclude_path:prepass
		inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
		{
			#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = normalize(lightDir);
			#endif
			
			half d = dot (s.Normal, lightDir)*0.5 + 0.5;
			half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;
			
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
			c.a = 0;
			return c;
		}


		sampler2D _MainTex;
		float4 _Color;

		struct Input {
			float2 uv_MainTex : TEXCOORD0;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG

	} 

	Fallback "Diffuse"
}
