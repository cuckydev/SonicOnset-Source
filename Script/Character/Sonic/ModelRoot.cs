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

namespace SonicOnset.Character.Sonic
{
	public partial class ModelRoot : Character.ModelRoot
	{
		// Model nodes
		private MeshInstance3D m_jumpball_node;

		// Tilt state
		private TiltBone m_tiltbone_head;
		private TiltBone m_tiltbone_upper_torso;
		private TiltBone m_tiltbone_lower_torso;

		private RootBone m_rootbone;

		private Util.FixedSlerp m_head_point_of_interest = new Util.FixedSlerp(Quaternion.Identity);
		private Util.FixedSlerp m_upper_torso_point_of_interest = new Util.FixedSlerp(Quaternion.Identity);

		// Tilt quaternions
		private Quaternion m_head_tilt = Quaternion.Identity;
		private Quaternion m_upper_torso_tilt = Quaternion.Identity;
		private Quaternion m_lower_torso_tilt = Quaternion.Identity;

		// Animation functions
		public override void PlayAnimation(string name, double speed = 1.0f)
		{
			// Play root animation
			base.PlayAnimation(name, speed);

			// Get spinning opacity
			float opacity = 0.0f;
			if (name == "Roll" || name == "RollFloor")
				opacity = Mathf.Clamp(Mathf.Abs((float)speed) * 0.3f - 0.8f, 0.0f, 1.0f);

			// Flicker spindash every frame, but in 16 frame intervals
			// if (aclass == "Spindash" && (Root.GetClock() & 0x11) == 0)
			// 	opacity = 0.0f;

			// Set jumpball opacity
			m_jumpball_node.Transparency = 1.0f - opacity;
		}

		// Model root node
		public override void _Ready()
		{
			// Get nodes
			m_animation_player = GetNode<Animator>("Model/AnimationPlayer");
			m_skeleton_node = GetNode<Skeleton3D>("Model/Armature/Skeleton3D");

			m_jumpball_node = GetNode<MeshInstance3D>("JumpballRoot/Jumpball/Jumpball");

			// Setup tilt bones
			m_tiltbone_head = new TiltBone(m_skeleton_node, "Head");
			m_tiltbone_upper_torso = new TiltBone(m_skeleton_node, "UpperTorso");
			m_tiltbone_lower_torso = new TiltBone(m_skeleton_node, "LowerTorso");

			m_rootbone = new RootBone(m_skeleton_node, "Root");

			// Get animation tracks
			m_rest_pose = new Util.Animation.SkeletonPose(m_skeleton_node);
			foreach (string animation in m_animation_player.GetAnimationList())
				m_animation_tracks[animation] = new Util.Animation.AnimationTrack(m_skeleton_node, m_animation_player.GetAnimation(animation));

			// Add animations
			m_animation_player.m_specs["Idle"] = new Animator.AnimationSpec(0.08, false, false, null,
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Idle") },
				new Dictionary<string, double>() { { "Run", 0.3 } }
			);
			m_animation_player.m_specs["SlideBack"] = new Animator.AnimationSpec(0.08, false, false, null,
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("SlideBack") }
			);
			m_animation_player.m_specs["SlideForward"] = new Animator.AnimationSpec(0.08, false, false, null,
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("SlideForward") }
			);

			m_animation_player.m_specs["Run"] = new Animator.AnimationSpec(0.08, false, false, null, new Animator.AnimationSpec.TrackSpec[]
				{
					new Animator.AnimationSpec.TrackSpec("Walk", 0.0, 0.0, 0.2, 1.1),
					new Animator.AnimationSpec.TrackSpec("Jog", 2.3, 1.0, 0.2, 1.1),
					new Animator.AnimationSpec.TrackSpec("Run", 3.7, 0.4, 0.2, 1.1),
					new Animator.AnimationSpec.TrackSpec("JetRun", 5.09, 1.5, 0.2, 1.0),
					new Animator.AnimationSpec.TrackSpec("MachRun", 7.635, 4.0, 0.2, 0.9),
				},
				new Dictionary<string, double>() { { "Idle", 0.2 } }
			);

