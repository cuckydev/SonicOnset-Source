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
		public partial class Spring : State
		{
			// Spring state
			int m_nocon = 0;

			// Fall state
			internal Spring(Player parent, int nocon)
			{
				// Set parent
				m_parent = parent;

				// Set spring state
				m_nocon = nocon;

				// Set animation
				m_parent.ClearAnimation();
				m_parent.PlayAnimation("Spring");
			}

			internal override void Process()
			{
				// Movement
				if (m_nocon >= 0)
				{
					// Fall to gravity
					m_parent.Velocity += m_parent.m_gravity * m_parent.m_param.m_gravity * Root.c_tick_rate;
				}

				// Physics
				float y_speed = m_parent.GetSpeedY();
				m_parent.PhysicsMove();
				m_parent.CheckGrip();

				if (m_parent.m_status.m_grounded)
					m_parent.SetStateLand(y_speed);

				// Check if nocon expired
				if (m_nocon < 0)
					m_nocon++;
				else
					m_nocon--;

				if (m_nocon == 0)
				{
					m_parent.SetState(new Fall(m_parent));
					return;
				}
			}

			// State overrides
			internal override bool CanDynamicPose()
			{
				return false;
			}
		}
	}
}
