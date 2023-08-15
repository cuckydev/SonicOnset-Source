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
		public partial class Brake : State
		{
			// Brake state
			public Brake(Player parent) 
			{
				// Set parent
				m_parent = parent;

				// Play brake sound
				m_parent.PlaySound("Brake");
			}

			internal override void AbilityProcess()
			{
				// Check if jumped
				if (m_parent.m_ability.CheckJump())
					return;

				// Check abilities
				if (m_parent.m_ability.CheckSpinAbility())
					return;

				// Check if we're holding forward
				if (m_parent.m_input_stick.m_length > 0.0f && Mathf.Abs(m_parent.m_input_stick.m_turn) <= Mathf.DegToRad(135.0f))
				{
					m_parent.SetState(new Run(m_parent));
					return;
				}

				// Check if we've stopped moving
				if (m_parent.GetAbsSpeedX() < m_parent.m_param.m_run_accel)
				{
					m_parent.SetState(new Idle(m_parent));
					return;
				}
			}

			internal override void Process()
			{
				// Movement
				m_parent.BrakeMovement();

				// Physics
				m_parent.PhysicsMove();
				m_parent.CheckGrip();

				if (!m_parent.m_status.m_grounded)
					m_parent.SetState(new Fall(m_parent));

				// Set animation
				m_parent.PlayAnimation("BrakeStop");
			}

			// State overrides
			internal override float GetTilt() { return 0.0f; }
		}
	}
}
