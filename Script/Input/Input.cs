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

namespace SonicOnset.Input
{
	// Input structures
	public class Button
	{
		// Button state
		public bool m_down = false;
		public bool m_pressed = false;
		public bool m_released = false;

		// Button internal state
		private bool m_last = false;

		// Button functions
		public void Update(bool down)
		{
			m_pressed = down && !m_last;
			m_released = !down && m_last;
			m_down = down;
			m_last = down;
		}
	}
		
	public class Stick
	{
		// Stick state
		public float m_x = 0.0f;
		public float m_y = 0.0f;
		public float m_angle = 0.0f;
		public float m_turn = 0.0f;
		public float m_length = 0.0f;

		// Stick internal state
		private Vector3 last_up = Vector3.Up;

		// Stick functions
		public void Update(Vector2 input_vector, Transform3D character_transform, Transform3D camera_transform, Vector3 target_up)
		{
			// Update derived from input vector
			m_x = input_vector.X;
			m_y = input_vector.Y;
			m_angle = Mathf.Atan2(-m_x, m_y);
			m_length = input_vector.Length();
			m_turn = 0.0f;

			// Cap stick magnitude
			if (m_length > 1.0f)
			{
				m_x /= m_length;
				m_y /= m_length;
				m_length = 1.0f;
			}

			if (m_length > 0.0f)
			{
				// Get character vectors
				Vector3 character_look = -character_transform.Basis.Z;
				Vector3 character_up = character_transform.Basis.Y;

				// Project camera look onto our target up vector
				Vector3 camera_look = -camera_transform.Basis.Z;
				camera_look = Util.Vector3.PlaneProject(camera_look, target_up).Normalized();

				// Get move vector in world space, aligned to our target up vector
				Vector3 camera_move = (new Basis(target_up, m_angle)) * camera_look;

				// Update last up
				if (last_up.Dot(last_up) > -1.0f + Mathf.Epsilon)
					last_up = character_up;

				// Get final rotation and move vector
				Basis final_rotation = Util.Basis.FromTo(target_up, last_up);

				Vector3 final_move = Util.Vector3.PlaneProject((final_rotation * camera_move), character_up);
				if (final_move == Vector3.Zero)
					final_move = character_look;

				// Get turn amount
				m_turn = character_look.SignedAngleTo(final_move, character_up);
			}
		}
	}
}
