import AppKit
let path=CommandLine.arguments[1]
let image=NSWorkspace.shared.icon(forFile:path)
let data=NSBitmapImageRep(data:image.tiffRepresentation!)!.representation(using:.png,properties:[:])!
try data.write(to:URL(fileURLWithPath:CommandLine.arguments[2]))
print("Resolved application icon: \(image.size), \(data.count) bytes")
