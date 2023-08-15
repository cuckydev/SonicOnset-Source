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

// TODOS:
// - Rewrite animation code

using Godot;

namespace SonicOnset.Character
{
	public partial class ModelRoot : SonicOnset.ModelRoot
	{
		// Tilt bone class
		internal class TiltBone
		{
			// Skeleton and bone
			private Skeleton3D m_skeleton_node;
			internal int m_bone_idx;

			// Constructor
			public TiltBone(Skeleton3D skeleton_node, string bone_name)
			{
				// Get bone
				m_skeleton_node = skeleton_node;
				m_bone_idx = m_skeleton_node.FindBone(bone_name);
			}

			// Tilt bone
			public void Tilt(Quaternion quat)
			{
				// Tilt bone pose
				Quaternion pose = m_skeleton_node.GetBonePoseRotation(m_bone_idx);
				pose = pose * quat;
				m_skeleton_node.SetBonePoseRotation(m_bone_idx, pose);
			}
		}

		// Root bone class
		internal class RootBone
		{
			// Skeleton and bone
			private Skeleton3D m_skeleton_node;
			internal int m_bone_idx;

			private Transform3D m_offset;

			// Constructor
			public RootBone(Skeleton3D skeleton_node, string bone_name)
			{
				// Get bone
				m_skeleton_node = skeleton_node;
				m_bone_idx = m_skeleton_node.FindBone(bone_name);

				// Get offset
				m_offset = m_skeleton_node.GetBoneRest(m_bone_idx);
			}

			// Set root bone
			public void Shear(Transform3D transform)
			{
				// Set bone pose
				Transform3D pose = m_offset * transform;
				m_skeleton_node.SetBoneEnabled(m_bone_idx, false);
				m_skeleton_node.SetBoneRest(m_bone_idx, pose);
			}
		}

		// Model nodes
		internal Animator m_animation_player;
		internal Skeleton3D m_skeleton_node;

		internal Util.Animation.SkeletonPose m_rest_pose;

		internal System.Collections.Generic.Dictionary<string, Util.Animation.AnimationTrack> m_animation_tracks = new System.Collections.Generic.Dictionary<string, Util.Animation.AnimationTrack>();

		// Animation functions
		internal const double c_anim_blend_slow = 0.4;
		internal const double c_anim_blend_fast = 0.12;

		public virtual void ClearAnimation() => m_animation_player.ClearAnimation();
		public virtual void PlayAnimation(string name, double speed = 1.0f) => m_animation_player.PlayAnimation(name, speed);

		// Tilt state
		internal Util.Spring m_tilt = new Util.Spring(3.0f, 0.0f);
		
		internal Transform3D m_shear = Transform3D.Identity;
		internal const float m_shear_lerp = 0.4f;

		internal Vector3? m_point_of_interest = null;

		public virtual void SetTilt(float tilt)
		{
			// Set jumpball tilt target
			m_tilt.m_goal = tilt;
		}

		public virtual void SetShear(Transform3D transform)
		{
			// Lerp shear
			Basis basis = new Basis(
				m_shear.Basis.Column0.Lerp(transform.Basis.Column0, m_shear_lerp),
				m_shear.Basis.Column1.Lerp(transform.Basis.Column1, m_shear_lerp),
				m_shear.Basis.Column2.Lerp(transform.Basis.Column2, m_shear_lerp)
			);
			Vector3 origin = m_shear.Origin.Lerp(transform.Origin, m_shear_lerp);
			m_shear = new Transform3D(basis, origin);
		}

		public virtual void SetPointOfInterest(Vector3? point_of_interest)
		{
			// Set point of interest
			m_point_of_interest = point_of_interest;
		}

		// Model root
		public override void _Ready()
		{
			// Setup base
			base._Ready();
		}

		public override void _Process(double delta)
		{
			// Update base
			base._Process(delta);

			// Update tilt
			m_tilt.Step((float)delta);
		}
	}
}
