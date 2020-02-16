// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AlphaWipeShader"
{
	Properties
	{
		_WipeTexture("WipeTexture", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Fade("Fade", Range( -0.6 , 0.6)) = 0
		_Sphere_Color("Sphere_Color", Color) = (1,1,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Overlay-1000" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Front
		ZWrite On
		ZTest Always
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Sphere_Color;
		uniform float _Fade;
		uniform sampler2D _WipeTexture;
		uniform float4 _WipeTexture_ST;
		uniform float _Cutoff = 0.5;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = _Sphere_Color.rgb;
			o.Alpha = 1;
			float2 uv_WipeTexture = i.uv_texcoord * _WipeTexture_ST.xy + _WipeTexture_ST.zw;
			clip( ( _Fade + tex2D( _WipeTexture, uv_WipeTexture ) ).r - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16700
1921;39;1278;1000;-107.936;575.5762;1;True;True
Node;AmplifyShaderEditor.SamplerNode;12;440.1888,157.1846;Float;True;Property;_WipeTexture;WipeTexture;0;0;Create;True;0;0;False;0;5f9bea2f583434c4eafd978cf66b4080;5f9bea2f583434c4eafd978cf66b4080;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;460.4663,65.09518;Float;False;Property;_Fade;Fade;2;0;Create;True;0;0;False;0;0;0.6;-0.6;0.6;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;769.825,45.17107;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;17;528.2208,-108.9312;Float;False;Property;_Sphere_Color;Sphere_Color;3;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;915.9396,-80.78117;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;AlphaWipeShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Front;1;False;-1;7;False;-1;False;1;False;-1;1;False;-1;False;7;Custom;0.5;True;False;-1000;False;TransparentCutout;;Overlay;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;5;False;-1;10;False;-1;0;4;False;-1;3;False;-1;0;False;-1;19;False;-1;0;False;0.01;1,1,1,0;VertexScale;False;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;16;0;11;0
WireConnection;16;1;12;0
WireConnection;0;2;17;0
WireConnection;0;10;16;0
ASEEND*/
//CHKSM=D1D5DA75390EE7DA565D8568F939C6CA9E95DEED