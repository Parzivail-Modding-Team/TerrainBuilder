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

using System;
using System.Linq;
using OpenTK;

namespace NanoVGDotNet.NanoVG
{
	public class PerfGraph
	{
	    private readonly int _graphHistoryCount;
	    private readonly GraphRenderStyle _style;
	    private readonly string _name;
	    private readonly float[] _values;
	    private int _head;

		public PerfGraph(GraphRenderStyle style, string name, int graphHistoryCount = 100)
		{
			_style = style;
			_name = name;
		    _graphHistoryCount = graphHistoryCount;
            _values = new float[_graphHistoryCount];
			_head = 0;
		}

		public void UpdateGraph(float frameTime)
		{
		    _values[_head] = frameTime;
            _head = (_head + 1) % _graphHistoryCount;
		}

		public float GetGraphAverage()
		{
		    return _values.Average();
		}

		public float GetGraphMin()
		{
		    return _values.Min();
		}

		public float GetGraphMax()
		{
		    return _values.Max();
		}

		public void RenderGraph(NvgContext vg, float x, float y)
		{
			int i;
		    string str;
			var avg = GetGraphAverage();
		    const float d = 1 / 60f;
			var min = GetGraphMin() + d;
			var max = GetGraphMax() - d;
            const float w = 100;
			const float h = 35;

			vg.BeginPath();
			vg.Rect(x, y, w, h);
			vg.FillColor(NanoVg.Rgba(255, 255, 255, 64));
			vg.Fill();

			vg.BeginPath();
			vg.MoveTo(x, y + h);
			switch (_style)
			{
			    case GraphRenderStyle.Fps:
			        for (i = 0; i < _graphHistoryCount; i++)
			        {
			            var v = _values[(_head + i) % _graphHistoryCount];
			            var vx = x + (float)i / (_graphHistoryCount - 1) * w;
			            var perc = MathHelper.Clamp((v - min) / (max - min), 0, 1);
                        var vy = y + h - perc * h;
			            vg.LineTo(vx, vy);
			        }
			        break;
			    case GraphRenderStyle.Raw:
			        for (i = 0; i < _graphHistoryCount; i++)
			        {
			            var v = _values[(_head + i) % _graphHistoryCount];
			            if (v > 100.0f)
			                v = 100.0f;
			            var vx = x + (float)i / (_graphHistoryCount - 1) * w;
			            var vy = y + h - v / 100.0f * h;
			            vg.LineTo(vx, vy);
			        }
			        break;
			    case GraphRenderStyle.Milliseconds:
			        for (i = 0; i < _graphHistoryCount; i++)
			        {
			            var v = _values[(_head + i) % _graphHistoryCount] * 1000.0f;
			            if (v > 20.0f)
			                v = 20.0f;
			            var vx = x + (float)i / (_graphHistoryCount - 1) * w;
			            var vy = y + h - v / 20.0f * h;
			            vg.LineTo(vx, vy);
			        }
                    break;
			}
			vg.LineTo(x + w, y + h);
			vg.FillColor(NanoVg.Rgba(255, 192, 0, 128));
			vg.Fill();

		    var avgPerc = MathHelper.Clamp((avg - min) / (max - min), 0, 1);
            vg.BeginPath();
		    vg.MoveTo(x, y + h - avgPerc * h);
            vg.LineTo(x + w, y + h - avgPerc * h);
		    vg.StrokeColor(NanoVg.Rgba(255, 255, 255, 128));
            vg.Stroke();

            vg.FontFace("sans");

			if (_name != null)
			{
				vg.FontSize(14.0f);
				vg.TextAlign(NvgAlign.Left | NvgAlign.Top);
				vg.FillColor(NanoVg.Rgba(240, 240, 240, 192));
				vg.Text(x + 3, y + 1, _name);
			}

			switch (_style)
			{
			    case GraphRenderStyle.Fps:
			        vg.FontSize(16.0f);
			        vg.TextAlign(NvgAlign.Right | NvgAlign.Top);
			        vg.FillColor(NanoVg.Rgba(240, 240, 240, 255));
			        str = $"{1.0f / avg:0.00} FPS";
			        vg.Text(x + w - 3, y + 1, str);

			        vg.FontSize(15.0f);
			        vg.TextAlign(NvgAlign.Right | NvgAlign.Bottom);
			        vg.FillColor(NanoVg.Rgba(240, 240, 240, 160));
			        str = $"{avg * 1000.0f:0.00} ms/f";
			        vg.Text(x + w - 3, y + h - 1, str);
			        break;
			    case GraphRenderStyle.Raw:
			        vg.FontSize(16.0f);
			        vg.TextAlign(NvgAlign.Right | NvgAlign.Top);
			        vg.FillColor(NanoVg.Rgba(240, 240, 240, 255));
			        str = $"{avg:0.00}";
			        vg.Text(x + w - 3, y + 1, str);
			        break;
			    case GraphRenderStyle.Milliseconds:
                    vg.FontSize(16.0f);
			        vg.TextAlign(NvgAlign.Right | NvgAlign.Top);
			        vg.FillColor(NanoVg.Rgba(240, 240, 240, 255));
			        str = $"{avg * 1000.0f:0.00} ms";
			        vg.Text(x + w - 3, y + 1, str);
			        break;
			}
		}
	}
}

