using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace TheLineHolds.Editor
{
    public static class DesktopIdentity
    {
        public const string Identifier="com.thelineholds.game";
        [UnityEditor.Callbacks.PostProcessBuild(100)]
        public static void RegisterMacApplication(BuildTarget target,string appPath)
        {
#if UNITY_EDITOR_OSX
            if(target!=BuildTarget.StandaloneOSX)return;
            // CLI launches execute the player directly. Register bundle metadata for the Dock first.
            const string registration="/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister";
            if(!File.Exists(registration))throw new FileNotFoundException("macOS application registration tool missing",registration);
            var start=new ProcessStartInfo(registration){UseShellExecute=false,CreateNoWindow=true};start.ArgumentList.Add("-f");start.ArgumentList.Add(Path.GetFullPath(appPath));
            using(var process=Process.Start(start)){process.WaitForExit();if(process.ExitCode!=0)throw new Exception("macOS application registration failed: "+process.ExitCode);}
#endif
        }
        public static string Configure()
        {
            var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TheLineHolds/Art/Identity/AppIcon.png");
            if(!icon)throw new Exception("Application icon missing");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone,Identifier);
            PlayerSettings.bundleVersion="0.1.1";PlayerSettings.macOS.buildNumber="2";
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});
            var sizes=PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Standalone,IconKind.Application);
            var icons=new Texture2D[sizes.Length];Array.Fill(icons,icon);
            if(icons.Length>0)PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone,icons,IconKind.Application);
            AssetDatabase.SaveAssets();return "Configured unique desktop bundle identity and explicit application icons";
        }
    }
}
