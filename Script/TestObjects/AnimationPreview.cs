using Godot;

namespace SonicOnset.TestObjects
{
	public partial class AnimationPreview : Node3D
	{
		[Export]
		public string m_animation_a;
		[Export]
		public string m_animation_b;

		[Export]
		public Skeleton3D m_skeleton_3d;
		[Export]
		public AnimationPlayer m_animation_player;

		private Util.Animation.SkeletonPose m_rest;

		private Util.Animation.AnimationTrack m_track_a;
		private Util.Animation.AnimationTrack m_track_b;

		private double m_time = 0.0;

		public override void _Ready()
		{
			base._Ready();

			m_rest = new Util.Animation.SkeletonPose(m_skeleton_3d);

			m_track_a = new Util.Animation.AnimationTrack(m_skeleton_3d, m_animation_player.GetAnimation(m_animation_a));
			m_track_b = new Util.Animation.AnimationTrack(m_skeleton_3d, m_animation_player.GetAnimation(m_animation_b));
		}

		public override void _Process(double delta)
		{
			// Anim test
			float ALPHA = Mathf.Sin((float)m_time * 1.2f) * 0.5f + 0.5f;

			Util.Animation.SkeletonPose pose_a = m_rest.Animate(m_track_a, m_time);
			Util.Animation.SkeletonPose pose_b = m_rest.Animate(m_track_b, m_time);
			Util.Animation.SkeletonPose pose = pose_a.Lerp(pose_b, ALPHA);

			pose.Apply(m_skeleton_3d);

			m_time += delta;
		}
	}
}
