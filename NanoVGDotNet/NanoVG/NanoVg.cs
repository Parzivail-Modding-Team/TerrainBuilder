
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

//#define ONLY_FOR_DEBUG

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NanoVGDotNet.FontStash;

namespace NanoVGDotNet.NanoVG
{
    public static class NanoVg
    {
        public const float NvgPi = 3.14159265358979323846264338327f;

        private const int NvgInitFontimageSize = 512;
        private const int NvgMaxFontimageSize = 2048;
        public const int NvgMaxFontimages = 4;

        private const int NvgInitCommandsSize = 256;
        private const int NvgInitPointsSize = 128;
        private const int NvgInitPathsSize = 16;
        private const int NvgInitVertsSize = 256;
        public const int NvgMaxStates = 32;
        // Length proportional to radius of a cubic bezier handle for 90deg arcs.
        private const float NvgKappa90 = 0.5522847493f;

        //if defined NANOVG_GL2_IMPLEMENTATION
        public const int NanovgGlUniformarraySize = 11;

        private static float Sqrtf(float a)
        {
            return (float)Math.Sqrt(a);
        }

        private static float Modf(float a, float b)
        {
            return a % b;
        }

        private static float Sinf(float a)
        {
            return (float)Math.Sin(a);
        }

        private static float Cosf(float a)
        {
            return (float)Math.Cos(a);
        }

        private static float Tanf(float a)
        {
            return (float)Math.Tan(a);
        }

        private static float Atan2F(float a, float b)
        {
            return (float)Math.Atan2(a, b);
        }

        private static float Acosf(float a)
        {
            return (float)Math.Acos(a);
        }

        private static int Mini(int a, int b)
        {
            return a < b ? a : b;
        }

        private static int Maxi(int a, int b)
        {
            return a > b ? a : b;
        }

        private static int Clampi(int a, int mn, int mx)
        {
            return a < mn ? mn : (a > mx ? mx : a);
        }

        private static float Minf(float a, float b)
        {
            return a < b ? a : b;
        }

        private static float Maxf(float a, float b)
        {
            return a > b ? a : b;
        }

        private static float Absf(float a)
        {
            return a >= 0.0f ? a : -a;
        }

        private static float Signf(float a)
        {
            return a >= 0.0f ? 1.0f : -1.0f;
        }

        private static float Clampf(float a, float mn, float mx)
        {
            return a < mn ? mn : (a > mx ? mx : a);
        }

        private static float Cross(float dx0, float dy0, float dx1, float dy1)
        {
            return dx1 * dy0 - dx0 * dy1;
        }


        private static float Normalize(ref float x, ref float y)
        {
            var d = Sqrtf(x * x + y * y);
            if (!(d > 1e-6f)) return d;
            var id = 1.0f / d;
            x *= id;
            y *= id;
            return d;
        }

        private static void AllocPathCache(out NvgPathCache c)
        {
            c = new NvgPathCache
            {
                Points = new NvgPoint[NvgInitPointsSize],
                Npoints = 0,
                Cpoints = NvgInitPointsSize,
                Paths = new NvgPath[NvgInitPathsSize],
                Npaths = 0,
                Cpaths = NvgInitPathsSize,
                Verts = new NvgVertex[NvgInitVertsSize],
                Nverts = 0,
                Cverts = NvgInitVertsSize
            };
        }

        private static void SetDevicePixelRatio(ref NvgContext ctx, float ratio)
        {
            ctx.TessTol = 0.25f / ratio;
            ctx.DistTol = 0.01f / ratio;
            ctx.FringeWidth = 1.0f / ratio;
            ctx.DevicePxRatio = ratio;
        }

        private static NvgCompositeOperationState CompositeOperationState(NvgCompositeOperation op)
        {
            NvgBlendFactor sfactor = 0;
            NvgBlendFactor dfactor = 0;

            switch (op)
            {
                case NvgCompositeOperation.SourceOver:
                    sfactor = NvgBlendFactor.One;
                    dfactor = NvgBlendFactor.OneMinusSrcAlpha;
                    break;
                case NvgCompositeOperation.SourceIn:
                    sfactor = NvgBlendFactor.DstAlpha;
                    dfactor = NvgBlendFactor.Zero;
                    break;
                case NvgCompositeOperation.SourceOut:
                    sfactor = NvgBlendFactor.OneMinusDstAlpha;
                    dfactor = NvgBlendFactor.Zero;
                    break;
                case NvgCompositeOperation.Atop:
                    sfactor = NvgBlendFactor.DstAlpha;
                    dfactor = NvgBlendFactor.OneMinusSrcAlpha;
                    break;
                case NvgCompositeOperation.DestinationOver:
                    sfactor = NvgBlendFactor.OneMinusDstAlpha;
                    dfactor = NvgBlendFactor.One;
                    break;
                case NvgCompositeOperation.DestinationIn:
                    sfactor = NvgBlendFactor.Zero;
                    dfactor = NvgBlendFactor.SrcAlpha;
                    break;
                case NvgCompositeOperation.DestinationOut:
                    sfactor = NvgBlendFactor.Zero;
                    dfactor = NvgBlendFactor.OneMinusSrcAlpha;
                    break;
                case NvgCompositeOperation.DestinationAtop:
                    sfactor = NvgBlendFactor.OneMinusDstAlpha;
                    dfactor = NvgBlendFactor.SrcAlpha;
                    break;
                case NvgCompositeOperation.Lighter:
                    sfactor = NvgBlendFactor.One;
                    dfactor = NvgBlendFactor.One;
                    break;
                case NvgCompositeOperation.Copy:
                    sfactor = NvgBlendFactor.One;
                    dfactor = NvgBlendFactor.Zero;
                    break;
                case NvgCompositeOperation.Xor:
                    sfactor = NvgBlendFactor.OneMinusDstAlpha;
                    dfactor = NvgBlendFactor.OneMinusSrcAlpha;
                    break;
            }

            NvgCompositeOperationState state;
            state.SrcRgb = sfactor;
            state.DstRgb = dfactor;
            state.SrcAlpha = sfactor;
            state.DstAlpha = dfactor;
            return state;
        }

        private static NvgState GetState(this NvgContext ctx)
        {
            return ctx.States[ctx.Nstates - 1];
        }

        // State setting
        public static void FontSize(this NvgContext ctx, float size)
        {
            var state = GetState(ctx);
            state.FontSize = size;
        }

        public static void FontBlur(this NvgContext ctx, float blur)
        {
            var state = GetState(ctx);
            state.FontBlur = blur;
        }

        public static void FontFace(this NvgContext ctx, string font)
        {
            var state = GetState(ctx);
            state.FontId = FontStash.FontStash.fonsGetFontByName(ctx.Fs, font);
        }

        public static NvgColor Rgba(byte r, byte g, byte b, byte a)
        {
            var color = default(NvgColor);
            // Use longer initialization to suppress warning.
            color.R = r / 255.0f;
            color.G = g / 255.0f;
            color.B = b / 255.0f;
            color.A = a / 255.0f;

            return color;
        }

        public static NvgColor Rgba(uint argb)
        {
            var b = argb & 0x000000FF;
            var g = (argb & 0x0000FF00) >> 8;
            var r = (argb & 0x00FF0000) >> 16;
            var a = (argb & 0xFF000000) >> 24;
            var color = default(NvgColor);
            // Use longer initialization to suppress warning.
            color.R = r / 255.0f;
            color.G = g / 255.0f;
            color.B = b / 255.0f;
            color.A = a / 255.0f;

            return color;
        }

        public static NvgColor Rgba(float r, float g, float b, float a)
        {
            var color = default(NvgColor);
            // Use longer initialization to suppress warning.
            color.R = r;
            color.G = g;
            color.B = b;
            color.A = a;
            return color;
        }

        public static NvgColor Rgba(Color color)
        {
            return Rgba(color.R, color.G, color.B, color.A);
        }

        private static float GetAverageScale(float[] t)
        {
            var sx = (float)Math.Sqrt(t[0] * t[0] + t[2] * t[2]);
            var sy = (float)Math.Sqrt(t[1] * t[1] + t[3] * t[3]);
            return (sx + sy) * 0.5f;
        }

        private static int CurveDivs(float r, float arc, float tol)
        {
            var da = (float)Math.Acos(r / (r + tol)) * 2.0f;
            return Maxi(2, (int)Math.Ceiling(arc / da));
        }

        private static void ButtCapStart(NvgVertex[] dst, ref int idst, NvgPoint p,
                                      float dx, float dy, float w, float d, float aa)
        {
            var px = p.X - dx * d;
            var py = p.Y - dy * d;
            var dlx = dy;
            var dly = -dx;
            Vset(ref dst[idst], px + dlx * w - dx * aa, py + dly * w - dy * aa, 0, 0);
            idst++;
            Vset(ref dst[idst], px - dlx * w - dx * aa, py - dly * w - dy * aa, 1, 0);
            idst++;
            Vset(ref dst[idst], px + dlx * w, py + dly * w, 0, 1);
            idst++;
            Vset(ref dst[idst], px - dlx * w, py - dly * w, 1, 1);
            idst++;
        }

        private static void RoundCapStart(NvgVertex[] dst, ref int idst, NvgPoint p,
                                       float dx, float dy, float w, int ncap)
        {
            int i;
            var px = p.X;
            var py = p.Y;
            var dlx = dy;
            var dly = -dx;
            for (i = 0; i < ncap; i++)
            {
                var a = i / (float)(ncap - 1) * NvgPi;
                float ax = (float)Math.Cos(a) * w, ay = (float)Math.Sin(a) * w;
                Vset(ref dst[idst], px - dlx * ax - dx * ay, py - dly * ax - dy * ay, 0, 1);
                idst++;
                Vset(ref dst[idst], px, py, 0.5f, 1);
                idst++;
            }
            Vset(ref dst[idst], px + dlx * w, py + dly * w, 0, 1);
            idst++;
            Vset(ref dst[idst], px - dlx * w, py - dly * w, 1, 1);
            idst++;
        }

        private static void ButtCapEnd(NvgVertex[] dst, ref int idst, NvgPoint p,
                                    float dx, float dy, float w, float d, float aa)
        {
            var px = p.X + dx * d;
            var py = p.Y + dy * d;
            var dlx = dy;
            var dly = -dx;
            Vset(ref dst[idst], px + dlx * w, py + dly * w, 0, 1);
            idst++;
            Vset(ref dst[idst], px - dlx * w, py - dly * w, 1, 1);
            idst++;
            Vset(ref dst[idst], px + dlx * w + dx * aa, py + dly * w + dy * aa, 0, 0);
            idst++;
            Vset(ref dst[idst], px - dlx * w + dx * aa, py - dly * w + dy * aa, 1, 0);
            idst++;
        }

        private static void RoundCapEnd(NvgVertex[] dst, ref int idst, NvgPoint p,
                                     float dx, float dy, float w, int ncap)
        {
            int i;
            var px = p.X;
            var py = p.Y;
            var dlx = dy;
            var dly = -dx;
            Vset(ref dst[idst], px + dlx * w, py + dly * w, 0, 1);
            idst++;
            Vset(ref dst[idst], px - dlx * w, py - dly * w, 1, 1);
            idst++;
            for (i = 0; i < ncap; i++)
            {
                var a = i / (float)(ncap - 1) * NvgPi;
                float ax = (float)Math.Cos(a) * w, ay = (float)Math.Sin(a) * w;
                Vset(ref dst[idst], px, py, 0.5f, 1);
                idst++;
                Vset(ref dst[idst], px - dlx * ax + dx * ay, py - dly * ax + dy * ay, 0, 1);
                idst++;
            }
        }

        private static void RoundJoin(NvgVertex[] dst, ref int idst, NvgPoint p0, NvgPoint p1,
                                   float lw, float rw, float lu, float ru, int ncap)
        {
            int i, n;
            var dlx0 = p0.Dy;
            var dly0 = -p0.Dx;
            var dlx1 = p1.Dy;
            var dly1 = -p1.Dx;
            if ((p1.Flags & (int)NvgPointFlags.Left) != 0)
            {
                ChooseBevel(p1.Flags & (int)NvgPointFlags.InnerBevel, p0, p1, lw,
                    out float lx0, out float ly0, out float lx1, out float ly1);
                var a0 = (float)Math.Atan2(-dly0, -dlx0);
                var a1 = (float)Math.Atan2(-dly1, -dlx1);
                if (a1 > a0)
                    a1 -= NvgPi * 2;

                Vset(ref dst[idst], lx0, ly0, lu, 1);
                idst++;
                Vset(ref dst[idst], p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1);
                idst++;

                n = Clampi((int)Math.Ceiling((a0 - a1) / NvgPi * ncap), 2, ncap);
                for (i = 0; i < n; i++)
                {
                    var u = i / (float)(n - 1);
                    var a = a0 + u * (a1 - a0);
                    var rx = (float)(p1.X + Math.Cos(a) * rw);
                    var ry = (float)(p1.Y + Math.Sin(a) * rw);
                    Vset(ref dst[idst], p1.X, p1.Y, 0.5f, 1);
                    idst++;
                    Vset(ref dst[idst], rx, ry, ru, 1);
                    idst++;
                }

                Vset(ref dst[idst], lx1, ly1, lu, 1);
                idst++;
                Vset(ref dst[idst], p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1);
                idst++;

            }
            else
            {
                ChooseBevel(p1.Flags & (int)NvgPointFlags.InnerBevel, p0, p1, -rw,
                    out float rx0, out float ry0, out float rx1, out float ry1);
                var a0 = (float)Math.Atan2(dly0, dlx0);
                var a1 = (float)Math.Atan2(dly1, dlx1);
                if (a1 < a0)
                    a1 += NvgPi * 2;

                Vset(ref dst[idst], p1.X + dlx0 * rw, p1.Y + dly0 * rw, lu, 1);
                idst++;
                Vset(ref dst[idst], rx0, ry0, ru, 1);
                idst++;

                n = Clampi((int)Math.Ceiling((a1 - a0) / NvgPi * ncap), 2, ncap);
                for (i = 0; i < n; i++)
                {
                    var u = i / (float)(n - 1);
                    var a = a0 + u * (a1 - a0);
                    var lx = (float)(p1.X + Math.Cos(a) * lw);
                    var ly = (float)(p1.Y + Math.Sin(a) * lw);
                    Vset(ref dst[idst], lx, ly, lu, 1);
                    idst++;
                    Vset(ref dst[idst], p1.X, p1.Y, 0.5f, 1);
                    idst++;
                }

                Vset(ref dst[idst], p1.X + dlx1 * rw, p1.Y + dly1 * rw, lu, 1);
                idst++;
                Vset(ref dst[idst], rx1, ry1, ru, 1);
                idst++;

            }
            //return dst;
        }

