﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Xaml
{
	public class Grid : UIElement
	{
        internal override void RenderElement(RenderContext context)
        {
            var grid = new TagBuilder("div", "grid");
            grid.RenderStart(context);
            grid.RenderEnd(context);
        }
    }
}
