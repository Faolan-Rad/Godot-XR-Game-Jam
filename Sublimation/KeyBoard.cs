using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

using Xilium.CefGlue;

using KeyBoardKey = (float Width,
 int winKey,
 char? charicter,
	string label)?;

public partial class KeyBoard : StaticBody3D
{
	[Export]
	public BoxShape3D BoxShape;
	[Export]
	public SubViewport viewport;
	[Export]
	public CollisionShape3D CollisionShape;

	public static KeyBoard _KeyBoard;

	public VBoxContainer shift;

	public VBoxContainer normal;

	public static CefEventFlags _cefEventFlags = CefEventFlags.None;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_KeyBoard = this;
		KeyBoardKey[][] mainKeys = [
			[(1f, 0x1B, null, "ESC"), null, (1f, 0x70, null, "F1"), (1f, 0x71, null, "F2"), (1f, 0x72, null, "F3"), (1f, 0x73, null, "F4"), null, (1f, 0x74, null, "F5"), (1f, 0x75, null, "F6"), (1f, 0x76, null, "F7"), (1f, 0x77, null, "F8"), null, (1f, 0x78, null, "F9"), (1f, 0x79, null, "F10"), (1f, 0x7A, null, "F11"), (1f, 0x7B, null, "F12"), (1f, 0x2C, null, "Print\nScreen"), (1f, 0x91, null, "Scroll\nLock"), (1f, 0x03, null, "Pause\nBreak")],
			null,
			[(1f, 0xC0, '`', "`"), (1f, 0x31, '1', "1"), (1f, 0x32, '2', "2"), (1f, 0x33, '3', "3"), (1f, 0x34, '4', "4"), (1f, 0x35, '5', "5"), (1f, 0x36, '6', "6"), (1f, 0x37, '7', "7"), (1f, 0x38, '8', "8"), (1f, 0x39, '9', "9"), (1f, 0x30, '0', "0"), (1f, 0x5E, '-', "-"), (1f, 0xBB, '=', "="), (2f, 0x08, '\b', "<--"), (1f, 0x2D, null, "Insert"), (1f, 0x24, null, "Home"), (1f, 0x21, null, "Page\nUp"), (1f, 0x90, null, "Num"), (1f, 0x6F, '/', "/"), (1f, 0x6A, '*', "*"), (1f, 0x6D, '-', "-")],
			[(1.5f, 0x09, '	', "Tab"), (1f, 0x51, 'q', "q"), (1f, 0x57, 'w', "w"), (1f, 0x45, 'e', "e"), (1f, 0x52, 'r', "r"), (1f, 0x54, 't', "t"), (1f, 0x59, 'y', "y"), (1f, 0x55, 'u', "u"), (1f, 0x49, 'i', "i"), (1f, 0x4F, 'o', "o"), (1f, 0x50, 'p', "p"), (1f, 0xDB, '[', "["), (1f, 0xDD, ']', "]"), (1.5f, 0xDC, '\\', "\\"), (1f, 0x2E, null, "Delete"), (1f, 0x23, null, "End"), (1f, 0x22, null, "Page\nDown"), (1f, 0x67, '7', "7"), (1f, 0x68, '8', "8"), (1f, 0x69, '9', "9"), (1f, 0x6B, '+', "+")],
			[(1.6f, 0x14, null, "Caps\nLock"), (1f, 0x41, 'a', "a"), (1f, 0x53, 's', "s"), (1f, 0x44, 'd', "d"), (1f, 0x46, 'f', "f"), (1f, 0x47, 'g', "g"), (1f, 0x48, 'h', "h"), (1f, 0x4A, 'j', "j"), (1f, 0x4B, 'k', "k"), (1f, 0x4C, 'l', "l"), (1f, 0xBA, ';', ";"), (1f, 0xDE, '\'', "\'"), (2.5f, 0x0D, '\n', "Enter"), null, null, null, (1f, 0x64, '4', "4"), (1f, 0x65, '5', "5"), (1f, 0x66, '6', "6"), (1f, 0x6B, '+', "+")],
			[(2.2f, 0xA0, null, "Shift"), (1f, 0x5A, 'z', "z"), (1f, 0x58, 'x', "x"), (1f, 0x43, 'c', "c"), (1f, 0x56, 'v', "v"), (1f, 0x42, 'b', "b"), (1f, 0x4E, 'n', "n"), (1f, 0x4D, 'm', "m"), (1f, 0xBC, ',', ","), (1f, 0xBE, '.', "."), (1f, 0xBF, '/', "/"), (3f, 0xA1, null, "Shift"), null, (1f, 0x26, null, "up"), null, (1f, 0x61, '1', "1"), (1f, 0x62, '2', "2"), (1f, 0x63, '3', "3"), (1f, 0x0D, '\n', "Enter")],
			[(1.5f, 0x11, null, "Ctrl"), (1f, 0x5B, null, "Win"), (1f, 0xA4, null, "Alt"), (6.5f, 0x20, ' ', " "), (1.5f, 0xA5, null, "Alt"), (1.2f, 0x5C, null, "Win"), (1.2f, 0x7C, null, "F13"), (1.6f, 0x11, null, "Ctrl"), (1f, 0x25, null, "left"), (1f, 0x28, null, "down"), (1f, 0x27, null, "right"), (2.1f, 0x60, '0', "0"), (1f, 0xBE, '.', "."), (1f, 0x0D, '\n', "Enter")]
		];

