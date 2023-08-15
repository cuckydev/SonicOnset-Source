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
		public partial class Idle : State
		{
			// Idle landed
			bool m_landed;

			// Idle state
			public Idle(Player parent, bool landed = false)
			{
				// Set parent
				m_parent = parent;

				// Set landed
				m_landed = landed;
			}

			internal override void AbilityProcess()
			{
				// Check if jumped
				if (m_parent.m_ability.CheckJump())
					return;

				// Check abilities
				if (m_parent.m_ability.CheckSpinAbility())
					return;

				// Check if we've started running
				if (m_parent.m_input_stick.m_length != 0)
				{
					m_parent.SetState(new Run(m_parent));
					return;
				}
			}

			internal override void Process()
			{
				// Movement
				m_parent.Movement();

				// Physics
				m_parent.PhysicsMove();
				m_parent.CheckGrip();

				if (!m_parent.m_status.m_grounded)
					m_parent.SetState(new Fall(m_parent));

				// Set animation
				float speed_x = m_parent.GetSpeedX();
				if (speed_x > m_parent.m_param.m_jog_speed)
				{
					// Sliding forward
					m_parent.PlayAnimation("SlideForward");
					m_landed = false;
				}
				else if (speed_x < -m_parent.m_param.m_jog_speed)
				{
					// Sliding backward
					m_parent.PlayAnimation("SlideBack");
					m_landed = false;
				}
				else if (m_landed)
				{
					// Landed from a jump
					m_parent.PlayAnimation("Land");
				}
				else
				{
					// Idle
					m_parent.PlayAnimation("Idle");
				}
			}
		}
	}
}
