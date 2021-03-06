﻿// Copyright © 2015-2017 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using System.Web;
using System.Dynamic;

using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;

using A2v10.Infrastructure;
using A2v10.Web.Identity;
using System.Security.Claims;

namespace A2v10.Web.Mvc.Hooks
{
	public class SetPersonNameHandler : IModelHandler
	{
		readonly IApplicationHost _host;
		readonly IOwinContext _context;
		readonly AppUserManager _userManager;
		readonly AppSignInManager _signInManager;

		public SetPersonNameHandler(IApplicationHost host)
		{
			_host = host;
			_context = HttpContext.Current.GetOwinContext();
			_userManager = _context.GetUserManager<AppUserManager>();
			_signInManager = _context.GetUserManager<AppSignInManager>();
		}

		public Task AfterSave(Object beforeData, Object afterData)
		{
			var after = afterData as ExpandoObject;

			var personName = after.Eval<String>("User.PersonName");
			var userId = after.Eval<Int64>("User.Id");


			// TODO: Update claims here
			var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
			var claim = identity.FindFirst("PersonName");
			if (claim != null)
				identity.RemoveClaim(claim);
			identity.AddClaim(new Claim("PersonName", personName));

			//var user = await _userManager.FindByIdAsync(userId);			
			_signInManager.AuthenticationManager.SignIn(identity);
			return Task.FromResult(0);
		}
	}
}