        private static void ExpandStroke(this NvgContext ctx, float w, NvgLineCap lineCap, NvgLineCap lineJoin, float miterLimit)
        {
            var cache = ctx.Cache;
            var aa = ctx.FringeWidth;
            var ncap = CurveDivs(w, NvgPi, ctx.TessTol);  // Calculate divisions per half circle.

            CalculateJoins(ctx, w, lineJoin, miterLimit);

            // only for debug
#if ONLY_FOR_DEBUG
			Console.WriteLine("[expandStroke()]");
			for (int cont = 0; cont < cache.npoints; cont++)
			{
				Console.Write(String.Format("Cache-Points-index {0}: ", cont));
				Console.WriteLine(String.Format("\tvalueX: {0}\tvalueY: {1} \tflags: {2}",
				                                cache.points[cont].x, cache.points[cont].y, cache.points[cont].flags));
			}
#endif


            // Calculate max vertex usage.
            var cverts = 0;
            for (var i = 0; i < cache.Npaths; i++)
            {
                var path = cache.Paths[i];
                var loop = path.Closed == 0 ? 0 : 1;
                if (lineJoin == NvgLineCap.Round)
                    cverts += (path.Count + path.Nbevel * (ncap + 2) + 1) * 2; // plus one for loop
                else
                    cverts += (path.Count + path.Nbevel * 5 + 1) * 2; // plus one for loop
                if (loop != 0) continue;
                // space for caps
                if (lineCap == NvgLineCap.Round)
                {
                    cverts += (ncap * 2 + 2) * 2;
                }
                else
                {
                    cverts += (3 + 3) * 2;
                }
            }

            var verts = AllocTempVerts(ctx, cverts);

            if (verts == null)
                return;

            for (var i = 0; i < cache.Npaths; i++)
            {
                var path = cache.Paths[i];
                var ipts = path.First;
                var pts = cache.Points;
                NvgPoint p0;
                int ip0;
                NvgPoint p1;
                int ip1;
                int s, e;
                float dx, dy;
                var iverts = 0;

                path.Fill = null;
                path.Nfill = 0;
                path.Ifill = 0;

                // Calculate fringe or stroke
                var loop = path.Closed == 0 ? 0 : 1;
                var dst = verts;
                var idst = iverts;
                path.Stroke = dst;
                path.Istroke = idst;

                if (loop != 0)
                {
                    // Looping
                    ip0 = ipts + path.Count - 1;
                    p0 = pts[ip0];
                    ip1 = ipts + 0;
                    p1 = pts[ip1];
                    s = 0;
                    e = path.Count;
                }
                else
                {
                    // Add cap
                    ip0 = ipts + 0;
                    p0 = pts[ip0];
                    ip1 = ipts + 1;
                    p1 = pts[ip1];
                    s = 1;
                    e = path.Count - 1;
                }

                if (loop == 0)
                {
                    // Add cap
                    dx = p1.X - p0.X;
                    dy = p1.Y - p0.Y;
                    Normalize(ref dx, ref dy);
                    if (lineCap == (int)NvgLineCap.Butt)
                        ButtCapStart(dst, ref idst, p0, dx, dy, w, -aa * 0.5f, aa);
                    else switch (lineCap)
                        {
                            case NvgLineCap.Butt:
                            case NvgLineCap.Square:
                                ButtCapStart(dst, ref idst, p0, dx, dy, w, w - aa, aa);
                                break;
                            case NvgLineCap.Round:
                                RoundCapStart(dst, ref idst, p0, dx, dy, w, ncap);
                                break;
                        }

                }

                for (var j = s; j < e; ++j)
                {
                    if ((p1.Flags & (int)(NvgPointFlags.Bevel | NvgPointFlags.InnerBevel)) != 0)
                    {
                        if (lineJoin == NvgLineCap.Round)
                        {
                            RoundJoin(dst, ref idst, p0, p1, w, w, 0, 1, ncap);
                        }
                        else
                        {
                            BevelJoin(dst, ref idst, p0, p1, w, w, 0, 1);
                        }
                    }
                    else
                    {
                        Vset(ref dst[idst], p1.X + p1.Dmx * w, p1.Y + p1.Dmy * w, 0, 1);
                        idst++;
                        Vset(ref dst[idst], p1.X - p1.Dmx * w, p1.Y - p1.Dmy * w, 1, 1);
                        idst++;
                    }
                    p0 = p1;
                    ip1 += 1;
                    p1 = pts[ip1];
                }

                if (loop != 0)
                {
                    // Loop it
                    Vset(ref dst[idst], verts[0].X, verts[0].Y, 0, 1);
                    idst++;
                    Vset(ref dst[idst], verts[1].X, verts[1].Y, 1, 1);
                    idst++;
                }
                else
                {
                    // Add cap
                    dx = p1.X - p0.X;
                    dy = p1.Y - p0.Y;
                    Normalize(ref dx, ref dy);
                    if (lineCap == NvgLineCap.Butt)
                        ButtCapEnd(dst, ref idst, p1, dx, dy, w, -aa * 0.5f, aa);
                    else switch (lineCap)
                        {
                            case NvgLineCap.Butt:
                            case NvgLineCap.Square:
                                ButtCapEnd(dst, ref idst, p1, dx, dy, w, w - aa, aa);
                                break;
                            case NvgLineCap.Round:
                                RoundCapEnd(dst, ref idst, p1, dx, dy, w, ncap);
                                break;
                        }
                }

                path.Nstroke = idst - iverts;

                verts = dst;
            }
        }

        public static NvgPaint BoxGradient(this NvgContext ctx,
                                              float x, float y, float w, float h, float r, float f,
                                              NvgColor icol, NvgColor ocol)
        {
            var p = new NvgPaint();

            TransformIdentity(p.Xform);
            p.Xform[4] = x + w * 0.5f;
            p.Xform[5] = y + h * 0.5f;

            p.Extent[0] = w * 0.5f;
            p.Extent[1] = h * 0.5f;

            p.Radius = r;

            p.Feather = Maxf(1.0f, f);

            p.InnerColor = icol;
            p.OuterColor = ocol;

            return p;
        }

        public static void ClosePath(this NvgContext ctx)
        {
            var vals = new[] { (float)NvgCommands.Close };
            AppendCommands(ctx, vals, NVG_COUNTOF(vals));
        }

        public static void PathWinding(this NvgContext ctx, NvgWinding dir)
        {
            var vals = new[] { (float)NvgCommands.Winding, (int)dir };
            AppendCommands(ctx, vals, NVG_COUNTOF(vals));
        }

        public static void Stroke(this NvgContext ctx)
        {
            var state = GetState(ctx);
            var scale = GetAverageScale(state.Xform);
            var strokeWidth = Clampf(state.StrokeWidth * scale, 0.0f, 200.0f);
            var strokePaint = state.Stroke.Clone();
            int i;

            if (strokeWidth < ctx.FringeWidth)
            {
                // If the stroke width is less than pixel size, use alpha to emulate coverage.
                // Since coverage is area, scale by alpha*alpha.
                var alpha = Clampf(strokeWidth / ctx.FringeWidth, 0.0f, 1.0f);
                strokePaint.InnerColor.A *= alpha * alpha;
                strokePaint.OuterColor.A *= alpha * alpha;
                strokeWidth = ctx.FringeWidth;
            }

            // Apply global alpha
            strokePaint.InnerColor.A *= state.Alpha;
            strokePaint.OuterColor.A *= state.Alpha;

            FlattenPaths(ctx);

            if (ctx.Params.EdgeAntiAlias != 0)
                ExpandStroke(ctx, strokeWidth * 0.5f + ctx.FringeWidth * 0.5f,
                    (NvgLineCap)state.LineCap, (NvgLineCap)state.LineJoin, state.MiterLimit);
            else
                ExpandStroke(ctx, strokeWidth * 0.5f, (NvgLineCap)state.LineCap, (NvgLineCap)state.LineJoin, state.MiterLimit);

            ctx.Params.RenderStroke(ctx.Params.UserPtr, ref strokePaint, ref state.Scissor, ctx.FringeWidth,
                strokeWidth, ctx.Cache.Paths, ctx.Cache.Npaths);

            // Count triangles
            for (i = 0; i < ctx.Cache.Npaths; i++)
            {
                var path = ctx.Cache.Paths[i];
                ctx.StrokeTriCount += path.Nstroke - 2;
                ctx.DrawCallCount++;
            }
        }

        // State handling
        public static void Save(this NvgContext ctx)
        {
            if (ctx.Nstates >= NvgMaxStates)
                return;
            if (ctx.Nstates > 0)
                //memcpy(&ctx->states[ctx->nstates], &ctx->states[ctx->nstates-1], sizeof(NVGstate));
                ctx.States[ctx.Nstates] = ctx.States[ctx.Nstates - 1].Clone();
            ctx.Nstates++;
        }

        public static void Restore(this NvgContext ctx)
        {
            if (ctx.Nstates <= 1)
                return;
            ctx.Nstates--;
        }

        private static void TransformPremultiply(float[] t, float[] s)
        {
            var s2 = new float[6];
            //memcpy(s2, s, sizeof(float)*6);
            Array.Copy(s, s2, 6);
            NvgTransformMultiply(s2, t);
            //memcpy(t, s2, sizeof(float)*6);
            Array.Copy(s2, t, 6);
        }

        private static void TransformRotate(float[] t, float a)
        {
            float cs = Cosf(a), sn = Sinf(a);
            t[0] = cs;
            t[1] = sn;
            t[2] = -sn;
            t[3] = cs;
            t[4] = 0.0f;
            t[5] = 0.0f;
        }

        private static void TransformTranslate(float[] t, float tx, float ty)
        {
            t[0] = 1.0f;
            t[1] = 0.0f;
            t[2] = 0.0f;
            t[3] = 1.0f;
            t[4] = tx;
            t[5] = ty;
        }

        public static float DegToRad(float deg)
        {
            return deg / 180.0f * NvgPi;
        }

        // Scissoring
        public static void Scissor(this NvgContext ctx, float x, float y, float w, float h)
        {
            var state = GetState(ctx);

            w = Maxf(0.0f, w);
            h = Maxf(0.0f, h);

            TransformIdentity(state.Scissor.Xform);
            state.Scissor.Xform[4] = x + w * 0.5f;
            state.Scissor.Xform[5] = y + h * 0.5f;
            NvgTransformMultiply(state.Scissor.Xform, state.Xform);

            state.Scissor.Extent[0] = w * 0.5f;
            state.Scissor.Extent[1] = h * 0.5f;
        }

        private static void IsectRects(float[] dst,
                                    float ax, float ay, float aw, float ah,
                                    float bx, float by, float bw, float bh)
        {
            var minx = Maxf(ax, bx);
            var miny = Maxf(ay, by);
            var maxx = Minf(ax + aw, bx + bw);
            var maxy = Minf(ay + ah, by + bh);
            dst[0] = minx;
            dst[1] = miny;
            dst[2] = Maxf(0.0f, maxx - minx);
            dst[3] = Maxf(0.0f, maxy - miny);
        }

        public static void IntersectScissor(this NvgContext ctx, float x, float y, float w, float h)
        {
            var state = GetState(ctx);
            float[] pxform = new float[6], invxorm = new float[6];
            var rect = new float[4];

            // If no previous scissor has been set, set the scissor as current scissor.
            if (state.Scissor.Extent[0] < 0)
            {
                Scissor(ctx, x, y, w, h);
                return;
            }

            // Transform the current scissor rect into current transform space.
            // If there is difference in rotation, this will be approximation.
            //memcpy(pxform, state->scissor.xform, sizeof(float)*6);
            Array.Copy(state.Scissor.Xform, pxform, 6);
            var ex = state.Scissor.Extent[0];
            var ey = state.Scissor.Extent[1];
            TransformInverse(invxorm, state.Xform);
            NvgTransformMultiply(pxform, invxorm);
            var tex = ex * Absf(pxform[0]) + ey * Absf(pxform[2]);
            var tey = ex * Absf(pxform[1]) + ey * Absf(pxform[3]);

            // Intersect rects.
            IsectRects(rect, pxform[4] - tex, pxform[5] - tey, tex * 2, tey * 2, x, y, w, h);

            Scissor(ctx, rect[0], rect[1], rect[2], rect[3]);
        }

