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
using System.Collections.Generic;

namespace SonicOnset
{
	public partial class Player
	{
		public partial class Spindash : State
		{
			// Spindash charged speed
			private float m_speed;
			private float m_init_speed;

			// Spindash state
			public Spindash(Player parent) 
			{
				// Set parent
				m_parent = parent;

				// Get spin speed
				m_init_speed = m_parent.GetSpeedX();
				m_speed = Mathf.Max(m_init_speed, 2.0f);

				// Play spindash charge sound
				m_parent.PlaySound("SpindashCharge");
			}

			internal override void Stop()
			{
				// Stop spindash charge sound
				m_parent.StopSound("SpindashCharge");
			}

			internal override void AbilityProcess()
			{
				// Check jump button
				if (m_parent.m_input_jump.m_pressed)
				{
					// Jump off floor
					Vector3 speed = m_parent.ToSpeed(m_parent.Velocity);
					if (m_parent.m_status.m_grounded)
						speed.X = m_init_speed * 1.25f;
					else
						speed.X *= 1.5f;
					speed.Y = m_parent.m_param.m_jump_speed * 0.8f;
					m_parent.Velocity = m_parent.FromSpeed(speed);

					// Play jump sound
					m_parent.PlaySound("Jump");

					// Switch to jump state
					m_parent.SetState(new Trick(m_parent, "SpindashCancelTrick"));
					m_parent.m_status.m_grounded = false;
					return;
				}

				// Check if we've released spin
				if (!m_parent.m_input_spin.m_down)
				{
					// Release speed
					Vector3 speed = m_parent.ToSpeed(m_parent.Velocity);
					if (m_parent.m_status.m_grounded)
						speed.X = m_speed;
					else
						speed.X = (speed.X + m_speed) * 0.5f;
					m_parent.Velocity = m_parent.FromSpeed(speed);

					// Switch to roll state
					m_parent.SetState(new Roll(m_parent));
					return;
				}
			}

			internal override void Process()
			{
				// Charge speed
				if (m_speed < 10.0f)
					m_speed += 0.4f;
				else
					m_init_speed *= 0.95f;

				// Movement
				m_parent.ControlTurnY(m_parent.m_input_stick.m_turn);
				if (m_parent.m_status.m_grounded)
				{
					// Brake to a stop
					m_parent.BrakeMovement();
				}
				else
				{
					// Limited air movement
					m_parent.AlwaysRotateToGravity();
					m_parent.RollMovement();
				}

				// Physics
				m_parent.PhysicsMove();
				m_parent.CheckGrip();

				// Set animation
				m_parent.PlayAnimation("RollFloor", m_speed);
			}

			internal override void Debug(List<string> debugs)
			{
				debugs.Add("Speed: " + m_speed);
				debugs.Add("Init Speed: " + m_init_speed);
			}

			// State overrides
			internal override bool CanDynamicPose()
			{
				return false;
			}

			internal override Transform3D GetShear()
			{
				// Get shear
				Basis shear = new Basis(
					1.0f, 0.0f, 0.0f,
					0.0f, 1.0f, 0.0f,
					0.0f, 0.5f, 1.0f
				);

				return new Transform3D(shear, new Vector3(0.0f, 0.0f, -0.7f));
			}
		}
	}
}
