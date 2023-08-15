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
		// Control functions
		internal void ControlTurnY(float turn)
		{
			// Get max turn
			float max_turn = Mathf.Abs(turn);

			if (max_turn <= Mathf.DegToRad(45.0f))
			{
				if (max_turn <= Mathf.DegToRad(22.5f))
					max_turn /= 8.0f;
				else
					max_turn /= 4.0f;
			}
			else
			{
				max_turn = Mathf.DegToRad(11.25f);
			}

			// Get intertia
			float inertia;
			if (!m_status.m_grounded)
				inertia = 0.9f;
			else
				inertia = (GetDotp() <= 0.4f) ? 0.5f : 0.01f;

			// Apply turn
			OriginRotate(GetUp(), Mathf.Clamp(turn, -max_turn, max_turn), inertia);
		}

		internal void ControlTurnYQ(float turn)
		{
			// Get max turn
			float max_turn = Mathf.DegToRad(45.0f);

			// Get inertia
			float inertia = 1.0f;

			// Apply turn
			OriginRotate(GetUp(), Mathf.Clamp(turn, -max_turn, max_turn), inertia);
		}

		internal void ControlTurnYS(float turn)
		{
			// Get max turn
			Vector3 speed = ToSpeed(Velocity);
			float max_turn = Mathf.DegToRad(1.40625f);
			if (speed.X > m_param.m_dash_speed)
				max_turn = Mathf.Max(max_turn - (Mathf.Sqrt((speed.X - m_param.m_dash_speed) * 0.0625f) * max_turn), 0.0f);

			// Get inertia
			float inertia = (GetDotp() <= 0.4f) ? 0.5f : 0.01f;

			// Apply turn
			OriginRotate(GetUp(), Mathf.Clamp(turn, -max_turn, max_turn), inertia);
		}

		// Physics functions
		internal void AlwaysRotateToGravity()
		{
			// Get vectors
			Vector3 from = GetUp();
			Vector3 to = -m_gravity;

			// Get angle left to rotate back
			float angle_left = from.AngleTo(to);
			if (angle_left < Mathf.Epsilon)
				return;

			// Get max angle and ratio of angle left
			float angle_max = Mathf.Min(Mathf.DegToRad(11.25f), angle_left);
			float angle_ratio = angle_max / angle_left;

			// Apply rotation
			CenterBasis(Util.Basis.FromTo(from, to, angle_ratio), 1.0f);
		}

		internal void RotateToGravity()
		{
			// Check if we're below the speed threshold
			Vector3 speed = ToSpeed(Velocity);
			if (speed.Length() > m_param.m_dash_speed)
				return;
			
			// Rotate to gravity
			AlwaysRotateToGravity();
		}

		internal void AlignToGravity()
		{
			// Get vectors
			Vector3 from = GetUp();
			Vector3 to = -m_gravity;

			// Apply rotation
			CenterBasis(Util.Basis.FromTo(from, to), 1.0f);
		}

		internal void Movement()
		{
			// Get current speed and dot product
			Vector3 speed = ToSpeed(Velocity);
			float dotp = GetDotp();

			// Get gravity force as initial acceleration
			Vector3 acc = ToSpeed(m_gravity * Root.c_tick_rate) * m_param.m_gravity;
			acc.Z *= m_input_stick.m_length;

			// Get acceleration drag start according to stick length
			float stick_accel = m_param.m_run_accel * m_input_stick.m_length;
			float stick_drag_start = m_param.m_run_dragstart * m_input_stick.m_length;

			// Apply X air drag
			// Try to drag us towards m_run_dragstart as long as we're past it
			if (m_input_stick.m_length > 0)
			{
				if (speed.X <= stick_drag_start || dotp <= 0.96f)
				{
					if (speed.X >= stick_drag_start)
						acc.X += (speed.X - stick_drag_start) * m_param.m_run_drag.X;
					else if (speed.X < 0.0f)
						acc.X += speed.X * m_param.m_run_drag.X;
				}
				else
				{
					acc.X += (speed.X - stick_drag_start) * (m_param.m_run_drag.X * 1.7f);
				}
			}
			else
			{
				if (speed.X > m_param.m_run_speed)
					acc.X += speed.X * m_param.m_run_drag.X;
			}

			// Apply Y and Z air drag
			acc.Y += speed.Y * m_param.m_air_drag.Y;
			acc.Z += speed.Z * m_param.m_air_drag.Z;

			// Movement
			if (m_input_stick.m_length > 0)
			{
				// Get acceleration
				float move_acc;
				if (speed.X >= stick_drag_start * m_input_stick.m_length)
				{
					// Use lower acceleration above max speed
					move_acc = stick_accel * 0.4f * m_input_stick.m_length;
				}
				else
				{
					// Accelerate up to max speed
					move_acc = stick_accel;
				}

				// Turning
				float abs_turn = Mathf.Abs(m_input_stick.m_turn);

				if (Mathf.Abs(speed.X) < m_param.m_run_accel && abs_turn > Mathf.DegToRad(22.5f))
				{
					// Turning on the spot
					move_acc = 0.0f;
					ControlTurnYQ(m_input_stick.m_turn);
				}
				else
				{
					if (speed.X < (m_param.m_jog_speed + m_param.m_run_speed) * 0.5f || abs_turn <= Mathf.DegToRad(22.5f))
					{
						if (speed.X < m_param.m_jog_speed || abs_turn >= Mathf.DegToRad(22.5f))
						{
							if (speed.X < m_param.m_dash_speed || !m_status.m_grounded)
							{
								if (speed.X >= m_param.m_jog_speed && speed.X <= m_param.m_rush_speed && abs_turn >= Mathf.DegToRad(45.0f))
									move_acc *= 0.8f; // Tight turn, decelerate
								ControlTurnY(m_input_stick.m_turn);
							}
							else
							{
								// Slow turn
								ControlTurnYS(m_input_stick.m_turn);
							}
						}
						else
						{
							// Slow turn
							ControlTurnYS(m_input_stick.m_turn);
						}
					}
					else
					{
						// Tight turn, decelerate
						move_acc = m_param.m_run_decel;
						ControlTurnY(m_input_stick.m_turn);
					}
				}

				// Apply movement acceleration
				acc.X += move_acc;
			}
			else
			{
				// Decelerate
				acc.X += -Mathf.Clamp(speed.X + acc.X, m_param.m_run_decel, -m_param.m_run_decel);
			}

			// Apply acceleration
			Velocity += FromSpeed(acc);
		}

		internal void AirMovement()
		{
			// Get current speed
			Vector3 speed = ToSpeed(Velocity);

			// Get gravity force as initial acceleration
			Vector3 acc = ToSpeed(m_gravity * Root.c_tick_rate) * m_param.m_gravity;

			// Apply air drag
			acc += speed * m_param.m_air_drag;

			// Get acceleration
			if (m_input_stick.m_length > 0.0f)
			{
				if (speed.X <= m_param.m_run_speed || Mathf.Abs(m_input_stick.m_turn) <= Mathf.DegToRad(135))
				{
					// Accelerate faster when moving down
					float accel;
					if (speed.Y >= 0.0f)
						accel = m_param.m_run_accel * m_input_stick.m_length;
					else
						accel = m_param.m_run_accel * m_input_stick.m_length * 2.0f;

					// Accelerate in a way that's less distracting when turning
					acc.X += Mathf.Max(Mathf.Cos(m_input_stick.m_turn) * accel, 0.0f);
					// acc.Z -= Mathf.Sin(m_input_stick.m_turn) * accel;

					// Turn
					ControlTurnY(m_input_stick.m_turn);
				}
				else
				{
					// Air brake
					acc.X += m_param.m_air_brake * m_input_stick.m_length;
				}
			}

			// Apply acceleration
			Velocity += FromSpeed(acc);
		}

		internal void BrakeMovement()
		{
			// Get current speed
			Vector3 speed = ToSpeed(Velocity);

			// Get gravity force as initial acceleration
			Vector3 acc = ToSpeed(m_gravity * Root.c_tick_rate) * m_param.m_gravity;

			// Apply air drag
			acc += speed * m_param.m_run_drag;

			// Decelerate
			acc.X += -Mathf.Clamp(speed.X + acc.X, m_param.m_run_brake, -m_param.m_run_brake);

			// Apply acceleration
			Velocity += FromSpeed(acc);
		}

		internal void RollMovement()
		{
			// Get current speed
			Vector3 speed = ToSpeed(Velocity);

			// Get gravity force as initial acceleration
			Vector3 acc = ToSpeed(m_gravity * Root.c_tick_rate) * m_param.m_gravity;

			// Apply X air drag
			if (GetDotp() < 0.98f)
				acc.X += speed.X * -0.0002f;
			else
				acc.X += speed.X * m_param.m_roll_drag.X;

			// Apply Y and Z air drag
			acc.Y += speed.Y * m_param.m_roll_drag.Y;
			acc.Z += speed.Z * m_param.m_roll_drag.Z;

			// Turn
			ControlTurnY(m_input_stick.m_turn);

			// Apply acceleration
			Velocity += FromSpeed(acc);
		}

		// Collision physics
		internal bool CheckTransform(Transform3D from, Transform3D to)
		{
			// Perform a raycast between our center points
			m_ray_query.From = from.Origin + from.Basis.Y * m_param.m_center_height;
			m_ray_query.To = to.Origin + to.Basis.Y * m_param.m_center_height;
			return m_directspace.IntersectRay(m_ray_query).Count > 0;
		}

		internal void CheckGrip()
		{
			// Check if we're below the speed threshold on a steep floor
			if (m_status.m_grounded)
			{
				Vector3 speed = ToSpeed(Velocity);
				if (GetDotp() < 0.4f && speed.Length() < 1.16f)
					m_status.m_grounded = false;
			}
		}

		internal void AlignFloor(Vector3 floor_position, Vector3 floor_normal)
		{
			// Stop velocity going into the floor
			Velocity = Util.Vector3.PlaneProject(Velocity, floor_normal);

			// Check if aligning will cause a collision
			Transform3D to_transform = new Transform3D(Util.Basis.FromTo(GetUp(), floor_normal) * GlobalTransform.Basis, floor_position);
			if (!CheckTransform(GlobalTransform, to_transform))
				GlobalTransform = to_transform;
		}

		internal void PhysicsMove()
		{
			// Set active collision shape
			// TODO: This is a bit of a hack, but it works for now
			// I'd like this to be dynamic
			if (m_state is Roll)
			{
				m_main_colshape_node.Disabled = true;
				m_roll_colshape_node.Disabled = false;
			}
			else
			{
				m_main_colshape_node.Disabled = false;
				m_roll_colshape_node.Disabled = true;
			}

			// Move and collide
			Vector3 delta = Velocity / Root.c_tick_rate;

			for (int i = 0; i < MaxSlides; i++)
			{
				// Move and collide
				KinematicCollision3D collision = MoveAndCollide(delta, true, SafeMargin, true, 6);

				if (collision != null)
				{
					// Respond to collisions
					GlobalTranslate(collision.GetTravel());
					delta = collision.GetRemainder();

					int collisions = collision.GetCollisionCount();
					for (int j = 0; j < collisions; j++)
					{
						// Get collision hit
						Vector3 hit_normal = collision.GetNormal(j);

						// Check if sliding on the wall would work against falling or our grip on the floor
						// Project wall normal onto our up vector
						// This will keep us from slowly sliding or flinging off a slanted wall
						Vector3 wall_normal = hit_normal;
						float dot_test = Util.Vector3.PlaneProject(Velocity, hit_normal).Dot(GetUp());
						if (m_status.m_grounded || (hit_normal.Dot(GetUp()) >= Mathf.Epsilon && dot_test <= -Mathf.Epsilon))
							wall_normal = Util.Vector3.PlaneProject(hit_normal, GetUp()).Normalized();

						if (m_status.m_grounded)
						{
							// Check if we hit a wall rather than the floor
							if (hit_normal.Dot(GetUp()) < Mathf.Cos(FloorMaxAngle))
							{
								// Clip our velocity and clip our remainder to the projected wall normal
								m_state.HitWall(wall_normal);
								delta = Util.Vector3.NormalProject(delta, wall_normal);
							}
							else
							{
								// Clip our remainder to the hit normal
								delta = Util.Vector3.NormalProject(delta, hit_normal);
							}
						}
						else
						{
							// Try to move us off of surfaces when airborne
							Vector3 adjust = Util.Vector3.PlaneProject(hit_normal, GetUp()).Normalized() * (collision.GetDepth() + 0.01f);
							GlobalTranslate(adjust);

							// Clip our velocity and clip our remainder to the projected wall normal
							m_state.HitWall(wall_normal);
							if (hit_normal.Dot(GetUp()) < -Mathf.Epsilon)
								delta = Util.Vector3.NormalProject(delta, wall_normal);
							else
								delta = Util.Vector3.NormalProject(delta, hit_normal);
						}
					}
				}
				else
				{
					// Early out if we didn't collide with anything
					break;
				}
			}

			// Move the rest of the way
			// Hopefully any relevant collisions will have been resolved by now
			// But we do this here to avoid the character getting stuck when MaxSlides apparently isn't enough
			// As well, check if we're going to collide with anything before moving
			// Since the slide loop got stuck, delta can still allow us to pass through walls
			if (!CheckTransform(GlobalTransform, GlobalTransform.Translated(delta)))
				GlobalTranslate(delta);

			// Set floor raycast
			m_ray_query.From = GlobalPosition + GetUp() * m_param.m_center_height;
			m_ray_query.To = GlobalPosition - GetUp() * (m_status.m_grounded ? m_param.m_floor_clip : 0.0f);

			Godot.Collections.Dictionary ray_intersect = m_directspace.IntersectRay(m_ray_query);
			if (ray_intersect.Count > 0)
			{
				// Hit floor
				Vector3 hit_position = ray_intersect["position"].AsVector3();
				Vector3 hit_normal = ray_intersect["normal"].AsVector3();

				if (hit_normal.Dot(GetUp()) < Mathf.Cos(FloorMaxAngle))
				{
					// Floor is too steep to stand on
					m_status.m_grounded = false;
				}
				else
				{
					if (m_status.m_grounded)
					{
						// Run along floor
						Vector3 speed = ToSpeed(Velocity);
						AlignFloor(hit_position, hit_normal);
						Velocity = Util.Vector3.PlaneProject(FromSpeed(speed), hit_normal);
					}
					// This was an epsilon, but something with collision was preventing us from being detected as moving towards the floor for a few frames
					// This would cause us to get stuck in an airborne state gliding along the floor sometimes
					// 0.1f is an arbitrary number but I'd like to find a better solution
					else if (Velocity.Dot(hit_normal) <= (0.1f * Root.c_tick_rate))
					{
						// Attach to floor
						m_status.m_grounded = true;

						GlobalPosition = hit_position;
						AlignFloor(hit_position, hit_normal);
					}
				}
			}
			else
			{
				// No floor
				m_status.m_grounded = false;
			}
		}
	}
}
