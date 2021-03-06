﻿// Copyright © 2015-2017 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Infrastructure
{
	public interface IMessaging
	{
		IMessage CreateMessage();
		void QueueMessage(IMessage message);
	}
}
