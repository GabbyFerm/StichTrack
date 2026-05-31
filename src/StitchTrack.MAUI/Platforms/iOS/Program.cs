// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using ObjCRuntime;
using StitchTrack.MAUI.Platforms.iOS;
using UIKit;

namespace StitchTrack.MAUI;

public class Program
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
