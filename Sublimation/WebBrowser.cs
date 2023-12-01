using Godot;

using Sublimation;

using System;

using Xilium.CefGlue;

public partial class WebBrowser : MeshInstance3D
{
	[Export]
	public PlaneMesh planeMesh;
	[Export]
	public BoxShape3D boxShape;
	[Export]
	public float scaler = 2f;
	[Export]
	public Vector2I _windowSize = new(1280, 720);
	[Export]
	public string _url = "https://www.youtube.com/";

	private GodotCEFClient _cefClient;
	private bool _hideScrollbars;

	Image _videoImage;
	ImageTexture _imageTexture;

	public override void _Ready() {
		var isHeadless = DisplayServer.GetName() == "headless";
		if (isHeadless) {
			throw new Exception("Should not be here");
		}

		var sizeX = (float)_windowSize.X / _windowSize.Y;
		planeMesh.Size = new Vector2(sizeX * scaler, scaler);
		boxShape.Size = new Vector3(planeMesh.Size.X, planeMesh.Size.Y, 0.01f);
		_videoImage = Image.Create(_windowSize.X, _windowSize.Y, false, Image.Format.Rgba8);
		MaterialOverride = new StandardMaterial3D {
			AlbedoTexture = _imageTexture = ImageTexture.CreateFromImage(_videoImage)
		};

		CefRuntime.Load();
		var cefMainArgs = new CefMainArgs(System.Environment.GetCommandLineArgs());
		var cefApp = new GodotCEFClient.GodotCEFApp();
		if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero) != -1) {
			GD.PushError("Could not start the secondary process.");
			System.Environment.Exit(0);
			return;
		}

		var cefSettings = new CefSettings {
			MultiThreadedMessageLoop = false,
			LogSeverity = CefLogSeverity.Verbose,
			LogFile = "cef.log",
			WindowlessRenderingEnabled = true,
			NoSandbox = true,

		};
		CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);
		var cefWindowInfo = CefWindowInfo.Create();
		cefWindowInfo.SetAsWindowless(IntPtr.Zero, true);
		var cefBrowserSettings = new CefBrowserSettings() {
			BackgroundColor = new CefColor(255, 60, 85, 115),
			JavaScript = CefState.Enabled,
			JavaScriptAccessClipboard = CefState.Enabled,
			JavaScriptCloseWindows = CefState.Enabled,
			JavaScriptDomPaste = CefState.Enabled,
			LocalStorage = CefState.Enabled,
		};
		_cefClient = new GodotCEFClient(_windowSize, _hideScrollbars);

		CefBrowserHost.CreateBrowser(cefWindowInfo, _cefClient, cefBrowserSettings, string.IsNullOrEmpty(_url) ? "http://www.google.com" : _url);
	}

	public override void _Process(double delta) {
		if (_cefClient is not null) {
			CefRuntime.DoMessageLoopWork();
			_cefClient.UpdateTexture(_imageTexture, _videoImage);
		}
	}

	public override void _ExitTree() {
		if (_cefClient is not null) {
			_cefClient.Shutdown();
			_cefClient = null;
		}
	}

	public override void _Input(InputEvent @event) {
		if (_cefClient?.Host is not null) {
			if (@event is InputEventMouseMotion mouse) {
				var cefMouse = new CefMouseEvent {
					X = (int)mouse.Position.X,
					Y = (int)mouse.Position.Y,
				};
				_cefClient.Host.SendMouseMoveEvent(cefMouse, false);
			}
			if (@event is InputEventMouseButton mouseClick) {
				var cefMouse = new CefMouseEvent {
					X = (int)mouseClick.Position.X,
					Y = (int)mouseClick.Position.Y,
				};
				CefMouseButtonType? cefMouseButt = null;
				if (mouseClick.ButtonIndex == MouseButton.Left) {
					cefMouseButt = CefMouseButtonType.Left;
				}
				else if (mouseClick.ButtonIndex == MouseButton.Right) {
					cefMouseButt = CefMouseButtonType.Right;
				}
				else if (mouseClick.ButtonIndex == MouseButton.Middle) {
					cefMouseButt = CefMouseButtonType.Middle;
				}
				if (cefMouseButt is not null) {
					_cefClient.Host.SendMouseClickEvent(cefMouse, cefMouseButt.Value, !mouseClick.Pressed, 1);
				}
			}
			if (@event is InputEventKey keyevent) {
				var flags = CefEventFlags.None;
				if (keyevent.AltPressed) {
					flags |= CefEventFlags.AltDown;
				}
				if (keyevent.CtrlPressed) {
					flags |= CefEventFlags.ControlDown;
				}
				if (keyevent.ShiftPressed) {
					flags |= CefEventFlags.ShiftDown;
				}
				var chare = (char)keyevent.Unicode;
				var cefKey = new CefKeyEvent {
					Modifiers = flags,
					EventType = keyevent.Pressed ? CefKeyEventType.KeyDown : CefKeyEventType.KeyUp,
					Character = chare,
					WindowsKeyCode = (int)keyevent.Keycode
				};
				_cefClient.Host.SendKeyEvent(cefKey);
			}
		}
	}
}
