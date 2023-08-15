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

namespace SonicOnset.Util
{
	public class Spring
	{
		public float m_freq = 0.0f;
		public float m_goal = 0.0f;
		public float m_pos = 0.0f;
		public float m_vel = 0.0f;

		public Spring(float freq, float pos = 0.0f)
		{
			m_freq = freq;
			m_goal = pos;
			m_pos = pos;
			m_vel = 0.0f;
		}

		public float Step(float dt)
		{
			float f = m_freq * 2.0f * Godot.Mathf.Pi;
			float g = m_goal;
			float p0 = m_pos;
			float v0 = m_vel;

			float offset = p0 - g;
			float decay = Godot.Mathf.Exp(-f * dt);

			float p1 = (offset * (1.0f + f * dt) + v0 * dt) * decay + g;
			float v1 = (v0 * (1.0f - f * dt) - offset * (f * f * dt)) * decay;

			m_pos = p1;
			m_vel = v1;

			return p1;
		}
	}

	public class Spring3D
	{
		private Spring m_x;
		private Spring m_y;
		private Spring m_z;

		public float m_freq
		{
			get { return m_x.m_freq; }
			set
			{
				m_x.m_freq = value;
				m_y.m_freq = value;
				m_z.m_freq = value;
			}
		}

		public Godot.Vector3 m_goal
		{
			get { return new Godot.Vector3(m_x.m_goal, m_y.m_goal, m_z.m_goal); }
			set
			{
				m_x.m_goal = value.X;
				m_y.m_goal = value.Y;
				m_z.m_goal = value.Z;
			}
		}

		public Godot.Vector3 m_pos
		{
			get { return new Godot.Vector3(m_x.m_pos, m_y.m_pos, m_z.m_pos); }
			set
			{
				m_x.m_pos = value.X;
				m_y.m_pos = value.Y;
				m_z.m_pos = value.Z;
			}
		}

		public Godot.Vector3 m_vel
		{
			get { return new Godot.Vector3(m_x.m_vel, m_y.m_vel, m_z.m_vel); }
			set
			{
				m_x.m_vel = value.X;
				m_y.m_vel = value.Y;
				m_z.m_vel = value.Z;
			}
		}

		public Spring3D(float freq, Godot.Vector3 pos = new Godot.Vector3())
		{
			m_x = new Spring(freq, pos.X);
			m_y = new Spring(freq, pos.Y);
			m_z = new Spring(freq, pos.Z);
		}
	}
}
