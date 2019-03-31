
//
// Copyright (c) 2009-2013 Mikko Mononen memon@inside.org
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//

/*
 * Por to C#
 * Copyright (c) 2016 Miguel A. Guirado L. https://sites.google.com/site/bitiopia/
 * 
 * 	NanoVG.net is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 *
 *  NanoVG.net is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NanoVG.net.  If not, see <http://www.gnu.org/licenses/>. See
 *  the file lgpl-3.0.txt for more details.
 */

#define NANOVG_GL3_IMPLEMENTATION
#define NANOVG_GL_USE_STATE_FILTER

#if NANOVG_GL2_IMPLEMENTATION
#define NANOVG_GL2
#define NANOVG_GL_IMPLEMENTATION
#elif NANOVG_GL3_IMPLEMENTATION
#define NANOVG_GL3
#define NANOVG_GL_IMPLEMENTATION
//#define NANOVG_GL_USE_UNIFORMBUFFER
#endif

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using TexPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace NanoVGDotNet.NanoVG
{
	public static class GlNanoVg
	{
	    private const int GlTrue = 1;

	    public const int IconLogin = 0xE740;
		public const int IconTrash = 0xE729;
		public const int IconSearch = 0x1F50D;
		public const int IconCircledCross = 0x2716;
		public const int IconChevronRight = 0xE75E;
		public const int IconCheck = 0x2713;

	    private static GlNvgContext _gl;

		#region SHADERS

	    private static string _shaderHeader =
#if NANOVG_GL2
			"#define NANOVG_GL2 1\n" +
#elif NANOVG_GL3
			"#version 150 core\n" +
			"#define NANOVG_GL3 1\n" +
#elif NANOVG_GLES2
			"#version 100\n" +
			"#define NANOVG_GL2 1\n" +
#elif NANOVG_GLES3
			"#version 300 es\n" +
			"#define NANOVG_GL3 1\n" +
#endif

#if NANOVG_GL_USE_UNIFORMBUFFER
			"#define USE_UNIFORMBUFFER 1\n" +
#else
			"#define UNIFORMARRAY_SIZE 11\n" +
#endif
			"\n";

	    private static string _fillVertShader =
			"#ifdef NANOVG_GL3\n" +
			"	uniform vec2 viewSize;\n" +
			"	in vec2 vertex;\n" +
			"	in vec2 tcoord;\n" +
			"	out vec2 ftcoord;\n" +
			"	out vec2 fpos;\n" +
			"#else\n" +
			"	uniform vec2 viewSize;\n" +
			"	attribute vec2 vertex;\n" +
			"	attribute vec2 tcoord;\n" +
			"	varying vec2 ftcoord;\n" +
			"	varying vec2 fpos;\n" +
			"#endif\n" +
			"void main(void) {\n" +
			"	ftcoord = tcoord;\n" +
			"	fpos = vertex;\n" +
			"	gl_Position = vec4(2.0*vertex.x/viewSize.x - 1.0, 1.0 - 2.0*vertex.y/viewSize.y, 0, 1);\n" +
			"}\n";

	    private static string _fillFragShader =
			"#ifdef GL_ES\n" +
			"#if defined(GL_FRAGMENT_PRECISION_HIGH) || defined(NANOVG_GL3)\n" +
			" precision highp float;\n" +
			"#else\n" +
			" precision mediump float;\n" +
			"#endif\n" +
			"#endif\n" +
			"#ifdef NANOVG_GL3\n" +
			"#ifdef USE_UNIFORMBUFFER\n" +
			"	layout(std140) uniform frag {\n" +
			"		mat3 scissorMat;\n" +
			"		mat3 paintMat;\n" +
			"		vec4 innerCol;\n" +
			"		vec4 outerCol;\n" +
			"		vec2 scissorExt;\n" +
			"		vec2 scissorScale;\n" +
			"		vec2 extent;\n" +
			"		float radius;\n" +
			"		float feather;\n" +
			"		float strokeMult;\n" +
			"		float strokeThr;\n" +
			"		int texType;\n" +
			"		int type;\n" +
			"	};\n" +
			"#else\n" + // NANOVG_GL3 && !USE_UNIFORMBUFFER
			"	uniform vec4 frag[UNIFORMARRAY_SIZE];\n" +
			"#endif\n" +
			"	uniform sampler2D tex;\n" +
			"	in vec2 ftcoord;\n" +
			"	in vec2 fpos;\n" +
			"	out vec4 outColor;\n" +
			"#else\n" + // !NANOVG_GL3
			"	uniform vec4 frag[UNIFORMARRAY_SIZE];\n" +
			"	uniform sampler2D tex;\n" +
			"	varying vec2 ftcoord;\n" +
			"	varying vec2 fpos;\n" +
			"#endif\n" +
			"#ifndef USE_UNIFORMBUFFER\n" +
			"	#define scissorMat mat3(frag[0].xyz, frag[1].xyz, frag[2].xyz)\n" +
			"	#define paintMat mat3(frag[3].xyz, frag[4].xyz, frag[5].xyz)\n" +
			"	#define innerCol frag[6]\n" +
			"	#define outerCol frag[7]\n" +
			"	#define scissorExt frag[8].xy\n" +
			"	#define scissorScale frag[8].zw\n" +
			"	#define extent frag[9].xy\n" +
			"	#define radius frag[9].z\n" +
			"	#define feather frag[9].w\n" +
			"	#define strokeMult frag[10].x\n" +
			"	#define strokeThr frag[10].y\n" +
			"	#define texType int(frag[10].z)\n" +
			"	#define type int(frag[10].w)\n" +
			"#endif\n" +
			"\n" +
			"float sdroundrect(vec2 pt, vec2 ext, float rad) {\n" +
			"	vec2 ext2 = ext - vec2(rad,rad);\n" +
			"	vec2 d = abs(pt) - ext2;\n" +
			"	return min(max(d.x,d.y),0.0) + length(max(d,0.0)) - rad;\n" +
			"}\n" +
			"\n" +
			"// Scissoring\n" +
			"float scissorMask(vec2 p) {\n" +
			"	vec2 sc = (abs((scissorMat * vec3(p,1.0)).xy) - scissorExt);\n" +
			"	sc = vec2(0.5,0.5) - sc * scissorScale;\n" +
			"	return clamp(sc.x,0.0,1.0) * clamp(sc.y,0.0,1.0);\n" +
			"}\n" +
			"#ifdef EDGE_AA\n" +
			"// Stroke - from [0..1] to clipped pyramid, where the slope is 1px.\n" +
			"float strokeMask() {\n" +
			"	return min(1.0, (1.0-abs(ftcoord.x*2.0-1.0))*strokeMult) * min(1.0, ftcoord.y);\n" +
			"}\n" +
			"#endif\n" +
			"\n" +
			"void main(void) {\n" +
			"   vec4 result;\n" +
			"	float scissor = scissorMask(fpos);\n" +
			"#ifdef EDGE_AA\n" +
			"	float strokeAlpha = strokeMask();\n" +
			"#else\n" +
			"	float strokeAlpha = 1.0;\n" +
			"#endif\n" +
			"	if (type == 0) {			// Gradient\n" +
			"		// Calculate gradient color using box gradient\n" +
			"		vec2 pt = (paintMat * vec3(fpos,1.0)).xy;\n" +
			"		float d = clamp((sdroundrect(pt, extent, radius) + feather*0.5) / feather, 0.0, 1.0);\n" +
			"		vec4 color = mix(innerCol,outerCol,d);\n" +
			"		// Combine alpha\n" +
			"		color *= strokeAlpha * scissor;\n" +
			"		result = color;\n" +
			"	} else if (type == 1) {		// Image\n" +
			"		// Calculate color fron texture\n" +
			"		vec2 pt = (paintMat * vec3(fpos,1.0)).xy / extent;\n" +
			"#ifdef NANOVG_GL3\n" +
			"		vec4 color = texture(tex, pt);\n" +
			"#else\n" +
			"		vec4 color = texture2D(tex, pt);\n" +
			"#endif\n" +
			"		if (texType == 1) color = vec4(color.xyz*color.w,color.w);" +
			"		if (texType == 2) color = vec4(color.x);" +
			"		// Apply color tint and alpha.\n" +
			"		color *= innerCol;\n" +
			"		// Combine alpha\n" +
			"		color *= strokeAlpha * scissor;\n" +
			"		result = color;\n" +
			"	} else if (type == 2) {		// Stencil fill\n" +
			"		result = vec4(1,1,1,1);\n" +
			"	} else if (type == 3) {		// Textured tris\n" +
			"#ifdef NANOVG_GL3\n" +
			"		vec4 color = texture(tex, ftcoord);\n" +
			"#else\n" +
			"		vec4 color = texture2D(tex, ftcoord);\n" +
			"#endif\n" +
			"		if (texType == 1) color = vec4(color.xyz*color.w,color.w);" +
			"		if (texType == 2) color = vec4(color.x);" +
			"		color *= scissor;\n" +
			"		result = color * innerCol;\n" +
			"	}\n" +
			"#ifdef EDGE_AA\n" +
			"	if (strokeAlpha < strokeThr) discard;\n" +
			"#endif\n" +
			"#ifdef NANOVG_GL3\n" +
			"	outColor = result;\n" +
			"#else\n" +
			"	gl_FragColor = result;\n" +
			"#endif\n" +
			"}\n";

		#endregion SHADERS

	    private static void CheckError(GlNvgContext gl, string str)
		{
		    if ((gl.Flags & (int)NvgCreateFlags.Debug) == 0)
				return;
			var err = GL.GetError();
		    if (err == ErrorCode.NoError) return;
		    Console.WriteLine($"Error {err} after {str}\n");
		}

	    private static void BindTexture(GlNvgContext gl, uint tex)
		{
#if NANOVG_GL_USE_STATE_FILTER
		    if (gl.BoundTexture == tex) return;
		    gl.BoundTexture = tex;
		    GL.BindTexture(TextureTarget.Texture2D, tex);
#else
			GL.BindTexture(TextureTarget.Texture2D, tex);
#endif
		}

	    private static int Maxi(int a, int b)
		{
			return a > b ? a : b;
		}

	    private static void DumpShaderError(int shader, string name, string type)
		{
            GL.GetShaderInfoLog(shader, out string info);
            // "Shader %s/%s error:\n%s\n", name, type, str
            Console.WriteLine($"Shader {name}/{type} error:\n{info}\n");
		}

	    private static void DumpProgramError(int prog, string name)
		{
            GL.GetShaderInfoLog(prog, out string sb);
            // printf("Program %s error:\n%s\n", name, str);
            Console.WriteLine($"Shader {name} error:\n{sb}\n");
		}

	    private static int CreateShader(out GlNvgShader shader, string name, string header, string opts, string vshader, string fshader)
		{
            var str = new string[3];
            str[0] = header;
			str[1] = opts ?? "";

			shader = new GlNvgShader();

			var prog = GL.CreateProgram();
			var vert = GL.CreateShader(ShaderType.VertexShader);
			var frag = GL.CreateShader(ShaderType.FragmentShader);
			str[2] = vshader;
			GL.ShaderSource(vert, str[0] + str[1] + vshader);
			str[2] = fshader;
			GL.ShaderSource(frag, str[0] + str[1] + fshader);

			GL.CompileShader(vert);
			GL.GetShader(vert, ShaderParameter.CompileStatus, out int status);
			if (status != GlTrue)
			{
				DumpShaderError(vert, name, "vert");
				return 0;
			}

			GL.CompileShader(frag);
			GL.GetShader(frag, ShaderParameter.CompileStatus, out status);
			if (status != GlTrue)
			{
				DumpShaderError(frag, name, "frag");
				return 0;
			}

			GL.AttachShader(prog, vert);
			GL.AttachShader(prog, frag);

			GL.BindAttribLocation(prog, 0, "vertex");
			GL.BindAttribLocation(prog, 1, "tcoord");

			GL.LinkProgram(prog);
			GL.GetProgram(prog, GetProgramParameterName.LinkStatus, out status);
			if (status != GlTrue)
			{
				DumpProgramError(prog, name);
				return 0;
			}

			shader.Prog = prog;
			shader.Vert = vert;
			shader.Frag = frag;

			return 1;
		}

	    private static GlNvgCall AllocCall(GlNvgContext gl)
		{
		    if (gl.Ncalls + 1 > gl.Ccalls)
			{
				var ccalls = Maxi(gl.Ncalls + 1, 128) + gl.Ccalls / 2; // 1.5x Overallocate
				//calls = (GLNVGcall*)realloc(gl->calls, sizeof(GLNVGcall) * ccalls);
				Array.Resize(ref gl.Calls, ccalls);

				for (var cont = gl.Ncalls; cont < ccalls; cont++)
					gl.Calls[cont] = new GlNvgCall();

				gl.Ccalls = ccalls;
			}

			var ret = gl.Calls[gl.Ncalls++];
			//memset(ret, 0, sizeof(GLNVGcall));
			return ret;
		}

	    private static int DeleteTexture(GlNvgContext gl, int id)
		{
			int i;
			for (i = 0; i < gl.Ntextures; i++)
			{
			    if (gl.Textures[i].Id != id) continue;
			    if (gl.Textures[i].Tex != 0 && (gl.Textures[i].Flags & (int)GlNvgImageFlags.NoDelete) == 0)
			        GL.DeleteTextures(1, ref gl.Textures[i].Tex);
			    //memset(&gl.textures[i], 0, sizeof(gl.textures[i]));
			    gl.Textures[i] = new GlNvgTexture();
			    return 1;
			}
			return 0;
		}

	    private static void AllocTexture(GlNvgContext gl, out GlNvgTexture tex)
		{
			int i;
			tex = null;

			for (i = 0; i < gl.Ntextures; i++)
			{
			    if (gl.Textures[i].Id != 0) continue;
			    tex = gl.Textures[i];
			    break;
			}
			if (tex == null)
			{
				if (gl.Ntextures + 1 > gl.Ctextures)
				{
					//GLNVGtexture[] textures;
					var ctextures = Maxi(gl.Ntextures + 1, 4) + gl.Ctextures / 2; // 1.5x Overallocate
					Array.Resize(ref gl.Textures, ctextures);
					//textures = new GLNVGtexture[ctextures];
					for (var cont = gl.Ntextures; cont < ctextures; cont++)
						gl.Textures[cont] = new GlNvgTexture();
					//gl.textures = textures;
					gl.Ctextures = ctextures;
				}
				tex = gl.Textures[gl.Ntextures++];
			}
			else
				tex = new GlNvgTexture();

			tex.Id = ++gl.TextureId;
		}

	    private static void GetUniforms(GlNvgShader shader)
		{
			shader.Loc[(int)GlNvgUniformLoc.LocViewSize] = GL.GetUniformLocation(shader.Prog, "viewSize");
			shader.Loc[(int)GlNvgUniformLoc.LocTex] = GL.GetUniformLocation(shader.Prog, "tex");

#if NANOVG_GL_USE_UNIFORMBUFFER
			shader.Loc[(int)GlNvgUniformLoc.LocFrag] = GL.GetUniformBlockIndex(shader.Prog, "frag");
#else
			shader.Loc[(int)GlNvgUniformLoc.LocFrag] = GL.GetUniformLocation(shader.Prog, "frag");
#endif
		}

	    private static int RenderCreate(object uptr)
		{
			var gl = (GlNvgContext)uptr;

            CheckError(gl, "init");

            if ((gl.Flags & (int)NvgCreateFlags.AntiAlias) != 0)
			{
				if (CreateShader(out gl.Shader, "shader", _shaderHeader, "#define EDGE_AA 1\n", _fillVertShader, _fillFragShader) == 0)
					return 0;
			}
			else
			{
				if (CreateShader(out gl.Shader, "shader", _shaderHeader, null, _fillVertShader, _fillFragShader) == 0)
					return 0;
			}

			CheckError(gl, "uniform locations");
			GetUniforms(gl.Shader);

			// Create dynamic vertex array
#if NANOVG_GL3
			GL.GenVertexArrays(1, out gl.VertArr);
#endif
			GL.GenBuffers(1, out gl.VertBuf);

#if NANOVG_GL_USE_UNIFORMBUFFER
			// Create UBOs
			var iBlock = gl.Shader.Loc[(int)GlNvgUniformLoc.LocFrag];
			GL.UniformBlockBinding(gl.Shader.Prog, iBlock, (int)GlNvgUniformBindings.FragBinding);
			GL.GenBuffers(1, out gl.FragBuf);
			GL.GetInteger(GetPName.UniformBufferOffsetAlignment, out int align);
#else
		    const int align = 4;
#endif

            var size = (int)GlNvgFragUniforms.GetSize; 
			gl.FragSize = size + align - size % align;

			CheckError(gl, "create done");

			GL.Finish();

			return 1;
		}

	    private static void DeleteShader(GlNvgShader shader)
		{
			if (shader.Prog != 0)
				GL.DeleteProgram(shader.Prog);
			if (shader.Vert != 0)
				GL.DeleteShader(shader.Vert);
			if (shader.Frag != 0)
				GL.DeleteShader(shader.Frag);
		}

	    private static void RenderCancel(object uptr)
		{
			var gl = (GlNvgContext)uptr;
			gl.Nverts = 0;
			gl.Npaths = 0;
			gl.Ncalls = 0;
			gl.Nuniforms = 0;
		}

		public static void RenderDelete(object uptr)
		{
			var gl = (GlNvgContext)uptr;
			int i;
			if (gl == null)
				return;

			DeleteShader(gl.Shader);

			#if NANOVG_GL3
			#if NANOVG_GL_USE_UNIFORMBUFFER
		    if (gl.FragBuf != 0)
		        GL.DeleteBuffer(gl.FragBuf);
			#endif
		if (gl.VertArr != 0)
			GL.DeleteVertexArray(gl.VertArr);
			#endif
			if (gl.VertBuf != 0)
				GL.DeleteBuffers(1, ref gl.VertBuf);

			for (i = 0; i < gl.Ntextures; i++)
			{
				if (gl.Textures[i].Tex != 0 && (gl.Textures[i].Flags & (int)GlNvgImageFlags.NoDelete) == 0)
					GL.DeleteTextures(1, ref gl.Textures[i].Tex);
			}
		}

	    private static int RenderDeleteTexture(object uptr, int image)
		{
			var gl = (GlNvgContext)uptr;
			return DeleteTexture(gl, image);
		}

	    private static int RenderCreateTexture2(object uptr, int type, int w, int h, int imageFlags, Bitmap bmp)
		{
			var gl = (GlNvgContext)uptr;
            AllocTexture(gl, out GlNvgTexture tex);

            GL.GenTextures(1, out tex.Tex);
			tex.Width = w;
			tex.Height = h;
			tex.Type = type;
			tex.Flags = imageFlags;
			BindTexture(gl, tex.Tex);

			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
			#if !NANOVG_GLES2
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, tex.Width);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
			#endif

			#if NANOVG_GL2
			// GL 1.4 and later has support for generating mipmaps using a tex parameter.
			if ((imageFlags & (int)NvgImageFlags.GenerateMipmaps) != 0)
			{
				//glTexParameteri(GL_TEXTURE_2D, GL_GENERATE_MIPMAP, GL_TRUE);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)GL_TRUE);
			}
			#endif

			var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
				                  ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		
			if (type == (int)NvgTexture.Rgba)
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0,
					TexPixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			else
				//glTexImage2D(GL_TEXTURE_2D, 0, GL_RED, w, h, 0, GL_RED, GL_UNSIGNED_BYTE, data);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, w, h, 0,
					TexPixelFormat.Red, PixelType.UnsignedByte, data.Scan0);

			bmp.UnlockBits(data);

			if ((imageFlags & (int)NvgImageFlags.GenerateMipmaps) != 0)
			{
				GL.TexParameter(TextureTarget.Texture2D,
					TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
			}
			else
			{
				GL.TexParameter(TextureTarget.Texture2D,
					TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
			}
			GL.TexParameter(TextureTarget.Texture2D,
				TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

			if ((imageFlags & (int)NvgImageFlags.RepeatX) != 0)
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
			else
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);

			if ((imageFlags & (int)NvgImageFlags.RepeatY) != 0)
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
			else
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);

			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
			#if !NANOVG_GLES2
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
			#endif

			CheckError(gl, "create tex");
			BindTexture(gl, 0);

			return tex.Id;
		}

	    private static int RenderCreateTexture(object uptr, int type, int w, int h, int imageFlags, byte[] data)
		{
			var gl = (GlNvgContext)uptr;
			GlNvgTexture tex;
			AllocTexture(gl, out tex);

			GL.GenTextures(1, out tex.Tex);
			tex.Width = w;
			tex.Height = h;
			tex.Type = type;
			tex.Flags = imageFlags;
			BindTexture(gl, tex.Tex);

			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
#if !NANOVG_GLES2
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, tex.Width);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
#endif

#if NANOVG_GL2
			// GL 1.4 and later has support for generating mipmaps using a tex parameter.
			if ((imageFlags & (int)NvgImageFlags.GenerateMipmaps) != 0)
			{
				//glTexParameteri(GL_TEXTURE_2D, GL_GENERATE_MIPMAP, GL_TRUE);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)GL_TRUE);
			}
#endif

			if (type == (int)NvgTexture.Rgba)
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0,
					TexPixelFormat.Rgba, PixelType.UnsignedByte, data);
			else
                //glTexImage2D(GL_TEXTURE_2D, 0, GL_RED, w, h, 0, GL_RED, GL_UNSIGNED_BYTE, data);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, w, h, 0,
					TexPixelFormat.Red, PixelType.UnsignedByte, data);

			if ((imageFlags & (int)NvgImageFlags.GenerateMipmaps) != 0)
			{
				GL.TexParameter(TextureTarget.Texture2D,
					TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
			}
			else
			{
				GL.TexParameter(TextureTarget.Texture2D,
					TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
			}
			GL.TexParameter(TextureTarget.Texture2D,
				TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

			if ((imageFlags & (int)NvgImageFlags.RepeatX) != 0)
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
			else
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);

			if ((imageFlags & (int)NvgImageFlags.RepeatY) != 0)
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
			else
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);

			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
#if !NANOVG_GLES2
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
#endif

			CheckError(gl, "create tex");
			BindTexture(gl, 0);

			return tex.Id;
		}

	    private static void RenderViewport(object uptr, int width, int height, float devicePixelRatio)
		{
			var gl = (GlNvgContext)uptr;
			gl.View[0] = width;
			gl.View[1] = height;
		}

	    private static void Vset(ref NvgVertex vtx, float x, float y, float u, float v)
		{
			vtx.X = x;
			vtx.Y = y;
			vtx.U = u;
			vtx.V = v;
		}

	    private static int MaxVertCount(NvgPath[] paths, int npaths)
		{
			int i, count = 0;
			for (i = 0; i < npaths; i++)
			{
				count += paths[i].Nfill;
				count += paths[i].Nstroke;
			}
			return count;
		}

	    private static int AllocPaths(GlNvgContext gl, int n)
		{
		    if (gl.Npaths + n > gl.Cpaths)
			{
				var cpaths = Maxi(gl.Npaths + n, 128) + gl.Cpaths / 2; // 1.5x Overallocate
				//paths = (GLNVGpath*)realloc(gl->paths, sizeof(GLNVGpath) * cpaths);
				Array.Resize(ref gl.Paths, cpaths);
				gl.Cpaths = cpaths;
			}
			var ret = gl.Npaths;
			gl.Npaths += n;
			return ret;
		}

	    private static int AllocVerts(GlNvgContext gl, int n)
		{
		    if (gl.Nverts + n > gl.Cverts)
			{
				var cverts = Maxi(gl.Nverts + n, 4096) + gl.Cverts / 2; // 1.5x Overallocate
				//verts = (NVGvertex*)realloc(gl->verts, sizeof(NVGvertex) * cverts);
				Array.Resize(ref gl.Verts, cverts);
				gl.Cverts = cverts;
			}
			var ret = gl.Nverts;
			gl.Nverts += n;
			return ret;
		}

	    private static int AllocFragUniforms(GlNvgContext gl, int n)
		{
			var structSize = gl.FragSize;
			if (gl.Nuniforms + n > gl.Cuniforms)
			{
				var cuniforms = Maxi(gl.Nuniforms + n, 128) + gl.Cuniforms / 2; // 1.5x Overallocate
				//uniforms = (unsigned char*)realloc(gl->uniforms, structSize * cuniforms);
				Array.Resize(ref gl.Uniforms, cuniforms);
				for (var cont = gl.Nuniforms; cont < cuniforms; cont++)
					gl.Uniforms[cont] = new GlNvgFragUniforms();
				gl.Cuniforms = cuniforms;
			}
			var ret = gl.Nuniforms * structSize;
			gl.Nuniforms += n;
			return ret;
		}

	    private static NvgColor PremulColor(NvgColor c)
		{
			c.R *= c.A;
			c.G *= c.A;
			c.B *= c.A;
			return c;
		}

	    private static void XformToMat3X4(float[] m3, float[] t)
		{
			m3[0] = t[0];
			m3[1] = t[1];
			m3[2] = 0.0f;
			m3[3] = 0.0f;
			m3[4] = t[2];
			m3[5] = t[3];
			m3[6] = 0.0f;
			m3[7] = 0.0f;
			m3[8] = t[4];
			m3[9] = t[5];
			m3[10] = 1.0f;
			m3[11] = 0.0f;
		}

	    private static void ConvertPaint(GlNvgContext gl, ref GlNvgFragUniforms frag, ref NvgPaint paint, ref NvgScissor scissor, float width, float fringe, float strokeThr)
		{
		    var invxform = new float[6];

			//memset((byte*)frag, 0, Marshal.SizeOf(*frag));

			frag.innerCol = PremulColor(paint.InnerColor);
			frag.outerCol = PremulColor(paint.OuterColor);

			if (scissor.Extent[0] < -0.5f || scissor.Extent[1] < -0.5f)
			{
				//memset((byte*)frag->unifGL2.scissorMat, 0, Marshal.SizeOf(frag->unifGL2.scissorMat));
				for (var cont = 0; cont < 12; cont++)
					frag.scissorMat[cont] = 0;
				frag.scissorExt[0] = 1.0f;
				frag.scissorExt[1] = 1.0f;
				frag.scissorScale[0] = 1.0f;
				frag.scissorScale[1] = 1.0f;
			}
			else
			{
				NanoVg.TransformInverse(invxform, scissor.Xform);
				XformToMat3X4(frag.scissorMat, invxform);
				frag.scissorExt[0] = scissor.Extent[0];
				frag.scissorExt[1] = scissor.Extent[1];
				frag.scissorScale[0] = (float)Math.Sqrt(scissor.Xform[0] * scissor.Xform[0] +
					scissor.Xform[2] * scissor.Xform[2]) / fringe;
				frag.scissorScale[1] = (float)Math.Sqrt(scissor.Xform[1] * scissor.Xform[1] +
					scissor.Xform[3] * scissor.Xform[3]) / fringe;
			}

			//memcpy((float*)frag.extent, paint.extent, 2);
			Array.Copy(paint.Extent, frag.extent, 2);
			frag.strokeMult = (width * 0.5f + fringe * 0.5f) / fringe;
			frag.strokeThr = strokeThr;

			if (paint.Image != 0)
			{
				var tex = FindTexture(gl, paint.Image);
				if (tex == null)
			        return;
			    if ((tex.Flags & (int)NvgImageFlags.FlipY) != 0)
				{
					var flipped = new float[6];
					NanoVg.TransformScale(flipped, 1.0f, -1.0f);
					NanoVg.NvgTransformMultiply(flipped, paint.Xform);
					NanoVg.TransformInverse(invxform, flipped);
				}
				else
				{
					NanoVg.TransformInverse(invxform, paint.Xform);
				}
				frag.Type = (int)GlNvgShaderType.FillImage;

				if (tex.Type == (int)NvgTexture.Rgba)
					frag.TexType = (tex.Flags & (int)NvgImageFlags.Premultiplied) != 0 ? 0 : 1;
				else
					frag.TexType = 2;
				//		printf("frag->texType = %d\n", frag->texType);
			}
			else
			{
				frag.Type = (int)GlNvgShaderType.FillGradient;
				frag.radius = paint.Radius;
				frag.feather = paint.Feather;
				NanoVg.TransformInverse(invxform, paint.Xform);
			}

			XformToMat3X4(frag.paintMat, invxform);

#if ONLY_FOR_DEBUG
			frag->ShowDebug();
#endif
		}

		public static int RenderUpdateTexture(object uptr, int image, int x, int y, int w, int h, byte[] data)
		{
			var gl = (GlNvgContext)uptr;
			var tex = FindTexture(gl, image);

			if (tex == null)
				return 0;
			BindTexture(gl, tex.Tex);

			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

#if NANOVG_GLES2 == false
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, tex.Width);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, x);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, y);
#else
			// No support for all of skip, need to update a whole row at a time.
			/*if (tex.type == (int)NVGtexture.NVG_TEXTURE_RGBA)
			data += y * tex.width * 4;
			else
			data += y * tex.width;*/
			x = 0;
			w = tex.width;
