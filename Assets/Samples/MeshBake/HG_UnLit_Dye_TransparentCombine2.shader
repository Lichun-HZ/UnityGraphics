// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/HeroGo/General/UnLit/HG_Unlit_Dye_TransparentCombine2"
{
	Properties
	{
		_MainTex1("Base (RGB) Trans (A)1", 2D) = "white" {}
		_DyeColor1("Dye color1",Color) = (1.0,1.0,1.0,1.0)
		_ColorStrength1("Color Strength1",Range(0, 1)) = 0.3
		[HideInInspector]_AlphaFactor1("Alpha factor1",Range(0,1)) = 1.0

		_MainTex2("Base (RGB) Trans (A)2", 2D) = "white" {}
		_DyeColor2("Dye color2",Color) = (1.0,1.0,1.0,1.0)
		_ColorStrength2("Color Strength2",Range(0, 1)) = 0.3
		[HideInInspector]_AlphaFactor2("Alpha factor2",Range(0,1)) = 1.0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
				float3 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 texcoord1 : TEXCOORD0;
				half2 texcoord2 : TEXCOORD1;
				half  matID : TEXCOORD2;
				UNITY_FOG_COORDS(3)
			};

			sampler2D _MainTex1;
			float4 _MainTex1_ST;
			fixed4 _DyeColor1;
			fixed _AlphaFactor1;
			half _ColorStrength1;

			sampler2D _MainTex2;
			float4 _MainTex2_ST;
			fixed4 _DyeColor2;
			fixed _AlphaFactor2;
			half _ColorStrength2;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord.xy, _MainTex1);
				o.texcoord2 = TRANSFORM_TEX(v.texcoord.xy, _MainTex2);
				o.matID = v.texcoord.z;

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col;
				if (i.matID < 0.5)
				{
					col = tex2D(_MainTex1, i.texcoord1) * _DyeColor1;
					col.rgb = lerp(col.rgb, col.rgb * normalize(col.rgb), _ColorStrength1);
					col.a *= _AlphaFactor1;
				}
				else
				{
					col = tex2D(_MainTex2, i.texcoord2) * _DyeColor2;
					col.rgb = lerp(col.rgb, col.rgb * normalize(col.rgb), _ColorStrength2);
					col.a *= _AlphaFactor2;
				}

				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}

			ENDCG
		}
	}

}
