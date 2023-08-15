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
		public partial class Homing : State
		{
			// Homing attack speed
			internal const float c_speed = 5.0f;

			// Homing timer
			private uint m_homing_timer = 0;
			private uint m_homing_abort = 0;

			private Node3D m_target_node = null;

			// Get homing target
			private Node3D GetHomingTarget()
			{
				// Query in radial area
				Godot.Collections.Array<Node3D> radial_nodes = m_parent.m_radial_trigger.QueryIntersections();

				// Check for nodes
				Node3D target_node = null;
				float target_distance = float.PositiveInfinity;

				foreach (Node3D node in radial_nodes)
				{
					// Get homing attackable node
					IObject node_object = node as IObject;
					if (node_object == null || !node_object.CanHomingAttack())
						continue;

					// Get offset from our global transform
					Vector3 offset = node.GlobalTransform.Origin - m_parent.GlobalTransform.Origin;

					// Check if too far above
					float y_offset = offset.Dot(m_parent.GetUp());
					if (y_offset > 20.0f)
						continue;

					// Get distance and dot value
					float distance = offset.Length();
					float dot = offset.Normalized().Dot(m_parent.GetLook());
					if (dot < 0.3825f)
						continue;

					float dot_value = distance / dot;
					if (dot_value < target_distance)
					{
						target_node = node;
						target_distance = dot_value;
					}
				}

				return target_node;
			}

			// Homing state
			public Homing(Player parent)
			{
				// Set parent
				m_parent = parent;

				// Set homing timer
				m_homing_timer = 0;

				// Play homing attack sound and grunt
				m_parent.PlaySound("HomingAttack");
				m_parent.PlaySound("VoiceGrunt");
			}

			internal override void AbilityProcess()
			{
				// Check for jump ability
				if (m_parent.m_ability.CheckJumpAbility())
					return;

				// Check for homing target
				if (m_target_node == null)
				{
					// Try to get homing target
					m_target_node = GetHomingTarget();

					if (m_target_node == null)
					{
						// If we can't find a target, fall
						if (++m_homing_timer >= 5)
						{
							m_parent.SetState(new Fall(m_parent));
							return;
						}
					}
				}
			}

			internal override void Process()
			{
				if (m_target_node == null)
				{
					// Movement
					m_parent.Movement();

					// Physics
					float y_speed = m_parent.GetSpeedY();
					m_parent.PhysicsMove();
					m_parent.CheckGrip();

					if (m_parent.m_status.m_grounded)
						m_parent.SetStateLand(y_speed);
				}
				else
				{
					// Rotate to target
					Transform3D center_transform = m_parent.GetCenterTransform();
					Transform3D transform = center_transform.LookingAt(m_target_node.GlobalTransform.Origin, -m_parent.m_gravity);
					m_parent.CenterBasis(transform.Basis * m_parent.GlobalTransform.Basis.Inverse(), 1);

					// Launch towards target
					float distance = (m_target_node.GlobalTransform.Origin - m_parent.GlobalTransform.Origin).Length();
					float speed = Mathf.Min(c_speed, distance);

					Vector3 velocity = m_parent.FromSpeed(new Vector3(speed, 0.0f, 0.0f));
					m_parent.Velocity = velocity;

					// Physics
					m_parent.PhysicsMove();

					// End if we hit the ground
					if (m_parent.m_status.m_grounded && velocity.Normalized().Dot(m_parent.GetUp()) < -0.5f)
					{
						if (++m_homing_abort >= 2)
							m_parent.SetStateLand(c_speed);
					}
				}

				// Set animation
				m_parent.PlayAnimation("Roll", 7.0f);
			}

			// State overrides
			internal override bool CanDynamicPose()
			{
				return false;
			}

			internal override void HitWall(Vector3 wall_normal)
			{
				// Check how much of our velocity is going into the wall
				m_parent.SetState(new Fall(m_parent));

				// Hit wall
				base.HitWall(wall_normal);
			}
		}
	}
}
