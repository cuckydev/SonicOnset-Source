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
		public partial class Roll : State
		{
			// Roll state
			public Roll(Player parent)
			{
				// Set parent
				m_parent = parent;

				// Play spindash release sound
				m_parent.PlaySound("SpindashRelease");
			}

			internal override void AbilityProcess()
			{
				// Check if jumped
				if (m_parent.m_ability.CheckJump())
					return;

				// Check if uncurl
				if (m_parent.m_input_spin.m_pressed || m_parent.m_input_quaternary.m_pressed)
				{
					m_parent.SetState(new Run(m_parent));
					return;
				}

				// Check if we've stopped rolling
				if (m_parent.GetAbsSpeedX() < m_parent.m_param.m_jog_speed)
				{
					m_parent.SetState(new Idle(m_parent));
					return;
				}
			}

			internal override void Process()
			{
				// Movement
				m_parent.RollMovement();

				// Physics
				m_parent.PhysicsMove();
				m_parent.CheckGrip();

				if (!m_parent.m_status.m_grounded)
					m_parent.SetState(new Jump(m_parent));

				// Set animation
				m_parent.PlayAnimation("RollFloor", m_parent.GetSpeedX());
			}

			// State overrides
			internal override bool CanDynamicPose()
			{
				return false;
			}
		}
	}
}