		KeyBoardKey[][] shiftKeys = [
	[(1f, 0x1B, null, "ESC"), null, (1f, 0x70, null, "F1"), (1f, 0x71, null, "F2"), (1f, 0x72, null, "F3"), (1f, 0x73, null, "F4"), null, (1f, 0x74, null, "F5"), (1f, 0x75, null, "F6"), (1f, 0x76, null, "F7"), (1f, 0x77, null, "F8"), null, (1f, 0x78, null, "F9"), (1f, 0x79, null, "F10"), (1f, 0x7A, null, "F11"), (1f, 0x7B, null, "F12"), (1f, 0x2C, null, "Print\nScreen"), (1f, 0x91, null, "Scroll\nLock"), (1f, 0x03, null, "Pause\nBreak")],
			null,
			[(1f, 0xC0, '~', "~"), (1f, 0x31, '!', "!"), (1f, 0x32, '@', "@"), (1f, 0x33, '#', "#"), (1f, 0x34, '$', "$"), (1f, 0x35, '%', "%"), (1f, 0x36, '^', "^"), (1f, 0x37, '&', "&"), (1f, 0x38, '*', "*"), (1f, 0x39, '(', "("), (1f, 0x30, ')', ")"), (1f, 0x5E, '_', "_"), (1f, 0xBB, '+', "+"), (2f, 0x08, '\b', "<--"), (1f, 0x2D, null, "Insert"), (1f, 0x24, null, "Home"), (1f, 0x21, null, "Page\nUp"), (1f, 0x90, null, "Num"), (1f, 0x6F, '/', "/"), (1f, 0x6A, '*', "*"), (1f, 0x6D, '-', "-")],
			[(1.5f, 0x09, '	', "Tab"), (1f, 0x51, 'Q', "Q"), (1f, 0x57, 'W', "W"), (1f, 0x45, 'E', "E"), (1f, 0x52, 'R', "R"), (1f, 0x54, 'T', "T"), (1f, 0x59, 'Y', "Y"), (1f, 0x55, 'U', "U"), (1f, 0x49, 'I', "I"), (1f, 0x4F, 'O', "O"), (1f, 0x50, 'P', "P"), (1f, 0xDB, '{', "{"), (1f, 0xDD, '}', "}"), (1.5f, 0xDC, '|', "|"), (1f, 0x2E, null, "Delete"), (1f, 0x23, null, "End"), (1f, 0x22, null, "Page\nDown"), (1f, 0x67, '7', "7"), (1f, 0x68, '8', "8"), (1f, 0x69, '9', "9"), (1f, 0x6B, '+', "+")],
			[(1.6f, 0x14, null, "Caps\nLock"), (1f, 0x41, 'A', "A"), (1f, 0x53, 'S', "S"), (1f, 0x44, 'D', "D"), (1f, 0x46, 'F', "F"), (1f, 0x47, 'G', "G"), (1f, 0x48, 'H', "H"), (1f, 0x4A, 'J', "J"), (1f, 0x4B, 'K', "K"), (1f, 0x4C, 'L', "L"), (1f, 0xBA, ':', ":"), (1f, 0xDE, '"', "\""), (2.5f, 0x0D, '\n', "Enter"), null, null, null, (1f, 0x64, '4', "4"), (1f, 0x65, '5', "5"), (1f, 0x66, '6', "6"), (1f, 0x6B, '+', "+")],
			[(2.2f, 0xA0, null, "Shift"), (1f, 0x5A, 'Z', "Z"), (1f, 0x58, 'X', "X"), (1f, 0x43, 'C', "C"), (1f, 0x56, 'V', "V"), (1f, 0x42, 'B', "B"), (1f, 0x4E, 'N', "N"), (1f, 0x4D, 'M', "M"), (1f, 0xBC, '<', "<"), (1f, 0xBE, '>', ">"), (1f, 0xBF, '?', "?"), (3f, 0xA1, null, "Shift"), null, (1f, 0x26, null, "up"), null, (1f, 0x61, '1', "1"), (1f, 0x62, '2', "2"), (1f, 0x63, '3', "3"), (1f, 0x0D, '\n', "Enter")],
			[(1.5f, 0x11, null, "Ctrl"), (1f, 0x5B, null, "Win"), (1f, 0xA4, null, "Alt"), (6.5f, 0x20, ' ', " "), (1.5f, 0xA5, null, "Alt"), (1.2f, 0x5C, null, "Win"), (1.2f, 0x7C, null, "F13"), (1.6f, 0x11, null, "Ctrl"), (1f, 0x25, null, "left"), (1f, 0x28, null, "down"), (1f, 0x27, null, "right"), (2.1f, 0x60, '0', "0"), (1f, 0xBE, '.', "."), (1f, 0x0D, '\n', "Enter")]
];

