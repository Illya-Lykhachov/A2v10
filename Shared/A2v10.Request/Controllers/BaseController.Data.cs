﻿// Copyright © 2015-2017 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using A2v10.Infrastructure;
using A2v10.Data.Interfaces;
using System.Collections.Specialized;
using System.Collections.Generic;
using A2v10.Interop;
using System.Text;

namespace A2v10.Request
{
	public partial class BaseController
	{
		public async Task Data(String command, Action<ExpandoObject> setParams /*Int32 tenantId, Int64 userId*/, String json, HttpResponseBase response)
		{
			switch (command.ToLowerInvariant())
			{
				case "save":
					await SaveData(setParams, json, response.Output);
					break;
				case "reload":
					await ReloadData(setParams, json, response.Output);
					break;
				case "dbremove":
					await DbRemove(setParams, json, response.Output);
					break;
				case "expand":
					await ExpandData(setParams, json, response.Output);
					break;
				case "loadlazy":
					await LoadLazyData(setParams, json, response.Output);
					break;
				case "invoke":
					await InvokeData(setParams, json, response);
					break;
				default:
					throw new RequestModelException($"Invalid data action {command}");
			}
		}

		void CheckUserState(ExpandoObject prms)
		{
			if (_userStateManager == null)
				return;
			Int64 userId = prms.Get<Int64>("UserId");
			if (_userStateManager.IsReadOnly(userId))
				throw new RequestModelException("UI:@[Error.DataReadOnly]");
		}

		async Task SaveData(Action<ExpandoObject> setParams, String json, TextWriter writer)
		{
			ExpandoObject dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
			String baseUrl = dataToSave.Get<String>("baseUrl");
			ExpandoObject data = dataToSave.Get<ExpandoObject>("data");
			var rm = await RequestModel.CreateFromBaseUrl(_host, Admin, baseUrl);
			RequestView rw = rm.GetCurrentAction();
			var prms = new ExpandoObject();
			setParams?.Invoke(prms);
			prms.Append(rw.parameters);
			CheckUserState(prms);
			IDataModel model = await _dbContext.SaveModelAsync(rw.CurrentSource, rw.UpdateProcedure, data, prms);
			IModelHandler handler = rw.GetHookHandler(_host);
			if (handler != null)
				await handler.AfterSave(data, model.Root);
			WriteDataModel(model, writer);
		}

		async Task InvokeData(Action<ExpandoObject> setParams, String json, HttpResponseBase response)
		{
			ExpandoObject dataToInvoke = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
			String baseUrl = dataToInvoke.Get<String>("baseUrl");
			String command = dataToInvoke.Get<String>("cmd");
			ExpandoObject dataToExec = dataToInvoke.Get<ExpandoObject>("data");
			if (dataToExec == null)
				dataToExec = new ExpandoObject();
			setParams?.Invoke(dataToExec);
			var rm = await RequestModel.CreateFromBaseUrl(_host, Admin, baseUrl);
			var cmd = rm.GetCommand(command);
			dataToExec.Append(cmd.parameters);
			await ExecuteCommand(cmd, dataToExec, response);
		}

		async Task ExecuteCommand(RequestCommand cmd, ExpandoObject dataToExec, HttpResponseBase response)
		{
			switch (cmd.type)
			{
				case CommandType.sql:
					await ExecuteSqlCommand(cmd, dataToExec, response.Output);
					break;
				case CommandType.startProcess:
					await StartWorkflow(cmd, dataToExec, response.Output);
					break;
				case CommandType.resumeProcess:
					await ResumeWorkflow(cmd, dataToExec, response.Output);
					break;
				case CommandType.clr:
					await ExecuteClrCommand(cmd, dataToExec, response.Output);
					break;
				case CommandType.xml:
					await ExecuteXmlCommand(cmd, dataToExec, response);
					break;
				default:
					throw new RequestModelException($"Invalid command type '{cmd.type}'");
			}
		}

		async Task ExecuteSqlCommand(RequestCommand cmd, ExpandoObject dataToExec, TextWriter writer)
		{
			IDataModel model = await _dbContext.LoadModelAsync(cmd.CurrentSource, cmd.CommandProcedure, dataToExec);
			WriteDataModel(model, writer);
		}

