﻿// Copyright © 2015-2017 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Infrastructure
{
	[Flags]
	public enum StdPermissions
	{
		None      =    0,
		CanView   = 0x01,
		CanEdit   = 0x02,
		CanDelete = 0x04,
		CanApply  = 0x08
	}
}
