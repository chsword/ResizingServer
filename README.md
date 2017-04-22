# ResizingServer

* .NET base
* Image server 
* Support resizing thumb
* Use [ImageResizer](http://imageresizing.net/) 



### Server Demo

Deploy ResizingServer to a web server 
and config
``` xml
  <appSettings>
    <add key="UploadRouteUrl" value="api" /><!--api routeurl http://host/{UploadRouteUrl} -->
    <add key="ApiKey" value="48DFD0EE-61A2-4CB5-B1D6-33E917A83202" /><!--when upload file use it -->
    <add key="AllowFolders" value="face,images" /><!-- folder/category  for diff biz line -->
  </appSettings>
```

physical path
like upload/face/1508/21/5a020a4161f543f197ddc0965aeeb66d.jpg
- upload
    - category(eg:face or images in config:AllowFolders)
        - yyMM (year and month)
            - dd (date)
                - {guid}.jpg

virtual path
you can use the ResizingClient to convert from a formatUrl to url
format url like /u/face/b96225af353d15504302a087f4f46bb0151d1c{0}x{1}{2}.jpg
url like /u/face/b96225af353d15504302a087f4f46bb0151d1c100x100c.jpg

### Client Demo

[![install from nuget](http://img.shields.io/nuget/v/ResizingClient.svg?style=flat-square)](https://www.nuget.org/packages/ResizingClient)
[![release](https://img.shields.io/github/release/chsword/ResizingServer.svg?style=flat-square)](https://github.com/chsword/ResizingServer/releases)
[![Build status](https://ci.appveyor.com/api/projects/status/wcumkaagutgapwmn?svg=true)](https://ci.appveyor.com/project/chsword/resizingserver)

Config the App.config / Web.config

```
  <appSettings>
    <add key ="ResizingServer.Host" value="http://192.168.1.99:43287/"/>
    <add key ="ResizingServer.UploadUrl" value="http://192.168.1.99:43287/api"/>
    <add key ="ResizingServer.ApiKey" value="48DFD0EE-61A2-4CB5-B1D6-33E917A83202"/>
  </appSettings>
```

Install nuget package
``` powershell
Install-Package ResizingClient
```


upload to server 
``` c# 
var result=ResizingUtil.Upload(File.ReadAllBytes("d:\\a.jpg"), "a.jpg", "face").Result;
Console.WriteLine(result.FormatUrl);//like /u/face/b96225af353d15504302a087f4f46bb0151d1c{0}x{1}{2}.jpg
//Assert.IsTrue(result.IsSuccess);
```
{0}:width
{1}:height
{2}:mode

mode enum:
- c:crop
- m:max
- p:pad 

convert format to url
``` c#
using ResizingClient;
// ...
var url1 = ResizingUtil.Format(url,100,100,ResizingMode.Pad);
var url1 = ResizingUtil.Format(url,100,100);

```


Open Source Projects in Use

[ImageResizer](http://imageresizing.net/) 
[Opserver](https://github.com/opserver/Opserver)
[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
