Shader "Camera" {
	Properties {
		_MainTex ("Main Texture", 2D) = "" {}
	}
	SubShader { 
		Pass{ 
			CGPROGRAM
			#pragma vertex vert 
			#pragma fragment frag 
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			
			float4 colorCoeff;			
			
			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata_base v)
			{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
			}

			
			float4 frag(v2f i) : SV_Target
			{

				float4 yuyv = tex2D(_MainTex, i.uv * 0.5);
	
				float y = 1.164 * (yuyv.x + yuyv.z) * 0.5 - (1.164 * 16.0/255.0);

				
				float cb = yuyv.y - (128.0/255.0);
				float cr = yuyv.w - (128.0/255.0);
				
				float r = y +  1.596 * cr;
				float g = y + -0.391 * cb - 0.813 * cr;
				float b = y +  2.018 * cb;
				
				float4 color = float4(r, g, b, 1.0);
				float4 ret = color * colorCoeff;

				//if (i.vertex.x < 0.5)
				//{
//					ret = float4(1,0,0,1);
	//			}
			
				return ret;				
				
			}
			
			ENDCG

		}
	}	 
}