			m_animation_player.m_specs["BrakeStop"] = new Animator.AnimationSpec(0.08, false, false, null,
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("BrakeStop") }
			);

			m_animation_player.m_specs["Spring"] = new Animator.AnimationSpec(0.0, false, false, "SpringLoop",
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("SpringStart") },
				new Dictionary<string, double>() { { "Fall", 0.5 } }
			);
			m_animation_player.m_specs["SpringLoop"] = new Animator.AnimationSpec(0.08, false, false, null,
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Spring") },
				new Dictionary<string, double>() { { "Fall", 0.5 } }
			);

			m_animation_player.m_specs["Roll"] = new Animator.AnimationSpec(0.05, false, false, null,
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Roll") }
			);
			m_animation_player.m_specs["RollFloor"] = new Animator.AnimationSpec(0.05, false, false, null,
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("RollFloor") }
			);

			m_animation_player.m_specs["Fall"] = new Animator.AnimationSpec(0.13, false, false, null,
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Fall") }
			);
			m_animation_player.m_specs["Land"] = new Animator.AnimationSpec(0.08, false, false, "Idle",
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("Land") }
			);

			m_animation_player.m_specs["HopJump"] = new Animator.AnimationSpec(0.08, false, false, "Fall",
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("HopJump") }
			);
			m_animation_player.m_specs["HummingTop"] = new Animator.AnimationSpec(0.08, false, false, "Fall",
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("HummingTop") }
			);
			m_animation_player.m_specs["SpindashCancelTrick"] = new Animator.AnimationSpec(0.08, false, false, "Fall",
				new Animator.AnimationSpec.TrackSpec[] { new Animator.AnimationSpec.TrackSpec("SpindashCancelTrick") }
			);

			// Setup base
			base._Ready();
		}

		public override void _PhysicsProcess(double delta)
		{
			// Process base
			base._PhysicsProcess(delta);

			// Update point of interest angles
			float point_of_interest_angle_y = 0.0f;
			float point_of_interest_angle_x = 0.0f;

			if (m_point_of_interest.HasValue)
			{
				// Get point of interest relative to neck
				Vector3 point_of_interest = m_point_of_interest.Value;
				Transform3D neck_transform = GlobalTransform * m_skeleton_node.GetBoneGlobalRest(m_tiltbone_head.m_bone_idx);
				Vector3 point_of_interest_from_neck = neck_transform.Inverse() * point_of_interest;

				// Get point of interest angles
				point_of_interest_angle_y = Mathf.Atan2(-point_of_interest_from_neck.X, -point_of_interest_from_neck.Z);
				point_of_interest_angle_x = Mathf.Atan2(-point_of_interest_from_neck.Y * 2.0f, Mathf.Sqrt(point_of_interest_from_neck.X * point_of_interest_from_neck.X + point_of_interest_from_neck.Z * point_of_interest_from_neck.Z));

				point_of_interest_angle_y = Mathf.Clamp(point_of_interest_angle_y, Mathf.Pi * -0.5f, Mathf.Pi * 0.5f);
				point_of_interest_angle_x = Mathf.Clamp(point_of_interest_angle_x, Mathf.Pi * -0.2f, Mathf.Pi * 0.1f);
			}

			// Get point of interest quaternions
			Quaternion upper_torso_point_of_interest = new Quaternion(new Vector3(0.0f, 1.0f, 0.0f), point_of_interest_angle_y * 0.3f);
			upper_torso_point_of_interest *= new Quaternion(new Vector3(1.0f, 0.0f, 0.0f), point_of_interest_angle_x * 0.4f);

			Quaternion head_point_of_interest = new Quaternion(new Vector3(0.0f, 1.0f, 0.0f), point_of_interest_angle_y * 0.4f);
			head_point_of_interest *= new Quaternion(new Vector3(1.0f, 0.0f, 0.0f), point_of_interest_angle_x * 0.5f);

			// Interpolate quaternions
			m_head_point_of_interest.Step(head_point_of_interest, 0.2f);
			m_upper_torso_point_of_interest.Step(upper_torso_point_of_interest, 0.2f);
		}

		public override void _Process(double delta)
		{
			// Process base
			base._Process(delta);

			// Get interpolation fraction
			float fraction = (float)Engine.GetPhysicsInterpolationFraction();

			// Shear root bone
			m_rootbone.Shear(m_shear);

			// Tilt bones
			m_head_tilt = new Quaternion(new Vector3(0.0f, 1.0f, 0.0f), m_tilt.m_pos * 0.15f);
			m_head_tilt *= new Quaternion(new Vector3(0.0f, 0.0f, 1.0f), m_tilt.m_pos * 0.1f);
			m_head_tilt *= new Quaternion(new Vector3(1.0f, 0.0f, 0.0f), -Mathf.Abs(m_tilt.m_pos * 0.3f));

			m_upper_torso_tilt = new Quaternion(new Vector3(0.0f, 0.0f, 1.0f), m_tilt.m_pos * -0.3f);
			m_upper_torso_tilt *= new Quaternion(new Vector3(0.0f, 1.0f, 0.0f), m_tilt.m_pos * 0.3f);

			m_lower_torso_tilt = new Quaternion(new Vector3(0.0f, 0.0f, 1.0f), m_tilt.m_pos * -0.25f);

			// Apply point of interest quaternions
			m_head_tilt *= m_head_point_of_interest.Get(fraction);
			m_upper_torso_tilt *= m_upper_torso_point_of_interest.Get(fraction);

			// Tilt bones
			m_tiltbone_head.Tilt(m_head_tilt);
			m_tiltbone_upper_torso.Tilt(m_upper_torso_tilt);
			m_tiltbone_lower_torso.Tilt(m_lower_torso_tilt);
		}
	}
}
