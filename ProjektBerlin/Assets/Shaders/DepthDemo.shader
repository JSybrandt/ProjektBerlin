Shader "DepthDemo" {
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
#if defined(SHADER_API_PSSL)
				/* We are rendering a linear texture that has a width/pitch of 80 texels, however the hardware only support a pitch of 128 texels
				... so we need to adjust the U value here
				*/
				float texturewidth = 80;
				float texturepitch = 128;
				float sRange = texturewidth / texturepitch;
				o.uv.x *= sRange;
#endif				
				return o;
			}

#if defined(SHADER_API_PSSL)
			float4 hsv_2_rgb( unsigned int h, unsigned int s, unsigned int v )
			{
				int i,f, m,n,k;
				float r,g,b;

				i = (h / 256) % 6;
				f = ( h % 256 );
				m = ( (v+1) * ( 255 - s ) ) /256 ;
				n = ( (v+1) * ( 255 - ( (s+1) * f ) /256 ) ) /256 ;
				k = ( (v+1) * ( 255 - ( (s+1) * ( 255 - f ) ) /256 )) /256 ;

				switch (i)	{
				case 0:	r = (float)v ; g = (float)k ; b = (float)m ; break ;
				case 1:	r = (float)n ; g = (float)v ; b = (float)m ; break ;
				case 2:	r = (float)m ; g = (float)v ; b = (float)k ; break ;
				case 3:	r = (float)m ; g = (float)n ; b = (float)v ; break ;
				case 4:	r = (float)k ; g = (float)m ; b = (float)v ; break ;
				case 5:	r = (float)v ; g = (float)m ; b = (float)n ; break ;
					default:	return ( -1 ) ;
				}
				
				float rr = clamp(r, 0, 255)/256.0f;
				float gg = clamp(g, 0, 255)/256.0f;
				float bb = clamp(b, 0, 255)/256.0f;
				return float4( rr, gg, bb, 1.0f );
			}

			float4 y8_to_rgba( float4 y )
			{
				float4 color = float4( 0.0f, 0.0f, 0.0f, 1.0f );
				int _depth16bit = (int)( y.x * 65535.0f );
				int _index = clamp( (_depth16bit >> 4), 0, 511 );
				if( _index < 256 )
				{
					float v = float(255-_index);
					color = hsv_2_rgb( (unsigned int)(255.0f*4.5f - v*5.5f),200,(unsigned int)(64.0f+v*0.75f) );
				}
				else
				{

					//float count = (float)(colorTableSize-256);
					//float v255 = distanceMode ? 0.0f : 1.0f;

					float m = (float)(512-_index) / (float)256;
					color = hsv_2_rgb( (int)(255*4.5*m), (int)(200.0f*m), (int)(64*m) );

				}

				return color;
			}
			
#endif
			
			float4 frag(v2f i) : SV_Target
			{
#if defined(SHADER_API_PSSL)
	const int m_pitch = 128;
	const int m_textureWidth = 80;
	const int m_textureHeight= 50;
	
//	float2 xy = In.TextureUV.xy;
	float2 xy = i.uv;

	float sr = float( m_pitch )/ float( m_textureWidth );
	
	int iY = int( xy.y * m_textureHeight);
	int iX = int (sr * xy.x * m_textureWidth);
//	int iX = int (xy.x * m_textureWidth);
	
	int bufferIndex = iX + ( iY * m_textureWidth );
	int buffery = bufferIndex / m_pitch;
	
	
	int bufferx = bufferIndex - buffery * m_pitch;
	float2 rxy = float2( float(bufferx) / float(m_pitch), float (buffery) / float( m_textureHeight ) );
//	return y8_to_rgba( colorMap.Sample(samp0, rxy) ).xyzw;

	return y8_to_rgba( tex2D(_MainTex, rxy ) ).xyzw;
#else			
				float4 color = tex2D(_MainTex, i.uv );
				//float4 color = float4(0,1,0,1);
				float4 ret = color; // * colorCoeff;
				return ret;				
#endif				
			}
			
			ENDCG

		}
	}	 
}