        public static void ResetScissor(this NvgContext ctx)
        {
            var state = GetState(ctx);
            //memset(state->scissor.xform, 0, sizeof(state->scissor.xform));
            for (var cont = 0; cont < state.Scissor.Xform.Length; cont++)
                state.Scissor.Xform[cont] = 0f;
            state.Scissor.Extent[0] = -1.0f;
            state.Scissor.Extent[1] = -1.0f;
        }

        public static void Rotate(this NvgContext ctx, float angle)
        {
            var state = GetState(ctx);
            var t = new float[6];
            TransformRotate(t, angle);
            TransformPremultiply(state.Xform, t);
        }

        public static void Scale(this NvgContext ctx, float x, float y)
        {
            var state = GetState(ctx);
            var t = new float[6];
            TransformScale(t, x, y);
            TransformPremultiply(state.Xform, t);
        }

        private static NvgPaint SetPaintColor(NvgColor color)
        {
            var p = new NvgPaint();
            // la anterior línea de código equivale a "memset(p, 0, sizeof(*p));", es
            // necesario de lo contrario aparece un degradado de color no uniforme
            TransformIdentity(p.Xform);
            p.Radius = 0.0f;
            p.Feather = 1.0f;
            p.InnerColor = color;
            p.OuterColor = color;

            return p;
        }

        public static void Translate(this NvgContext ctx, float x, float y)
        {
            var state = GetState(ctx);
            var t = new float[6];
            TransformTranslate(t, x, y);
            TransformPremultiply(state.Xform, t);
        }

        private static void Reset(this NvgContext ctx)
        {
            var state = GetState(ctx);

            state.Fill = SetPaintColor(Rgba(255, 255, 255, 255));
            state.Stroke = SetPaintColor(Rgba(0, 0, 0, 255));
            state.CompositeOperation = CompositeOperationState((int)NvgCompositeOperation.SourceOver);
            state.StrokeWidth = 1.0f;
            state.MiterLimit = 10.0f;
            state.LineCap = (int)NvgLineCap.Butt;
            state.LineJoin = (int)NvgLineCap.Miter;
            state.Alpha = 1.0f;
            TransformIdentity(state.Xform);

            state.Scissor.Extent[0] = -1.0f;
            state.Scissor.Extent[1] = -1.0f;

            state.FontSize = 16.0f;
            state.LetterSpacing = 0.0f;
            state.LineHeight = 1.0f;
            state.FontBlur = 0.0f;
            state.TextAlign = (int)NvgAlign.Left | (int)NvgAlign.Baseline;
            state.FontId = 0;
        }

        public static void CreateInternal(ref NvgParams params_, out NvgContext ctx)
        {
            var fontParams = new FONSparams();
            ctx = new NvgContext();
            int i;

            ctx.Params = params_;
            for (i = 0; i < NvgMaxFontimages; i++)
                ctx.FontImages[i] = 0;

            ctx.Commands = new float[NvgInitCommandsSize];
            ctx.Ncommands = 0;
            ctx.Ccommands = NvgInitCommandsSize;

            AllocPathCache(out ctx.Cache);

            Save(ctx);
            Reset(ctx);

            SetDevicePixelRatio(ref ctx, 1.0f);

            if (ctx.Params.RenderCreate(ctx.Params.UserPtr) == 0)
                return;

            // Init font rendering
            //memset(&fontParams, 0, sizeof(fontParams));
            fontParams.width = NvgInitFontimageSize;
            fontParams.height = NvgInitFontimageSize;
            fontParams.flags = FONSflags.FONS_ZERO_TOPLEFT;
            fontParams.renderCreate = null;
            fontParams.renderUpdate = null;
            fontParams.renderDraw = null;
            fontParams.renderDelete = null;
            fontParams.userPtr = null;
            ctx.Fs = FontStash.FontStash.fonsCreateInternal(ref fontParams);

            // Create font texture
            ctx.FontImages[0] = ctx.Params.RenderCreateTextureByte(ctx.Params.UserPtr, (int)NvgTexture.Alpha,
                fontParams.width, fontParams.height, 0, null);
            if (ctx.FontImages[0] == 0)
                throw new Exception("NanoVG.nvgCreateInternal(): Error, creating font texture");
            ctx.FontImageIdx = 0;
        }

        public static void DeleteImage(this NvgContext ctx, int image)
        {
            ctx.Params.RenderDeleteTexture(ctx.Params.UserPtr, image);
        }

        public static void EndFrame(this NvgContext ctx)
        {
            var state = GetState(ctx);
            //Corrige(state);
            ctx.Params.RenderFlush(ctx.Params.UserPtr, state.CompositeOperation);
            if (ctx.FontImageIdx == 0) return;
            var fontImage = ctx.FontImages[ctx.FontImageIdx];
            int i, j, iw = 0, ih = 0;
            // delete images that smaller than current one
            if (fontImage == 0)
                return;
            ImageSize(ctx, fontImage, ref iw, ref ih);
            for (i = j = 0; i < ctx.FontImageIdx; i++)
            {
                if (ctx.FontImages[i] != 0)
                {
                    int nw = 0, nh = 0;
                    ImageSize(ctx, ctx.FontImages[i], ref nw, ref nh);
                    if (nw < iw || nh < ih)
                        DeleteImage(ctx, ctx.FontImages[i]);
                    else
                        ctx.FontImages[j++] = ctx.FontImages[i];
                }
            }
            // make current font image to first
            ctx.FontImages[j++] = ctx.FontImages[0];
            ctx.FontImages[0] = fontImage;
            ctx.FontImageIdx = 0;
            // clear all images after j
            for (i = j; i < NvgMaxFontimages; i++)
                ctx.FontImages[i] = 0;
        }


        // Draw
        public static void BeginPath(this NvgContext ctx)
        {
            ctx.Ncommands = 0;
            ClearPathCache(ctx);
        }

        private static void ClearPathCache(this NvgContext ctx)
        {
            ctx.Cache.Npoints = 0;
            ctx.Cache.Npaths = 0;
        }

        private static void TransformPoint(out float dx, out float dy, float[] t, float sx, float sy)
        {
            dx = sx * t[0] + sy * t[2] + t[4];
            dy = sx * t[1] + sy * t[3] + t[5];
        }

        private static void AppendCommands(this NvgContext ctx, float[] vals, int nvals)
        {
            var state = GetState(ctx);

            if (ctx.Ncommands + nvals > ctx.Ccommands)
            {
                var ccommands = ctx.Ncommands + nvals + ctx.Ccommands / 2;
                //commands = (float*)realloc(ctx->commands, sizeof(float)*ccommands);
                Array.Resize(ref ctx.Commands, ccommands);
                ctx.Ccommands = ccommands;
            }

            if ((int)vals[0] != (int)NvgCommands.Close &&
                (int)vals[0] != (int)NvgCommands.Winding)
            {
                ctx.Commandx = vals[nvals - 2];
                ctx.Commandy = vals[nvals - 1];
            }

            // transform commands
            var i = 0;
            while (i < nvals)
            {
                var cmd = (int)vals[i];
                switch (cmd)
                {
                    case (int)NvgCommands.MoveTo:
                        TransformPoint(out vals[i + 1], out vals[i + 2], state.Xform, vals[i + 1], vals[i + 2]);
                        i += 3;
                        break;
                    case (int)NvgCommands.LineTo:
                        TransformPoint(out vals[i + 1], out vals[i + 2], state.Xform, vals[i + 1], vals[i + 2]);
                        i += 3;
                        break;
                    case (int)NvgCommands.BezierTo:
                        TransformPoint(out vals[i + 1], out vals[i + 2], state.Xform, vals[i + 1], vals[i + 2]);
                        TransformPoint(out vals[i + 3], out vals[i + 4], state.Xform, vals[i + 3], vals[i + 4]);
                        TransformPoint(out vals[i + 5], out vals[i + 6], state.Xform, vals[i + 5], vals[i + 6]);
                        i += 7;
                        break;
                    case (int)NvgCommands.Close:
                        i++;
                        break;
                    case (int)NvgCommands.Winding:
                        i += 2;
                        break;
                    default:
                        i++;
                        break;
                }
            }

            //memcpy(&ctx->commands[ctx->ncommands], vals, nvals * sizeof(float));

            Array.Copy(vals, 0, ctx.Commands, ctx.Ncommands, nvals);

            // only for debug
#if ONLY_FOR_DEBUG
			Console.WriteLine("C#");
			for (int cont = 0; cont < nvals; cont++)
			{
				Console.Write(String.Format("index {0}: ", cont));
				Console.WriteLine(String.Format("value: {0}", ctx.commands[ctx.ncommands + cont]));
			}
#endif

            ctx.Ncommands += nvals;
        }

        private static int NVG_COUNTOF(float[] arr)
        {
            return arr.Length; //(sizeof(arr) / sizeof(0[arr]));
        }

        private static void AddPath(this NvgContext ctx)
        {
            if (ctx.Cache.Npaths + 1 > ctx.Cache.Cpaths)
            {
                var cpaths = ctx.Cache.Npaths + 1 + ctx.Cache.Cpaths / 2;
                //paths = (NVGpath*)realloc(ctx->cache->paths, sizeof(NVGpath)*cpaths);
                Array.Resize(ref ctx.Cache.Paths, cpaths);
                var paths = ctx.Cache.Paths;
                if (paths == null)
                    return;
                ctx.Cache.Paths = paths;
                ctx.Cache.Cpaths = cpaths;
            }
            var path = ctx.Cache.Paths[ctx.Cache.Npaths];
            if (path == null)
            {
                path = new NvgPath();
                ctx.Cache.Paths[ctx.Cache.Npaths] = path;
            }
            else
            {
                path.Closed = 0;
                path.Convex = 0;
                path.Count = 0;
                path.Fill = null;
                path.Ifill = 0;
                path.First = 0;
                path.Nbevel = 0;
                path.Nfill = 0;
                path.Nstroke = 0;
                path.Stroke = null;
                path.Istroke = 0;
                path.Winding = 0;
            }

            path.First = ctx.Cache.Npoints;
            path.Winding = (int)NvgWinding.CounterClockwise;

            ctx.Cache.Npaths++;
        }

        private static NvgPoint LastPoint(this NvgContext ctx)
        {
            if (ctx.Cache.Npoints > 0)
                return ctx.Cache.Points[ctx.Cache.Npoints - 1];
            return null;
        }

        private static NvgPath LastPath(this NvgContext ctx)
        {
            if (ctx.Cache.Npaths > 0)
                return ctx.Cache.Paths[ctx.Cache.Npaths - 1];
            return null;
        }

        private static bool PtEquals(float x1, float y1, float x2, float y2, float tol)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            return dx * dx + dy * dy < tol * tol;
        }

        private static void AddPoint(this NvgContext ctx, float x, float y, int flags)
        {
            NvgPoint pt;
            var path = LastPath(ctx);
            if (path == null)
                return;

            if (path.Count > 0 && ctx.Cache.Npoints > 0)
            {
                pt = LastPoint(ctx);
                if (PtEquals(pt.X, pt.Y, x, y, ctx.DistTol))
                {
                    pt.Flags |= (byte)flags;
                    return;
                }
            }

            if (ctx.Cache.Npoints + 1 > ctx.Cache.Cpoints)
            {
                var cpoints = ctx.Cache.Npoints + 1 + ctx.Cache.Cpoints / 2;
                //points = (NVGpoint*)realloc(ctx->cache->points, sizeof(NVGpoint)*cpoints);
                Array.Resize(ref ctx.Cache.Points, cpoints);
                var points = ctx.Cache.Points;

                if (points == null)
                    return;
                ctx.Cache.Points = points;
                ctx.Cache.Cpoints = cpoints;
            }

            pt = new NvgPoint();

            ctx.Cache.Points[ctx.Cache.Npoints] = pt;
            //memset(pt, 0, sizeof(*pt));
            pt.X = x;
            pt.Y = y;
            pt.Flags = (byte)flags;

            //only for debug
#if ONLY_FOR_DEBUG
			Console.WriteLine(String.Format("Added point: {0}", pt));
#endif

            ctx.Cache.Npoints++;
            path.Count++;
        }

        private static void ClosePathInternal(this NvgContext ctx)
        {
            var path = LastPath(ctx);
            if (path == null)
                return;
            path.Closed = 1;
        }

        private static void PathWindingInternal(this NvgContext ctx, int winding)
        {
            var path = LastPath(ctx);
            if (path == null)
                return;
            path.Winding = winding;
        }

        private static float Triarea2(float ax, float ay, float bx, float by, float cx, float cy)
        {
            var abx = bx - ax;
            var aby = by - ay;
            var acx = cx - ax;
            var acy = cy - ay;
            return acx * aby - abx * acy;
        }

        private static float PolyArea(NvgPoint[] pts, int ipts, int npts)
        {
            int i;
            float area = 0;
            for (i = 2; i < npts; i++)
            {
                var a = pts[0 + ipts];
                var b = pts[i - 1 + ipts];
                var c = pts[i + ipts];
                area += Triarea2(a.X, a.Y, b.X, b.Y, c.X, c.Y);
            }
            return area * 0.5f;
        }

