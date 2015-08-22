# ResizingServer

### Server Demo

### Client Demo

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
