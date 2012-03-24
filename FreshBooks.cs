﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Dynamic;

namespace HastyAPI.FreshBooks {
	public class FreshBooks {
		string Username { get; set; }
		string Token { get; set; }

		public FreshBooks(string username, string token) {
			Username = username;
			Token = token;
		}

		public dynamic Call(string method, Action<dynamic> getinputs = null) {
			var req = CreateRequestXML(method, getinputs);

			System.Diagnostics.Debug.WriteLine("Sending request:\r\n" + req.ToString());

			var text = new APIRequest("https://" + Username + ".freshbooks.com/api/2.1/xml-in")
				.WithBasicCredentials(Token, null)
				.WithData(req.ToString())
				.Post()
				.EnsureStatus(200)
				.Text;

			var resp = XDocument.Parse(text);
			System.Diagnostics.Debug.WriteLine("Received response:\r\n" + resp.ToString());

			return resp.ToDynamic();
		}

		private XDocument CreateRequestXML(string method, Action<dynamic> getinputs) {
			var xml = new XDocument();

			var request = new XElement("request");
			request.SetAttributeValue("method", method);
			xml.Add(request);

			if(getinputs != null) {
				dynamic inputs = new ExpandoObject();
				getinputs(inputs);

				foreach(var p in (inputs as IDictionary<string, object>)) {
					var name = p.Key;
					var value = p.Value;

					// special formatting (see http://developers.freshbooks.com/)
					if(value is DateTime) value = ((DateTime)value).ToString("yyyy-MM-dd hh:mm:ss");
					else if(value is bool) value = (bool)value ? "1" : "0";

					request.Add(new XElement(name, value));
				}
			}

			return xml;
		}

	}
}
