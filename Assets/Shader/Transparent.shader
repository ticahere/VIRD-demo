// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Transparent" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,0.6)
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
		SubShader{
			Tags { "Queue" = "Transparent" }
			Pass{
				Blend SrcAlpha OneMinusSrcAlpha
				Color[_Color]
				SetTexture[_MainTex]{
					combine previous * texture
				}

			}
	}
		SubShader{
			Tags { "Queue" = "Transparent" }
			CGPROGRAM
			#pragma surface surf Lambert alpha

			fixed4 _Color;
			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
			};
			void surf(Input IN, inout SurfaceOutput o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}

			ENDCG
	}
		SubShader{
			Tags { "Queue" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			Pass{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				fixed4 _Color;
				sampler2D _MainTex;

				struct v2f {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				v2f vert(appdata_base v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = v.texcoord;
					return o;
				}

				fixed4 frag(v2f i) :COLOR{
					return tex2D(_MainTex, i.texcoord) * _Color;
				}

				ENDCG
			}
			}
				FallBack "Transparent/Diffuse"
}