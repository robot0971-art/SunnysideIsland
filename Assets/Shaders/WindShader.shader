Shader "SunnysideIsland/WindShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Hit Effect)]
        _Flash_Amount ("Flash Amount", Range(0, 1)) = 0.0
        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        
        [Header(Sparkle Settings)]
        _SparkleStrength ("Sparkle Strength", Range(0, 10)) = 1.0
        _SparkleSpeed ("Sparkle Speed", Float) = 2.0
        _SparkleDensity ("Sparkle Density", Range(0, 10)) = 1.0

        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaScan ("AlphaScan", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Pass
        {
            Name "2DPass"
            Tags { "LightMode"="Universal2D" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 positionWS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float _Flash_Amount;
            float4 _FlashColor;
            float _SparkleStrength;
            float _SparkleSpeed;
            float _SparkleDensity;
            CBUFFER_END
            
            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.color = input.color * _Color;
                output.positionWS = positionWS;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                color *= input.color;
                
                if (color.a < 0.01) discard;
                
                // --- 피격 및 반짝임 효과 ---
                if (_Flash_Amount > 0)
                {
                    float2 noiseUV = input.uv * 15.0 * _SparkleDensity;
                    float noise = Hash(floor(noiseUV) + floor(_Time.y * _SparkleSpeed));
                    float sparkle = pow(noise, 8.0) * _SparkleStrength * 2.0;
                    
                    float3 flashColor = lerp(color.rgb, _FlashColor.rgb, _Flash_Amount);
                    color.rgb = flashColor + (sparkle * _Flash_Amount * color.a);
                }
                
                return color;
            }
            ENDHLSL
        }
    }
    Fallback "Sprites/Default"
}
