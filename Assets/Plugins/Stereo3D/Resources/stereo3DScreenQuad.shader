Shader "Stereo3D Screen Quad" 
{
	Properties 
	{
	   [NoScaleOffset] _LeftTex ("Left Texture", 2D) = "white" {}
	   [NoScaleOffset] _RightTex ("Right Texture", 2D) = "white" {}

	   _LeftCol ("Left Color", Color) = (1, 0, 0)
	   _RightCol ("Right Color", Color) = (0, 1, 1)

	   _Columns ("Columns", Int) = 1
	   _Rows ("Rows", Int) = 1
	}

	SubShader 
	{
		//pass0 Interleaved Row
		Pass 
		{
			ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		
			struct vertexData
			{
				float2 pos;
				float2 uv;
			};

			StructuredBuffer<vertexData> buffer;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(uint vertexID : SV_VertexID)
			{
				v2f o;
				o.vertex = float4(buffer[vertexID].pos, 0, 1);
				o.uv = buffer[vertexID].uv;
				return o;
			}

			int _Rows;
			Texture2D  _LeftTex;
			Texture2D  _RightTex;
			SamplerState clamp_point_sampler;

			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 left = _LeftTex.Sample(clamp_point_sampler, i.uv);
				fixed4 right = _RightTex.Sample(clamp_point_sampler, i.uv);

				uint intRow = i.uv.y * _Rows;
				uint odd = intRow - intRow / 2 * 2;
				return odd == 0 ? left : right;
			}
			ENDCG
		}

		//pass1 Interleaved Column
		Pass 
		{
			ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		
			struct vertexData
			{
				float2 pos;
				float2 uv;
			};

			StructuredBuffer<vertexData> buffer;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(uint vertexID : SV_VertexID)
			{
				v2f o;
				o.vertex = float4(buffer[vertexID].pos, 0, 1);
				o.uv = buffer[vertexID].uv;
				return o;
			}

			int _Columns;
			Texture2D  _LeftTex;
			Texture2D  _RightTex;
			SamplerState clamp_point_sampler;

			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 left = _LeftTex.Sample(clamp_point_sampler, i.uv);
				fixed4 right = _RightTex.Sample(clamp_point_sampler, i.uv);

				uint intColumn = i.uv.x * _Columns;
				uint odd = intColumn - intColumn / 2 * 2;
				return odd == 0 ? left : right;
			}
			ENDCG
		}

		//pass2 Interleaved Checkerboard
		Pass 
		{
			ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		
			struct vertexData
			{
				float2 pos;
				float2 uv;
			};

			StructuredBuffer<vertexData> buffer;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(uint vertexID : SV_VertexID)
			{
				v2f o;
				o.vertex = float4(buffer[vertexID].pos, 0, 1);
				o.uv = buffer[vertexID].uv;
				return o;
			}

			int _Columns;
			int _Rows;
			Texture2D  _LeftTex;
			Texture2D  _RightTex;
			SamplerState clamp_point_sampler;

			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 left = _LeftTex.Sample(clamp_point_sampler, i.uv);
				fixed4 right = _RightTex.Sample(clamp_point_sampler, i.uv);

				uint intRow = i.uv.y * _Rows;
				uint intColumn = i.uv.x * _Columns;
				uint oddRow = intRow - intRow / 2 * 2;
				uint oddColumn = intColumn - intColumn / 2 * 2;
				uint switcher = oddRow + oddColumn - 2 * oddRow * oddColumn;

				return switcher == 0 ? left : right;
			}
			ENDCG
		}
   
		//pass3 Side By Side
		Pass
		{
			ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct vertexData
			{
				float2 pos;
				float2 uv;
			};

			StructuredBuffer<vertexData> buffer;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : uv;
			};

			v2f vert(uint vertexID : SV_VertexID)
			{
				v2f o;
				o.vertex = float4(buffer[vertexID].pos, 0, 1);
				o.uv = buffer[vertexID].uv;
				return o;
			}

			Texture2D  _LeftTex;
			Texture2D  _RightTex;
			SamplerState repeat_point_sampler;

			fixed4 frag(v2f i) : SV_Target
			{
				return i.uv.x < 1 ? _LeftTex.Sample(repeat_point_sampler, i.uv) : _RightTex.Sample(repeat_point_sampler, i.uv);
			}
			ENDCG
		}

		//pass4 Over Under
		Pass
		{
			ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct vertexData
			{
				float2 pos;
				float2 uv;
			};

			StructuredBuffer<vertexData> buffer;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : uv;
			};

			v2f vert(uint vertexID : SV_VertexID)
			{
				v2f o;
				o.vertex = float4(buffer[vertexID].pos, 0, 1);
				o.uv = buffer[vertexID].uv;
				return o;
			}

			Texture2D  _LeftTex;
			Texture2D  _RightTex;
			SamplerState repeat_point_sampler;

			fixed4 frag(v2f i) : SV_Target
			{
				return i.uv.y < 1 ? _LeftTex.Sample(repeat_point_sampler, i.uv) : _RightTex.Sample(repeat_point_sampler, i.uv);
			}
			ENDCG
		}

		//pass5 Anaglyph
		Pass
		{
			ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct vertexData
			{
				float2 pos;
				float2 uv;
			};

			StructuredBuffer<vertexData> buffer;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : uv;
			};

			v2f vert(uint vertexID : SV_VertexID)
			{
				v2f o;
				o.vertex = float4(buffer[vertexID].pos, 0, 1);
				o.uv = buffer[vertexID].uv;
				return o;
			}

			Texture2D  _LeftTex;
			Texture2D  _RightTex;
			SamplerState clamp_point_sampler;
			fixed4 _LeftCol;
			fixed4 _RightCol;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 leftColor = _LeftTex.Sample(clamp_point_sampler, i.uv) * _LeftCol;
				fixed4 rightColor = _RightTex.Sample(clamp_point_sampler, i.uv) * _RightCol;
				return leftColor + rightColor;
			}
			ENDCG
		}
	}   
} 