#endif

		    GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, w, h,
		        tex.Type == (int) NvgTexture.Rgba ? TexPixelFormat.Rgba : TexPixelFormat.Red, PixelType.UnsignedByte, data);

		    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
#if NANOVG_GLES2 == false
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
#endif

			BindTexture(gl, 0);

			return 1;
		}

		public static void RenderTriangles(object uptr, ref NvgPaint paint, ref NvgScissor scissor,
		                                          NvgVertex[] verts, int nverts)
		{
			var gl = (GlNvgContext)uptr;
			var call = AllocCall(gl);

		    //if (call == NULL) return;

			call.Type = (int)GlNvgCallType.Triangles;
			call.Image = paint.Image;

			// Allocate vertices for all the paths.
			call.TriangleOffset = AllocVerts(gl, nverts);
			if (call.TriangleOffset == -1)
				goto error;
			call.TriangleCount = nverts;

			//memcpy(&gl->verts[call->triangleOffset], verts, sizeof(NVGvertex) * nverts);
			Array.Copy(verts, 0, gl.Verts, call.TriangleOffset, nverts);

			// Fill shader
			call.UniformOffset = AllocFragUniforms(gl, 1);
			if (call.UniformOffset == -1)
				goto error;

			var frag = nvg__fragUniformPtr(gl, call.UniformOffset);
			// aquí 'frag' es una copia de 'gl.uniforms[call.uniformOffset]'

			ConvertPaint(gl, ref frag, ref paint, ref scissor, 1.0f, 1.0f, -1.0f);

			frag.Type = (int)GlNvgShaderType.ShaderImage;

			nvg__setFragUniform(gl, call.UniformOffset, ref frag);

			// only for debug
#if ONLY_FOR_DEBUG
			Console.WriteLine("Frag Show");
			frag.ShowDebug();
#endif

			// only for debug
#if ONLY_FOR_DEBUG
			Console.WriteLine("Uniforms[0] Show");
			gl.uniforms[0].ShowDebug();
#endif

			return;

			error:
			// We get here if call alloc was ok, but something else is not.
			// Roll back the last call to prevent drawing it.
			if (gl.Ncalls > 0)
				gl.Ncalls--;
		}

		public static int RenderGetTextureSize(object uptr, int image, ref int w, ref int h)
		{
			var gl = (GlNvgContext)uptr;
			var tex = FindTexture(gl, image);
			if (tex == null)
				return 0;
			w = tex.Width;
			h = tex.Height;
			return 1;
		}

		public static void RenderStroke(object uptr, ref NvgPaint paint, ref NvgScissor scissor,
		                                       float fringe, float strokeWidth, NvgPath[] paths, int npaths)
		{
			GlNvgFragUniforms frag;
			var gl = (GlNvgContext)uptr;
			var call = AllocCall(gl);
			//if (call == NULL) return;

			call.Type = (int)GlNvgCallType.Stroke;
			call.PathOffset = AllocPaths(gl, npaths);
			if (call.PathOffset == -1)
				goto error;
			call.PathCount = npaths;
			call.Image = paint.Image;

			// Allocate vertices for all the paths.
			var maxverts = MaxVertCount(paths, npaths);
			var offset = AllocVerts(gl, maxverts);
			if (offset == -1)
				goto error;

			for (var i = 0; i < npaths; i++)
			{
				var copy = gl.Paths[call.PathOffset + i];
				var path = paths[i];
				//memset(copy, 0, sizeof(GLNVGpath));
				copy.FillCount = 0;
				copy.FillOffset = 0;
				copy.StrokeCount = 0;
				copy.StrokeOffset = 0;

			    if (path.Nstroke == 0) continue;
			    copy.StrokeOffset = offset;
			    copy.StrokeCount = path.Nstroke;
			    //memcpy(&gl->verts[offset], path->stroke, sizeof(NVGvertex) * path->nstroke);
			    Array.Copy(path.Stroke, 0, gl.Verts, offset, path.Nstroke);
			    offset += path.Nstroke;
			    gl.Paths[call.PathOffset + i] = copy;
			}

			if ((gl.Flags & (int)NvgCreateFlags.StencilStrokes) != 0)
			{
				// Fill shader
				call.UniformOffset = AllocFragUniforms(gl, 2);
				if (call.UniformOffset == -1)
					goto error;

				frag = nvg__fragUniformPtr(gl, call.UniformOffset);
				ConvertPaint(gl, ref frag, ref paint, ref scissor, strokeWidth, fringe, -1.0f);
				// new setfrag
				nvg__setFragUniform(gl, call.UniformOffset, ref frag);

				frag = nvg__fragUniformPtr(gl, call.UniformOffset + gl.FragSize);
				ConvertPaint(gl, ref frag, ref paint, ref scissor, strokeWidth, fringe, 1.0f - 0.5f / 255.0f);
				// new setfrag
				nvg__setFragUniform(gl, call.UniformOffset + gl.FragSize, ref frag);
			}
			else
			{
				// Fill shader
				call.UniformOffset = AllocFragUniforms(gl, 1);
				if (call.UniformOffset == -1)
					goto error;
				frag = nvg__fragUniformPtr(gl, call.UniformOffset);
				ConvertPaint(gl, ref frag, ref paint, ref scissor, strokeWidth, fringe, -1.0f);
				// new setfrag
				nvg__setFragUniform(gl, call.UniformOffset, ref frag);
			}

			return;

			error:

			// We get here if call alloc was ok, but something else is not.
			// Roll back the last call to prevent drawing it.
			if (gl.Ncalls > 0)
				gl.Ncalls--;
		}

	    private static void RenderFill(object uptr, ref NvgPaint paint, ref NvgScissor scissor, float fringe,
		                              float[] bounds, NvgPath[] paths, int npaths)
		{
			var gl = (GlNvgContext)uptr;
			var call = AllocCall(gl);

		    GlNvgFragUniforms frag;

			call.Type = (int)GlNvgCallType.Fill;
			call.PathOffset = AllocPaths(gl, npaths);
			if (call.PathOffset == -1)
				goto error;
			call.PathCount = npaths;
			call.Image = paint.Image;

			if (npaths == 1 && paths[0].Convex != 0)
				call.Type = (int)GlNvgCallType.ConvexFill;

			// Allocate vertices for all the paths.
			var maxverts = MaxVertCount(paths, npaths) + 6;
			var offset = AllocVerts(gl, maxverts);
			if (offset == -1)
				goto error;

			for (var i = 0; i < npaths; i++)
			{
				var icopy = call.PathOffset + i;
				var copy = gl.Paths[icopy];
				var ipath = i;
				var path = paths[ipath];

				//memset(copy, 0, sizeof(GLNVGpath));
				copy.FillCount = 0;
				copy.FillOffset = 0;
				copy.StrokeCount = 0;
				copy.StrokeOffset = 0;

				if (path.Nfill > 0)
				{
					copy.FillOffset = offset;
					copy.FillCount = path.Nfill;
					//memcpy(&gl->verts[offset], path->fill, sizeof(NVGvertex) * path->nfill);
					Array.Copy(path.Fill, path.Ifill, gl.Verts, offset, path.Nfill);
					offset += path.Nfill;
				}
				if (path.Nstroke > 0)
				{
					copy.StrokeOffset = offset;
					copy.StrokeCount = path.Nstroke;
					//memcpy(&gl->verts[offset], path->stroke, sizeof(NVGvertex) * path->nstroke);
					Array.Copy(path.Stroke, path.Istroke, gl.Verts, offset, path.Nstroke);
					offset += path.Nstroke;
				}

				gl.Paths[icopy] = copy;
			}

			// Quad
			call.TriangleOffset = offset;
			call.TriangleCount = 6;
			var quad = gl.Verts;
			var iquad = call.TriangleOffset;
			Vset(ref quad[0 + iquad], bounds[0], bounds[3], 0.5f, 1.0f);
			Vset(ref quad[1 + iquad], bounds[2], bounds[3], 0.5f, 1.0f);
			Vset(ref quad[2 + iquad], bounds[2], bounds[1], 0.5f, 1.0f);

			Vset(ref quad[3 + iquad], bounds[0], bounds[3], 0.5f, 1.0f);
			Vset(ref quad[4 + iquad], bounds[2], bounds[1], 0.5f, 1.0f);
			Vset(ref quad[5 + iquad], bounds[0], bounds[1], 0.5f, 1.0f);

			// Setup uniforms for draw calls
			if (call.Type == (int)GlNvgCallType.Fill)
			{
				call.UniformOffset = AllocFragUniforms(gl, 2);
				if (call.UniformOffset == -1)
					goto error;
				// Simple shader for stencil
				frag = nvg__fragUniformPtr(gl, call.UniformOffset);
				//memset(frag, 0, sizeof(*frag));
				frag.strokeThr = -1.0f;
				frag.Type = (int)GlNvgShaderType.FillSimple;
				// new setfrag
				nvg__setFragUniform(gl, call.UniformOffset, ref frag);
				// Fill shader
				var frag1 = nvg__fragUniformPtr(gl, call.UniformOffset + gl.FragSize);
				ConvertPaint(gl, ref frag1, ref paint, ref scissor, fringe, fringe, -1.0f);
				// new setfrag
				nvg__setFragUniform(gl, call.UniformOffset + gl.FragSize, ref frag1);
			}
			else
			{
				call.UniformOffset = AllocFragUniforms(gl, 1);
				if (call.UniformOffset == -1)
					goto error;
				// Fill shader
				frag = nvg__fragUniformPtr(gl, call.UniformOffset);

#if ONLY_FOR_DEBUG
				frag.ShowDebug();
#endif

				ConvertPaint(gl, ref frag, ref paint, ref scissor, fringe, fringe, -1.0f);
				// new setfrag
				nvg__setFragUniform(gl, call.UniformOffset, ref frag);
			}

			return;

			error:
			// We get here if call alloc was ok, but something else is not.
			// Roll back the last call to prevent drawing it.
			if (gl.Ncalls > 0)
				gl.Ncalls--;
		}

	    private static BlendingFactorSrc glnvg_convertBlendFuncFactor(int factor)
		{
			//NVGblendFactor bf = (NVGblendFactor)factor;

			switch (factor)
			{
			    case (int)NvgBlendFactor.One:
			        return BlendingFactorSrc.One;
			    case (int)NvgBlendFactor.SrcColor:
			        return BlendingFactorSrc.Src1Color;
			    case (int)NvgBlendFactor.OneMinusSrcColor:
			        return BlendingFactorSrc.OneMinusSrc1Color;
			    case (int)NvgBlendFactor.DstColor:
			        return BlendingFactorSrc.DstColor;
			    case (int)NvgBlendFactor.OneMinusDstColor:
			        return BlendingFactorSrc.OneMinusDstColor;
			    case (int)NvgBlendFactor.SrcAlpha:
			        return BlendingFactorSrc.SrcAlpha;
			    case (int)NvgBlendFactor.OneMinusSrcAlpha:
			        return BlendingFactorSrc.OneMinusSrcAlpha;
			    case (int)NvgBlendFactor.DstAlpha:
			        return BlendingFactorSrc.DstAlpha;
			    case (int)NvgBlendFactor.OneMinusDstAlpha:
			        return BlendingFactorSrc.OneMinusDstAlpha;
			    case (int)NvgBlendFactor.SrcAlphaSaturate:
			        return BlendingFactorSrc.SrcAlphaSaturate;
			}
		    //if (factor == (int)NVGblendFactor.NVG_ZERO)
			return BlendingFactorSrc.Zero;
		}

	    private static void BlendCompositeOperation(NvgCompositeOperationState op)
		{
			var bfs1 = glnvg_convertBlendFuncFactor(op.SrcRgb);
			var bfd1 = (BlendingFactorDest)glnvg_convertBlendFuncFactor(op.DstRgb);
			var bfs2 = glnvg_convertBlendFuncFactor(op.SrcAlpha);
			var bfd2 = (BlendingFactorDest)glnvg_convertBlendFuncFactor(op.DstAlpha);
			/*
			int bs1 = (int)bfs1;
			int bs2 = (int)bfs2;
			int bd1 = (int)bfd1;
			int bd2 = (int)bfd2;
			*/
			GL.BlendFuncSeparate(bfs1, bfd1, bfs2, bfd2);
		}

	    private static void StencilMask(GlNvgContext gl, uint mask)
		{
#if NANOVG_GL_USE_STATE_FILTER
		    if (gl.StencilMask == mask) return;
		    gl.StencilMask = mask;
		    GL.StencilMask(mask);
#else
			GL.StencilMask(mask);
#endif
		}

	    private static void StencilFunc(GlNvgContext gl, StencilFunction func, int ref_, uint mask)
		{
#if NANOVG_GL_USE_STATE_FILTER
		    if (gl.StencilFunc == func && gl.StencilFuncRef == ref_ && gl.StencilFuncMask == mask) return;
		    gl.StencilFunc = func;
		    gl.StencilFuncRef = ref_;
		    gl.StencilFuncMask = mask;
		    GL.StencilFunc(func, ref_, mask);
#else
			GL.StencilFunc(func, ref_, mask);
#endif
		}

	    private static GlNvgTexture FindTexture(GlNvgContext gl, int id)
		{
			int i;
			for (i = 0; i < gl.Ntextures; i++)
				if (gl.Textures[i].Id == id)
					return gl.Textures[i];
			return null;
		}

		#region ¡POINTERS!

	    private static GlNvgFragUniforms nvg__fragUniformPtr(GlNvgContext gl, int offset)
		{
			// size of GLNVGfragUniforms = 180 bytes
			offset = offset / 180;

			return gl.Uniforms[offset];
		}

	    private static void nvg__setFragUniform(GlNvgContext gl, int offset, ref GlNvgFragUniforms frag)
		{
			// size of GLNVGfragUniforms = 180 bytes
			offset = offset / 180;

			gl.Uniforms[offset] = frag;
		}

	    private static void SetUniforms(GlNvgContext gl, int uniformOffset, int image)
		{
#if NANOVG_GL_USE_UNIFORMBUFFER
		    GL.BindBufferRange(BufferRangeTarget.UniformBuffer, (int) GlNvgUniformBindings.FragBinding, gl.FragBuf,
		        GlNvgFragUniforms.GetSize);
#else
			var frag = nvg__fragUniformPtr(gl, uniformOffset);

			//CorrigeFrag(ref frag);

			var lt = gl.Shader.Loc[(int)GlNvgUniformLoc.LocFrag];
			/*
			// only for debug
			Console.WriteLine("************** UniformsArray NO Corregido *************");
			frag.ShowDebug();

			CorrigeSetUniforms(ref frag);

			// only for debug
			Console.WriteLine("************** UniformsArray Corregido ****************");
			frag.ShowDebug();
			*/

			// GL.Uniform4(); NanoVG.NANOVG_GL_UNIFORMARRAY_SIZE = 11; Indica que se pasan 11 vectores de 4 floats

			var farr = frag.GetFloats;

			GL.Uniform4(lt, NanoVg.NanovgGlUniformarraySize, farr); //frag.uniformArray);

#endif

			if (image != 0)
			{
				var tex = FindTexture(gl, image);
				BindTexture(gl, tex?.Tex ?? 0);
				CheckError(gl, "tex paint tex");
			}
			else
			{
				BindTexture(gl, 0);
			}
		}

		#endregion ¡POINTERS!

	    private static void Fill(GlNvgContext gl, ref GlNvgCall call)
		{
			var paths = gl.Paths;
			var pathOffset = call.PathOffset;
			int i, npaths = call.PathCount;

			// Draw shapes
			GL.Enable(EnableCap.StencilTest);
			StencilMask(gl, 0xff);
			StencilFunc(gl, StencilFunction.Always, 0x00, 0xff);
			GL.ColorMask(false, false, false, false);

			// set bindpoint for solid loc
			SetUniforms(gl, call.UniformOffset, 0);
			CheckError(gl, "fill simple");

			GL.StencilOpSeparate(StencilFace.Front, StencilOp.Keep, StencilOp.Keep, StencilOp.IncrWrap);
			GL.StencilOpSeparate(StencilFace.Back, StencilOp.Keep, StencilOp.Keep, StencilOp.DecrWrap);

			GL.Disable(EnableCap.CullFace);
			for (i = 0; i < npaths; i++)
				GL.DrawArrays(PrimitiveType.TriangleFan, 
					paths[i + pathOffset].FillOffset,
					paths[i + pathOffset].FillCount);
			GL.Enable(EnableCap.CullFace);

			// Draw anti-aliased pixels
			GL.ColorMask(true, true, true, true);

			SetUniforms(gl, call.UniformOffset + gl.FragSize, call.Image);
			CheckError(gl, "fill fill");

			if ((gl.Flags & (int)NvgCreateFlags.AntiAlias) != 0)
			{
				StencilFunc(gl, StencilFunction.Equal, 0x00, 0xff);
				GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
				// Draw fringes
				for (i = 0; i < npaths; i++)
					GL.DrawArrays(PrimitiveType.TriangleStrip, 
						paths[i + pathOffset].StrokeOffset,
						paths[i + pathOffset].StrokeCount);
			}

			// Draw fill
			StencilFunc(gl, StencilFunction.Notequal, 0x0, 0xff);
			GL.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
			GL.DrawArrays(PrimitiveType.Triangles, call.TriangleOffset, call.TriangleCount);

			GL.Disable(EnableCap.StencilTest);
		}

	    private static void ConvexFill(GlNvgContext gl, ref GlNvgCall call)
		{
			var paths = gl.Paths;
			var pathOffset = call.PathOffset;
			int i, npaths = call.PathCount;

			SetUniforms(gl, call.UniformOffset, call.Image);
			CheckError(gl, "convex fill");

			for (i = 0; i < npaths; i++)
				GL.DrawArrays(PrimitiveType.TriangleFan, 
					paths[i + pathOffset].FillOffset,
					paths[i + pathOffset].FillCount);

		    if ((gl.Flags & (int) NvgCreateFlags.AntiAlias) == 0) return;
		    // Draw fringes
		    for (i = 0; i < npaths; i++)
		        GL.DrawArrays(PrimitiveType.TriangleStrip, 
		            paths[i + pathOffset].StrokeOffset,
		            paths[i + pathOffset].StrokeCount);
		}

	    private static void Stroke(GlNvgContext gl, ref GlNvgCall call)
		{
			var paths = gl.Paths;
			var pathOffset = call.PathOffset;
			int npaths = call.PathCount, i;

			if ((gl.Flags & (int)NvgCreateFlags.StencilStrokes) != 0)
			{

				GL.Enable(EnableCap.StencilTest);
				StencilMask(gl, 0xff);

				// Fill the stroke base without overlap
				StencilFunc(gl, StencilFunction.Equal, 0x0, 0xff);
				GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
				SetUniforms(gl, call.UniformOffset + gl.FragSize, call.Image);
				CheckError(gl, "stroke fill 0");

				for (i = 0; i < npaths; i++)
					GL.DrawArrays(PrimitiveType.TriangleStrip, 
						paths[i + pathOffset].StrokeOffset,
						paths[i + pathOffset].StrokeCount);

				// Draw anti-aliased pixels.
				SetUniforms(gl, call.UniformOffset, call.Image);
				StencilFunc(gl, StencilFunction.Equal, 0x00, 0xff);
				GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

				for (i = 0; i < npaths; i++)
					GL.DrawArrays(PrimitiveType.TriangleStrip, 
						paths[i + pathOffset].StrokeOffset,
						paths[i + pathOffset].StrokeCount);

				// Clear stencil buffer.
				GL.ColorMask(false, false, false, false);
				StencilFunc(gl, StencilFunction.Always, 0x0, 0xff);
				GL.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
				CheckError(gl, "stroke fill 1");

				for (i = 0; i < npaths; i++)
					GL.DrawArrays(PrimitiveType.TriangleStrip, 
						paths[i + pathOffset].StrokeOffset,
						paths[i + pathOffset].StrokeCount);
			
				GL.ColorMask(true, true, true, true);
				GL.Disable(EnableCap.StencilTest);

				// convertPaint(gl, nvg__fragUniformPtr(gl, call->uniformOffset + gl->fragSize), paint, scissor, strokeWidth, fringe, 1.0f - 0.5f/255.0f);

			}
			else
			{
				SetUniforms(gl, call.UniformOffset, call.Image);
				CheckError(gl, "stroke fill");

				// Draw Strokes
				for (i = 0; i < npaths; i++)
					GL.DrawArrays(PrimitiveType.TriangleStrip, 
						paths[i + pathOffset].StrokeOffset,
						paths[i + pathOffset].StrokeCount);
			}
		}

	    private static void Triangles(GlNvgContext gl, ref GlNvgCall call)
		{
			SetUniforms(gl, call.UniformOffset, call.Image);
			CheckError(gl, "triangles fill");

			GL.DrawArrays(PrimitiveType.Triangles, call.TriangleOffset, call.TriangleCount);
		}

	    private static void RenderFlush(object uptr, NvgCompositeOperationState compositeOperation)
		{
			var gl = (GlNvgContext)uptr;

		    if (gl.Ncalls > 0)
			{
				// Setup require GL state.
				GL.UseProgram(gl.Shader.Prog);

				BlendCompositeOperation(compositeOperation);
				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.Back);
				GL.FrontFace(FrontFaceDirection.Ccw);
				GL.Enable(EnableCap.Blend);
				GL.Disable(EnableCap.DepthTest);
				GL.Disable(EnableCap.ScissorTest);
				GL.ColorMask(true, true, true, true);
				GL.StencilMask(0xffffffff);
				GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
				GL.StencilFunc(StencilFunction.Always, 0, 0xffffffff);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, 0);

