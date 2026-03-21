Shader "TextMeshPro/ColorSwap_Single"
{
    Properties
    {
        // Standard TMPro texture (the font atlas)
        _MainTex ("Font Atlas", 2D) = "white" {}
        
        // The color the text should be when NOT touching the target color
        _FaceColor ("Default Color (White)", Color) = (1,1,1,1)
        
        // The specific Hex/Color to look for in the background (e.g., FFF540)
        _TargetColor ("Yellow to Detect", Color) = (1, 0.96, 0.25, 1) 
        
        // The color the text should become when it overlaps the target
        _OverlapColor ("Color on Yellow (Black)", Color) = (0,0,0,1)
        
        // How "strict" the color matching is (0 = exact match only, 1 = anything)
        _Threshold ("Detection Sensitivity", Range(0, 1)) = 0.5

        // --- TMPro INTERNAL PROPERTIES ---
        // These are required so the TMPro scripts don't throw "Missing Property" errors.
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
        // Render in the Transparent queue so background objects (like paper) are drawn first
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }    

        // STEP 1: Take a "snapshot" of the screen pixels currently behind this object
        // This is saved into the variable "_BackgroundTexture"
        GrabPass { "_BackgroundTexture" }

        // Standard UI settings
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] }
        
        // ZTest LEqual ensures that if an object is physically in front (closer Z), 
        // this text will correctly hide behind it.
        ZTest LEqual
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull [_CullMode]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { 
                float4 vertex : POSITION; 
                fixed4 color : COLOR; 
                float2 texcoord : TEXCOORD0; 
            };

            struct v2f { 
                float4 vertex : SV_POSITION; 
                fixed4 color : COLOR; 
                float2 texcoord : TEXCOORD0; 
                float4 grabPos : TEXCOORD1; // Used to map the GrabPass to screen coordinates
            };

            sampler2D _MainTex;
            sampler2D _BackgroundTexture;
            fixed4 _FaceColor;
            fixed4 _TargetColor;
            fixed4 _OverlapColor;
            float _Threshold;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Convert vertex position to a coordinate we can use to look up the GrabPass texture
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // STEP 2: Sample the pixel color from the background snapshot
                fixed4 bgColor = tex2Dproj(_BackgroundTexture, i.grabPos);
                
                // STEP 3: Compare the background pixel to our Target Yellow
                float dist = distance(bgColor.rgb, _TargetColor.rgb);
                
                // STEP 4: Choose the text color based on the distance/threshold
                // If the distance is small (meaning it's yellow), use Black. Otherwise, White.
                fixed4 finalTextColor = (dist < _Threshold) ? _OverlapColor : _FaceColor;
                
                // STEP 5: Apply the actual font shape (alpha) from the TMPro atlas
                finalTextColor.a *= tex2D(_MainTex, i.texcoord).a;
                
                return finalTextColor;
            }
            ENDCG
        }
    }
}