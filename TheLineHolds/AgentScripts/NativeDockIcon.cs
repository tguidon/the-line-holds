using System;
using System.IO;
using System.Runtime.InteropServices;
return DockIconProbe.Capture();
public static class DockIconProbe
{
 const string Lib="/usr/lib/libobjc.A.dylib";
 [DllImport(Lib)] static extern IntPtr objc_getClass(string name);
 [DllImport(Lib)] static extern IntPtr sel_registerName(string name);
 [DllImport(Lib,EntryPoint="objc_msgSend")] static extern IntPtr Send(IntPtr receiver,IntPtr selector);
 [DllImport(Lib,EntryPoint="objc_msgSend")] static extern ulong Length(IntPtr receiver,IntPtr selector);
 public static string Capture()
 {
  var app=Send(objc_getClass("NSApplication"),sel_registerName("sharedApplication"));var icon=Send(app,sel_registerName("applicationIconImage"));
  if(icon==IntPtr.Zero)throw new Exception("Running application has no Dock icon image");
  var data=Send(icon,sel_registerName("TIFFRepresentation"));int size=checked((int)Length(data,sel_registerName("length")));var bytes=Send(data,sel_registerName("bytes"));
  if(size<4096)throw new Exception("Running icon has no meaningful bitmap");var copy=new byte[size];Marshal.Copy(bytes,copy,0,size);string path=Path.Combine(Path.GetTempPath(),"line-holds-running-icon.tiff");File.WriteAllBytes(path,copy);return $"Read actual running NSApplication icon: {size} bytes at {path}";
 }
}