#if NANOVG_GL_USE_STATE_FILTER
				gl.BoundTexture = 0;
				gl.StencilMask = 0xffffffff;
				gl.StencilFunc = StencilFunction.Always;
				gl.StencilFuncRef = 0;
				gl.StencilFuncMask = 0xffffffff;
#endif

#if NANOVG_GL_USE_UNIFORMBUFFER
				// Upload ubo for frag shaders
				glBindBuffer(GL_UNIFORM_BUFFER, gl.FragBuf);
				glBufferData(GL_UNIFORM_BUFFER, gl.nuniforms * gl.fragSize, gl.uniforms, GL_STREAM_DRAW);
#endif

				// Upload vertex data
#if NANOVG_GL3
				GL.BindVertexArray(gl.VertArr);
#endif
				GL.BindBuffer(BufferTarget.ArrayBuffer, gl.VertBuf);
				//GL.BufferData(BufferTarget.ArrayBuffer, gl.nverts * Marshal.SizeOf(typeof(NVGvertex)), gl.verts, BufferUsageHint.StaticDraw);
				var iptr = (IntPtr)(gl.Nverts * Marshal.SizeOf(typeof(NvgVertex)));
				GL.BufferData(BufferTarget.ArrayBuffer, iptr, gl.Verts, BufferUsageHint.StreamDraw);
				GL.EnableVertexAttribArray(0);
				GL.EnableVertexAttribArray(1);

				var s = Marshal.SizeOf(typeof(NvgVertex));
				GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, s, IntPtr.Zero);
				var st = 2 * sizeof(float);
				GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, s, (IntPtr)st);

				// Set view and texture just once per frame.
				var loc1 = gl.Shader.Loc[(int)GlNvgUniformLoc.LocTex];
				GL.Uniform1(loc1, 0);
				var loc2 = gl.Shader.Loc[(int)GlNvgUniformLoc.LocViewSize];
				GL.Uniform2(loc2, 1, gl.View);