        private static void PolyReverse(NvgPoint[] pts, int ipts, int npts)
        {
            int i = 0, j = npts - 1;
            while (i < j)
            {
                var tmp = pts[i + ipts].Clone();
                pts[i + ipts] = pts[j + ipts].Clone();
                pts[j + ipts] = tmp;
                i++;
                j--;
            }
        }

        private static void TesselateBezier(NvgContext ctx,
                                         float x1, float y1, float x2, float y2,
                                         float x3, float y3, float x4, float y4,
                                         int level, int type)
        {
            if (level > 10)
                return;

            var x12 = (x1 + x2) * 0.5f;
            var y12 = (y1 + y2) * 0.5f;
            var x23 = (x2 + x3) * 0.5f;
            var y23 = (y2 + y3) * 0.5f;
            var x34 = (x3 + x4) * 0.5f;
            var y34 = (y3 + y4) * 0.5f;
            var x123 = (x12 + x23) * 0.5f;
            var y123 = (y12 + y23) * 0.5f;

            var dx = x4 - x1;
            var dy = y4 - y1;
            var d2 = Absf((x2 - x4) * dy - (y2 - y4) * dx);
            var d3 = Absf((x3 - x4) * dy - (y3 - y4) * dx);

            if ((d2 + d3) * (d2 + d3) < ctx.TessTol * (dx * dx + dy * dy))
            {
                AddPoint(ctx, x4, y4, type);
                return;
            }

            /*	if (absf(x1+x3-x2-x2) + absf(y1+y3-y2-y2) + absf(x2+x4-x3-x3) + absf(y2+y4-y3-y3) < ctx->tessTol) {
					addPoint(ctx, x4, y4, type);
				return;
			}*/

            var x234 = (x23 + x34) * 0.5f;
            var y234 = (y23 + y34) * 0.5f;
            var x1234 = (x123 + x234) * 0.5f;
            var y1234 = (y123 + y234) * 0.5f;

            TesselateBezier(ctx, x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1, 0);
            TesselateBezier(ctx, x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1, type);
        }

        private static void FlattenPaths(this NvgContext ctx)
        {
            var cache = ctx.Cache;

            if (cache.Npaths > 0)
                return;

            // Flatten
            var i = 0;
            while (i < ctx.Ncommands)
            {
                var cmd = (int)ctx.Commands[i];

                float[] p;
                int ip;
                switch (cmd)
                {
                    case (int)NvgCommands.MoveTo:
                        AddPath(ctx);
                        p = ctx.Commands;
                        ip = i + 1;
                        AddPoint(ctx, p[0 + ip], p[1 + ip], (int)NvgPointFlags.Corner);
                        i += 3;
                        break;
                    case (int)NvgCommands.LineTo:
                        p = ctx.Commands;
                        ip = i + 1;
                        AddPoint(ctx, p[0 + ip], p[1 + ip], (int)NvgPointFlags.Corner);
                        i += 3;
                        break;
                    case (int)NvgCommands.BezierTo:
                        var last = LastPoint(ctx);
                        if (last != null)
                        {
                            var cp1 = ctx.Commands;
                            var icp1 = i + 1;
                            var cp2 = ctx.Commands;
                            var icp2 = i + 3;
                            p = ctx.Commands;
                            ip = i + 5;
                            TesselateBezier(ctx, last.X, last.Y,
                                cp1[0 + icp1], cp1[1 + icp1],
                                cp2[0 + icp2], cp2[1 + icp2],
                                p[0 + ip],
                                p[1 + ip],
                                0, (int)NvgPointFlags.Corner);
                        }
                        i += 7;
                        break;
                    case (int)NvgCommands.Close:
                        ClosePathInternal(ctx);
                        i++;
                        break;
                    case (int)NvgCommands.Winding:
                        PathWindingInternal(ctx, (int)ctx.Commands[i + 1]);
                        i += 2;
                        break;
                    default:
                        i++;
                        break;
                }
            }

            cache.Bounds[0] = cache.Bounds[1] = 1e6f;
            cache.Bounds[2] = cache.Bounds[3] = -1e6f;

            // Calculate the direction and length of line segments.
            for (var j = 0; j < cache.Npaths; j++)
            {
                var path = cache.Paths[j];
                var pts = cache.Points;
                var ipts = path.First;

                // If the first and last points are the same, remove the last, mark as closed path.
                var p0 = pts[ipts + path.Count - 1];
                var ip1 = ipts;
                var p1 = pts[ip1];

                if (PtEquals(p0.X, p0.Y, p1.X, p1.Y, ctx.DistTol))
                {
                    if (ipts > 0)
                        path.Count--;
                    p0 = pts[ipts + path.Count - 1];
                    path.Closed = 1;
                }

                // Enforce winding.
                if (path.Count > 2)
                {
                    var area = PolyArea(pts, ipts, path.Count);
                    if (path.Winding == (int)NvgWinding.CounterClockwise && area < 0.0f)
                    {
                        PolyReverse(pts, ipts, path.Count);
                        p0 = pts[ipts + path.Count - 1];
                        p1 = pts[ip1];
                    }
                    if (path.Winding == (int)NvgWinding.Clockwise && area > 0.0f)
                    {
                        PolyReverse(pts, ipts, path.Count);
                        p0 = pts[ipts + path.Count - 1];
                        p1 = pts[ip1];
                    }
                }

                for (i = 0; i < path.Count; i++)
                {
                    // Calculate segment direction and length
                    p0.Dx = p1.X - p0.X;
                    p0.Dy = p1.Y - p0.Y;
                    p0.Len = Normalize(ref p0.Dx, ref p0.Dy);
                    // Update bounds
                    cache.Bounds[0] = Minf(cache.Bounds[0], p0.X);
                    cache.Bounds[1] = Minf(cache.Bounds[1], p0.Y);
                    cache.Bounds[2] = Maxf(cache.Bounds[2], p0.X);
                    cache.Bounds[3] = Maxf(cache.Bounds[3], p0.Y);
                    // Advance
                    p0 = p1;
                    ip1 += 1;
                    if (ip1 < pts.Length)
                        p1 = pts[ip1];
                }
            }
        }

        public static void NvgTransformMultiply(float[] t, float[] s)
        {
            var t0 = t[0] * s[0] + t[1] * s[2];
            var t2 = t[2] * s[0] + t[3] * s[2];
            var t4 = t[4] * s[0] + t[5] * s[2] + s[4];
            t[1] = t[0] * s[1] + t[1] * s[3];
            t[3] = t[2] * s[1] + t[3] * s[3];
            t[5] = t[4] * s[1] + t[5] * s[3] + s[5];
            t[0] = t0;
            t[2] = t2;
            t[4] = t4;
        }

        public static void LineJoin(this NvgContext ctx, NvgLineCap join)
        {
            var state = GetState(ctx);
            state.LineJoin = (int)join;
        }

        public static void MoveTo(this NvgContext ctx, float x, float y)
        {
            var vals = new[] { (float)NvgCommands.MoveTo, x, y };
            AppendCommands(ctx, vals, NVG_COUNTOF(vals));
        }

        public static void BezierTo(this NvgContext ctx, float c1X, float c1Y, float c2X, float c2Y, float x, float y)
        {
            var vals = new[] { (float)NvgCommands.BezierTo, c1X, c1Y, c2X, c2Y, x, y };
            AppendCommands(ctx, vals, NVG_COUNTOF(vals));
        }

        public static void LineTo(this NvgContext ctx, float x, float y)
        {
            var vals = new[] { (float)NvgCommands.LineTo, x, y };
            AppendCommands(ctx, vals, NVG_COUNTOF(vals));
        }

        public static void LineCap(this NvgContext ctx, NvgLineCap cap)
        {
            var state = GetState(ctx);
            state.LineCap = (int)cap;
        }

        public static void FillPaint(this NvgContext ctx, NvgPaint paint)
        {
            var state = GetState(ctx);
            state.Fill = paint.Clone();
            NvgTransformMultiply(state.Fill.Xform, state.Xform);
        }

        public static void FillColor(this NvgContext ctx, NvgColor color)
        {
            var state = GetState(ctx);
            state.Fill = SetPaintColor(color);
        }

        public static void StrokePaint(this NvgContext ctx, NvgPaint paint)
        {
            var state = GetState(ctx);
            state.Stroke = paint.Clone();
            NvgTransformMultiply(state.Stroke.Xform, state.Xform);
        }

        public static void StrokeColor(this NvgContext ctx, NvgColor color)
        {
            var state = GetState(ctx);
            state.Stroke = SetPaintColor(color);
        }

        // State setting
        public static void StrokeWidth(this NvgContext ctx, float width)
        {
            var state = GetState(ctx);
            state.StrokeWidth = width;
        }

        private static void Vset(ref NvgVertex vtx, float x, float y, float u, float v)
        {
            vtx.X = x;
            vtx.Y = y;
            vtx.U = u;
            vtx.V = v;
        }

        private static void CalculateJoins(this NvgContext ctx, float w, NvgLineCap lineJoin, float miterLimit)
        {
            var cache = ctx.Cache;
            var iw = 0.0f;

            if (w > 0.0f)
                iw = 1.0f / w;

            // Calculate which joins needs extra vertices to append, and gather vertex count.
            for (var i = 0; i < cache.Npaths; i++)
            {
                var path = cache.Paths[i];

                var ipts = path.First;
                var pts = cache.Points;

                var p0 = pts[ipts + path.Count - 1];

                var ip1 = ipts;
                var p1 = pts[ip1 + 0];

                var nleft = 0;

                path.Nbevel = 0;

                for (var j = 0; j < path.Count; j++)
                {
                    var dlx0 = p0.Dy;
                    var dly0 = -p0.Dx;
                    var dlx1 = p1.Dy;
                    var dly1 = -p1.Dx;
                    // Calculate extrusions
                    p1.Dmx = (dlx0 + dlx1) * 0.5f;
                    p1.Dmy = (dly0 + dly1) * 0.5f;
                    var dmr2 = p1.Dmx * p1.Dmx + p1.Dmy * p1.Dmy;
                    if (dmr2 > 0.000001f)
                    {
                        var scale = 1.0f / dmr2;
                        if (scale > 600.0f)
                        {
                            scale = 600.0f;
                        }
                        p1.Dmx *= scale;
                        p1.Dmy *= scale;
                    }

                    // Clear flags, but keep the corner.
                    p1.Flags = (byte)((p1.Flags &
                                       (int)NvgPointFlags.Corner) != 0 ? (int)NvgPointFlags.Corner : 0);

                    // Keep track of left turns.
                    var cross = p1.Dx * p0.Dy - p0.Dx * p1.Dy;
                    if (cross > 0.0f)
                    {
                        nleft++;
                        p1.Flags |= (int)NvgPointFlags.Left;
                    }

                    // Calculate if we should use bevel or miter for inner join.
                    var limit = Maxf(1.01f, Minf(p0.Len, p1.Len) * iw);
                    if (dmr2 * limit * limit < 1.0f)
                        p1.Flags |= (int)NvgPointFlags.InnerBevel;

                    // Check to see if the corner needs to be beveled.
                    if ((p1.Flags & (int)NvgPointFlags.Corner) != 0)
                    {
                        if (dmr2 * miterLimit * miterLimit < 1.0f ||
                            lineJoin == NvgLineCap.Bevel ||
                            lineJoin == NvgLineCap.Round)
                        {
                            p1.Flags |= (int)NvgPointFlags.Bevel;
                        }
                    }

                    if ((p1.Flags & ((int)NvgPointFlags.Bevel | (int)NvgPointFlags.InnerBevel)) != 0)
                        path.Nbevel++;

                    p0 = p1;
                    ip1 += 1;
                    if (ip1 < pts.Length)
                        p1 = pts[ip1];
                }

                path.Convex = nleft == path.Count ? 1 : 0;
            }
        }

        private static NvgVertex[] AllocTempVerts(this NvgContext ctx, int nverts)
        {
            if (nverts <= ctx.Cache.Cverts) return ctx.Cache.Verts;
            var cverts = (nverts + 0xff) & ~0xff; // Round up to prevent allocations when things change just slightly.
            //verts = (NVGvertex*)realloc(ctx->cache->verts, sizeof(NVGvertex)*cverts);
            Array.Resize(ref ctx.Cache.Verts, cverts);
            ctx.Cache.Cverts = cverts;

            return ctx.Cache.Verts;
        }

        private static void ChooseBevel(int bevel, NvgPoint p0, NvgPoint p1, float w,
                                     out float x0, out float y0, out float x1, out float y1)
        {
            if (bevel != 0)
            {
                x0 = p1.X + p0.Dy * w;
                y0 = p1.Y - p0.Dx * w;
                x1 = p1.X + p1.Dy * w;
                y1 = p1.Y - p1.Dx * w;
            }
            else
            {
                x0 = p1.X + p1.Dmx * w;
                y0 = p1.Y + p1.Dmy * w;
                x1 = p1.X + p1.Dmx * w;
                y1 = p1.Y + p1.Dmy * w;
            }
        }

