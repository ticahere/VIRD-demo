Shader "Unlit/CircleDivide"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WinColor("Main Color", Color) = (0,0,1,1)
        _LoseColor("Main Color", Color) = (1,0,0,1)
        _MatchNum("Match Num", Float) = 3
        _WinFirstMatch("_WIN1", Float) = 1
        _WinSecondMatch("_WIN2", Float) = 0
        _WinThirdMatch("_WIN3", Float) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MatchNum;
            float4 _WinColor;
            float4 _LoseColor;

            bool _WinFirstMatch;
            bool _WinSecondMatch;
            bool _WinThirdMatch;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float length = 1.0 / _MatchNum;

            fixed4 col;
            if (_MatchNum == 2)
            {
                if (i.uv.y < length)
                    col = _WinFirstMatch ? _WinColor : _LoseColor;
                else
                    col = _WinSecondMatch ? _WinColor : _LoseColor;
            }
            else
            {
                if (i.uv.y < length)
                    col = _WinFirstMatch ? _WinColor : _LoseColor;
                else if (i.uv.y < 2 * length)
                    col = _WinSecondMatch ? _WinColor : _LoseColor;
                else
                    col = _WinThirdMatch ? _WinColor : _LoseColor;
            }
                // sample the texture
                col.w = tex2D(_MainTex, i.uv).w;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
