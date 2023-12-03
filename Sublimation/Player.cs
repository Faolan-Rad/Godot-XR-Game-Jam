using Godot;

using System;

public partial class Player : XROrigin3D
{
	[Export]
	public XRCamera3D Camera;
	[Export]
	public XRController3D Left;
	[Export]
	public XRController3D Right;

	[Export]
	public XRController3D LeftAim;
	[Export]
	public XRController3D RightAim;

	[Export]
	public Sprite3D LeftRender;
	[Export]
	public Sprite3D RightRender;

	public PhysicsRayQueryParameters3D PhysicsRayQueryParameters;

	private static Godot.Variant _point = "point";
	private static Godot.Variant _position = "position";
	private static Godot.Variant _normal = "normal";
	private static Godot.Variant _collider = "collider";
	private static Godot.Variant _collider_id = "collider_id";
	private static Godot.Variant _shape = "shape";
	private static Godot.Variant _rid = "rid";
	private static Godot.Variant _linear_velocity = "linear_velocity";

	private Shape3D _lastShape;

	public void UpdateLaser(bool left, Vector3 from, Vector3 to) {
		PhysicsRayQueryParameters ??= new();
		PhysicsRayQueryParameters.From = from;
		PhysicsRayQueryParameters.To = to;
		var state = GetWorld3D().DirectSpaceState;

		var intersect = state.IntersectRay(PhysicsRayQueryParameters);

		if (intersect.Count == 0) {
			if (left) {
				LeftRender.GlobalPosition = to;
			}
			else {
				RightRender.GlobalPosition = to;
			}
			return;
		}

		var position = (Vector3)intersect[_position];
		var normal = (Vector3)intersect[_normal];
		var godotCollider = (CollisionObject3D)intersect[_collider];
		var shape = (int)intersect[_shape];

		var shaper = godotCollider.ShapeOwnerGetShape(0, shape);
		if (_lastShape != shaper) {
			if (_lastShape == WebBrowser._WebBrowser.boxShape) {
				WebBrowser._WebBrowser.LaserHit(this, left, position, true);
			}
			else if (_lastShape == KeyBoard._KeyBoard.BoxShape) {
				KeyBoard._KeyBoard.LaserHit(this, left, position, true);
			}
		}
		if (shaper == WebBrowser._WebBrowser.boxShape) {
			WebBrowser._WebBrowser.LaserHit(this, left, position, false);
		}
		else if (shaper == KeyBoard._KeyBoard.BoxShape) {
			KeyBoard._KeyBoard.LaserHit(this, left, position, false);
		}

		_lastShape = shaper;

		if (left) {
			LeftRender.GlobalPosition = position;
		}
		else {
			RightRender.GlobalPosition = position;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	private double _lastDelta;

	private Vector2 _mousePos;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		_lastDelta = delta;
		if (!(XRServer.PrimaryInterface?.IsInitialized() ?? false)) {
			if (_mouseHasPressed) {
				UpdateMouseMode(TargetMouseMode ?? Godot.Input.MouseModeEnum.Captured);
			}
			UpdateHead();
			UpdateLaser(false, Camera.GlobalPosition, (Camera.ProjectRayNormal(_mousePos) * 5000f) + Camera.GlobalPosition);
		}
		else {
			UpdateMouseMode(Godot.Input.MouseModeEnum.Visible);
			UpdateLaser(false, RightAim.GlobalPosition, RightAim.GlobalPosition + (RightAim.GlobalBasis * new Vector3(0, 0, -5000f)));
			UpdateLaser(true, LeftAim.GlobalPosition, LeftAim.GlobalPosition + (LeftAim.GlobalBasis * new Vector3(0, 0, -5000f)));
		}
	}

	private float _headYaw;
	private float _headPitch;
	public Input.MouseModeEnum? TargetMouseMode { get; set; }

	private void UpdateHead() {
		Camera.Transform = new Transform3D(new Basis(Quaternion.FromEuler(new Vector3(-_headPitch * (Mathf.Pi / 180), -_headYaw * (Mathf.Pi / 180), 0))), new Vector3(0, 1.7f, 0));
	}

	private static void UpdateMouseMode(Input.MouseModeEnum TargetMode) {
		if (Godot.Input.MouseMode != TargetMode) {
			Godot.Input.MouseMode = TargetMode;
		}
	}

	public void ToggleVisableMouse() {
		if (TargetMouseMode is null) {
			TargetMouseMode = Godot.Input.MouseModeEnum.Visible;
		}
		else if (TargetMouseMode == Godot.Input.MouseModeEnum.Visible) {
			TargetMouseMode = null;
		}
	}

	private bool _mouseHasPressed = false;

	private float _headYawSpeed = 10;


	private float _headPitchSpeed = 10;

	private static StringName _trigger = "trigger";
	private static StringName _trigger_click = "trigger_click";
	
	private static StringName _grip = "grip";
	private static StringName _grip_click = "grip_click";
	
	private static StringName _primary = "primary";
	private static StringName _primary_click = "primary_click";

	public float GetMiddelClick(bool left) {
		var value = 0f;
		if (!left && Input.IsMouseButtonPressed(MouseButton.Middle)) {
			value += 1f;
		}

		if (XRServer.PrimaryInterface?.IsInitialized() ?? false) {
			var targetControler = Right;
			if (left) {
				targetControler = Left;
			}
			value += targetControler.GetFloat(_primary);
			if (targetControler.IsButtonPressed(_primary_click)) {
				value += 1f;
			}
		}

		return Mathf.Clamp(value, 0, 1);

	}
	public float GetLeftClick(bool left) {
		var value = 0f;
		if (!left && Input.IsMouseButtonPressed(MouseButton.Left)) {
			value += 1f;
		}

		if (XRServer.PrimaryInterface?.IsInitialized() ?? false) {
			var targetControler = Right;
			if (left) {
				targetControler = Left;
			}
			value += targetControler.GetFloat(_trigger);
			if (targetControler.IsButtonPressed(_trigger_click)) {
				value += 1f;
			}
		}

		return Mathf.Clamp(value, 0, 1);
	}

	public float GetRightClick(bool left) {
		var value = 0f;
		if (!left && Input.IsMouseButtonPressed(MouseButton.Right)) {
			value += 1f;
		}

		if (XRServer.PrimaryInterface?.IsInitialized() ?? false) {
			var targetControler = Right;
			if (left) {
				targetControler = Left;
			}
			value += targetControler.GetFloat(_grip);
			if (targetControler.IsButtonPressed(_grip_click)) {
				value += 1f;
			}
		}

		return Mathf.Clamp(value, 0, 1);
	}
	public bool GetMiddelClickBool(bool left) {
		return GetMiddelClick(left) >= 0.7f;
	}

	public bool GetRightClickBool(bool left) {
		return GetRightClick(left) >= 0.7f;
	}

	public bool GetLeftClickBool(bool left) {
		return GetLeftClick(left) >= 0.7f;
	}


	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouse mouse) {
			_mousePos = mouse.GlobalPosition;
		}
		if (@event is InputEventMouseButton) {
			_mouseHasPressed = true;
		}
		if (@event is InputEventJoypadMotion motion1) {
			if (motion1.Axis == JoyAxis.RightX) {
				_headYaw += motion1.AxisValue * _headYawSpeed * (float)_lastDelta;
			}
			if (motion1.Axis == JoyAxis.RightY) {
				_headPitch = Math.Clamp(_headPitch + (motion1.AxisValue * _headPitchSpeed * (float)_lastDelta), -90, 90);
			}
		}
		if (@event is InputEventMouseMotion motion) {
			if (Godot.Input.MouseMode == Godot.Input.MouseModeEnum.Captured) {
				_headYaw += motion.Relative.X * _headYawSpeed / 100f;
				_headPitch = Math.Clamp(_headPitch + (motion.Relative.Y * _headPitchSpeed / 100f), -90, 90);
			}
		}
		if (@event is InputEventKey keyEvent) {
			if (keyEvent.PhysicalKeycode == Key.R && keyEvent.Pressed && !keyEvent.Echo) {
				ToggleVisableMouse();
			}
		}
	}
}