        private static void BevelJoin(NvgVertex[] dst, ref int idst,
                                   NvgPoint p0, NvgPoint p1, float lw, float rw, float lu, float ru)
        {
            float rx0, ry0;
            float lx0, ly0;
            var dlx0 = p0.Dy;
            var dly0 = -p0.Dx;
            var dlx1 = p1.Dy;
            var dly1 = -p1.Dx;

            if ((p1.Flags & (int)NvgPointFlags.Left) != 0)
            {
                ChooseBevel(p1.Flags & (int)NvgPointFlags.InnerBevel,
                    p0, p1, lw, out lx0, out ly0, out float lx1, out float ly1);

                Vset(ref dst[idst], lx0, ly0, lu, 1);
                idst++;
                Vset(ref dst[idst], p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1);
                idst++;

                if ((p1.Flags & (int)NvgPointFlags.Bevel) != 0)
                {
                    Vset(ref dst[idst], lx0, ly0, lu, 1);
                    idst++;
                    Vset(ref dst[idst], p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1);
                    idst++;

                    Vset(ref dst[idst], lx1, ly1, lu, 1);
                    idst++;
                    Vset(ref dst[idst], p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1);
                    idst++;
                }
                else
                {
                    rx0 = p1.X - p1.Dmx * rw;
                    ry0 = p1.Y - p1.Dmy * rw;

                    Vset(ref dst[idst], p1.X, p1.Y, 0.5f, 1);
                    idst++;
                    Vset(ref dst[idst], p1.X - dlx0 * rw, p1.Y - dly0 * rw, ru, 1);
                    idst++;

                    Vset(ref dst[idst], rx0, ry0, ru, 1);
                    idst++;
                    Vset(ref dst[idst], rx0, ry0, ru, 1);
                    idst++;

                    Vset(ref dst[idst], p1.X, p1.Y, 0.5f, 1);
                    idst++;
                    Vset(ref dst[idst], p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1);
                    idst++;
                }

                Vset(ref dst[idst], lx1, ly1, lu, 1);
                idst++;
                Vset(ref dst[idst], p1.X - dlx1 * rw, p1.Y - dly1 * rw, ru, 1);
                idst++;

            }
            else
            {
                ChooseBevel(p1.Flags & (int)NvgPointFlags.InnerBevel,
                    p0, p1, -rw, out rx0, out ry0, out float rx1, out float ry1);

                Vset(ref dst[idst], p1.X + dlx0 * lw, p1.Y + dly0 * lw, lu, 1);
                idst++;
                Vset(ref dst[idst], rx0, ry0, ru, 1);
                idst++;

                if ((p1.Flags & (int)NvgPointFlags.Bevel) != 0)
                {
                    Vset(ref dst[idst], p1.X + dlx0 * lw, p1.Y + dly0 * lw, lu, 1);
                    idst++;
                    Vset(ref dst[idst], rx0, ry0, ru, 1);
                    idst++;

                    Vset(ref dst[idst], p1.X + dlx1 * lw, p1.Y + dly1 * lw, lu, 1);
                    idst++;
                    Vset(ref dst[idst], rx1, ry1, ru, 1);
                    idst++;
                }
                else
                {
                    lx0 = p1.X + p1.Dmx * lw;
                    ly0 = p1.Y + p1.Dmy * lw;

                    Vset(ref dst[idst], p1.X + dlx0 * lw, p1.Y + dly0 * lw, lu, 1);
                    idst++;
                    Vset(ref dst[idst], p1.X, p1.Y, 0.5f, 1);
                    idst++;

                    Vset(ref dst[idst], lx0, ly0, lu, 1);
                    idst++;
                    Vset(ref dst[idst], lx0, ly0, lu, 1);
                    idst++;

                    Vset(ref dst[idst], p1.X + dlx1 * lw, p1.Y + dly1 * lw, lu, 1);
                    idst++;
                    Vset(ref dst[idst], p1.X, p1.Y, 0.5f, 1);
                    idst++;
                }

                Vset(ref dst[idst], p1.X + dlx1 * lw, p1.Y + dly1 * lw, lu, 1);
                idst++;
                Vset(ref dst[idst], rx1, ry1, ru, 1);
                idst++;
            }
        }

        private static void ExpandFill(this NvgContext ctx, float w, NvgLineCap lineJoin, float miterLimit)
        {
            var cache = ctx.Cache;
            var iverts = 0;
            int i;
            var aa = ctx.FringeWidth;
            var fringe = w > 0.0f;

            CalculateJoins(ctx, w, lineJoin, miterLimit);

            // Calculate max vertex usage.
            var cverts = 0;
            for (i = 0; i < cache.Npaths; i++)
            {
                var path = cache.Paths[i];
                cverts += path.Count + path.Nbevel + 1;
                if (fringe)
                    cverts += (path.Count + path.Nbevel * 5 + 1) * 2; // plus one for loop
            }

            var verts = AllocTempVerts(ctx, cverts);
            if (verts == null)
                return;

            var convex = cache.Npaths == 1 && cache.Paths[0].Convex != 0;

            for (i = 0; i < cache.Npaths; i++)
            {
                var path2 = cache.Paths[i];
                var pts = cache.Points;
                var ipts = path2.First;
                NvgPoint p0;
                int ip0;
                NvgPoint p1;
                int ip1;

                // Calculate shape vertices.
                var woff = 0.5f * aa;
                var dst = verts;
                var idst = iverts;
                path2.Fill = dst;
                path2.Ifill = idst;

                int j;
                if (fringe)
                {
                    // Looping
                    ip0 = ipts + path2.Count - 1;
                    p0 = pts[ip0];
                    ip1 = ipts + 0;
                    p1 = pts[ip1];

                    for (j = 0; j < path2.Count; ++j)
                    {
                        if ((p1.Flags & (int)NvgPointFlags.Bevel) != 0)
                        {
                            var dlx0 = p0.Dy;
                            var dly0 = -p0.Dx;
                            var dlx1 = p1.Dy;
                            var dly1 = -p1.Dx;
                            if ((p1.Flags & (int)NvgPointFlags.Left) != 0)
                            {
                                var lx = p1.X + p1.Dmx * woff;
                                var ly = p1.Y + p1.Dmy * woff;
                                Vset(ref dst[idst], lx, ly, 0.5f, 1);
                                idst++;
                            }
                            else
                            {
                                var lx0 = p1.X + dlx0 * woff;
                                var ly0 = p1.Y + dly0 * woff;
                                var lx1 = p1.X + dlx1 * woff;
                                var ly1 = p1.Y + dly1 * woff;
                                Vset(ref dst[idst], lx0, ly0, 0.5f, 1);
                                idst++;
                                Vset(ref dst[idst], lx1, ly1, 0.5f, 1);
                                idst++;
                            }
                        }
                        else
                        {
                            Vset(ref dst[idst], p1.X + p1.Dmx * woff, p1.Y + p1.Dmy * woff, 0.5f, 1);
                            idst++;
                        }
                        p0 = p1;
                        ip1 += 1;
                        p1 = pts[ip1];
                    }
                }
                else
                {
                    for (j = 0; j < path2.Count; ++j)
                    {
                        Vset(ref dst[idst], pts[j + ipts].X, pts[j + ipts].Y, 0.5f, 1);
                        idst++;
                    }
                }

                path2.Nfill = idst - iverts;
                verts = dst;
                iverts = idst;

                // Calculate fringe (Calcula flecos)
                if (fringe)
                {
                    var lw = w + woff;
                    var rw = w - woff;
                    float lu = 0;
                    float ru = 1;
                    idst = iverts;
                    dst = verts;
                    path2.Stroke = dst;
                    path2.Istroke = idst;

                    // Create only half a fringe for convex shapes so that
                    // the shape can be rendered without stenciling.
                    if (convex)
                    {
                        lw = woff;  // This should generate the same vertex as fill inset above.
                        lu = 0.5f;  // Set outline fade at middle.
                    }

                    // Looping
                    ip0 = ipts + path2.Count - 1;
                    p0 = pts[ip0];
                    ip1 = 0 + ipts;
                    p1 = pts[ip1];

                    for (j = 0; j < path2.Count; ++j)
                    {
                        if ((p1.Flags &
                            ((int)NvgPointFlags.Bevel | (int)NvgPointFlags.InnerBevel)) != 0)
                        {
                            BevelJoin(dst, ref idst, p0, p1, lw, rw, lu, ru);
                        }
                        else
                        {
                            Vset(ref dst[idst], p1.X + p1.Dmx * lw, p1.Y + p1.Dmy * lw, lu, 1);
                            idst++;
                            Vset(ref dst[idst], p1.X - p1.Dmx * rw, p1.Y - p1.Dmy * rw, ru, 1);
                            idst++;
                        }
                        p0 = p1;
                        ip1 += 1;
                        p1 = pts[ip1];
                    }

                    // Loop it
                    Vset(ref dst[idst], verts[0 + iverts].X, verts[0 + iverts].Y, lu, 1);
                    idst++;
                    Vset(ref dst[idst], verts[1 + iverts].X, verts[1 + iverts].Y, ru, 1);
                    idst++;

                    path2.Nstroke = idst - iverts;
                    iverts = idst;
                    verts = dst;
                }
                else
                {
                    path2.Stroke = null;
                    path2.Nstroke = 0;
                }

#if ONLY_FOR_DEBUG
				for(int cont=path2.istroke, cont2=0; cont < path2.nstroke; cont++, cont2++)
					Console.WriteLine(String.Format("Index-stroke[{0}] x={1} y={2} u={3} v={4}",
						cont2, path2.stroke[cont].x,
						path2.stroke[cont].y,
						path2.stroke[cont].u,
						path2.stroke[cont].v));
#endif
            }

            //ctx.cache.verts = verts;
        }

        public static void TransformScale(float[] t, float sx, float sy)
        {
            t[0] = sx;
            t[1] = 0.0f;
            t[2] = 0.0f;
            t[3] = sy;
            t[4] = 0.0f;
            t[5] = 0.0f;
        }

        public static int TransformInverse(float[] inv, float[] t)
        {
            var det = (double)t[0] * t[3] - (double)t[2] * t[1];
            if (det > -1e-6 && det < 1e-6)
            {
                TransformIdentity(inv);
                return 0;
            }
            var invdet = 1.0 / det;
            inv[0] = (float)(t[3] * invdet);
            inv[2] = (float)(-t[2] * invdet);
            inv[4] = (float)(((double)t[2] * t[5] - (double)t[3] * t[4]) * invdet);
            inv[1] = (float)(-t[1] * invdet);
            inv[3] = (float)(t[0] * invdet);
            inv[5] = (float)(((double)t[1] * t[4] - (double)t[0] * t[5]) * invdet);
            return 1;
        }

        public static void Fill(this NvgContext ctx)
        {
            var state = GetState(ctx);
            var fillPaint = state.Fill.Clone();
            int i;

            FlattenPaths(ctx);

            ExpandFill(ctx, ctx.Params.EdgeAntiAlias != 0 ? ctx.FringeWidth : 0.0f, NvgLineCap.Miter, 2.4f);

            // Apply global alpha
            fillPaint.InnerColor.A *= state.Alpha;
            fillPaint.OuterColor.A *= state.Alpha;

            ctx.Params.RenderFill(ctx.Params.UserPtr, ref fillPaint, ref state.Scissor, ctx.FringeWidth,
                ctx.Cache.Bounds, ctx.Cache.Paths, ctx.Cache.Npaths);

            // Count triangles
            for (i = 0; i < ctx.Cache.Npaths; i++)
            {
                var path = ctx.Cache.Paths[i];
                ctx.FillTriCount += path.Nfill - 2;
                ctx.FillTriCount += path.Nstroke - 2;
                ctx.DrawCallCount += 2;
            }
        }

        public static void Rect(this NvgContext ctx, float x, float y, float w, float h)
        {
            float[] vals =
            {
                (float)NvgCommands.MoveTo, x, y,
                (float)NvgCommands.LineTo, x, y + h,
                (float)NvgCommands.LineTo, x + w, y + h,
                (float)NvgCommands.LineTo, x + w, y,
                (float)NvgCommands.Close
            };
            AppendCommands(ctx, vals, NVG_COUNTOF(vals));
        }

        private static float DistPtSeg(float x, float y, float px, float py, float qx, float qy)
        {
            var pqx = qx - px;
            var pqy = qy - py;
            var dx = x - px;
            var dy = y - py;
            var d = pqx * pqx + pqy * pqy;
            var t = pqx * dx + pqy * dy;
            if (d > 0) t /= d;
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
            dx = px + t * pqx - x;
            dy = py + t * pqy - y;
            return dx * dx + dy * dy;
        }

