﻿// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Windows.Markup;

namespace A2v10.Xaml
{
	[ContentProperty("Content")]
	public class ContentControl : Control
	{
		public Object Content { get; set; }

        internal override void RenderElement(RenderContext context, Action<TagBuilder> onRender = null)
        {
            if (Content is UIElementBase)
                (Content as UIElementBase).RenderElement(context, onRender);
        }

        internal override void MergeAttributes(TagBuilder tag, RenderContext context, MergeAttrMode mode = MergeAttrMode.All)
        {
            base.MergeAttributes(tag, context, mode);
            var contBind = GetBinding(nameof(Content));
            if (contBind != null)
                tag.MergeAttribute("v-text", contBind.GetPathFormat(context));
        }

        internal void RenderContent(RenderContext context)
        {
            RenderContent(context, Content);
        }

		protected override void OnEndInit()
		{
			base.OnEndInit();
			XamlElement xamlElem = Content as XamlElement;
			if (xamlElem != null)
				xamlElem.SetParent(this); 
		}

        internal Boolean HasContent
        {
            get
            {
                if (GetBinding(nameof(Content)) != null)
                    return true;
                if (Content is String)
                    return !String.IsNullOrWhiteSpace(Content.ToString());
                if (Content != null)
                    return true;
                return false;
            }
        }
	}
}
