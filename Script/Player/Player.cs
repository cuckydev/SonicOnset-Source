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

// Big TODOs:
// - Just clean up the code in general, this is a mess

using Godot;
using System.Collections.Generic;

namespace SonicOnset
{
	// Player controller class
	public partial class Player : CharacterBody3D, IObject
	{
		// Player nodes
		[Export]
		private Camera m_camera_node;

		private CollisionShape3D m_main_colshape_node;
		private CollisionShape3D m_roll_colshape_node;

		private PlayerParam m_param_node;

		private ObjectTriggerInterest m_radial_trigger;

		internal Character.ModelRoot m_modelroot;
		private Transform3D m_modelroot_offset;

		// Debug context
		private UI.DebugUI.DebugUI.DebugUIContext m_debug_context = UI.DebugUI.DebugUI.AcquireContext("Player");

		// Player physics state
		[Export]
		public Vector3 m_gravity = -Vector3.Up;

		internal Param m_param = new Param();

		private PhysicsDirectSpaceState3D m_directspace;
		private PhysicsRayQueryParameters3D m_ray_query;

		// Player game state
		public ulong m_score { get; private set; } = 0;
		public ulong m_time { get; private set; } = 0;
		public uint m_rings { get; private set; } = 0;

		public void AddScore(ulong score)
		{
			m_score += score;
		}

		public void AddRings(uint rings)
		{
			m_rings += rings;
		}

		// Player status
		public struct Status
		{
			public bool m_grounded = true;

			public Status() {}
		}
		public Status m_status = new Status();

		// Player input state
		public Input.Stick m_input_stick = new Input.Stick();
		public Input.Button m_input_jump = new Input.Button();
		public Input.Button m_input_spin = new Input.Button();
		public Input.Button m_input_secondary = new Input.Button();
		public Input.Button m_input_tertiary = new Input.Button();
		public Input.Button m_input_quaternary = new Input.Button();

		public Debounce m_input_stop = new Debounce();
		public Debounce m_input_speed = new Debounce();

		// Player ability
		public partial class Ability
		{
			// Parent player
			internal Player m_parent;

			// Ability interface
			virtual internal bool CheckJump() { return false; }
			virtual internal bool CheckJumpAbility() { return false; }
			virtual internal bool CheckFallAbility() { return false; }
			virtual internal bool CheckSpinAbility() { return false; }
			virtual internal bool CheckLandAbility() { return false; }

			virtual internal void FlagHitBounce() { } // When you bounce off an object, for special abilities like Air Kick and Chaos Snap
			virtual internal void ClearHitBounce() { } // Performing an ability should clear the bounce flag

			// Ability list class
			public partial class AbilityList : Ability
			{
				// Abilities
				internal System.Collections.Generic.List<Ability> m_abilities = new System.Collections.Generic.List<Ability>();

