// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Gradient"
{
	Properties
	{
		_HueShift("HueShift", Float) = 0
		_Sat("Saturation", Float) = 1
		_Val("Value", Float) = 1

		_WorldScale("WorldScale", Float) = 1
		_AmplifyWorldScale("AmplifyWorldScale", Float) = 100
		_CameraScale("CameraScale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

			struct v2f {
				float4 pos : SV_POSITION;
				float4 worldSpacePosition : TEXCOORD0;
			};

			float _HueShift;
			float _Sat;
			float _Val;

			float _WorldScale;
			float _CameraScale;
			float _AmplifyWorldScale;

			float3 shift_col(float3 RGB, float3 shift)
			{
				float3 RESULT = float3(RGB);
				float VSU = shift.z*shift.y*cos(shift.x*3.14159265 / 180);
				float VSW = shift.z*shift.y*sin(shift.x*3.14159265 / 180);

				RESULT.x = (.299*shift.z + .701*VSU + .168*VSW)*RGB.x
					+ (.587*shift.z - .587*VSU + .330*VSW)*RGB.y
					+ (.114*shift.z - .114*VSU - .497*VSW)*RGB.z;

				RESULT.y = (.299*shift.z - .299*VSU - .328*VSW)*RGB.x
					+ (.587*shift.z + .413*VSU + .035*VSW)*RGB.y
					+ (.114*shift.z - .114*VSU + .292*VSW)*RGB.z;

				RESULT.z = (.299*shift.z - .3*VSU + 1.25*VSW)*RGB.x
					+ (.587*shift.z - .588*VSU - 1.05*VSW)*RGB.y
					+ (.114*shift.z + .886*VSU - .203*VSW)*RGB.z;

				return (RESULT);
			}

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			float WorldPosToValue(float4 pos)
			{
				float x = pos.x * _WorldScale;
				float y = pos.y * _WorldScale;
				float z = pos.z * _WorldScale;
				return (sin(5 * x + 3 * y) - cos(2 * z - 7 * x) - sin(x + 3 * z) + cos(2 * y)) * _AmplifyWorldScale;
			}

            fixed4 frag (v2f i) : SV_Target
            {
				 float4 col = {1, 0, 0, 1}; //red
				 float3 shift = float3(_HueShift + WorldPosToValue(i.worldSpacePosition) + i.pos.x * _CameraScale, _Sat, _Val);
				 return half4(half3(shift_col(col, shift)), col.a);
            }
            ENDCG
        }
    }
}
