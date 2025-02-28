// From: https://www.ronja-tutorials.com/post/012-fresnel/
Shader "Custom/FresnelSurface"
{
    Properties
    {
        _Emission ("Emission", Color) = (0,0,0,1)
        _FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
        [PowerSlider(4)] _FresnelExponent ("Fresnel Exponent", Range(0.25, 30)) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float3 worldNormal;
            float3 viewDir;
            INTERNAL_DATA  // Unity needs this to generate the worldspace normal
        };

        half3 _Emission;
        float3 _FresnelColor;
        float _FresnelExponent;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input i, inout SurfaceOutputStandard o)
        {
            o.Albedo = 0;
            o.Alpha = 0;

            float fresnel = 1 - dot(i.worldNormal, i.viewDir);
            fresnel = pow(fresnel, _FresnelExponent);
            fresnel = saturate(fresnel);
            float3 fresnelColor = fresnel * _FresnelColor;
            o.Emission = _Emission + fresnelColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
