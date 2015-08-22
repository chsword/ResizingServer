# ResizingServer

1. .NET base
1. image server 
1. support resizing thumb
1. use [ImageResizer](http://imageresizing.net/) 



### Server Demo


deploy ResizingServer to a web server 
and config
``` xml

```

### Client Demo

[![install from nuget](http://img.shields.io/nuget/v/ResizingClient.svg?style=flat-square)](https://www.nuget.org/packages/ResizingClient)
[![downloads](http://img.shields.io/nuget/dt/ResizingClient.svg?style=flat-square)](https://www.nuget.org/packages/ResizingClient)
[![release](https://img.shields.io/github/release/chsword/ResizingServer.svg?style=flat-square)](https://github.com/chsword/ResizingServer/releases)

upload to server 
``` c# 
var result=ResizingUtil.Upload(File.ReadAllBytes("d:\\a.jpg"), "a.jpg", "face").Result;
Console.WriteLine(result.FormatUrl);//like /u/face/b96225af353d15504302a087f4f46bb0151d1c{0}x{1}{2}.jpg
//Assert.IsTrue(result.IsSuccess);
```
convert format to url
``` c#
using ResizingClient;
// ...
var url1 = ResizingUtil.Format(url,100,100,ResizingMode.Pad);
var url1 = ResizingUtil.Format(url,100,100);

```
