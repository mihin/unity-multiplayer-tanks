Shader "Layers"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex1 ("Color 1 (RGB)", 2D) = "white" {}
        _MainTex2 ("Color 2 (RGB)", 2D) = "white" {}
        _MainTex3 ("Color 3 (RGB)", 2D) = "white" {}
        _Mask ("Mixing Mask (RGBA)", 2D) = "gray" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 400
        CGPROGRAM
            #pragma surface surf BlinnPhong
            //#include "UnityCG.cginc"

            sampler2D _MainTex1;
            sampler2D _MainTex2;
            sampler2D _MainTex3;
            sampler2D _Mask;
             
            struct Input
            {
                float2 uv_MainTex1;
                float2 uv_MainTex2;
                float2 uv_MainTex3;
                //float2 uv_MainTex4;
            };

            void surf (Input IN, inout SurfaceOutput o)
            {
                half4 color1 = tex2D( _MainTex1, IN.uv_MainTex1.xy );
                half4 color2 = tex2D( _MainTex2, IN.uv_MainTex2.xy );
                half4 color3 = tex2D( _MainTex3, IN.uv_MainTex3.xy );
                // get the mixing mask texture
                half4 mask = tex2D( _Mask, IN.uv_MainTex1.xy );
                // mix the three layers
                half4 color = color1 * mask.r + color2 * mask.g + color3 * mask.b;

                if (mask.r > 0.3 && mask.g > 0.3) { color.r = 0.1; color.b = 0.1; color.g = 0.1; }
                if (mask.r > 0.3 && mask.b > 0.3) { color.r = 0.1; color.b = 0.1; color.g = 0.1; }
                if (mask.b > 0.3 && mask.g > 0.3) { color.r = 0.1; color.b = 0.1; color.g = 0.1; }

                // multiply and double by the lighting
                color = color * 1.5;
                o.Albedo = color.rgb;
            }
            #pragma target 3.0
        ENDCG
    }
}
