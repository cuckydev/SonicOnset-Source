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
		public partial class Bounce : State
		{
			// Bounce state
			private float m_bounce_speed;
			private bool m_can_bounce;
			private bool m_second_bounce;

			private float m_squash = 0.0f;

			private void StartBounce()
			{
				// Set bounce speed
				float attack_speed = m_second_bounce ? -7.0f : -5.0f;
				m_bounce_speed = m_second_bounce ? 3.49f : 2.79f;

				Vector3 speed = m_parent.ToSpeed(m_parent.Velocity);
				speed.Y = attack_speed;
				m_parent.Velocity = m_parent.FromSpeed(speed);

				// Set bounce state
				m_can_bounce = false;
				m_second_bounce = true;

				// Set animation speed
				m_jump_speed = -attack_speed;
			}

			// Jump speed
			private float m_jump_speed;

			// Bounce state
			public Bounce(Player parent)
			{
				// Set parent
				m_parent = parent;

				// Start bounce
				StartBounce();
			}

			internal override void AbilityProcess()
			{
				// Check for jump ability
				if (m_parent.m_ability.CheckJumpAbility())
					return;

				/*
				// Cancel if roll is pressed
				if (m_parent.m_input_quaternary.m_pressed)
				{
					// Switch to jump state
					m_parent.SetState(new Jump(m_parent));
					return;
				}
				*/

				// Check for bounce ability
				if (m_parent.m_input_spin.m_pressed && m_can_bounce)
				{
					StartBounce();
					return;
				}
			}

			internal override void Process()
			{
				// Movement
				m_parent.RotateToGravity();
				m_parent.AirMovement();

				// Physics
				float y_speed = m_parent.GetSpeedY();
				m_parent.PhysicsMove();
				m_parent.CheckGrip();

				if (m_parent.m_status.m_grounded)
				{
					if (m_can_bounce)
					{
						// Land
						m_parent.SetStateLand(y_speed);
					}
					else
					{
						// Bounce off floor
						m_parent.Velocity += m_parent.GetUp() * m_bounce_speed * Root.c_tick_rate;
						m_squash = m_bounce_speed * 0.1f;

						// Play bounce sound
						m_parent.PlaySound("Bounce");

						// Set bounce state
						m_can_bounce = true;
					}
				}

				// Set animation
				if (m_can_bounce)
					m_jump_speed = 1.0f + Mathf.Max(m_parent.GetSpeedY(), 0.0f) * 1.5f;
				else
					m_jump_speed = 1.0f + m_parent.ToSpeed(m_parent.Velocity).Length() * 1.5f;
				m_parent.PlayAnimation("Roll", 1.5f + m_jump_speed * 0.8f);

				// Reduce squash
				m_squash *= 0.8f;
			}

			internal override void HitWall(Vector3 wall_normal)
			{
				// Check if we're moving primarily into the wall and upwards
				Vector3 speed = m_parent.ToSpeed(m_parent.Velocity);
				if (speed.Y > 0.0f && (m_parent.Velocity.Dot(wall_normal) / -Root.c_tick_rate) > speed.Y)
				{
					// Preserve our velocity
					Vector3 pre_velocity = m_parent.Velocity;
					m_parent.Velocity = Util.Vector3.NormalProject(m_parent.Velocity, wall_normal);
					m_parent.Velocity = m_parent.Velocity.Normalized() * pre_velocity.Length();
				}
				else
				{
					// Regular wall hit
					base.HitWall(wall_normal);
				}
			}

			// State overrides
			internal override bool CanDynamicPose()
			{
				return false;
			}

			internal override Transform3D GetShear()
			{
				float speed_y = m_parent.GetSpeedY();

				float bounce = 1.0f - m_squash;
				if (speed_y > 0.0f || !m_can_bounce)
					bounce += Mathf.Abs(speed_y) * 0.03f;

				float inv_bounce = 1.0f / bounce;

				Vector3 scale = new Vector3(inv_bounce, bounce, inv_bounce);
				Vector3 offset = new Vector3(0.0f, m_parent.m_param.m_center_height * (1.0f - bounce), 0.0f);

				return new Transform3D(Basis.FromScale(scale), offset);
			}
		}
	}
}
