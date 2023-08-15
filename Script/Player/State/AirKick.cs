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
		public partial class AirKick : State
		{
			// Air kick state
			bool m_humming_top;

			int m_kick_timer;
			int m_impact_timer;

			// Air kick state
			internal AirKick(Player parent, bool humming_top)
			{
				// Set parent
				m_parent = parent;

				// Align to gravity
				m_parent.AlignToGravity();

				// Set humming top mode
				m_humming_top = humming_top;

				if (m_humming_top)
				{
					// Launch us forward
					Vector3 speed = m_parent.ToSpeed(m_parent.Velocity);
					m_parent.Velocity = m_parent.FromSpeed(new Vector3(speed.X * 0.75f + 6.5f, 0.3f, 0.0f));

					// Set kick timer
					m_kick_timer = 130;
					m_impact_timer = 8;
				}
				else
				{
					// Launch up upwards
					Vector3 speed = m_parent.ToSpeed(m_parent.Velocity);
					m_parent.Velocity = m_parent.FromSpeed(new Vector3(speed.X * 0.9f + 1.0f, speed.Y * 0.25f + 3.85f, 0.0f));

					// Set kick timer
					m_kick_timer = 60;
					m_impact_timer = 8;
				}

				// Play sound
				m_parent.PlaySound("AirKick");
			}

			internal override void Process()
			{
				// Movement
				Vector3 speed = m_parent.ToSpeed(m_parent.Velocity);
				Vector3 acc = m_parent.ToSpeed(m_parent.m_gravity * Root.c_tick_rate) * m_parent.m_param.m_gravity * 0.525f;

				acc.X += Mathf.Cos(m_parent.m_input_stick.m_turn) * m_parent.m_param.m_air_accel * 0.6f;
				acc += speed * (m_parent.m_param.m_air_drag * new Vector3(0.285f, 1.0f, 1.0f));

				if (Mathf.Abs(m_parent.m_input_stick.m_turn) <= Mathf.DegToRad(145.0f))
					m_parent.ControlTurnYS(m_parent.m_input_stick.m_turn);
				else
					acc.X -= m_parent.m_param.m_air_accel * 0.6f;

				m_parent.Velocity += m_parent.FromSpeed(acc);

				// Physics
				float y_speed = m_parent.GetSpeedY();

				if (m_impact_timer > 0)
				{
					// Move with impact
					float div = 1.0f + m_impact_timer;
					m_impact_timer--;

					m_parent.Velocity /= div;

					m_parent.PhysicsMove();
					m_parent.CheckGrip();

					m_parent.Velocity *= div;

					// Play celebrate sound after impact timer expires
					if (m_impact_timer == 0)
						m_parent.PlaySound("VoiceCelebrate");
				}
				else
				{
					// Move without impact
					m_parent.PhysicsMove();
					m_parent.CheckGrip();
				}

				if (m_parent.m_status.m_grounded)
					m_parent.SetStateLand(y_speed);

				// Set animation
				if (m_humming_top)
					m_parent.PlayAnimation("HummingTop");
				else
					m_parent.PlayAnimation("HopJump");

				// Check if timer expired
				m_kick_timer--;
				if (m_kick_timer <= 0)
					m_parent.SetState(new Fall(m_parent));
			}

			// State overrides
			internal override bool CanDynamicPose()
			{
				if (m_humming_top)
					return false;
				else
					return true;
			}

			internal override float GetTilt()
			{
				if (m_humming_top)
					return 0.0f;
				else
					return Mathf.Clamp(m_parent.m_input_stick.m_turn, Mathf.Pi * -0.5f, Mathf.Pi * 0.5f);
			}

			internal override void HitWall(Vector3 wall_normal)
			{
				// Check how much of our velocity is going into the wall
				float hit_dots = m_parent.Velocity.Normalized().Dot(wall_normal);
				if (hit_dots < (m_humming_top ? -0.1f : -0.707f))
					m_parent.SetState(new Fall(m_parent));

				// Hit wall
				base.HitWall(wall_normal);
			}
		}
	}
}
