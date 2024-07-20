# WebSocket4Net

[![build](https://github.com/kerryjiang/WebSocket4Net/workflows/build/badge.svg)](https://travis-ci.org/kerryjiang/WebSocket4Net)
[![MyGet Version](https://img.shields.io/myget/websocket4net/vpre/WebSocket4Net)](https://www.myget.org/feed/websocket4net/package/nuget/WebSocket4Net)
[![NuGet Beta Version](https://img.shields.io/nuget/vpre/WebSocket4Net.svg?style=flat)](https://www.nuget.org/packages/WebSocket4Net/)
[![NuGet Version](https://img.shields.io/nuget/v/WebSocket4Net.svg?style=flat)](https://www.nuget.org/packages/WebSocket4Net/)
[![NuGet](https://img.shields.io/nuget/dt/WebSocket4Net.svg)](https://www.nuget.org/packages/WebSocket4Net)
[![Badge](https://img.shields.io/badge/link-996.icu-red.svg)](https://996.icu/#/en_US)


A popular .NET WebSocket Client

This new version is built on SuperSocket 2.0 and modern .NET (.NET Core). It includes breaking changes from the previous WebSocket4Net version, so code adjustments may be necessary for upgrading.

## Usage 1: Read messages from event handler.

```csharp

using WebSocket4Net;

var websocket = new WebSocket("https://localhost/live");

websocket.PackageHandler += (sender, package) =>
{
    Console.WriteLine(package.Message);
}

await websocket.OpenAsync();

websocket.StartReceive();

await websocket.SendAsync("Hello");

//...

await websocket.CloseAsync();

```

## Usage 1: Read messages on demand.

```csharp

using WebSocket4Net;

var websocket = new WebSocket("https://localhost/live");

await websocket.OpenAsync();

await websocket.SendAsync("Hello");

while (true)
{
    var package = await websocket.ReceiveAsync();

    if (package == null)
        break;

    Console.WriteLine(package.Message);
}

//...

await websocket.CloseAsync();

```