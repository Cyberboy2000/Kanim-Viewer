// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Kanim"
{
    Properties
    {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _a("A", Float) = 1
		[PerRendererData] _b("B", Float) = 0
		[PerRendererData] _c("C", Float) = 0
		[PerRendererData] _d("D", Float) = 1
		[PerRendererData] _tx("TX", Float) = 0
		[PerRendererData] _ty("TY", Float) = 0
		[PerRendererData] _CM("Color Multiply", Color) = (1,1,1,1)
		[PerRendererData] _CA("Color Add", Color) = (0,0,0,0)
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex KanimVert
            #pragma fragment KanimFram
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float, _a)
#define _a_arr Props
				UNITY_DEFINE_INSTANCED_PROP(float, _b)
#define _b_arr Props
				UNITY_DEFINE_INSTANCED_PROP(float, _c)
#define _c_arr Props
				UNITY_DEFINE_INSTANCED_PROP(float, _d)
#define _d_arr Props
				UNITY_DEFINE_INSTANCED_PROP(float, _tx)
#define _tx_arr Props
				UNITY_DEFINE_INSTANCED_PROP(float, _ty)
#define _ty_arr Props
				UNITY_DEFINE_INSTANCED_PROP(fixed4, _CM)
				UNITY_DEFINE_INSTANCED_PROP(fixed4, _CA)
			UNITY_INSTANCING_BUFFER_END(Props)

			struct v2f2
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				fixed4 colorAdd : COLOR1;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f2 KanimVert(appdata_t IN)
			{
				v2f2 OUT;

				UNITY_SETUP_INSTANCE_ID(IN);

				OUT.vertex = UnityWorldToClipPos(float3(IN.vertex.x * UNITY_ACCESS_INSTANCED_PROP(Props, _a) + IN.vertex.y * UNITY_ACCESS_INSTANCED_PROP(Props, _c) + UNITY_ACCESS_INSTANCED_PROP(Props, _tx),
					IN.vertex.x * UNITY_ACCESS_INSTANCED_PROP(Props, _b) + IN.vertex.y * UNITY_ACCESS_INSTANCED_PROP(Props, _d) + UNITY_ACCESS_INSTANCED_PROP(Props, _ty),0));

				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * UNITY_ACCESS_INSTANCED_PROP(Props, _CM);
				OUT.colorAdd = UNITY_ACCESS_INSTANCED_PROP(Props, _CA);

				return OUT;
			}

			fixed4 KanimFram(v2f2 IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color + IN.colorAdd;
				c.rgb *= c.a;
				return c;
			}
        ENDCG
        }
    }
}