				// Ability list interface
				internal override bool CheckJump()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckJump())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override bool CheckJumpAbility()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckJumpAbility())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override bool CheckFallAbility()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckFallAbility())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override bool CheckSpinAbility()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckSpinAbility())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override bool CheckLandAbility()
				{
					foreach (Ability ability in m_abilities)
					{
						if (ability.CheckLandAbility())
						{
							ClearHitBounce();
							return true;
						}
					}
					return false;
				}

				internal override void FlagHitBounce()
				{
					foreach (Ability ability in m_abilities)
						ability.FlagHitBounce();
				}

				internal override void ClearHitBounce()
				{
					foreach (Ability ability in m_abilities)
						ability.ClearHitBounce();
				}
			}
		}

		public Ability m_ability { get; internal set; }

		// Player state
		public partial class State
		{
			// Parent player
			internal Player m_parent;

			// State interface
			virtual internal void Ready() { }
			virtual internal void Stop() { }

			virtual internal void AbilityProcess() { }
			virtual internal void Process() { }

			virtual internal void Debug(List<string> debugs) { }

			// State overrides
			virtual internal bool CanDynamicPose()
			{
				return true;
			}
			
			virtual internal float GetTilt()
			{
				// Get our turning proportional to our speed
				float turn = m_parent.m_input_stick.m_turn;
				turn = Mathf.Clamp(turn, Mathf.Pi * -0.3f, Mathf.Pi * 0.3f);

				float speed = m_parent.GetAbsSpeedX();
				float tilt = turn * (0.5f + Mathf.Clamp(speed / 3.0f, 0.0f, 1.8f));
				return tilt;
			}

			virtual internal Transform3D GetShear()
			{
				// No shearing
				return Transform3D.Identity;
			}

			virtual internal void HitWall(Vector3 wall_normal)
			{
				if (m_parent.m_status.m_grounded)
				{
					// Project velocity onto wall
					m_parent.Velocity = Util.Vector3.NormalProject(m_parent.Velocity, wall_normal);
				}
				else
				{
					// Try to preserve our Y velocity
					Vector3 pre_velocity = m_parent.Velocity;
					m_parent.Velocity = Util.Vector3.NormalProject(m_parent.Velocity, wall_normal);

					// Check if the wall is too steep for us to stand on
					if (wall_normal.Dot(m_parent.GetUp()) < Mathf.Cos(m_parent.FloorMaxAngle))
					{
						// If the projection inverted our velocity, cancel preservation
						if (m_parent.Velocity.Dot(pre_velocity) < 0.0f)
							return;

						// Get our Y velocity before and after the projection
						float pre_y = Mathf.Abs(pre_velocity.Dot(m_parent.GetUp()));
						float post_y = Mathf.Abs(m_parent.Velocity.Dot(m_parent.GetUp()));
						if (post_y < Mathf.Epsilon || pre_y < Mathf.Epsilon)
							return;

						// If we're losing Y velocity, cancel preservation
						// As that would cause us to gain extreme XZ velocity
						if (pre_y > post_y)
							return;

						// Preserve our Y velocity
						m_parent.Velocity *= pre_y / post_y;
					}
				}
			}

			virtual public bool HitObject(Node3D node) { return true; }
		}

		public State m_state { get; internal set; }

		public void SetState(State state)
		{
			// Stop the current state
			if (m_state != null)
			{
				m_state.Stop();
				m_state = null;
			}

			// Start the new state
			m_state = state;
			m_state.Ready();
		}

		// Common states
		internal void SetStateLand(float y_speed = 0.0f)
		{
			// Check for land ability
			if (m_ability.CheckLandAbility())
				return;
			
			// Play landing sound
			float audio_db = Util.Audio.MultiplierToDb(Mathf.Clamp(0.2f + -y_speed * 0.17f, 0.0f, 1.0f));
			PlaySound("Land", audio_db);

			// Check if we have enough speed to run
			if (GetAbsSpeedX() < m_param.m_jog_speed)
				SetState(new Idle(this, y_speed < -2.5f));
			else
				SetState(new Run(this));
		}

		// Coordinate systems
		internal Vector3 GetLook()
		{
			return -GlobalTransform.Basis.Z;
		}

		internal Vector3 GetUp()
		{
			return GlobalTransform.Basis.Y;
		}

		internal Vector3 GetRight()
		{
			return GlobalTransform.Basis.X;
		}

		internal Vector3 ToSpeed(Vector3 vector)
		{
			Vector3 speed = GlobalTransform.Basis.Inverse() * vector;
			return new Vector3(-speed.Z, speed.Y, speed.X) / Root.c_tick_rate;
		}

		internal Vector3 FromSpeed(Vector3 speed)
		{
			Vector3 vector = GlobalTransform.Basis * new Vector3(speed.Z, speed.Y, -speed.X);
			return vector * Root.c_tick_rate;
		}

		// Rotation functions
		internal void OriginBasis(Basis basis, float inertia = 1.0f)
		{
			Vector3 prev_speed = ToSpeed(Velocity);
			Basis = (basis * Basis).Orthonormalized();
			Velocity = Velocity * inertia + FromSpeed(prev_speed) * (1.0f - inertia);
		}

		internal void OriginRotate(Vector3 axis, float angle, float inertia = 1.0f)
		{
			OriginBasis(new Basis(axis, angle), inertia);
		}

		internal void CenterBasis(Basis basis, float inertia = 1.0f)
		{
			Vector3 prev_speed = ToSpeed(Velocity);
			GlobalTranslate(GetUp() * m_param.m_center_height);
			Basis = (basis * Basis).Orthonormalized();
			GlobalTranslate(GetUp() * -m_param.m_center_height);
			Velocity = Velocity * inertia + FromSpeed(prev_speed) * (1.0f - inertia);
		}

		internal void CenterRotate(Vector3 axis, float angle, float inertia = 1.0f)
		{
			CenterBasis(new Basis(axis, angle), inertia);
		}

		internal Transform3D GetCenterTransform()
		{
			return GlobalTransform.Translated(Vector3.Up * m_param.m_center_height);
		}

		// Common values
		internal float GetSpeedX()
		{
			return GetLook().Dot(Velocity) / Root.c_tick_rate;
		}
		internal float GetAbsSpeedX()
		{
			return Mathf.Abs(GetSpeedX());
		}

		internal float GetSpeedY()
		{
			return GetUp().Dot(Velocity) / Root.c_tick_rate;
		}
		internal float GetAbsSpeedY()
		{
			return Mathf.Abs(GetSpeedY());
		}

		internal float GetSpeedZ()
		{
			return GetRight().Dot(Velocity) / Root.c_tick_rate;
		}
		internal float GetAbsSpeedZ()
		{
			return Mathf.Abs(GetSpeedZ());
		}

		internal float GetDotp()
		{
			return -GetUp().Dot(m_gravity);
		}

		// Sound functions
		internal Util.IAudioStreamPlayer GetSound(string name)
		{
			// Get sound node
			return Util.IAudioStreamPlayer.FromNode(GetNode("Sound/" + name));
		}

		internal void PlaySound(string name, float db = 0.0f, float speed = 1.0f)
		{
			// Get sound node
			Util.IAudioStreamPlayer sound_node = GetSound(name);

			// Set sound volume and speed
			sound_node.VolumeDb = db;
			sound_node.PitchScale = speed;

			// Play sound
			sound_node.Stop();
			sound_node.Play();
		}

		internal void StopSound(string name)
		{
			// Get sound node
			Util.IAudioStreamPlayer sound_node = GetSound(name);

			// Stop sound
			sound_node.Stop();
		}

		internal void UpdateSound(float db = 0.0f, float speed = 1.0f)
		{
			// Get sound node
			Util.IAudioStreamPlayer sound_node = GetSound("Footstep");

			// Set sound volume and speed
			sound_node.VolumeDb = db;
			sound_node.PitchScale = speed;
		}

		// Animation functions
		internal void ClearAnimation() => m_modelroot.ClearAnimation();
		internal void PlayAnimation(string name, double speed = 1.0f) => m_modelroot.PlayAnimation(name, speed);

		// Godot methods
		public override void _Ready()
		{
			// Get nodes
			m_main_colshape_node = GetNode<CollisionShape3D>("MainColShape");
			m_roll_colshape_node = GetNode<CollisionShape3D>("RollColShape");

			m_param_node = GetNode<PlayerParam>("PlayerParam");

			m_radial_trigger = GetNode<ObjectTriggerInterest>("RadialTrigger");

			m_modelroot = GetNode<Character.ModelRoot>("ModelRoot");
			m_modelroot_offset = GlobalTransform.Inverse() * m_modelroot.GlobalTransform;

			// Initialize collision
			m_directspace = GetWorld3D().DirectSpaceState;

			m_ray_query = new PhysicsRayQueryParameters3D();
			m_ray_query.CollisionMask = CollisionMask;
			m_ray_query.Exclude.Add(GetRid());
			m_ray_query.HitBackFaces = false;

			// Initialize player state
			SetState(new Idle(this));

			// Setup base
			base._Ready();
		}

		public override void _PhysicsProcess(double delta)
		{
			// Reset if too low
			if (GlobalPosition.Y < -100.0f)
			{
				GlobalTransform = Transform3D.Identity;
				Velocity = Vector3.Zero;
			}

			// Update player parameters
			m_param = m_param_node.m_param;

			// Update input state
			{
				Vector2 input_stick = Input.Server.GetMoveVector();
				bool input_jump = Input.Server.GetButton("move_jump");
				bool input_spin = Input.Server.GetButton("move_spin");
				bool input_secondary = Input.Server.GetButton("move_secondary");
				bool input_tertiary = Input.Server.GetButton("move_tertiary");
				bool input_quaternary = Input.Server.GetButton("move_quaternary");

				m_input_stick.Update(input_stick, GlobalTransform, m_camera_node.GlobalTransform, -m_gravity);
				m_input_jump.Update(input_jump);
				m_input_spin.Update(input_spin);
				m_input_secondary.Update(input_secondary);
				m_input_tertiary.Update(input_tertiary);
				m_input_quaternary.Update(input_quaternary);

				if (!m_input_stop.Check())
				{
					m_input_stick.m_x = 0.0f;
					m_input_stick.m_y = 0.0f;
					m_input_stick.m_turn = 0.0f;
					m_input_stick.m_length = 0.0f;
					m_input_stick.m_angle = 0.0f;
				}

				if (!m_input_speed.Check())
				{
					m_input_stick.m_x = 0.0f;
					m_input_stick.m_y = 1.0f;
					m_input_stick.m_turn = 0.0f;
					m_input_stick.m_length = 1.0f;
					m_input_stick.m_angle = 0.0f;
				}
			}

			// Process state
			m_state.AbilityProcess();
			m_state.Process();

			// Update model root
			m_modelroot.SetTransform(GlobalTransform * m_modelroot_offset);

			if (m_state.CanDynamicPose())
			{
				m_modelroot.SetTilt(m_state.GetTilt());
				m_modelroot.SetPointOfInterest(null);
			}
			else
			{
				m_modelroot.SetTilt(0.0f);
				m_modelroot.SetPointOfInterest(null);
			}

			m_modelroot.SetShear(m_state.GetShear());

			// Send RPC update
			Root.Rpc(this, "HostRpc_Update", GlobalTransform);

			// Increment time
			m_time++;

			// Update debug context
#if DEBUG
			List<string> debugs = new List<string>();

			debugs.Add(string.Format("= Physics ="));

			Vector3 position = GlobalPosition;
			debugs.Add(string.Format("Position ({0:0.00}, {1:0.00}, {2:0.00})", position.X, position.Y, position.Z));

			Vector3 velocity = Velocity / Root.c_tick_rate;
			debugs.Add(string.Format("Velocity ({0:0.00}, {1:0.00}, {2:0.00})", velocity.X, velocity.Y, velocity.Z));

			Vector3 speed = ToSpeed(Velocity);
			debugs.Add(string.Format("Speed ({0:0.00}, {1:0.00}, {2:0.00})", speed.X, speed.Y, speed.Z));

			Vector3 rotation = GlobalRotation * 180.0f / Mathf.Pi;
			debugs.Add(string.Format("Rotation ({0:0.00}, {1:0.00}, {2:0.00})", rotation.X, rotation.Y, rotation.Z));

			debugs.Add(string.Format("= State {0} =", m_state));
			m_state.Debug(debugs);

			m_debug_context.SetItems(debugs);
#endif
		}

		// RPC methods
		private void HostRpc_Update(Transform3D transform)
		{
			// Forward the RPC to all clients
			Root.GetHostServer().RpcAll(this, "Rpc_Update", transform);
		}
	}
}