        public static void ArcTo(this NvgContext ctx, float x1, float y1, float x2, float y2, float radius)
        {
            var x0 = ctx.Commandx;
            var y0 = ctx.Commandy;
            float cx, cy, a0, a1;
            NvgWinding dir;

            if (ctx.Ncommands == 0)
            {
                return;
            }

            // Handle degenerate cases.
            if (PtEquals(x0, y0, x1, y1, ctx.DistTol) ||
                PtEquals(x1, y1, x2, y2, ctx.DistTol) ||
                DistPtSeg(x1, y1, x0, y0, x2, y2) < ctx.DistTol * ctx.DistTol ||
                radius < ctx.DistTol)
            {
                LineTo(ctx, x1, y1);
                return;
            }

            // Calculate tangential circle to lines (x0,y0)-(x1,y1) and (x1,y1)-(x2,y2).
            var dx0 = x0 - x1;
            var dy0 = y0 - y1;
            var dx1 = x2 - x1;
            var dy1 = y2 - y1;
            Normalize(ref dx0, ref dy0);
            Normalize(ref dx1, ref dy1);
            var a = Acosf(dx0 * dx1 + dy0 * dy1);
            var d = radius / Tanf(a / 2.0f);

            //	printf("a=%f° d=%f\n", a/NVG_PI*180.0f, d);

            if (d > 10000.0f)
            {
                LineTo(ctx, x1, y1);
                return;
            }

            if (Cross(dx0, dy0, dx1, dy1) > 0.0f)
            {
                cx = x1 + dx0 * d + dy0 * radius;
                cy = y1 + dy0 * d + -dx0 * radius;
                a0 = Atan2F(dx0, -dy0);
                a1 = Atan2F(-dx1, dy1);
                dir = NvgWinding.Clockwise;
                //		printf("CW c=(%f, %f) a0=%f° a1=%f°\n", cx, cy, a0/NVG_PI*180.0f, a1/NVG_PI*180.0f);
            }
            else
            {
                cx = x1 + dx0 * d + -dy0 * radius;
                cy = y1 + dy0 * d + dx0 * radius;
                a0 = Atan2F(-dx0, dy0);
                a1 = Atan2F(dx1, -dy1);
                dir = NvgWinding.CounterClockwise;
                //		printf("CCW c=(%f, %f) a0=%f° a1=%f°\n", cx, cy, a0/NVG_PI*180.0f, a1/NVG_PI*180.0f);
            }

            Arc(ctx, cx, cy, radius, a0, a1, dir);
        }

        public static void QuadTo(this NvgContext ctx, float cx, float cy, float x, float y)
        {
            var x0 = ctx.Commandx;
            var y0 = ctx.Commandy;
            float[] vals = { (int)NvgCommands.BezierTo,
                x0 + 2.0f/3.0f*(cx - x0), y0 + 2.0f/3.0f*(cy - y0),
                x + 2.0f/3.0f*(cx - x), y + 2.0f/3.0f*(cy - y),
                x, y
            };
            AppendCommands(ctx, vals, NVG_COUNTOF(vals));
        }

        public static void Arc(this NvgContext ctx, float cx, float cy, float r, float a0, float a1, NvgWinding dir)
        {
            float px = 0, py = 0, ptanx = 0, ptany = 0;
            var vals = new float[3 + 5 * 7 + 100];
            int i;
            var move = ctx.Ncommands > 0 ? (int)NvgCommands.LineTo : (int)NvgCommands.MoveTo;

            // Clamp angles
            var da = a1 - a0;
            if (dir == NvgWinding.Clockwise)
            {
                if (Absf(da) >= NvgPi * 2)
                {
                    da = NvgPi * 2;
                }
                else
                {
                    while (da < 0.0f)
                        da += NvgPi * 2;
                }
            }
            else
            {
                if (Absf(da) >= NvgPi * 2)
                {
                    da = -NvgPi * 2;
                }
                else
                {
                    while (da > 0.0f)
                        da -= NvgPi * 2;
                }
            }

            // Split arc into max 90 degree segments.
            var ndivs = Maxi(1, Mini((int)(Absf(da) / (NvgPi * 0.5f) + 0.5f), 5));
            var hda = da / ndivs / 2.0f;
            var kappa = Absf(4.0f / 3.0f * (1.0f - Cosf(hda)) / Sinf(hda));

            if (dir == NvgWinding.CounterClockwise)
                kappa = -kappa;

            var nvals = 0;
            for (i = 0; i <= ndivs; i++)
            {
                var a = a0 + da * (i / (float)ndivs);
                var dx = Cosf(a);
                var dy = Sinf(a);
                var x = cx + dx * r;
                var y = cy + dy * r;
                var tanx = -dy * r * kappa;
                var tany = dx * r * kappa;

                if (i == 0)
                {
                    vals[nvals++] = move;
                    vals[nvals++] = x;
                    vals[nvals++] = y;
                }
                else
                {
                    vals[nvals++] = (float)NvgCommands.BezierTo;
                    vals[nvals++] = px + ptanx;
                    vals[nvals++] = py + ptany;
                    vals[nvals++] = x - tanx;
                    vals[nvals++] = y - tany;
                    vals[nvals++] = x;
                    vals[nvals++] = y;
                }
                px = x;
                py = y;
                ptanx = tanx;
                ptany = tany;
            }

            AppendCommands(ctx, vals, nvals);
        }

        public static void RoundedRect(this NvgContext ctx, float x, float y, float w, float h, float r)
        {
            if (r < 0.1f)
            {
                Rect(ctx, x, y, w, h);
            }
            else
            {
                float rx = Minf(r, Absf(w) * 0.5f) * Signf(w), ry = Minf(r, Absf(h) * 0.5f) * Signf(h);
                float[] vals =
                {
                    (float)NvgCommands.MoveTo, x, y + ry,
                    (float)NvgCommands.LineTo, x, y + h - ry,
                    (float)NvgCommands.BezierTo, x, y + h - ry * (1 - NvgKappa90), x + rx * (1 - NvgKappa90), y + h, x + rx, y + h,
                    (float)NvgCommands.LineTo, x + w - rx, y + h,
                    (float)NvgCommands.BezierTo, x + w - rx * (1 - NvgKappa90), y + h, x + w, y + h - ry * (1 - NvgKappa90), x + w, y + h - ry,
                    (float)NvgCommands.LineTo, x + w, y + ry,
                    (float)NvgCommands.BezierTo, x + w, y + ry * (1 - NvgKappa90), x + w - rx * (1 - NvgKappa90), y, x + w - rx, y,
                    (float)NvgCommands.LineTo, x + rx, y,
                    (float)NvgCommands.BezierTo, x + rx * (1 - NvgKappa90), y, x, y + ry * (1 - NvgKappa90), x, y + ry,
                    (float)NvgCommands.Close
                };
                AppendCommands(ctx, vals, NVG_COUNTOF(vals));
            }
        }

        public static void RoundedRectVarying(this NvgContext ctx, float x, float y, float w, float h, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            if (radTopLeft < 0.1f && radTopRight < 0.1f && radBottomRight < 0.1f && radBottomLeft < 0.1f)
            {
                Rect(ctx, x, y, w, h);
                return;
            }

            float halfw = Absf(w) * 0.5f;
            float halfh = Absf(h) * 0.5f;
            float rxBl = Minf(radBottomLeft, halfw) * Signf(w), ryBl = Minf(radBottomLeft, halfh) * Signf(h);
            float rxBr = Minf(radBottomRight, halfw) * Signf(w), ryBr = Minf(radBottomRight, halfh) * Signf(h);
            float rxTr = Minf(radTopRight, halfw) * Signf(w), ryTr = Minf(radTopRight, halfh) * Signf(h);
            float rxTl = Minf(radTopLeft, halfw) * Signf(w), ryTl = Minf(radTopLeft, halfh) * Signf(h);
            float[] vals = {
                (float)NvgCommands.MoveTo, x, y + ryTl,
                (float)NvgCommands.LineTo, x, y + h - ryBl,
                (float)NvgCommands.BezierTo, x, y + h - ryBl*(1 - NvgKappa90), x + rxBl*(1 - NvgKappa90), y + h, x + rxBl, y + h,
                (float)NvgCommands.LineTo, x + w - rxBr, y + h,
                (float)NvgCommands.BezierTo, x + w - rxBr*(1 - NvgKappa90), y + h, x + w, y + h - ryBr*(1 - NvgKappa90), x + w, y + h - ryBr,
                (float)NvgCommands.LineTo, x + w, y + ryTr,
                (float)NvgCommands.BezierTo, x + w, y + ryTr*(1 - NvgKappa90), x + w - rxTr*(1 - NvgKappa90), y, x + w - rxTr, y,
                (float)NvgCommands.LineTo, x + rxTl, y,
                (float)NvgCommands.BezierTo, x + rxTl*(1 - NvgKappa90), y, x, y + ryTl*(1 - NvgKappa90), x, y + ryTl,
                (float)NvgCommands.Close
            };
            AppendCommands(ctx, vals, NVG_COUNTOF(vals));
        }

        public static NvgPaint LinearGradient(this NvgContext ctx,
                                                 float sx, float sy, float ex, float ey,
                                                 NvgColor icol, NvgColor ocol)
        {
            var p = new NvgPaint();
            const float large = (float)1e5;
            //NVG_NOTUSED(ctx);
            //memset(&p, 0, sizeof(p));

            // Calculate transform aligned to the line
            var dx = ex - sx;
            var dy = ey - sy;
            var d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d > 0.0001f)
            {
                dx /= d;
                dy /= d;
            }
            else
            {
                dx = 0;
                dy = 1;
            }

            p.Xform[0] = dy;
            p.Xform[1] = -dx;
            p.Xform[2] = dx;
            p.Xform[3] = dy;
            p.Xform[4] = sx - dx * large;
            p.Xform[5] = sy - dy * large;

            p.Extent[0] = large;
            p.Extent[1] = large + d * 0.5f;

            p.Radius = 0.0f;

            p.Feather = Maxf(1.0f, d);

            p.InnerColor = icol;
            p.OuterColor = ocol;

