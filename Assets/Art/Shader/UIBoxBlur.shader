Shader "Custom/UIBoxBlur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {} // Texture
		_BlurSize ("Blur Size", Float) = 0.003 // Blur Strength
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _BlurSize;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;
				float2 offset = float2(_BlurSize, _BlurSize);

				// 9-point sampling
				fixed4 col = tex2D(_MainTex, uv) * 1.0;
				col += tex2D(_MainTex, uv + float2(offset.x, 0.0));
				col += tex2D(_MainTex, uv + float2(-offset.x, 0.0));
				col += tex2D(_MainTex, uv + float2(0.0, offset.y));
				col += tex2D(_MainTex, uv + float2(0.0, -offset.y));
				col += tex2D(_MainTex, uv + offset);
				col += tex2D(_MainTex, uv - offset);
				col += tex2D(_MainTex, uv + float2(offset.x, -offset.y));
				col += tex2D(_MainTex, uv + float2(-offset.x, offset.y));
				col /= 9.0;
				return col;
			}
			ENDCG
		}
	}
}