using Godot;

using Sublimation;

using System;
using System.Runtime.InteropServices;

using Xilium.CefGlue;

public partial class WebBrowser : MeshInstance3D
{
	[Export]
	public PlaneMesh planeMesh;
	[Export]
	public BoxShape3D boxShape;
	[Export]
	public float scaler = 4f;
	[Export]
	public Vector2I _windowSize = new(1280, 720);
	[Export]
	public string _url = "https://www.google.com/";
	[Export]
	public CollisionShape3D CollisionShape;

	private GodotCEFClient _cefClient;
	private bool _hideScrollbars;

	Image _videoImage;
	ImageTexture _imageTexture;

	public static WebBrowser _WebBrowser;

	public override void _Ready() {
		var isHeadless = DisplayServer.GetName() == "headless";
		if (isHeadless) {
			throw new Exception("Should not be here");
		}
		_WebBrowser = this;
		var sizeX = (float)_windowSize.X / _windowSize.Y;
		planeMesh.Size = new Vector2(sizeX * scaler, scaler);
		boxShape.Size = new Vector3(planeMesh.Size.X, planeMesh.Size.Y, 0.01f);
		_videoImage = Image.Create(_windowSize.X, _windowSize.Y, false, Image.Format.Rgba8);
		MaterialOverride = new StandardMaterial3D {
			AlbedoTexture = _imageTexture = ImageTexture.CreateFromImage(_videoImage),
			CullMode = BaseMaterial3D.CullModeEnum.Disabled
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
		KeyboardInput += WebBrowser_KeyboardInput;
	}

	public static event Action<CefEventFlags, bool, int, char?> KeyboardInput;

	public static void TrainsKeyboardInput(CefEventFlags cefEventFlags, bool pressed, int winKeyCode, char? charicter) {
		KeyboardInput(cefEventFlags, pressed, winKeyCode, charicter);
	}

	private void WebBrowser_KeyboardInput(CefEventFlags cefEventFlags, bool pressed, int winKeyCode, char? charicter) {
		if (charicter is not null && pressed) {
			var cefKey = new CefKeyEvent {
				Modifiers = cefEventFlags,
				EventType = CefKeyEventType.Char,
				Character = charicter ?? '\0',
				WindowsKeyCode = winKeyCode,
				FocusOnEditableField = true,
			};
			_cefClient.Host.SendKeyEvent(cefKey);
		}
		if (pressed) {
			var cefKey = new CefKeyEvent {
				Modifiers = cefEventFlags,
				EventType = CefKeyEventType.RawKeyDown,
				Character = charicter ?? '\0',
				UnmodifiedCharacter = charicter ?? '\0',
				WindowsKeyCode = winKeyCode,
				IsSystemKey = false
			};
			_cefClient.Host.SendKeyEvent(cefKey);
		}
		else {
			var cefKey = new CefKeyEvent {
				Modifiers = cefEventFlags,
				EventType = CefKeyEventType.KeyUp,
				Character = charicter ?? '\0',
				WindowsKeyCode = winKeyCode,
				IsSystemKey = false
			};
			_cefClient.Host.SendKeyEvent(cefKey);
		}
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

	public void GoToURL(string url) {
		_cefClient?.Host?.GetBrowser()?.GetMainFrame()?.LoadUrl(url);
	}

	public void Back() {
		_cefClient?.Host?.GetBrowser()?.GoBack();
	}

	public void Forward() {
		_cefClient?.Host?.GetBrowser()?.GoForward();
	}

	public void Reload() {
		_cefClient?.Host?.GetBrowser()?.Reload();
	}

	private bool _lastFrameLeftClick;
	private bool _lastFrameMiddleClick;
	private bool _lastFrameRightClick;

	public void LaserHit(Player player, bool left, Vector3 position, bool isLeave) {
		var localPos = CollisionShape.GlobalTransform.Inverse() * position;
		localPos += boxShape.Size / 2;
		localPos /= boxShape.Size;
		localPos *= new Vector3(_windowSize.X, _windowSize.Y, 0);
		localPos = new Vector3(localPos.X, _windowSize.Y - localPos.Y, localPos.Z);
		if (_cefClient?.Host is not null) {
			var cefMOuseEvent = new CefMouseEvent {
				Modifiers = KeyBoard._cefEventFlags,
				X = (int)localPos.X,
				Y = (int)localPos.Y
			};
			var leftClick = player.GetLeftClickBool(left);
			var middleClick = player.GetMiddelClickBool(left);
			var rightClick = player.GetRightClickBool(left);

			if (leftClick != _lastFrameLeftClick) {
				var cefTouch = new CefTouchEvent {
					Type = leftClick? CefTouchEventType.Pressed : CefTouchEventType.Released,
					Id = left ? 2 : 1,
					PointerType = CefPointerType.Touch,
					Modifiers = KeyBoard._cefEventFlags,
					X = (int)localPos.X,
					Y = (int)localPos.Y,
					Pressure = player.GetLeftClick(left)
				};
				_cefClient.Host.SendTouchEvent(cefTouch);
			}
			else if (leftClick) {
				var cefTouch = new CefTouchEvent {
					Type = CefTouchEventType.Moved,
					Id = left ? 2 : 1,
					PointerType = CefPointerType.Touch,
					Modifiers = KeyBoard._cefEventFlags,
					X = (int)localPos.X,
					Y = (int)localPos.Y,
					Pressure = player.GetLeftClick(left)
				};
				_cefClient.Host.SendTouchEvent(cefTouch);
			}
			_cefClient.Host.SendMouseMoveEvent(cefMOuseEvent, isLeave);

			_lastFrameLeftClick = leftClick;


			if (middleClick != _lastFrameMiddleClick) {
				if (middleClick) {
					_cefClient.Host.SendMouseClickEvent(cefMOuseEvent, CefMouseButtonType.Middle, true, 1);
				}
				else {
					_cefClient.Host.SendMouseClickEvent(cefMOuseEvent, CefMouseButtonType.Middle, false, 0);
				}
			}
			_lastFrameMiddleClick = middleClick;

			if (rightClick != _lastFrameRightClick) {
				if (rightClick) {
					_cefClient.Host.SendMouseClickEvent(cefMOuseEvent, CefMouseButtonType.Right, true, 1);
				}
				else {
					_cefClient.Host.SendMouseClickEvent(cefMOuseEvent, CefMouseButtonType.Right, false, 0);
				}
			}
			_lastFrameRightClick = rightClick;
		}
	}
}