		async Task ExecuteXmlCommand(RequestCommand cmd, ExpandoObject dataToExec, HttpResponseBase response)
		{
			List<String> xmlSchemaPathes = null;
			if (cmd.xmlSchemas != null)
			{
				xmlSchemaPathes = new List<String>();
				foreach (var schema in cmd.xmlSchemas)
					xmlSchemaPathes.Add(Host.MakeFullPath(false, cmd.Path, schema + ".xsd"));
			}

			if(xmlSchemaPathes == null)
				throw new RequestModelException("The xml-schemes are not specified");

			foreach (var path in xmlSchemaPathes)
			{
				if (!System.IO.File.Exists(path))
					throw new RequestModelException($"File not found '{path}'");
			}

			IDataModel dm = await DbContext.LoadModelAsync(cmd.CurrentSource, cmd.XmlProcedure, dataToExec);

			var xmlCreator = new XmlCreator(xmlSchemaPathes, dm, "UTF-8")
			{
				Validate = cmd.validate
			};
			var bytes = xmlCreator.CreateXml();
			response.ContentType = "text/xml";
			var chars = Encoding.UTF8.GetString(bytes).ToCharArray();
			response.Write(chars, 0, chars.Length);
		}

		async Task ExecuteClrCommand(RequestCommand cmd, ExpandoObject dataToExec, TextWriter writer)
		{
			if (String.IsNullOrEmpty(cmd.clrType))
				throw new RequestModelException($"clrType must be specified for command '{cmd.command}'");
			var invoker = new ClrInvoker();
			Object result;
			if (cmd.async)
				result = await invoker.InvokeAsync(cmd.clrType, dataToExec);
			else
				result = invoker.Invoke(cmd.clrType, dataToExec);
			writer.Write(JsonConvert.SerializeObject(result, StandardSerializerSettings));
		}

		async Task StartWorkflow(RequestCommand cmd, ExpandoObject dataToStart, TextWriter writer)
		{
			if (_workflowEngine == null)
				throw new InvalidOperationException($"Service 'IWorkflowEngine' not registered");
			var swi = new StartWorkflowInfo
			{
				DataSource = cmd.CurrentSource,
				Schema = cmd.CurrentSchema,
				Model = cmd.CurrentModel,
				ModelId = dataToStart.Get<Int64>("Id"),
				ActionBase = cmd.ActionBase
			};
			if (swi.ModelId == 0)
				throw new RequestModelException("Id must be specified to 'startProcess' command");
			if (!String.IsNullOrEmpty(cmd.file))
				swi.Source = $"file:{cmd.file}";
			swi.Comment = dataToStart.Get<String>("Comment");
			swi.UserId = dataToStart.Get<Int64>("UserId");
			if (swi.Source == null)
				throw new RequestModelException($"File or clrType must be specified to 'startProcess' command");
			WorkflowResult wr = await _workflowEngine.StartWorkflow(swi);
			WriteJsonResult(writer, wr);
		}

		async Task ResumeWorkflow(RequestCommand cmd, ExpandoObject dataToStart, TextWriter writer)
		{
			if (_workflowEngine == null)
				throw new InvalidOperationException($"Service 'IWorkflowEngine' not registered");
			var rwi = new ResumeWorkflowInfo
			{
				Id = dataToStart.Get<Int64>("Id")
			};
			if (rwi.Id == 0)
				throw new RequestModelException("InboxId must be specified");
			rwi.UserId = dataToStart.Get<Int64>("UserId");
			rwi.Answer = dataToStart.Get<String>("Answer");
			rwi.Comment = dataToStart.Get<String>("Comment");
			rwi.Params = dataToStart.Get<ExpandoObject>("Params");
			WorkflowResult wr = await _workflowEngine.ResumeWorkflow(rwi);
			WriteJsonResult(writer, wr);
		}

		void WriteJsonResult(TextWriter writer, Object data)
		{
			writer.Write(JsonConvert.SerializeObject(data, StandardSerializerSettings));
		}

		async Task ReloadData(Action<ExpandoObject> setParams, String json, TextWriter writer)
		{
			ExpandoObject dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
			String baseUrl = dataToSave.Get<String>("baseUrl");

			ExpandoObject loadPrms = new ExpandoObject();
			if (baseUrl.Contains("?"))
			{
				var parts = baseUrl.Split('?');
				baseUrl = parts[0];
				// parts[1] contains query parameters
				var qryParams = HttpUtility.ParseQueryString(parts[1]);
				loadPrms.Append(CheckPeriod(qryParams), toPascalCase: true);
			}

			if (baseUrl == null)
				throw new RequestModelException("There are not base url for command 'reload'");

			var rm = await RequestModel.CreateFromBaseUrl(_host, Admin, baseUrl);
			RequestView rw = rm.GetCurrentAction();
			String loadProc = rw.LoadProcedure;
			if (loadProc == null)
				throw new RequestModelException("The data model is empty");
			setParams?.Invoke(loadPrms);
			loadPrms.Set("Id", rw.Id);
			loadPrms.Append(rw.parameters);
			ExpandoObject prms2 = loadPrms;
			if (rw.indirect)
			{
				// for indirect action - @UserId and @Id only
				prms2 = new ExpandoObject();
				setParams?.Invoke(prms2);
				prms2.Set("Id", rw.Id);
			}
			IDataModel model = await _dbContext.LoadModelAsync(rw.CurrentSource, loadProc, prms2);
			rw = await LoadIndirect(rw, model, loadPrms);
			WriteDataModel(model, writer);
		}

