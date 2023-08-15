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

namespace SonicOnset.Util
{
	public static class Animation
	{
		// Bone pose
		// Stores the position, rotation and scale of a bone pose
		private struct BonePose
		{
			// Identity bone pose
			public Godot.Vector3 m_position = Godot.Vector3.Zero;
			public Quaternion m_rotation = Quaternion.Identity;
			public Godot.Vector3 m_scale = Godot.Vector3.One;

			// Bone pose constructors
			public BonePose() { }
			public BonePose(Godot.Vector3 position, Quaternion rotation, Godot.Vector3 scale)
			{
				m_position = position;
				m_rotation = rotation;
				m_scale = scale;
			}

			// Lerp between two bone poses
			public static BonePose Lerp(BonePose a, BonePose b, float t)
			{
				return new BonePose
				(
					a.m_position.Lerp(b.m_position, t),
					a.m_rotation.Slerp(b.m_rotation, t),
					a.m_scale.Lerp(b.m_scale, t)
				);
			}

			public BonePose Lerp(BonePose b, float t)
			{
				return Lerp(this, b, t);
			}

			// Add one bone pose to another
			public static BonePose Add(BonePose a, BonePose rest, BonePose b, float t = 1.0f)
			{
				Quaternion inv_rest = rest.m_rotation.Inverse();
				return new BonePose
				(
					a.m_position + (inv_rest * a.m_rotation) * (b.m_position - a.m_position) * t,
					a.m_rotation * Quaternion.Identity.Slerp(inv_rest * b.m_rotation, t),
					a.m_scale * (((b.m_scale - Godot.Vector3.One) * t) + Godot.Vector3.One)
				);
			}

			public BonePose Add(BonePose rest, BonePose b, float t = 1.0f)
			{
				return Add(this, rest, b, t);
			}
		}

		// Animation track which preprocesses animation tracks and which bones they go to
		// It stores a list of track indices to bone indices, so that we only process the tracks we need
		public struct AnimationTrack
		{
			// Animation and tracks
			public Godot.Animation m_animation;
			public List<KeyValuePair<int, int>> m_tracks = new List<KeyValuePair<int, int>>();

			public AnimationTrack(Skeleton3D skeleton, Godot.Animation animation)
			{
				// Store animation
				m_animation = animation;

				// Go through all tracks
				for (int i = 0; i < animation.GetTrackCount(); i++)
				{
					// Get bone name
					string track_path = animation.TrackGetPath(i);
					string bone_name = track_path.Substring(track_path.LastIndexOf(":") + 1);

					// Store track to bone mapping
					int bone = skeleton.FindBone(bone_name);
					if (bone != -1)
						m_tracks.Add(new KeyValuePair<int, int>(i, bone));
				}
			}
		}

		// Skeleton pose struct
		// This contains a pose for each bone in the skeleton
		// It stores both the rest pose from a Skeleton3D and animated poses from Animations
		public struct SkeletonPose
		{
			// Bone poses
			private BonePose[] m_bone_poses;

			// From SkeletonPose
			public SkeletonPose(SkeletonPose skeleton_pose)
			{
				// Copy bone poses
				m_bone_poses = new BonePose[skeleton_pose.m_bone_poses.Length];
				System.Array.Copy(skeleton_pose.m_bone_poses, m_bone_poses, m_bone_poses.Length);
			}

			// From Skeleton3D
			public SkeletonPose(Skeleton3D skeleton)
			{
				// Create rest bone poses
				m_bone_poses = new BonePose[skeleton.GetBoneCount()];
				for (int i = 0; i < m_bone_poses.Length; i++)
				{
					Transform3D rest = skeleton.GetBoneRest(i);
					m_bone_poses[i] = new BonePose
					(
						rest.Origin,
						rest.Basis.GetRotationQuaternion().Normalized(),
						rest.Basis.Scale
					);
				}
			}

			// From Animation
			public SkeletonPose Animate(AnimationTrack track, double time)
			{
				// Create new skeleton pose
				SkeletonPose pose = new SkeletonPose(this);

				// Get animation and time
				Godot.Animation animation = track.m_animation;

				switch (animation.LoopMode)
				{
					case Godot.Animation.LoopModeEnum.None:
						time = Mathf.Min(time, animation.Length);
						break;
					case Godot.Animation.LoopModeEnum.Linear:
						time = Mathf.PosMod(time, animation.Length);
						break;
					case Godot.Animation.LoopModeEnum.Pingpong:
						time = Mathf.PingPong(time, animation.Length);
						break;
				}

				// Get interpolated pose
				foreach (KeyValuePair<int, int> track_pair in track.m_tracks)
				{
					// Apply track
					int track_i = track_pair.Key;
					int bone_i = track_pair.Value;

					ref BonePose bone_pose = ref pose.m_bone_poses[bone_i];
					switch (animation.TrackGetType(track_i))
					{
						case Godot.Animation.TrackType.Position3D:
							bone_pose.m_position = animation.PositionTrackInterpolate(track_i, time);
							break;
						case Godot.Animation.TrackType.Rotation3D:
							bone_pose.m_rotation = animation.RotationTrackInterpolate(track_i, time).Normalized();
							break;
						case Godot.Animation.TrackType.Scale3D:
							bone_pose.m_scale = animation.ScaleTrackInterpolate(track_i, time);
							break;
					}
				}

				// Return pose
				return pose;
			}

			// Apply onto Skeleton3D
			public void Apply(Skeleton3D skeleton)
			{
				// Apply bone poses
				for (int i = 0; i < m_bone_poses.Length; i++)
				{
					skeleton.SetBonePosePosition(i, m_bone_poses[i].m_position);
					skeleton.SetBonePoseRotation(i, m_bone_poses[i].m_rotation);
					skeleton.SetBonePoseScale(i, m_bone_poses[i].m_scale);
				}
			}

			// Lerp between two skeleton poses
			public static SkeletonPose Lerp(SkeletonPose a, SkeletonPose b, float t)
			{
				// Create new skeleton pose
				SkeletonPose pose = new SkeletonPose(a);

				// Lerp between bone poses
				for (int i = 0; i < pose.m_bone_poses.Length; i++)
					pose.m_bone_poses[i] = BonePose.Lerp(a.m_bone_poses[i], b.m_bone_poses[i], t);

				// Return pose
				return pose;
			}

			public SkeletonPose Lerp(SkeletonPose b, float t)
			{
				return Lerp(this, b, t);
			}
		}
	}
}
