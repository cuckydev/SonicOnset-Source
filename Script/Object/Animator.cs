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
	public partial class Animator : AnimationPlayer
	{
		// Animation spec types
		// Animation track is determined by "X" which is a point on a 1D line
		// Each track has a starting X and envelope
		// For the running animation for example, the X axis would be the speed of the character
		public class AnimationSpec
		{
			public struct TrackSpec
			{
				// Name of track to play
				public string m_name;

				// X axis and envelope
				public double m_x = 0.0;
				public double m_envelope = 0.0;

				// Speed by X
				public double m_speed_base = 0.0;
				public double m_speed_mult = 1.0;

				public TrackSpec(string name, double x = 0.0, double envelope = 0.0, double speed_base = 0.0, double speed_mult = 1.0)
				{
					m_name = name;
					m_x = x;
					m_envelope = envelope;
					m_speed_base = speed_base;
					m_speed_mult = speed_mult;
				}
			}

			public double m_blend = 0.0; // Blend time
			public bool m_mirror_animation = false; // Animation is mirrored if negative
			public bool m_mirror_x = false; // X axis is mirrored if negative
			public string m_next_anim = null; // Next animation to play after this one

			public TrackSpec[] m_tracks = null;
			public Dictionary<string, double> m_blends = null;

			public AnimationSpec(double blend, bool mirror_animation, bool mirror_x, string next_anim, TrackSpec[] tracks, Dictionary<string, double> blends = null)
			{
				m_blend = blend;
				m_mirror_animation = mirror_animation;
				m_mirror_x = mirror_x;
				m_next_anim = next_anim;
				m_tracks = tracks;
				m_blends = blends;
			}
		}

		// Skeleton node
		[Export]
		private Skeleton3D m_skeleton_node;

		private Util.Animation.SkeletonPose m_rest_pose;

		// Animation tracks
		private Dictionary<string, Util.Animation.AnimationTrack> m_tracks = new();
		public Dictionary<string, AnimationSpec> m_specs = new();

		// Animation state
		public string m_current_anim { get; private set; } = null;

		// Animation states
		private const int c_animation_states = 4; // How many concurrent animation states are played at once

		private struct AnimationState
		{
			public AnimationSpec m_spec;
			public double m_x = 0.0;

			public double m_time = 0.0;

			public float m_weight = 0.0f;
			public float m_weight_inc = 0.0f;

			public AnimationState() { }
		}

		private AnimationState[] m_states = new AnimationState[c_animation_states];
		private int m_state_tail = 0;
		private int m_state_head = -1;

		private ref AnimationState AllocateState()
		{
			// Increment head and tail if the head catches up to the tail
			if (m_state_head >= 0)
			{
				m_state_head = (m_state_head + 1) % c_animation_states;
				if (m_state_head == m_state_tail)
					m_state_tail = (m_state_tail + 1) % c_animation_states;
			}
			else
			{
				m_state_head = 0;
				m_state_tail = 0;
			}

			// Get and initialize reference to state at head
			ref AnimationState state = ref m_states[m_state_head];
			state.m_time = 0.0;
			state.m_weight = 0.0f;

			return ref state;
		}

		private void PushAnimation(string name, double x = 1.0)
		{
			// Get animation spec
			AnimationSpec new_spec = m_specs[name];

			// Get blend time between this and the previous animation
			double blend_time = new_spec.m_blend;
			if (m_state_head >= 0)
			{
				AnimationSpec old_spec = m_states[m_state_head].m_spec;
				if (old_spec.m_blends != null && old_spec.m_blends.ContainsKey(name))
					blend_time = old_spec.m_blends[name];
			}

			// Allocate new state
			ref AnimationState state = ref AllocateState();
			state.m_spec = new_spec;
			state.m_x = x;

			if (blend_time <= 0.0)
			{
				// Set this as the only state, as it shall not be blended
				m_state_tail = m_state_head;
			}
			else
			{
				// Gradually increase weight over time
				state.m_weight_inc = (float)(1.0 / blend_time);
			}
		}

		public void PlayAnimation(string name, double x = 1.0)
		{
			// Check if this is a new animation
			if (name != m_current_anim)
			{
				// Push animation
				PushAnimation(name, x);

				// Remember animation name
				m_current_anim = name;
			}
			else
			{
				// Update animation X
				ref AnimationState state = ref m_states[m_state_head];
				state.m_x = x;
			}
		}

		public void ClearAnimation()
		{
			// Clear current animation
			m_current_anim = null;
		}

		// Animator node
		public override void _Ready()
		{
			// Disable animation player
			PlaybackProcessMode = AnimationProcessCallback.Manual;

			// Ready base
			base._Ready();

			// Setup animation tracks
			string[] animations = GetAnimationList();
			foreach (string animation in animations)
				m_tracks[animation] = new(m_skeleton_node, GetAnimation(animation));

			// Get rest pose
			m_rest_pose = new(m_skeleton_node);
		}

		public override void _Process(double delta)
		{
			// Process base
			base._Process(delta);

			// Check if any animations are playing
			if (m_state_head < 0)
			{
				// m_rest_pose.Apply(m_skeleton_node);
				return;
			}

			// Process animation states
			Util.Animation.SkeletonPose pose = m_rest_pose;

			for (int state = m_state_tail; ; state = (state + 1) % c_animation_states)
			{
				// Get corresponding state
				ref AnimationState anim_state = ref m_states[state];
				AnimationSpec anim_spec = anim_state.m_spec;

				// Increment weight
				if (state == m_state_tail)
				{
					// Tail should always be full weight, or the rest pose will be exposed
					anim_state.m_weight = 1.0f;
				}
				else
				{
					anim_state.m_weight += anim_state.m_weight_inc * (float)delta;
					if (anim_state.m_weight >= 1.0f)
					{
						m_state_tail = state; // Any states before this are now redundant
						anim_state.m_weight = 1.0f;
					}
				}

				// Get track spec
				ref AnimationSpec.TrackSpec track_spec_a = ref anim_spec.m_tracks[0];
				ref AnimationSpec.TrackSpec track_spec_b = ref anim_spec.m_tracks[0];
				float track_spec_blend;

				if (anim_spec.m_tracks.Length == 1)
				{
					// Only one track, use that
					track_spec_blend = 0.0f;
				}
				else
				{
					// Find first and second track to blend between
					int track_a = 0;
					int track_b = 0;

					for (int i = 0; i < anim_spec.m_tracks.Length; i++)
					{
						if (anim_spec.m_tracks[i].m_x >= anim_state.m_x)
							break;
						track_a = i;
					}

					track_b = Mathf.Min(track_a + 1, anim_spec.m_tracks.Length - 1);
					track_spec_a = ref anim_spec.m_tracks[track_a];
					track_spec_b = ref anim_spec.m_tracks[track_b];

					// If tracks are the same or don't blend, only use one track
					if (track_a == track_b || anim_spec.m_tracks[track_b].m_envelope <= 0.0)
						track_spec_blend = 0.0f;
					else
						track_spec_blend = (float)Mathf.Clamp(1.0 - (anim_spec.m_tracks[track_b].m_x - anim_state.m_x) / anim_spec.m_tracks[track_b].m_envelope, 0.0, 1.0);
				}

				// Get state pose
				double speed = 0.0;

				if (track_spec_blend > 0.0f)
				{
					// Get pose for both tracks
					Util.Animation.SkeletonPose pose_a = m_rest_pose.Animate(m_tracks[track_spec_a.m_name], anim_state.m_time);
					Util.Animation.SkeletonPose pose_b = m_rest_pose.Animate(m_tracks[track_spec_b.m_name], anim_state.m_time);

					// Blend pose
					pose = pose.Lerp(pose_a.Lerp(pose_b, track_spec_blend), anim_state.m_weight);

					if (state == m_state_head)
					{
						// Calculate speed and increment time
						double speed_a = track_spec_a.m_speed_base + track_spec_a.m_speed_mult * anim_state.m_x;
						double speed_b = track_spec_b.m_speed_base + track_spec_b.m_speed_mult * anim_state.m_x;
						speed = Mathf.Lerp(speed_a, speed_b, track_spec_blend);
					}
				}
				else
				{
					// Get pose for track A
					Util.Animation.SkeletonPose pose_a = m_rest_pose.Animate(m_tracks[track_spec_a.m_name], anim_state.m_time);

					// Blend pose
					pose = pose.Lerp(pose_a, anim_state.m_weight);

					// Calculate speed and increment time
					if (state == m_state_head)
					{
						double speed_a = track_spec_a.m_speed_base + track_spec_a.m_speed_mult * anim_state.m_x;
						speed = speed_a;
					}
				}

				// Increment time
				if (state == m_state_head)
				{
					// Add speed to time
					anim_state.m_time += delta * speed;

					// If animation ended, push next animation
					if (anim_spec.m_next_anim != null)
					{
						double length = m_tracks[track_spec_a.m_name].m_animation.Length;
						if (anim_state.m_time >= length)
							PushAnimation(anim_spec.m_next_anim, anim_state.m_x);
					}
				}

				// Break if this state is the head
				if (state == m_state_head)
					break;
			}

			// Apply pose
			pose.Apply(m_skeleton_node);
		}
	}
}