#if NANOVG_GL_USE_UNIFORMBUFFER
				glBindBuffer(GL_UNIFORM_BUFFER, gl->fragBuf);
#endif
                
			    for (var i = 0; i < gl.Ncalls; i++)
			    {
			        var call = gl.Calls[i];
			        switch (call.Type)
			        {
			            case (int)GlNvgCallType.Fill:
			                Fill(gl, ref call);
			                break;
			            case (int)GlNvgCallType.ConvexFill:
			                ConvexFill(gl, ref call);
			                break;
			            case (int)GlNvgCallType.Stroke:
			                Stroke(gl, ref call);
			                break;
			            case (int)GlNvgCallType.Triangles:
			                Triangles(gl, ref call);
			                break;
			        }
			    }

				GL.DisableVertexAttribArray(0);
				GL.DisableVertexAttribArray(1);
#if NANOVG_GL3
				GL.BindVertexArray(0);
#endif
				GL.Disable(EnableCap.CullFace);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				GL.UseProgram(0);
				BindTexture(gl, 0);
			}

			// Reset calls
			gl.Nverts = 0;
			gl.Npaths = 0;
			gl.Ncalls = 0;
			gl.Nuniforms = 0;
		}

		/// <summary>
		/// nvgCreateGL2 == nvgCreateGL3
		/// </summary>
		/// <param name="nvgFlags">Flags.</param>
		public static NvgContext CreateGl(NvgCreateFlags nvgFlags = NvgCreateFlags.None)
		{
		    var flags = (int) nvgFlags;
			var params_ = new NvgParams();
			_gl = new GlNvgContext();

			params_.RenderCreate = RenderCreate;
			params_.RenderCreateTextureByte = RenderCreateTexture;
			params_.RenderCreateTextureBmp = RenderCreateTexture2;
			params_.RenderFlush = RenderFlush;
			params_.RenderFill = RenderFill;
			params_.RenderStroke = RenderStroke;
			params_.RenderTriangles = RenderTriangles;
			params_.RenderGetTextureSize = RenderGetTextureSize;
			params_.RenderViewport = RenderViewport;
			params_.RenderUpdateTexture = RenderUpdateTexture;
			params_.RenderDeleteTexture = RenderDeleteTexture;
			params_.RenderCancel = RenderCancel;
			params_.RenderDelete = RenderDelete;
			params_.UserPtr = _gl;
			params_.EdgeAntiAlias = (flags & (int)NvgCreateFlags.AntiAlias) != 0 ? 1 : 0;

			_gl.Flags = flags;

			NanoVg.CreateInternal(ref params_, out NvgContext ctx);
		    return ctx;
		}
	}
}

