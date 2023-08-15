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
		public partial class Jump : State
		{
			// Jump timer
			private uint m_jump_timer;

			// Jump speed
			private float m_jump_speed;

			// Jump state
			public Jump(Player parent)
			{
				// Set parent
				m_parent = parent;

				// Set jump timer
				m_jump_timer = m_parent.m_param.m_jump_hang;

				// Get jump speed
				m_jump_speed = m_parent.Velocity.Length() / Root.c_tick_rate;
			}

			internal override void AbilityProcess()
			{
				// Check for jump ability
				if (m_parent.m_ability.CheckJumpAbility())
					return;
			}

			internal override void Process()
			{
				// Lift up when jump is held
				if (m_parent.m_input_jump.m_down && m_jump_timer != 0)
				{
					m_parent.Velocity += m_parent.FromSpeed(Vector3.Up * m_parent.m_param.m_jump_addit * 0.8f);
					m_jump_timer--;
				}

				// Movement
				m_parent.RotateToGravity();
				m_parent.AirMovement();

				// Physics
				float y_speed = m_parent.GetSpeedY();
				m_parent.PhysicsMove();
				m_parent.CheckGrip();

				if (m_parent.m_status.m_grounded)
					m_parent.SetStateLand(y_speed);

				// Set animation
				m_parent.PlayAnimation("Roll", 1.25f + m_jump_speed * 0.85f);
			}

			// State overrides
			internal override bool CanDynamicPose()
			{
				return false;
			}
		}
	}
}