		async Task DbRemove(Action<ExpandoObject> setParams, String json, TextWriter writer)
		{
			ExpandoObject jsonData = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
			String baseUrl = jsonData.Get<String>("baseUrl");
			Object id = jsonData.Get<Object>("id");
			String propName = jsonData.Get<String>("prop");
			var rm = await RequestModel.CreateFromBaseUrl(_host, Admin, baseUrl);
			var action = rm.GetCurrentAction();
			if (action == null)
				throw new RequestModelException("There are no current action");
			String deleteProc = action.DeleteProcedure(propName);
			if (deleteProc == null)
				throw new RequestModelException("The data model is empty");
			ExpandoObject execPrms = new ExpandoObject();
			setParams?.Invoke(execPrms);
			execPrms.Set("Id", id);
			execPrms.Append(action.parameters);
			CheckUserState(execPrms);
			await _dbContext.LoadModelAsync(action.CurrentSource, deleteProc, execPrms);
			writer.Write("{\"status\": \"OK\"}"); // JSON!
		}

		public NameValueCollection CheckPeriod(NameValueCollection coll)
		{
			var res = new NameValueCollection();
			foreach (var key in coll.Keys)
			{
				var k = key?.ToString();
				if (k.ToLowerInvariant() == "period")
				{
					// parse period
					var ps = coll[k].Split('-');
					res.Remove("From"); // replace prev value
					res.Remove("To");
					if (ps[0].ToLowerInvariant() == "all")
					{
						// from js! utils.date.minDate/maxDate
						res.Add("From", "19010101");
						res.Add("To", "29991231"); 
					}
					else
					{
						res.Add("From", ps[0]);
						res.Add("To", ps.Length == 2 ? ps[1] : ps[0]);
					}
				}
				else
				{
					res.Add(k, coll[k]);
				}
			}
			return res;
		}

		void AddParamsFromUrl(ExpandoObject prms, String baseUrl)
		{
			if (baseUrl.Contains("?"))
			{
				// add query params from baseUrl
				var nvc = HttpUtility.ParseQueryString(baseUrl.Split('?')[1]);
				prms.Append(CheckPeriod(nvc), toPascalCase: true);
			}
		}

		async Task ExpandData(Action<ExpandoObject> setParams, String json, TextWriter writer)
		{
			ExpandoObject jsonData = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
			String baseUrl = jsonData.Get<String>("baseUrl");
			Object id = jsonData.Get<Object>("id");
			var rm = await RequestModel.CreateFromBaseUrl(_host, Admin, baseUrl);
			var action = rm.GetCurrentAction();
			if (action == null)
				throw new RequestModelException("There are no current action");
			String expandProc = action.ExpandProcedure;
			if (expandProc == null)
				throw new RequestModelException("The data model is empty");
			ExpandoObject execPrms = new ExpandoObject();
			AddParamsFromUrl(execPrms, baseUrl);
			setParams?.Invoke(execPrms);
			execPrms.Set("Id", id);
			execPrms.Append(action.parameters);
			IDataModel model = await _dbContext.LoadModelAsync(action.CurrentSource, expandProc, execPrms);
			WriteDataModel(model, writer);
		}

		async Task LoadLazyData(Action<ExpandoObject> setParams, String json, TextWriter writer)
		{
			ExpandoObject jsonData = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
			String baseUrl = jsonData.Get<String>("baseUrl");
			ExpandoObject execPrms = new ExpandoObject();
			AddParamsFromUrl(execPrms, baseUrl);
			Object id = jsonData.Get<Object>("id");
			String propName = jsonData.Get<String>("prop");
			var rm = await RequestModel.CreateFromBaseUrl(_host, Admin, baseUrl);
			var action = rm.GetCurrentAction();
			if (action == null)
				throw new RequestModelException("There are no current action");
			String loadProc = action.LoadLazyProcedure(propName.ToPascalCase());
			if (loadProc == null)
				throw new RequestModelException("The data model is empty");
			setParams?.Invoke(execPrms);
			execPrms.Set("Id", id);
			//execPrms.Append(action.parameters); // not needed

			IDataModel model = await _dbContext.LoadModelAsync(action.CurrentSource, loadProc, execPrms);
			WriteDataModel(model, writer);
		}

		void WriteDataModel(IDataModel model, TextWriter writer)
		{
			// Write data to output
			writer.Write(JsonConvert.SerializeObject(model.Root, StandardSerializerSettings));
		}
	}
}
