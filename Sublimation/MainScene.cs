using Godot;

using Sublimation;

using System;

using Xilium.CefGlue;

public partial class MainScene : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		var isHeadless = DisplayServer.GetName() == "headless";
		if (isHeadless) {
			CefRuntime.Load();
			var cefMainArgs = new CefMainArgs(System.Environment.GetCommandLineArgs());
			var cefApp = new GodotCEFClient.GodotCEFApp();

			// This is where the code path diverges for child processes.
			var code = CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero);
			if (code == -1) {
				GD.PushError("Could not start the secondary process.");
				GetTree().Quit(code);
				return;
			}
			GetTree().Quit(code);
			return;
		}

		if (XRServer.PrimaryInterface?.IsInitialized() ?? false) {
			GetViewport().UseXR = true;
		}

		GetTree().CallDeferred("change_scene_to_file", "res://LoadedScene.tscn");
	}
}
