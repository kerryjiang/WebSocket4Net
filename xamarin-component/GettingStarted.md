# Getting Started with WebSocket4Net


## 1. Add namespace using

    using WebSocket4Net;

## 2. Create instance and then define event handler

    WebSocket websocket = new WebSocket("ws://localhost:2012/");
	websocket.Opened += new EventHandler(websocket_Opened);
	websocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
	websocket.Closed += new EventHandler(websocket_Closed);
	websocket.MessageReceived += new EventHandler(websocket_MessageReceived);


## 3. Communication after the connection open

	private void websocket_Opened(object sender, EventArgs e)
	{
	     websocket.Send("Hello World!");
	}