		var root = new VBoxContainer();

		var hbox = new HBoxContainer();
		root.AddChild(hbox);
		void AddURLButton(string url) {
			var button = new Button {
				Text = url.Replace("https://", "").Replace("http://", ""),
				CustomMinimumSize = new Vector2(sizeOfThing * 1, sizeOfThing),
				Alignment = HorizontalAlignment.Center,
			};
			button.Pressed += () => WebBrowser._WebBrowser.GoToURL(url);
			hbox.AddChild(button);
		}

		AddURLButton("https://google.com/");
		AddURLButton("https://duckduckgo.com/");
		AddURLButton("http://frogfind.com/");
		AddURLButton("https://youtube.com/");
		AddURLButton("https://godotengine.org/");
		AddURLButton("https://github.com/Faolan-Rad/Godot-XR-Game-Jam");
		AddURLButton("https://itch.io/jam/godot-xr-game-jam");

		var container = new VBoxContainer();
		normal = container;

		var first = BuildKeyboard(mainKeys, container).ToArray().First();

		{
			var button = new Button {
				Text = "Back",
				CustomMinimumSize = new Vector2(sizeOfThing * 1, sizeOfThing),
				Alignment = HorizontalAlignment.Center,
			};
			button.Pressed += () => WebBrowser._WebBrowser.Back();
			first.AddChild(button);
		}
		{
			var button = new Button {
				Text = "Forward",
				CustomMinimumSize = new Vector2(sizeOfThing * 1, sizeOfThing),
				Alignment = HorizontalAlignment.Center,
			};
			button.Pressed += () => WebBrowser._WebBrowser.Forward();
			first.AddChild(button);
		}
		{
			var button = new Button {
				Text = "Refresh",
				CustomMinimumSize = new Vector2(sizeOfThing * 1, sizeOfThing),
				Alignment = HorizontalAlignment.Center,
			};
			button.Pressed += () => WebBrowser._WebBrowser.Reload();
			first.AddChild(button);
		}


		root.AddChild(container);

		var grabButton = new Button {
			CustomMinimumSize = new Vector2(sizeOfThing * 20, sizeOfThing * 1.25f),
			GrowHorizontal = Control.GrowDirection.Both,
			SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
			ButtonMask = MouseButtonMask.Right
		};
		grabButton.Pressed += GrabButton_Pressed;
		root.AddChild(grabButton);

