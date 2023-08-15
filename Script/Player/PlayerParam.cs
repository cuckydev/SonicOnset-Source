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
	public partial class Player
	{
		public struct Param
		{
			public uint m_jump_hang = 60;
			public float m_jump_speed = 1.66f;
			public float m_jump_addit = 0.076f;

			public float m_gravity = 0.08f;

			public float m_air_accel = 0.031f;
			public float m_air_decel = -0.06f;
			public float m_air_brake = -0.17f;

			public float m_run_dragstart = 3.0f;
			public float m_jog_speed = 0.46f;
			public float m_run_speed = 1.39f;
			public float m_rush_speed = 2.3f;
			public float m_crash_speed = 3.7f;
			public float m_dash_speed = 5.09f;

			public float m_run_accel = 0.05f;
			public float m_run_decel = -0.06f;
			public float m_run_brake = -0.18f;
			
			public Vector3 m_air_drag = new Vector3(-0.028f, -0.01f, -0.4f);
			public Vector3 m_run_drag = new Vector3(-0.008f, -0.01f, -0.4f);
			public Vector3 m_roll_drag = new Vector3(-0.008f, -0.01f, -0.4f);
			
			public float m_eye_height = 7.0f;
			public float m_center_height = 5.4f;

			public float m_floor_clip = 2.0f;

			public Param() { }
		}
	}

	// Node wrapper so the parameters can be exported to the scene
	public partial class PlayerParam : Node
	{
		public Player.Param m_param;

		[Export]
		public uint m_jump_hang { get { return m_param.m_jump_hang; } set { m_param.m_jump_hang = value; } }
		[Export]
		public float m_jump_speed { get { return m_param.m_jump_speed; } set { m_param.m_jump_speed = value; } }
		[Export]
		public float m_jump_addit { get { return m_param.m_jump_addit; } set { m_param.m_jump_addit = value; } }
		[Export]
		public float m_gravity { get { return m_param.m_gravity; } set { m_param.m_gravity = value; } }
		[Export]
		public float m_air_accel { get { return m_param.m_air_accel; } set { m_param.m_air_accel = value; } }
		[Export]
		public float m_air_decel { get { return m_param.m_air_decel; } set { m_param.m_air_decel = value; } }
		[Export]
		public float m_air_brake { get { return m_param.m_air_brake; } set { m_param.m_air_brake = value; } }
		[Export]
		public float m_run_dragstart { get { return m_param.m_run_dragstart; } set { m_param.m_run_dragstart = value; } }
		[Export]
		public float m_jog_speed { get { return m_param.m_jog_speed; } set { m_param.m_jog_speed = value; } }
		[Export]
		public float m_run_speed { get { return m_param.m_run_speed; } set { m_param.m_run_speed = value; } }
		[Export]
		public float m_rush_speed { get { return m_param.m_rush_speed; } set { m_param.m_rush_speed = value; } }
		[Export]
		public float m_crash_speed { get { return m_param.m_crash_speed; } set { m_param.m_crash_speed = value; } }
		[Export]
		public float m_dash_speed { get { return m_param.m_dash_speed; } set { m_param.m_dash_speed = value; } }
		[Export]
		public float m_run_accel { get { return m_param.m_run_accel; } set { m_param.m_run_accel = value; } }
		[Export]
		public float m_run_decel { get { return m_param.m_run_decel; } set { m_param.m_run_decel = value; } }
		[Export]
		public float m_run_brake { get { return m_param.m_run_brake; } set { m_param.m_run_brake = value; } }
		[Export]
		public Vector3 m_air_drag { get { return m_param.m_air_drag; } set { m_param.m_air_drag = value; } }
		[Export]
		public Vector3 m_run_drag { get { return m_param.m_run_drag; } set { m_param.m_run_drag = value; } }
		[Export]
		public Vector3 m_roll_drag { get { return m_param.m_roll_drag; } set { m_param.m_roll_drag = value; } }
		[Export]
		public float m_eye_height { get { return m_param.m_eye_height; } set { m_param.m_eye_height = value; } }
		[Export]
		public float m_center_height { get { return m_param.m_center_height; } set { m_param.m_center_height = value; } }
		[Export]
		public float m_floor_clip { get { return m_param.m_floor_clip; } set { m_param.m_floor_clip = value; } }
	}
}
