/*
 * [ Sonic Onset Adventure]
 * Copyright (c) 2023 Regan "CKDEV" Green
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using Godot;

namespace SonicOnset
{
	public partial class Camera : Camera3D
	{
		// Target node
		[Export]
		public Node3D target_node;

		private float m_x = 0.0f;
		private float m_y = -0.2f;

		private bool m_locked = false;
		private bool m_right_down = false;

		private Util.Spring m_zoom = new Util.Spring(2.0f, 1.0f);

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			// Capture our mouse
			SetLocked(true, false);
		}

		// Input
		private void SetLocked(bool locked, bool right_down)
		{
			m_locked = locked;
			m_right_down = right_down;

			if (m_locked || m_right_down)
				Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
			else
				Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
		}

		public override void _Input(InputEvent motionUnknown)
		{
			InputEventMouseMotion motion = motionUnknown as InputEventMouseMotion;
			if (motion != null)
			{
				if (m_locked || m_right_down)
				{
					m_x += motion.Relative.X * -0.008f;
					m_y += motion.Relative.Y * -0.005f;
				}
			}

			InputEventMouseButton button = motionUnknown as InputEventMouseButton;
			if (button != null)
			{
				if (button.IsPressed())
				{
					if (button.ButtonIndex == MouseButton.WheelUp)
						m_zoom.m_goal -= 0.1f;
					if (button.ButtonIndex == MouseButton.WheelDown)
						m_zoom.m_goal += 0.1f;

					if (button.ButtonIndex == MouseButton.Middle)
						SetLocked(!m_locked, m_right_down);
					if (button.ButtonIndex == MouseButton.Right)
						SetLocked(m_locked, true);
				}
				else
				{
					if (button.ButtonIndex == MouseButton.Right)
						SetLocked(m_locked, false);
				}

				m_zoom.m_goal = Mathf.Clamp(m_zoom.m_goal, 0.4f, 1.2f);
			}
		}

		// Update
		public override void _Process(double delta)
		{
			// Zoom camera
			m_zoom.Step((float)delta);

			// Move camera by rotate vector
			Vector2 rotate_vector = Input.Server.GetLookVector();
			m_x += rotate_vector.X * -4.0f * (float)delta;
			m_y += rotate_vector.Y *  3.0f * (float)delta;

			// Limit camera
			m_x %= Mathf.Pi * 2.0f;
			m_y = Mathf.Clamp(m_y, Mathf.Pi * -0.499f, Mathf.Pi * 0.499f);

			// Move behind the target node
			var temp_transform = target_node.GlobalTransform;
			temp_transform.Basis = new Basis(new Vector3(0.0f, 1.0f, 0.0f), m_x) * new Basis(new Vector3(1.0f, 0.0f, 0.0f), m_y);
			temp_transform.Origin += temp_transform.Basis.Z * 24.0f * m_zoom.m_pos;
			temp_transform.Origin.Y += 6.0f * m_zoom.m_pos;
			Transform = temp_transform;
		}
	}
}
