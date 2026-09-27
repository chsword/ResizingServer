# ResizingServer

[![install from nuget](http://img.shields.io/nuget/v/ResizingClient.svg?style=flat-square)](https://www.nuget.org/packages/ResizingClient)
[![release](https://img.shields.io/github/release/chsword/ResizingServer.svg?style=flat-square)](https://github.com/chsword/ResizingServer/releases)
[![Build status](https://ci.appveyor.com/api/projects/status/wcumkaagutgapwmn?svg=true)](https://ci.appveyor.com/project/chsword/resizingserver)
[![CodeFactor](https://www.codefactor.io/repository/github/chsword/resizingserver/badge)](https://www.codefactor.io/repository/github/chsword/resizingserver)

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

ResizingClient package supports `net45`, `netstandard2.0`, `net8.0`, and `net10.0`.


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
var url2 = ResizingUtil.Format(url,100,100);
var tencentUrl = ResizingUtil.FormatTencentCdnUrl("https://img.example.com/a.jpg", 100, 100, ResizingMode.Crop);
var aliyunUrl = ResizingUtil.FormatAliyunCdnUrl("https://img.example.com/a.jpg", 100, 100, ResizingMode.Max);
var awsUrl = ResizingUtil.FormatAwsCdnUrl(
    "https://images.example.com", "source-bucket", "a.jpg",
    100, 100, ResizingFormat.WebP, ResizingMode.Pad);

```

### Output format conversion

`ResizingFormat` supports `Original` (no explicit conversion), `Jpeg`, `Png`, and
`WebP`. Existing resize-only overloads remain available. New overloads take
`outputFormat` before the optional `mode`.

``` c#
// Self-hosted: retain the source extension; select the output using ?format=webp.
var localWebP = ResizingUtil.FormatUrl(result.FormatUrl, 200, 300, ResizingFormat.WebP);
var localFormatOnly = ResizingUtil.FormatImageUrl("https://img.example.com/upload/a.png", ResizingFormat.Jpeg);

// Tencent COS / Cloud Infinite: resize and convert, or convert only.
var tencentWebP = ResizingUtil.FormatTencentCdnUrl(
    "https://img.example.com/a.jpg", 200, 300, ResizingFormat.WebP, ResizingMode.Crop);
var tencentPng = ResizingUtil.FormatTencentCdnUrl("https://img.example.com/a.jpg", ResizingFormat.Png);

// Aliyun OSS image processing.
var aliyunWebP = ResizingUtil.FormatAliyunCdnUrl(
    "https://img.example.com/a.jpg", 200, 300, ResizingFormat.WebP, ResizingMode.Max);
var aliyunJpeg = ResizingUtil.FormatAliyunCdnUrl("https://img.example.com/a.png", ResizingFormat.Jpeg);

// AWS Dynamic Image Transformation for Amazon CloudFront (deployed separately).
var awsWebP = ResizingUtil.FormatAwsCdnUrl(
    "https://images.example.com", "source-bucket", "folder/a.jpg",
    200, 300, ResizingFormat.WebP, ResizingMode.Pad);
var awsPng = ResizingUtil.FormatAwsCdnUrl(
    "https://images.example.com", "source-bucket", "folder/a.jpg", ResizingFormat.Png);
```

| Backend | Resize modes: Crop / Max / Pad | Format syntax |
| --- | --- | --- |
| Self-hosted ImageResizer | `crop` / `max` / `pad` | `format=jpg`, `format=png`, `format=webp` |
| Tencent COS / CI | Cover resize then center crop / fit within / fit with padding | `imageMogr2/.../format/webp` |
| Aliyun OSS | `m_fill` / `m_lfit` / `m_pad` | `x-oss-process=image/.../format,webp` |
| AWS Dynamic Image Transformation | Sharp `cover` / `inside` / `contain` | Base64 UTF-8 JSON path with `bucket`, `key`, `edits.resize`, `edits.toFormat` |

Tencent Crop now uses `thumbnail/!200x300r` before center cropping, rather than
fitting inside the target rectangle (which could leave too few pixels to crop).
Provider-specific upscaling limits, padding colors, transparency, animation and
encoding defaults still apply; the methods do not guarantee byte-identical output.
New cloud resize calls require positive width/height and a defined mode/format.

Use original, unprocessed image URLs for Tencent, OSS and self-hosted methods.
Ordinary query parameters and fragments are retained, but existing image-processing
instructions are not merged. Generate processing URLs **before signing**: these
helpers neither create signatures nor re-sign modified URLs.

AWS requires an actual deployment of
[Dynamic Image Transformation for Amazon CloudFront](https://github.com/aws-solutions/dynamic-image-transformation-for-amazon-cloudfront).
Pass its HTTP(S) endpoint (without query parameters, fragment or credentials), an
allowed source bucket, and the exact, **unescaped S3 object key**, not an S3 URL.
JSON escaping and UTF-8 Base64 encoding are handled by the client. The bucket must
be in the deployment's `SOURCE_BUCKETS`; if signatures are enabled, sign the final
generated URL separately. `Original` omits the format edit; backend auto-format
settings such as `AUTO_WEBP` can still apply.

The old `FormatAwsCdnUrl(url, width, height, mode)` overload is deprecated and
retained only for compatibility with custom services that understand its legacy
`x-amz-process` query. **S3 and CloudFront do not natively implement that protocol.**
Use the endpoint/bucket/key overloads above for the AWS solution.

Protocol references:
[Tencent resize](https://cloud.tencent.com/document/product/460/36540),
[Tencent format](https://cloud.tencent.com/document/product/460/36543),
[OSS format](https://help.aliyun.com/zh/oss/user-guide/convert-image-formats-2),
[AWS request parsing](https://github.com/aws-solutions/dynamic-image-transformation-for-amazon-cloudfront/blob/main/source/image-handler/image-request.ts).

### Self-hosted WebP deployment

The server now requires **Windows IIS and .NET Framework 4.7.2 or later**.
The client library's supported frameworks are unchanged. The server registers
ImageResizer's `WebPEncoder` with Imazen.WebP 11 and maintained Windows x86/x64
native runtime packages; the pinned runtime package 1.6.1 contains libwebp 1.6.0.
Do not install the obsolete native binaries bundled with the old WebP package.

- Install the Microsoft Visual C++ 2015–2022 Redistributable matching the IIS
  application pool architecture.
- Build/publish the server and deploy the complete output, including
  `Imazen.WebP.dll`, `ImageResizer.Plugins.WebP.dll`, and all four native DLLs in
  **each** of `bin/runtimes/win-x64/native` and `bin/runtimes/win-x86/native`.
  Remove obsolete native WebP DLLs from earlier deployments; use a clean deployment.
- Keep the Imazen.WebP binding redirect and the `WebPEncoder` registration in
  `Web.config`. **Do not add `downloadNativeDependencies`, even with value `false`:**
  the legacy plugin checks attribute presence and would download obsolete binaries.
- `WebPEncoder` requires appropriate **ImageResizer Elite licensing**; deploying
  this repository does not grant that license.
- Keep `.jpg`/`.png` source URLs and request `?format=webp`. Changing the source
  extension to `.webp` is not conversion. This config enables WebP **output** from
  existing sources, not WebP uploads/decoding through the `/u/` route.

The existing MSTest project includes native-encoder integration tests for JPEG/PNG
to WebP, decoded dimensions, MIME types, transparency, standard-format regressions,
and native deployment files. On Windows, restore/build the server first, then run:

``` powershell
nuget restore source/ResizingServer.sln
msbuild source/ResizingServer/ResizingServer.csproj /p:Configuration=Release /p:Platform=AnyCPU
dotnet test source/ResizingClient.Tests/ResizingClient.Tests.csproj -c Release -p:RunServerTests=true
```

Use .NET SDK 10 for restoring all client targets. The Windows CI job runs these
tests; the default test command continues to run the net8.0 client tests.
Cloud URL tests validate request syntax, not live cloud responses. Verify provider
configuration and actual IIS responses in your deployment environment.

Open Source Projects in Use

[ImageResizer](http://imageresizing.net/) 

[Opserver](https://github.com/opserver/Opserver)

[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
