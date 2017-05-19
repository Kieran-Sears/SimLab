Shader "Custom/Stencil/MaskOneZLess"
{
	Properties
	{
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1" }
		ColorMask 0
		ZWrite off

		Stencil
		{
			Ref 1
			Comp always
			Pass replace
		}

		Pass
		{
			Cull Back
			ZTest Less

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				return half4(1,1,0,1);
			}
			ENDCG
		}
	}
}
