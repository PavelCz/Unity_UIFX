Shader "Custom/CRTGluGUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // UI Masking Properties
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        // Global Glitch Properties
        _ImageScale ("Image Scale (Margin)", Range(1, 3)) = 1.2

        // Offset Glitch Properties
        [Toggle] _EnableOffset ("Enable Offset Glitch", Float) = 1.0
        _GlitchIntensity ("Offset Glitch Intensity", Range(0, 1)) = 0.5
        _OffsetAmount ("Offset Amount (Shift)", Range(0, 1)) = 0.2
        _OffsetChance ("Offset Chance", Range(0, 1)) = 0.11
        _OffsetBlockSize ("Offset Line Number", Range(1, 100)) = 55.0
        
        // RGB Split Properties
        [Toggle] _EnableRGBSplit ("Enable RGB Split", Float) = 1.0
        _RGBSplitAmount ("RGB Split Amount", Range(0, 0.1)) = 0.07
        _RGBSplitChance ("RGB Split Chance", Range(0, 1)) = 0.14
        
        // Scanline Properties
        [Toggle] _EnableScanline ("Enable Scanlines", Float) = 1.0
        _ScanlineCount ("Scanline Count", Float) = 500.0
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.15
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;

            float _EnableOffset;
            float _EnableRGBSplit;
            float _EnableScanline;

            float _GlitchIntensity;
            float _ImageScale;

            float _OffsetAmount;
            float _OffsetChance;
            float _OffsetBlockSize;
            
            float _RGBSplitAmount;
            float _RGBSplitChance;
            
            float _ScanlineCount;
            float _ScanlineIntensity;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float random(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Checks if the shifted UV is safely inside the 0-1 bounds
            float checkBounds(float2 uv) {
                return step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 0. MARGIN PADDING (Creates a buffer for the glitch to bleed into)
                float2 uv = i.uv;
                uv = (uv - 0.5) * _ImageScale + 0.5;

                // 1. OFFSET (Block Displacement)
                float timeFloor = floor(_Time.y * 10.0);

                // Global trigger: is the screen block-glitching right now?
                float glitchOn = step(1.0 - _OffsetChance, random(float2(timeFloor, 0.0)));

                // Block evaluation: which row is this, and should it shift?
                float lineBlock = floor(uv.y * _OffsetBlockSize);
                float lineNoise = random(float2(lineBlock, timeFloor));
                float glitchMask = step(1.0 - _GlitchIntensity, lineNoise);

                // Shift amount: how far left or right?
                float shift = (random(float2(lineBlock, timeFloor * 1.2)) - 0.5) * _OffsetAmount;

                uv.x += shift * glitchMask * glitchOn * _EnableOffset;

                // 2. RGB SPLIT (Chromatic Aberration)
                float rgbTimeFloor = floor(_Time.y * 15.0);
                
                // Separate chance trigger so the RGB split occurs independently
                float rgbNoise = random(float2(rgbTimeFloor, 89.0)); 
                float rgbTrigger = step(1.0 - _RGBSplitChance, rgbNoise);

                float rgbJitter = random(float2(rgbTimeFloor, 0.0)) * 2.0 - 1.0; 
                float splitAmount = _RGBSplitAmount * _GlitchIntensity * rgbJitter * rgbTrigger * _EnableRGBSplit;

                float2 r_uv = uv + float2(splitAmount, 0);
                float2 b_uv = uv - float2(splitAmount, 0);

                // Multiply by checkBounds to prevent smeared edges from texture wrapping
                fixed4 colR = tex2D(_MainTex, r_uv) * checkBounds(r_uv);
                fixed4 colG = tex2D(_MainTex, uv) * checkBounds(uv);
                fixed4 colB = tex2D(_MainTex, b_uv) * checkBounds(b_uv);

                fixed4 finalCol = fixed4(colR.r, colG.g, colB.b, 1.0);
                finalCol.a = max(colR.a, max(colG.a, colB.a));
                finalCol *= i.color;

                // 3. SCANLINE & FLICKER
                float scanline = sin(uv.y * _ScanlineCount + _Time.y * 10.0);
                scanline = smoothstep(-1.0, 1.0, scanline);
                finalCol.rgb -= scanline * _ScanlineIntensity * _EnableScanline;

                // uGUI CLIPPING
                finalCol.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                
                return finalCol;
            }
            ENDCG
        }
    }
}
