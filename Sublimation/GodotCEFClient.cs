using System;
using System.IO;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;


using Godot;

using Xilium.CefGlue;

namespace Sublimation
{
	internal class GodotCEFClient : CefClient
	{
		private readonly OffscreenLoadHandler _loadHandler;
		private readonly GodotRenderHandler _renderHandler;

		private static readonly object _sPixelLock = new();
		private byte[] _sPixelBuffer;
		protected int width;
		protected int height;

		public CefBrowserHost Host { get; private set; }

		public GodotCEFClient(Vector2I windowSize, bool hideScrollbars = false) {
			_loadHandler = new OffscreenLoadHandler(this, hideScrollbars);
			_renderHandler = new GodotRenderHandler(windowSize.X, windowSize.Y, this);

			_sPixelBuffer = new byte[windowSize.X * windowSize.Y * 4];

			width = windowSize.X;
			height = windowSize.Y;

			GD.Print("Constructed Offscreen Client");
		}

		public void UpdateTexture(ImageTexture pTexture,Image image) {
			if (Host != null) {
				lock (_sPixelLock) {
					if (Host != null) {
						image.SetData(width, height, false, Image.Format.Rgba8, _sPixelBuffer);
						pTexture.Update(image);
					}
				}
			}
		}

		public void Shutdown() {
			if (Host != null) {
				GD.Print("Host Cleanup");
				Host.CloseBrowser(true);
				Host.Dispose();
				Host = null;
			}
		}

		#region Interface

		protected override CefRenderHandler GetRenderHandler() {
			return _renderHandler;
		}

		protected override CefLoadHandler GetLoadHandler() {
			return _loadHandler;
		}



		#endregion Interface
		#region Handlers

		internal class OffscreenLoadHandler(GodotCEFClient client, bool hideScrollbars) : CefLoadHandler
		{
			private readonly GodotCEFClient _client = client;
			private readonly bool _hideScrollbars = hideScrollbars;

			protected override void OnLoadStart(CefBrowser browser, CefFrame frame, CefTransitionType transitionType) {
				if (browser != null) {
					_client.Host = browser.GetHost();
				}

				if (frame.IsMain) {
					GD.Print($"START: {browser.GetMainFrame().Url}");
				}
			}

			protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode) {
				if (frame.IsMain) {
					GD.Print($"END: {browser.GetMainFrame().Url}, {httpStatusCode}" );

					if (_hideScrollbars) {
						HideScrollbars(frame);
					}
				}
			}

			private static void HideScrollbars(CefFrame frame) {
				var jsScript = "var head = document.head;" +
								  "var style = document.createElement('style');" +
								  "style.type = 'text/css';" +
								  "style.appendChild(document.createTextNode('::-webkit-scrollbar { visibility: hidden; }'));" +
								  "head.appendChild(style);";
				frame.ExecuteJavaScript(jsScript, string.Empty, 107);
			}
		}

		internal class GodotRenderHandler(int windowWidth, int windowHeight, GodotCEFClient client) : CefRenderHandler
		{
			private readonly GodotCEFClient _client = client;

			private readonly int _windowWidth = windowWidth;
			private readonly int _windowHeight = windowHeight;

			protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect) {
				GetViewRect(browser, out rect);
				return true;
			}

			protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY) {
				screenX = viewX;
				screenY = viewY;
				return true;
			}

			protected override void GetViewRect(CefBrowser browser, out CefRectangle rect) {
				rect = new CefRectangle {
					X = 0,
					Y = 0,
					Width = _windowWidth,
					Height = _windowHeight
				};
			}

			unsafe void Convert(int pixelCount, IntPtr rgbData) {
				if ((_client._sPixelBuffer?.Length ?? -1) != pixelCount * sizeof(uint)) {
					_client._sPixelBuffer = new byte[pixelCount * sizeof(uint)];
				}
				fixed (byte* rgbaP = &_client._sPixelBuffer[0]) {
					var rgbP = (byte*)rgbData;
					for (long i = 0; i < pixelCount; i++) {
						var end = ((uint*)rgbaP) + i;
						var start = ((uint*)rgbP) + i;
						//rgba = bgra
						((byte*)end)[0] = ((byte*)start)[2];
						((byte*)end)[1] = ((byte*)start)[1];
						((byte*)end)[2] = ((byte*)start)[0];
						((byte*)end)[3] = ((byte*)start)[3];
					}
				}
			}

			protected unsafe override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height) {
				if (browser != null) {
					lock (_sPixelLock) {
						if (browser != null) {
							//Cloud make follow dirtyRects to make faster
							Convert(height * width, buffer);
						}
					}
				}
			}

			protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo) {
				screenInfo.DeviceScaleFactor = 1;
				screenInfo.Depth = 1;
				screenInfo.Rectangle = new CefRectangle {
					X = 0,
					Y = 0,
					Width = _windowWidth,
					Height = _windowHeight
				};
				screenInfo.IsMonochrome = false;
				return true;
			}


			protected override void OnPopupSize(CefBrowser browser, CefRectangle rect) {
			}

			protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y) {
			}

			protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds) {
			}

			private sealed class OffscreenCefAccessibilityHandler : CefAccessibilityHandler
			{
				protected override void OnAccessibilityLocationChange(CefValue value) {
				}

				protected override void OnAccessibilityTreeChange(CefValue value) {
				}
			}

			private readonly OffscreenCefAccessibilityHandler _offscreenCefAccessibilityHandler = new();

			protected override CefAccessibilityHandler GetAccessibilityHandler() {
				return _offscreenCefAccessibilityHandler;
			}

			protected override void OnAcceleratedPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, nint sharedHandle) {

			}
		}

		#endregion Handlers

		public class GodotCEFApp : CefApp
		{
			private sealed class BrowserProcessHandler: CefBrowserProcessHandler {
				protected override void OnBeforeChildProcessLaunch(CefCommandLine commandLine) {
					commandLine.AppendArgument("--disable-render-loop");
					commandLine.AppendArgument("--headless");
				}
			}

			private readonly BrowserProcessHandler _browserProcessHandler = new ();

			protected override CefBrowserProcessHandler GetBrowserProcessHandler() {
				return _browserProcessHandler;
			}
		}
	}
}