            return p;
        }

        private static float Quantize(float a, float d)
        {
            return (int)(a / d + 0.5f) * d;
        }

        public static NvgPaint RadialGradient(this NvgContext ctx,
                                                 float cx, float cy, float inr, float outr,
                                                 NvgColor icol, NvgColor ocol)
        {
            var p = new NvgPaint();
            var r = (inr + outr) * 0.5f;
            var f = outr - inr;
            //NVG_NOTUSED(ctx);
            //memset(&p, 0, sizeof(p));

            TransformIdentity(p.Xform);
            p.Xform[4] = cx;
            p.Xform[5] = cy;

            p.Extent[0] = r;
            p.Extent[1] = r;

            p.Radius = r;

            p.Feather = Maxf(1.0f, f);

            p.InnerColor = icol;
            p.OuterColor = ocol;

            return p;
        }

        public static void Ellipse(this NvgContext ctx, float cx, float cy, float rx, float ry)
        {
            float[] vals =
            {
                (float)NvgCommands.MoveTo, cx - rx, cy,
                (float)NvgCommands.BezierTo, cx - rx, cy + ry * NvgKappa90, cx - rx * NvgKappa90, cy + ry, cx, cy + ry,
                (float)NvgCommands.BezierTo, cx + rx * NvgKappa90, cy + ry, cx + rx, cy + ry * NvgKappa90, cx + rx, cy,
                (float)NvgCommands.BezierTo, cx + rx, cy - ry * NvgKappa90, cx + rx * NvgKappa90, cy - ry, cx, cy - ry,
                (float)NvgCommands.BezierTo, cx - rx * NvgKappa90, cy - ry, cx - rx, cy - ry * NvgKappa90, cx - rx, cy,
                (float)NvgCommands.Close
            };
            AppendCommands(ctx, vals, NVG_COUNTOF(vals));
        }

        public static void Circle(this NvgContext ctx, float cx, float cy, float r)
        {
            Ellipse(ctx, cx, cy, r, r);
        }

        public static int TextGlyphPositions(this NvgContext ctx, float x, float y, string string_,
                                                NvgGlyphPosition[] positions, int maxPositions)
        {
            var state = GetState(ctx);
            var scale = GetFontScale(state) * ctx.DevicePxRatio;
            var invscale = 1.0f / scale;
            var iter = new FONStextIter();
            var q = new FONSquad();
            var npos = 0;

            if (state.FontId == FontStash.FontStash.FONS_INVALID)
                return 0;

            //if (end == NULL)
            //	end = string + strlen(string);

            //if (string_ == end)
            //	return 0;

            FontStash.FontStash.fonsSetSize(ref ctx.Fs, state.FontSize * scale);
            FontStash.FontStash.fonsSetSpacing(ref ctx.Fs, state.LetterSpacing * scale);
            FontStash.FontStash.fonsSetBlur(ref ctx.Fs, state.FontBlur * scale);
            FontStash.FontStash.fonsSetAlign(ctx.Fs, (FONSalign)state.TextAlign);
            FontStash.FontStash.fonsSetFont(ref ctx.Fs, state.FontId);

            FontStash.FontStash.fonsTextIterInit(ctx.Fs, ref iter, x * scale, y * scale, string_);
            var prevIter = iter;
            while (FontStash.FontStash.fonsTextIterNext(ctx.Fs, ref iter, ref q) != 0)
            {
                if (iter.prevGlyphIndex < 0 && AllocTextAtlas(ctx) > 0)
                { // can not retrieve glyph?
                    iter = prevIter;
                    FontStash.FontStash.fonsTextIterNext(ctx.Fs, ref iter, ref q); // try again
                }
                prevIter = iter;
                positions[npos].Str = iter.iStr;
                positions[npos].X = iter.x * invscale;
                positions[npos].Minx = Minf(iter.x, q.x0) * invscale;
                positions[npos].Maxx = Maxf(iter.nextx, q.x1) * invscale;
                npos++;
                if (npos >= maxPositions)
                    break;
            }

            return npos;
        }

        public static void TextBox(this NvgContext ctx, float x, float y, float breakRowWidth, string string_)
        {
            var state = GetState(ctx);
            var rows = new NvgTextRow[2];
            int nrows;
            var oldAlign = state.TextAlign;
            var haling = state.TextAlign & ((int)NvgAlign.Left | (int)NvgAlign.Center | (int)NvgAlign.Right);
            var valign = state.TextAlign & ((int)NvgAlign.Top | (int)NvgAlign.Middle | (int)NvgAlign.Bottom | (int)NvgAlign.Baseline);
            float lineh = 0;
            float fnull = 0;

            if (state.FontId == FontStash.FontStash.FONS_INVALID)
                return;

            TextMetrics(ctx, ref fnull, ref fnull, ref lineh);

            state.TextAlign = (int)NvgAlign.Left | valign;

            while ((nrows = TextBreakLines(ctx, string_, breakRowWidth, rows, 2)) > 0)
            {
                for (var i = 0; i < nrows; i++)
                {
                    string str;
                    var row = rows[i];
                    if ((haling & (int)NvgAlign.Left) != 0)
                    {
                        str = string_.Substring(row.Start, row.End - row.Start);
                        Text(ctx, x, y, str);
                    }
                    else if ((haling & (int)NvgAlign.Center) != 0)
                    {
                        str = string_.Substring(row.Start, row.End - row.Start);
                        Text(ctx, x + breakRowWidth * 0.5f - row.Width * 0.5f, y, str);
                    }
                    else if ((haling & (int)NvgAlign.Right) != 0)
                    {
                        str = string_.Substring(row.Start, row.End - row.Start);
                        Text(ctx, x + breakRowWidth - row.Width, y, str);
                    }
                    y += lineh * state.LineHeight;
                }
                string_ = string_.Substring(rows[nrows - 1].Next);

                if (string_.Length == 1)
                    string_ = "";
            }

            state.TextAlign = oldAlign;
        }

        public static void TextBoxBounds(this NvgContext ctx, float x, float y, float breakRowWidth, string string_, float[] bounds)
        {
            var state = GetState(ctx);
            var rows = new NvgTextRow[2];
            var scale = GetFontScale(state) * ctx.DevicePxRatio;
            var invscale = 1.0f / scale;
            int nrows;
            var oldAlign = state.TextAlign;
            var haling = state.TextAlign & ((int)NvgAlign.Left | (int)NvgAlign.Center | (int)NvgAlign.Right);
            var valign = state.TextAlign & ((int)NvgAlign.Top | (int)NvgAlign.Middle | (int)NvgAlign.Bottom | (int)NvgAlign.Baseline);
            float lineh = 0, rminy = 0, rmaxy = 0;
            float maxx, maxy;
            float fnull = 0;

            if (state.FontId == FontStash.FontStash.FONS_INVALID)
            {
                if (bounds != null)
                    bounds[0] = bounds[1] = bounds[2] = bounds[3] = 0.0f;
                return;
            }

            TextMetrics(ctx, ref fnull, ref fnull, ref lineh);

            state.TextAlign = (int)NvgAlign.Left | valign;

            var minx = maxx = x;
            var miny = maxy = y;

            FontStash.FontStash.fonsSetSize(ref ctx.Fs, state.FontSize * scale);
            FontStash.FontStash.fonsSetSpacing(ref ctx.Fs, state.LetterSpacing * scale);
            FontStash.FontStash.fonsSetBlur(ref ctx.Fs, state.FontBlur * scale);
            FontStash.FontStash.fonsSetAlign(ctx.Fs, (FONSalign)state.TextAlign);
            FontStash.FontStash.fonsSetFont(ref ctx.Fs, state.FontId);
            FontStash.FontStash.fonsLineBounds(ctx.Fs, 0, ref rminy, ref rmaxy);
            rminy *= invscale;
            rmaxy *= invscale;

            while ((nrows = TextBreakLines(ctx, string_, breakRowWidth, rows, 2)) > 0)
            {
                for (var i = 0; i < nrows; i++)
                {
                    var row = rows[i];
                    float dx = 0;
                    // Horizontal bounds
                    if ((haling & (int)NvgAlign.Left) != 0)
                        dx = 0;
                    else if ((haling & (int)NvgAlign.Center) != 0)
                        dx = breakRowWidth * 0.5f - row.Width * 0.5f;
                    else if ((haling & (int)NvgAlign.Right) != 0)
                        dx = breakRowWidth - row.Width;
                    var rminx = x + row.MinX + dx;
                    var rmaxx = x + row.MaxX + dx;
                    minx = Minf(minx, rminx);
                    maxx = Maxf(maxx, rmaxx);
                    // Vertical bounds.
                    miny = Minf(miny, y + rminy);
                    maxy = Maxf(maxy, y + rmaxy);

                    y += lineh * state.LineHeight;
                }
                string_ = string_.Substring(rows[nrows - 1].Next);

                if (string_.Length == 1)
                    string_ = "";
            }
            state.TextAlign = oldAlign;

            if (bounds == null) return;
            bounds[0] = minx;
            bounds[1] = miny;
            bounds[2] = maxx;
            bounds[3] = maxy;
        }

        public static float TextBounds(this NvgContext ctx, float x, float y, string string_, float[] bounds)
        {
            var state = GetState(ctx);
            var scale = GetFontScale(state) * ctx.DevicePxRatio;
            var invscale = 1.0f / scale;

            if (state.FontId == FontStash.FontStash.FONS_INVALID)
                return 0;

            FontStash.FontStash.fonsSetSize(ref ctx.Fs, state.FontSize * scale);
            FontStash.FontStash.fonsSetSpacing(ref ctx.Fs, state.LetterSpacing * scale);
            FontStash.FontStash.fonsSetBlur(ref ctx.Fs, state.FontBlur * scale);
            FontStash.FontStash.fonsSetAlign(ctx.Fs, (FONSalign)state.TextAlign);
            FontStash.FontStash.fonsSetFont(ref ctx.Fs, state.FontId);

            var width = FontStash.FontStash.fonsTextBounds(ref ctx.Fs, x * scale, y * scale, string_, bounds);
            if (bounds == null) return width * invscale;
            // Use line bounds for height.
            FontStash.FontStash.fonsLineBounds(ctx.Fs, y * scale, ref bounds[1], ref bounds[3]);
            bounds[0] *= invscale;
            bounds[1] *= invscale;
            bounds[2] *= invscale;
            bounds[3] *= invscale;
            return width * invscale;
        }

        public static void TextMetrics(this NvgContext ctx, ref float ascender, ref float descender, ref float lineh)
        {
            var state = GetState(ctx);
            var scale = GetFontScale(state) * ctx.DevicePxRatio;
            var invscale = 1.0f / scale;

            if (state.FontId == FontStash.FontStash.FONS_INVALID)
                return;

            FontStash.FontStash.fonsSetSize(ref ctx.Fs, state.FontSize * scale);
            FontStash.FontStash.fonsSetSpacing(ref ctx.Fs, state.LetterSpacing * scale);
            FontStash.FontStash.fonsSetBlur(ref ctx.Fs, state.FontBlur * scale);
            FontStash.FontStash.fonsSetAlign(ctx.Fs, (FONSalign)state.TextAlign);
            FontStash.FontStash.fonsSetFont(ref ctx.Fs, state.FontId);

            FontStash.FontStash.fonsVertMetrics(ref ctx.Fs, ref ascender, ref descender, ref lineh);
            ascender *= invscale;
            descender *= invscale;
            lineh *= invscale;
        }

        public static int TextBreakLines(this NvgContext ctx, string string_,
                                            float breakRowWidth, NvgTextRow[] rows, int maxRows)
        {
            var state = GetState(ctx);
            var scale = GetFontScale(state) * ctx.DevicePxRatio;
            var invscale = 1.0f / scale;
            var iter = new FONStextIter();
            var q = new FONSquad();
            var nrows = 0;
            float rowStartX = 0;
            float rowWidth = 0;
            float rowMinX = 0;
            float rowMaxX = 0;
            var rowStart = -1;
            var rowEnd = -1;
            var wordStart = -1;
            float wordStartX = 0;
            float wordMinX = 0;
            var breakEnd = -1;
            float breakWidth = 0;
            float breakMaxX = 0;
            int ptype = (int)NvgCodepointType.Space;
            uint pcodepoint = 0;

            if (string_ == null)
                return 0;
            var end = string_.Length - 1;

            if (maxRows == 0)
                return 0;
            if (state.FontId == FontStash.FontStash.FONS_INVALID)
                return 0;

            FontStash.FontStash.fonsSetSize(ref ctx.Fs, state.FontSize * scale);
            FontStash.FontStash.fonsSetSpacing(ref ctx.Fs, state.LetterSpacing * scale);
            FontStash.FontStash.fonsSetBlur(ref ctx.Fs, state.FontBlur * scale);
            FontStash.FontStash.fonsSetAlign(ctx.Fs, (FONSalign)state.TextAlign);
            FontStash.FontStash.fonsSetFont(ref ctx.Fs, state.FontId);

            breakRowWidth *= scale;

            FontStash.FontStash.fonsTextIterInit(ctx.Fs, ref iter, 0, 0, string_);
            var prevIter = iter;
            while (FontStash.FontStash.fonsTextIterNext(ctx.Fs, ref iter, ref q) != 0)
            {
                // can not retrieve glyph?
                if (iter.prevGlyphIndex < 0 && AllocTextAtlas(ctx) > 0)
                {
                    iter = prevIter;
                    FontStash.FontStash.fonsTextIterNext(ctx.Fs, ref iter, ref q); // try again
                }
                prevIter = iter;
                int type;
                switch (iter.codepoint)
                {
                    case 9:         // \t
                    case 11:        // \v
                    case 12:        // \f
                    case 32:        // space
                    case 0x00a0:    // NBSP
                        type = (int)NvgCodepointType.Space;
                        break;
                    case 10:        // \n
                        type = pcodepoint == 13 ? (int)NvgCodepointType.Space : (int)NvgCodepointType.Newline;
                        break;
                    case 13:        // \r
                        type = pcodepoint == 10 ? (int)NvgCodepointType.Space : (int)NvgCodepointType.Newline;
                        break;
                    case 0x0085:    // NEL
                        type = (int)NvgCodepointType.Newline;
                        break;
                    default:
                        type = (int)NvgCodepointType.Char;
                        break;
                }

                if (type == (int)NvgCodepointType.Newline)
                {
                    // Always handle new lines.
                    rows[nrows].Start = rowStart >= 0 ? rowStart : iter.iStr;

                    rows[nrows].End = rowEnd >= 0 ? rowEnd : iter.iStr;

                    rows[nrows].Width = rowWidth * invscale;
                    rows[nrows].MinX = rowMinX * invscale;
                    rows[nrows].MaxX = rowMaxX * invscale;
                    rows[nrows].Next = iter.iNext;

                    nrows++;
                    if (nrows >= maxRows)
                        return nrows;
                    // Set null break point
                    breakEnd = rowStart;
                    breakWidth = 0.0f;
                    breakMaxX = 0.0f;
                    // Indicate to skip the white space at the beginning of the row.
                    rowStart = -1;
                    rowEnd = -1;
                    rowWidth = 0;
                    rowMinX = rowMaxX = 0;
                }
                else
                {
                    if (rowStart < 0)
                    {
                        // Skip white space until the beginning of the line
                        if (type == (int)NvgCodepointType.Char)
                        {
                            // The current char is the row so far
                            rowStartX = iter.x;
                            rowStart = iter.iStr;

                            rowEnd = iter.iNext;

                            rowWidth = iter.nextx - rowStartX; // q.x1 - rowStartX;
                            rowMinX = q.x0 - rowStartX;
                            rowMaxX = q.x1 - rowStartX;
                            wordStart = iter.iStr;

                            wordStartX = iter.x;
                            wordMinX = q.x0 - rowStartX;
                            // Set null break point
                            breakEnd = rowStart;
                            breakWidth = 0.0f;
                            breakMaxX = 0.0f;
                        }
                    }
                    else
                    {
                        var nextWidth = iter.nextx - rowStartX;

                        // track last non-white space character
                        if (type == (int)NvgCodepointType.Char)
                        {
                            rowEnd = iter.iNext;
                            rowWidth = iter.nextx - rowStartX;
                            rowMaxX = q.x1 - rowStartX;
                        }
                        // track last end of a word
                        if (ptype == (int)NvgCodepointType.Char && type == (int)NvgCodepointType.Space)
                        {
                            breakEnd = iter.iStr;
                            breakWidth = rowWidth;
                            breakMaxX = rowMaxX;
                        }
                        // track last beginning of a word
                        if (ptype == (int)NvgCodepointType.Space && type == (int)NvgCodepointType.Char)
                        {
                            wordStart = iter.iStr;
                            wordStartX = iter.x;
                            wordMinX = q.x0 - rowStartX;
                        }

                        // Break to new line when a character is beyond break width.
                        if (type == (int)NvgCodepointType.Char && nextWidth > breakRowWidth)
                        {
                            // The run length is too long, need to break to new line.
                            if (breakEnd == rowStart)
                            {
                                // The current word is longer than the row length, just break it from here.
                                rows[nrows].Start = rowStart;
                                rows[nrows].End = iter.iStr;
                                rows[nrows].Width = rowWidth * invscale;
                                rows[nrows].MinX = rowMinX * invscale;
                                rows[nrows].MaxX = rowMaxX * invscale;
                                rows[nrows].Next = iter.iStr;
                                nrows++;
                                if (nrows >= maxRows)
                                    return nrows;
                                rowStartX = iter.x;
                                rowStart = iter.iStr;
                                rowEnd = iter.iNext;
                                rowWidth = iter.nextx - rowStartX;
                                rowMinX = q.x0 - rowStartX;
                                rowMaxX = q.x1 - rowStartX;
                                wordStart = iter.iStr;
                                wordStartX = iter.x;
                                wordMinX = q.x0 - rowStartX;
                            }
                            else
                            {
                                // Break the line from the end of the last word, and start new line from the beginning of the new.
                                rows[nrows].Start = rowStart;
                                rows[nrows].End = breakEnd;
                                rows[nrows].Width = breakWidth * invscale;
                                rows[nrows].MinX = rowMinX * invscale;
                                rows[nrows].MaxX = breakMaxX * invscale;
                                rows[nrows].Next = wordStart;
                                nrows++;
                                if (nrows >= maxRows)
                                    return nrows;
                                rowStartX = wordStartX;
                                rowStart = wordStart;
                                rowEnd = iter.iNext;
                                rowWidth = iter.nextx - rowStartX;
                                rowMinX = wordMinX;
                                rowMaxX = q.x1 - rowStartX;
                                // No change to the word start
                            }
                            // Set null break point
                            breakEnd = rowStart;
                            breakWidth = 0.0f;
                            breakMaxX = 0.0f;
                        }
                    }
                }

                pcodepoint = iter.codepoint;
                ptype = type;
            }

            // Break the line from the end of the last word, and start new line from the beginning of the new.
            if (rowStart < 0) return nrows;
            rows[nrows].Start = rowStart;
            rows[nrows].End = rowEnd;
            rows[nrows].Width = rowWidth * invscale;
            rows[nrows].MinX = rowMinX * invscale;
            rows[nrows].MaxX = rowMaxX * invscale;
            rows[nrows].Next = end;
            nrows++;

            return nrows;
        }

        public static void TextLineHeight(this NvgContext ctx, float lineHeight)
        {
            var state = GetState(ctx);
            state.LineHeight = lineHeight;
        }

        public static void TextAlign(this NvgContext ctx, NvgAlign align)
        {
            var state = GetState(ctx);
            state.TextAlign = (int)align;
        }

        private static float GetFontScale(NvgState state)
        {
            return Minf(Quantize(GetAverageScale(state.Xform), 0.01f), 4.0f);
        }

        public static void ImageSize(this NvgContext ctx, int image, ref int w, ref int h)
        {
            ctx.Params.RenderGetTextureSize(ctx.Params.UserPtr, image, ref w, ref h);
        }

        public static NvgPaint ImagePattern(this NvgContext ctx,
                                               float cx, float cy, float w, float h, float angle,
                                               int image, float alpha)
        {
            var p = new NvgPaint();
            //NVG_NOTUSED(ctx);
            //memset(&p, 0, sizeof(p));

            TransformRotate(p.Xform, angle);
            p.Xform[4] = cx;
            p.Xform[5] = cy;

            p.Extent[0] = w;
            p.Extent[1] = h;

            p.Image = image;

            p.InnerColor = p.OuterColor = Rgba(1, 1, 1, alpha);

            return p;
        }

        private static int AllocTextAtlas(this NvgContext ctx)
        {
            int iw = 0, ih = 0;
            FlushTextTexture(ctx);
            if (ctx.FontImageIdx >= NvgMaxFontimages - 1)
                return 0;
            // if next fontImage already have a texture
            if (ctx.FontImages[ctx.FontImageIdx + 1] != 0)
                ImageSize(ctx, ctx.FontImages[ctx.FontImageIdx + 1], ref iw, ref ih);
            else
            { // calculate the new font image size and create it.
                ImageSize(ctx, ctx.FontImages[ctx.FontImageIdx], ref iw, ref ih);
                if (iw > ih)
                    ih *= 2;
                else
                    iw *= 2;
                if (iw > NvgMaxFontimageSize || ih > NvgMaxFontimageSize)
                    iw = ih = NvgMaxFontimageSize;
                ctx.FontImages[ctx.FontImageIdx + 1] = ctx.Params.RenderCreateTextureByte(ctx.Params.UserPtr,
                    (int)NvgTexture.Alpha, iw, ih, 0, null);
            }
            ++ctx.FontImageIdx;
            FontStash.FontStash.fonsResetAtlas(ctx.Fs, iw, ih);
            return 1;
        }

        public static float Text(this NvgContext ctx, float x, float y, string string_)
        {
            var state = GetState(ctx);
            var iter = new FONStextIter();
            var q = new FONSquad();
            var scale = GetFontScale(state) * ctx.DevicePxRatio;
            var invscale = 1.0f / scale;
            var nverts = 0;

            var end = string_.Length;

            if (state.FontId == FontStash.FontStash.FONS_INVALID)
                return x;

            FontStash.FontStash.fonsSetSize(ref ctx.Fs, state.FontSize * scale);
            FontStash.FontStash.fonsSetSpacing(ref ctx.Fs, state.LetterSpacing * scale);
            FontStash.FontStash.fonsSetBlur(ref ctx.Fs, state.FontBlur * scale);
            FontStash.FontStash.fonsSetAlign(ctx.Fs, (FONSalign)state.TextAlign);
            FontStash.FontStash.fonsSetFont(ref ctx.Fs, state.FontId);

            var cverts = Maxi(2, end) * 6;
            var verts = AllocTempVerts(ctx, cverts);
            if (verts == null)
                return x;

            FontStash.FontStash.fonsTextIterInit(ctx.Fs, ref iter, x * scale, y * scale, string_);
            var prevIter = iter;
            while (FontStash.FontStash.fonsTextIterNext(ctx.Fs, ref iter, ref q) != 0)
            {
                var c = new float[4 * 2];
                if (iter.prevGlyphIndex == -1)
                { // can not retrieve glyph?
                    if (AllocTextAtlas(ctx) == 0)
                        break; // no memory :(
                    if (nverts != 0)
                    {
                        RenderText(ctx, verts, nverts);
                        nverts = 0;
                    }
                    iter = prevIter;
                    FontStash.FontStash.fonsTextIterNext(ctx.Fs, ref iter, ref q); // try again
                    if (iter.prevGlyphIndex == -1) // still can not find glyph?
                        break;
                }
                prevIter = iter;
                // Transform corners.
                TransformPoint(out c[0], out c[1], state.Xform, q.x0 * invscale, q.y0 * invscale);
                TransformPoint(out c[2], out c[3], state.Xform, q.x1 * invscale, q.y0 * invscale);
                TransformPoint(out c[4], out c[5], state.Xform, q.x1 * invscale, q.y1 * invscale);
                TransformPoint(out c[6], out c[7], state.Xform, q.x0 * invscale, q.y1 * invscale);
                // Create triangles
                if (nverts + 6 > cverts) continue;
                Vset(ref verts[nverts], c[0], c[1], q.s0, q.t0);
                nverts++;
                Vset(ref verts[nverts], c[4], c[5], q.s1, q.t1);
                nverts++;
                Vset(ref verts[nverts], c[2], c[3], q.s1, q.t0);
                nverts++;
                Vset(ref verts[nverts], c[0], c[1], q.s0, q.t0);
                nverts++;
                Vset(ref verts[nverts], c[6], c[7], q.s0, q.t1);
                nverts++;
                Vset(ref verts[nverts], c[4], c[5], q.s1, q.t1);
                nverts++;
            }

            //ctx.cache.verts = verts;

            // TODO: add back-end bit to do this just once per frame.
            FlushTextTexture(ctx);

            RenderText(ctx, verts, nverts);

            return iter.x;
        }

        private static void RenderText(this NvgContext ctx, NvgVertex[] verts, int nverts)
        {
            var state = GetState(ctx);
            // last change
            var paint = state.Fill.Clone();

            // Render triangles.
            paint.Image = ctx.FontImages[ctx.FontImageIdx];

            // Apply global alpha
            paint.InnerColor.A *= state.Alpha;
            paint.OuterColor.A *= state.Alpha;

            ctx.Params.RenderTriangles(ctx.Params.UserPtr, ref paint, ref state.Scissor, verts, nverts);

            ctx.DrawCallCount++;
            ctx.TextTriCount += nverts / 3;
        }

        private static void FlushTextTexture(this NvgContext ctx)
        {
            var dirty = new int[4];

            if (FontStash.FontStash.fonsValidateTexture(ctx.Fs, dirty) == 0) return;
            var fontImage = ctx.FontImages[ctx.FontImageIdx];
            // Update texture
            if (fontImage == 0) return;
            int iw = 0, ih = 0;
            var data = FontStash.FontStash.fonsGetTextureData(ctx.Fs, ref iw, ref ih);
            var x = dirty[0];
            var y = dirty[1];
            var w = dirty[2] - dirty[0];
            var h = dirty[3] - dirty[1];
            ctx.Params.RenderUpdateTexture(ctx.Params.UserPtr, fontImage, x, y, w, h, data);
        }

        public static void GlobalAlpha(this NvgContext ctx, float alpha)
        {
            var state = GetState(ctx);
            state.Alpha = alpha;
        }

        private static void TransformIdentity(float[] t)
        {
            t[0] = 1.0f;
            t[1] = 0.0f;
            t[2] = 0.0f;
            t[3] = 1.0f;
            t[4] = 0.0f;
            t[5] = 0.0f;
        }

        private static float Hue(float h, float m1, float m2)
        {
            if (h < 0)
                h += 1;
            if (h > 1)
                h -= 1;
            if (h < 1.0f / 6.0f)
                return m1 + (m2 - m1) * h * 6.0f;
            if (h < 3.0f / 6.0f)
                return m2;
            if (h < 4.0f / 6.0f)
                return m1 + (m2 - m1) * (2.0f / 3.0f - h) * 6.0f;
            return m1;
        }

        public static NvgColor Hsla(float h, float s, float l, byte a)
        {
            NvgColor col;
            h = Modf(h, 1.0f);
            if (h < 0.0f)
                h += 1.0f;
            s = Clampf(s, 0.0f, 1.0f);
            l = Clampf(l, 0.0f, 1.0f);
            var m2 = l <= 0.5f ? l * (1 + s) : l + s - l * s;
            var m1 = 2 * l - m2;
            col.R = Clampf(Hue(h + 1.0f / 3.0f, m1, m2), 0.0f, 1.0f);
            col.G = Clampf(Hue(h, m1, m2), 0.0f, 1.0f);
            col.B = Clampf(Hue(h - 1.0f / 3.0f, m1, m2), 0.0f, 1.0f);
            col.A = a / 255.0f;
            return col;
        }

        public static int AddFallbackFontId(this NvgContext ctx, int baseFont, int fallbackFont)
        {
            if (baseFont == -1 || fallbackFont == -1)
                return 0;
            return FontStash.FontStash.fonsAddFallbackFont(ctx.Fs, baseFont, fallbackFont);
        }

        public static void BeginFrame(this NvgContext ctx, int windowWidth, int windowHeight, float devicePixelRatio)
        {
            ctx.Nstates = 0;
            Save(ctx);
            Reset(ctx);

            SetDevicePixelRatio(ref ctx, devicePixelRatio);

            ctx.Params.RenderViewport(ctx.Params.UserPtr, windowWidth, windowHeight, devicePixelRatio);

            ctx.DrawCallCount = 0;
            ctx.FillTriCount = 0;
            ctx.StrokeTriCount = 0;
            ctx.TextTriCount = 0;
        }

        /// <summary>
        /// Create font from *.ttf <see cref="fileName"/>.
        /// </summary>
        /// <returns>The create font id.</returns>
        /// <param name="ctx">NanoVG context.</param>
        /// <param name="internalFontName">Internal font name.</param>
        /// <param name="fileName">File name of *.ttf font file (can include a path).</param>
        public static int CreateFont(this NvgContext ctx, string internalFontName, string fileName)
        {
            return FontStash.FontStash.fonsAddFont(ctx.Fs, internalFontName, fileName);
        }

        /// <summary>
        /// Create font from *.ttf <see cref="fileName"/>.
        /// </summary>
        /// <returns>The create font id.</returns>
        /// <param name="ctx">NanoVG context.</param>
        /// <param name="internalFontName">Internal font name.</param>
        /// <param name="fontFile">Bytes of *.ttf font file.</param>
        public static int CreateFont(this NvgContext ctx, string internalFontName, byte[] fontFile)
        {
            return FontStash.FontStash.fonsAddFont(ctx.Fs, internalFontName, fontFile);
        }

        public static byte[] ImageToByteArray(Image imageIn)
        {
            var ms = new MemoryStream();
            imageIn.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }

        private static int CreateImageRgbaBmp(ref NvgContext ctx, int w, int h, int imageFlags, Bitmap bmp)
        {
            return ctx.Params.RenderCreateTextureBmp(ctx.Params.UserPtr, (int)NvgTexture.Rgba, w, h, imageFlags, bmp);
        }

        public static int CreateImage(ref NvgContext ctx, string filename, int imageFlags)
        {
            //int w, h, n;
            //byte[] img;
            Bitmap bmp;
            //stbi_set_unpremultiply_on_load(1);
            //stbi_convert_iphone_png_to_rgb(1);
            //img = stbi_load(filename, &w, &h, &n, 4);
            //Image loadedImg = Image.FromFile(filename);
            //img = ImageToByteArray(loadedImg);
            try
            {
                bmp = new Bitmap(filename);
            }
            catch
            {
                //if (img == null)
                {
                    //		printf("Failed to load %s - %s\n", filename, stbi_failure_reason());
                    return 0;
                }
            }
            var image = CreateImageRgbaBmp(ref ctx, bmp.Width, bmp.Height, imageFlags, bmp);
            //stbi_image_free(img);
            return image;
        }
    }
}

