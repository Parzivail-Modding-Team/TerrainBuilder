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
			_head = (_head + 1) % _graphHistoryCount;
			_values[_head] = frameTime;
		}

		public float GetGraphAverage()
		{
			int i;
			float avg = 0;
			for (i = 0; i < _graphHistoryCount; i++)
			{
				avg += _values[i];
			}
			return avg / _graphHistoryCount;
		}

		public void RenderGraph(NvgContext vg, float x, float y)
		{
			int i;
		    string str;
			var avg = GetGraphAverage();
			const float w = 200;
			const float h = 35;

			vg.BeginPath();
			vg.Rect(x, y, w, h);
			vg.FillColor(NanoVg.Rgba(0, 0, 0, 128));
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
			            var vy = y + h - v / avg * 0.5f * h;
			            vg.LineTo(vx, vy);
			        }

			        break;
			    case GraphRenderStyle.Percent:
			        for (i = 0; i < _graphHistoryCount; i++)
			        {
			            var v = _values[(_head + i) % _graphHistoryCount] * 1.0f;
			            if (v > 100.0f)
			                v = 100.0f;
			            var vx = x + (float)i / (_graphHistoryCount - 1) * w;
			            var vy = y + h - v / 100.0f * h;
			            vg.LineTo(vx, vy);
			        }

			        break;
			    default:
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

			vg.FontFace("sans");

			if (_name[0] != '\0')
			{
				vg.FontSize(14.0f);
				vg.TextAlign(NvgAlign.Left | NvgAlign.Top);
				vg.FillColor(NanoVg.Rgba(240, 240, 240, 192));
				vg.Text(x + 3, y + 1, _name);
			}

			switch (_style)
			{
			    case GraphRenderStyle.Fps:
			        vg.FontSize(18.0f);
			        vg.TextAlign(NvgAlign.Right | NvgAlign.Top);
			        vg.FillColor(NanoVg.Rgba(240, 240, 240, 255));
			        str = $"{1.0f / avg:0.00} FPS";
			        vg.Text(x + w - 3, y + 1, str);

			        vg.FontSize(15.0f);
			        vg.TextAlign(NvgAlign.Right | NvgAlign.Bottom);
			        vg.FillColor(NanoVg.Rgba(240, 240, 240, 160));
			        str = $"{avg * 1000.0f:0.00} ms";
			        vg.Text(x + w - 3, y + h - 1, str);
			        break;
			    case GraphRenderStyle.Percent:
			        vg.FontSize(18.0f);
			        vg.TextAlign(NvgAlign.Right | NvgAlign.Top);
			        vg.FillColor(NanoVg.Rgba(240, 240, 240, 255));
			        str = $"{avg * 1.0f:0.0} %";
			        vg.Text(x + w - 3, y + 1, str);
			        break;
			    default:
			        vg.FontSize(18.0f);
			        vg.TextAlign(NvgAlign.Right | NvgAlign.Top);
			        vg.FillColor(NanoVg.Rgba(240, 240, 240, 255));
			        str = $"{avg * 1000.0f:0.00} ms";
			        vg.Text(x + w - 3, y + 1, str);
			        break;
			}
		}
	}
}

