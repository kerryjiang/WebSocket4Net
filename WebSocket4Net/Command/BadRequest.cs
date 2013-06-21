using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WebSocket4Net.Command
{
	public class BadRequest : WebSocketCommandBase
	{
		private const string m_WebSocketVersion = "Sec-WebSocket-Version";
		private static readonly string[] m_ValueSeparator = new string[] { ", " };

		public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
		{
			var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			commandInfo.Text.ParseMimeHeader(dict);

			string websocketVersion = dict.GetValue(m_WebSocketVersion, string.Empty);

			if (!session.NotSpecifiedVersion)
			{
				if (string.IsNullOrEmpty(websocketVersion))
					session.FireError(new Exception("the server doesn't support the websocket protocol version your client was using"));
				else
					session.FireError(new Exception(string.Format("the server(version: {0}) doesn't support the websocket protocol version your client was using", websocketVersion)));
				session.CloseWithoutHandshake();
				return;
			}

			if (string.IsNullOrEmpty(websocketVersion))
			{
				session.FireError(new Exception("unknown server protocol version"));
				session.CloseWithoutHandshake();
				return;
			}

			var versions = websocketVersion.Split(',');

			versions = versions.Where((x) => x.Trim().Length > 0).ToArray();


			var versionValues = new int[versions.Length];

			for (var i = 0; i < versions.Length; i++)
			{
				try
				{
					int value = int.Parse(versions[i]);
					versionValues[i] = value;
				}
				catch (FormatException)
				{
					session.FireError(new Exception("invalid websocket version"));
					session.CloseWithoutHandshake();
					return;
				}
			}

			if (!session.GetAvailableProcessor(versionValues))
			{
				session.FireError(new Exception("unknown server protocol version"));
				session.CloseWithoutHandshake();
				return;
			}

			session.ProtocolProcessor.SendHandshake(session);
		}

		public override string Name
		{
			get { return OpCode.BadRequest.ToString(); }
		}
	}
}
