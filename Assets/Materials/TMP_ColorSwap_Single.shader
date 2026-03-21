Shader "TextMeshPro/ColorSwap_Single"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("Default Color (White)", Color) = (1,1,1,1)
        _TargetColor ("Yellow to Detect", Color) = (1, 0.96, 0.25, 1) // FFF540
        _OverlapColor ("Color on Yellow (Black)", Color) = (0,0,0,1)
        _Threshold ("Detection Sensitivity", Range(0, 1)) = 0.5

        // TMPro Boilerplate to stop errors
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" }

        // This "grabs" the screen color behind the text
        GrabPass { "_BackgroundTexture" }

        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull [_CullMode]
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; float4 grabPos : TEXCOORD1; };

            sampler2D _MainTex;
            sampler2D _BackgroundTexture;
            fixed4 _FaceColor;
            fixed4 _TargetColor;
            fixed4 _OverlapColor;
            float _Threshold;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Look at the pixel color directly behind this text pixel
                fixed4 bgColor = tex2Dproj(_BackgroundTexture, i.grabPos);
                
                // Calculate how close the background is to your FFF540 Yellow
                float dist = distance(bgColor.rgb, _TargetColor.rgb);
                
                // If the background is Yellow, use Black. Otherwise, use White.
                fixed4 finalTextColor = (dist < _Threshold) ? _OverlapColor : _FaceColor;
                
                // Apply the font texture alpha (the actual letter shape)
                finalTextColor.a *= tex2D(_MainTex, i.texcoord).a;
                
                return finalTextColor;
            }
            ENDCG
        }
    }
}