		var econtainer = new VBoxContainer {
			Visible = false
		};
		BuildKeyboard(shiftKeys, econtainer).ToArray();
		shift = econtainer;
		viewport.GetChild(0).AddChild(root);
	}

	private void GrabButton_Pressed() {

	}

	public int sizeOfThing = 54;

	private IEnumerable<HBoxContainer> BuildKeyboard(KeyBoardKey[][] keys, VBoxContainer vBoxContainer) {
		var rowOne = true;
		foreach (var item in keys) {
			if (item is null) {
				var sep = new HSeparator {
					CustomMinimumSize = new Vector2(sizeOfThing / 2, sizeOfThing / 2)
				};
				vBoxContainer.AddChild(sep);
				rowOne = false;
				continue;
			}
			var hbox = new HBoxContainer();
			vBoxContainer.AddChild(hbox);
			foreach (var key in item) {
				if (key is not null) {
					var button = new Button {
						Text = key.Value.label,
						CustomMinimumSize = new Vector2(sizeOfThing * key.Value.Width, sizeOfThing),
						Alignment = HorizontalAlignment.Center,
					};
					hbox.AddChild(button);
					SetUpKey(button, key.Value.label, key.Value.winKey, key.Value.charicter);
				}
				else {
					if (rowOne) {
						var sep = new Control {
							CustomMinimumSize = new Vector2(sizeOfThing * 0.75f, sizeOfThing / 2)
						};
						hbox.AddChild(sep);
					}
					else {
						var sep = new Control {
							CustomMinimumSize = new Vector2(sizeOfThing, sizeOfThing / 2)
						};
						hbox.AddChild(sep);
					}
				}
			}
			rowOne = false;
			yield return hbox;
		}
	}

	private void SetUpKey(Button button, string label, int winKeyCode, char? charicter) {
		if (label is "Ctrl" or "Alt" or "Shift") {
			button.Toggled += (evente) => {
				WebBrowser.TrainsKeyboardInput(_cefEventFlags, evente, winKeyCode, charicter);
				if (evente && label == "Shift") {
					//shift.Visible = !shift.Visible;
					//normal.Visible = !normal.Visible;
				}
			};
			button.ToggleMode = true;
			return;
		}
		button.ButtonDown += () => WebBrowser.TrainsKeyboardInput(_cefEventFlags, true, winKeyCode, charicter);
		button.ButtonUp += () => WebBrowser.TrainsKeyboardInput(_cefEventFlags, false, winKeyCode, charicter);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {

	}

	private bool _lastFrameLeftClick;
	private bool _lastFrameMiddleClick;
	private bool _lastFrameRightClick;

	public void LaserHit(Player player, bool left, Vector3 position, bool isLeave) {
		var localPos = CollisionShape.GlobalTransform.Inverse() * position;
		localPos += BoxShape.Size / 2;
		localPos /= BoxShape.Size;
		localPos *= new Vector3(viewport.Size.X, viewport.Size.Y, 0);
		var pos = new Vector2(localPos.X, viewport.Size.Y - localPos.Y);
		viewport.PushInput(new InputEventMouseMotion {
			Device = left ? 1 : 2,
			Position = pos
		});
		var leftClick = player.GetLeftClickBool(left);
		var middleClick = player.GetMiddelClickBool(left);
		var rightClick = player.GetRightClickBool(left);

		if (leftClick != _lastFrameLeftClick) {
			viewport.PushInput(new Godot.InputEventMouseButton {
				Device = left ? 1 : 2,
				Pressed = leftClick,
				Position = pos,
				ButtonMask = MouseButtonMask.Left,
				ButtonIndex = MouseButton.Left
			});
		}
		_lastFrameLeftClick = leftClick;


		if (middleClick != _lastFrameMiddleClick) {
			viewport.PushInput(new Godot.InputEventMouseButton {
				Device = left ? 1 : 2,
				Pressed = leftClick,
				Position = pos,
				ButtonMask = MouseButtonMask.Middle,
				ButtonIndex = MouseButton.Middle
			});
		}
		_lastFrameMiddleClick = middleClick;

		if (rightClick != _lastFrameRightClick) {
			viewport.PushInput(new Godot.InputEventMouseButton {
				Device = left ? 1 : 2,
				Pressed = leftClick,
				Position = pos,
				ButtonMask = MouseButtonMask.Right,
				ButtonIndex = MouseButton.Right
			});
		}
		_lastFrameRightClick = rightClick;
	}
}
