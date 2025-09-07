Shader "Custom/UIGaussianBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 8)) = 2.0
        _StandardDeviation ("Standard Deviation", Range(0.1, 3)) = 1.5
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "GaussianBlur"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _ClipSoftness;
            float _BlurSize;
            float _StandardDeviation;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            // Calculate Gaussian weight
            float gaussianWeight(float distance, float standardDeviation)
            {
                float variance = standardDeviation * standardDeviation;
                return (1.0 / sqrt(2.0 * 3.14159265 * variance)) * 
                       exp(-(distance * distance) / (2.0 * variance));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                fixed4 color = fixed4(0, 0, 0, 0);
                float totalWeight = 0.0;
                
                // Optimized sampling strategy: use variable sample radius
                int sampleRadius = max(1, (int)(_BlurSize * 0.8));
                sampleRadius = min(sampleRadius, 5); // Limit max samples for performance
                
                // Center sampling
                float centerWeight = gaussianWeight(0, _StandardDeviation);
                color += (tex2D(_MainTex, uv) + _TextureSampleAdd) * centerWeight;
                totalWeight += centerWeight;
                
                // Ring sampling - more efficient Gaussian approximation
                for (int ring = 1; ring <= sampleRadius; ring++)
                {
                    float ringDistance = (float)ring;
                    float weight = gaussianWeight(ringDistance, _StandardDeviation);
                    
                    // 8 sampling points per ring
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = (float)i * 0.78539816; // 45 degree intervals
                        float2 offset = float2(
                            cos(angle) * ringDistance * _MainTex_TexelSize.x * _BlurSize,
                            sin(angle) * ringDistance * _MainTex_TexelSize.y * _BlurSize
                        );
                        
                        color += (tex2D(_MainTex, uv + offset) + _TextureSampleAdd) * weight;
                        totalWeight += weight;
                    }
                }
                
                color = color / totalWeight * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}