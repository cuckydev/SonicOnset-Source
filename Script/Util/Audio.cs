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

namespace SonicOnset.Util
{
	// Audio helper class
	public class Audio
	{
		// Multiplier to dB
		public static float MultiplierToDb(float multiplier)
		{
			return 20.0f * (float)System.Math.Log10(multiplier);
		}
	}

	// AudioStreamPlayer interface
	// This is used to abstract the AudioStreamPlayer, AudioStreamPlayer2D, and AudioStreamPlayer3D classes
	// This is useful for when you want to use the same code for both 2D and 3D audio
	// It also allows you to randomly select an AudioStreamPlayer under a parent node
	public interface IAudioStreamPlayer
	{
		// Interface
		float GetPlaybackPosition();
		public AudioStreamPlayback GetStreamPlayback();
		public bool HasStreamPlayback();
		public void Play();
		public void Seek(float to_position);
		public void Stop();
		public Node GetNode();

		public bool Autoplay { get; set; }
		public StringName Bus { get; set; }
		public int MaxPolyphony { get; set; }
		public float PitchScale { get; set; }
		public bool Playing { get; set; }
		public AudioStream Stream { get; set; }
		public bool StreamPaused { get; set; }
		public float VolumeDb { get; set; }

		public static IAudioStreamPlayer FromNode(Node node)
		{
			// Return the correct specialization
			if (node is Godot.AudioStreamPlayer)
				return new AudioStreamPlayer(node as Godot.AudioStreamPlayer);
			if (node is Godot.AudioStreamPlayer2D)
				return new AudioStreamPlayer2D(node as Godot.AudioStreamPlayer2D);
			if (node is Godot.AudioStreamPlayer3D)
				return new AudioStreamPlayer3D(node as Godot.AudioStreamPlayer3D);

			// Search for audio within children
			Godot.Collections.Array<Node> children = new Godot.Collections.Array<Node>();
			children.AddRange(node.FindChildren("", "AudioStreamPlayer"));
			children.AddRange(node.FindChildren("", "AudioStreamPlayer2D"));
			children.AddRange(node.FindChildren("", "AudioStreamPlayer3D"));

			// Pick random child
			if (children.Count > 0)
			{
				int index = Mathf.RoundToInt(System.Random.Shared.Next(children.Count));
				return FromNode(children[index]);
			}

			return null;
		}
	}

	// AudioStreamPlayer specializations
	public partial class AudioStreamPlayer : IAudioStreamPlayer
	{
		// AudioStreamPlayer
		private Godot.AudioStreamPlayer m_audio_stream_player;

		// Interface specialization
		public AudioStreamPlayer(Godot.AudioStreamPlayer audio_stream_player)
		{
			m_audio_stream_player = audio_stream_player;
		}

		float IAudioStreamPlayer.GetPlaybackPosition() { return m_audio_stream_player.GetPlaybackPosition(); }
		public AudioStreamPlayback GetStreamPlayback() { return m_audio_stream_player.GetStreamPlayback(); }
		public bool HasStreamPlayback() { return m_audio_stream_player.HasStreamPlayback(); }
		public void Play() { m_audio_stream_player.Play(); }
		public void Seek(float to_position) { m_audio_stream_player.Seek(to_position); }
		public void Stop() { m_audio_stream_player.Stop(); }
		public Node GetNode() { return m_audio_stream_player; }

		public bool Autoplay { get { return m_audio_stream_player.Autoplay; } set { m_audio_stream_player.Autoplay = value; } }
		public StringName Bus { get { return m_audio_stream_player.Bus; } set { m_audio_stream_player.Bus = value; } }
		public int MaxPolyphony { get { return m_audio_stream_player.MaxPolyphony; } set { m_audio_stream_player.MaxPolyphony = value; } }
		public float PitchScale { get { return m_audio_stream_player.PitchScale; } set { m_audio_stream_player.PitchScale = value; } }
		public bool Playing { get { return m_audio_stream_player.Playing; } set { m_audio_stream_player.Playing = value; } }
		public AudioStream Stream { get { return m_audio_stream_player.Stream; } set { m_audio_stream_player.Stream = value; } }
		public bool StreamPaused { get { return m_audio_stream_player.StreamPaused; } set { m_audio_stream_player.StreamPaused = value; } }
		public float VolumeDb { get { return m_audio_stream_player.VolumeDb; } set { m_audio_stream_player.VolumeDb = value; } }
	}

	public partial class AudioStreamPlayer2D : IAudioStreamPlayer
	{
		// AudioStreamPlayer
		private Godot.AudioStreamPlayer2D m_audio_stream_player;

		// Interface specialization
		public AudioStreamPlayer2D(Godot.AudioStreamPlayer2D audio_stream_player)
		{
			m_audio_stream_player = audio_stream_player;
		}

		float IAudioStreamPlayer.GetPlaybackPosition() { return m_audio_stream_player.GetPlaybackPosition(); }
		public AudioStreamPlayback GetStreamPlayback() { return m_audio_stream_player.GetStreamPlayback(); }
		public bool HasStreamPlayback() { return m_audio_stream_player.HasStreamPlayback(); }
		public void Play() { m_audio_stream_player.Play(); }
		public void Seek(float to_position) { m_audio_stream_player.Seek(to_position); }
		public void Stop() { m_audio_stream_player.Stop(); }
		public Node GetNode() { return m_audio_stream_player; }

		public bool Autoplay { get { return m_audio_stream_player.Autoplay; } set { m_audio_stream_player.Autoplay = value; } }
		public StringName Bus { get { return m_audio_stream_player.Bus; } set { m_audio_stream_player.Bus = value; } }
		public int MaxPolyphony { get { return m_audio_stream_player.MaxPolyphony; } set { m_audio_stream_player.MaxPolyphony = value; } }
		public float PitchScale { get { return m_audio_stream_player.PitchScale; } set { m_audio_stream_player.PitchScale = value; } }
		public bool Playing { get { return m_audio_stream_player.Playing; } set { m_audio_stream_player.Playing = value; } }
		public AudioStream Stream { get { return m_audio_stream_player.Stream; } set { m_audio_stream_player.Stream = value; } }
		public bool StreamPaused { get { return m_audio_stream_player.StreamPaused; } set { m_audio_stream_player.StreamPaused = value; } }
		public float VolumeDb { get { return m_audio_stream_player.VolumeDb; } set { m_audio_stream_player.VolumeDb = value; } }
	}

	public partial class AudioStreamPlayer3D : IAudioStreamPlayer
	{
		// AudioStreamPlayer
		private Godot.AudioStreamPlayer3D m_audio_stream_player;

		// Interface specialization
		public AudioStreamPlayer3D(Godot.AudioStreamPlayer3D audio_stream_player)
		{
			m_audio_stream_player = audio_stream_player;
		}

		float IAudioStreamPlayer.GetPlaybackPosition() { return m_audio_stream_player.GetPlaybackPosition(); }
		public AudioStreamPlayback GetStreamPlayback() { return m_audio_stream_player.GetStreamPlayback(); }
		public bool HasStreamPlayback() { return m_audio_stream_player.HasStreamPlayback(); }
		public void Play() { m_audio_stream_player.Play(); }
		public void Seek(float to_position) { m_audio_stream_player.Seek(to_position); }
		public void Stop() { m_audio_stream_player.Stop(); }
		public Node GetNode() { return m_audio_stream_player; }

		public bool Autoplay { get { return m_audio_stream_player.Autoplay; } set { m_audio_stream_player.Autoplay = value; } }
		public StringName Bus { get { return m_audio_stream_player.Bus; } set { m_audio_stream_player.Bus = value; } }
		public int MaxPolyphony { get { return m_audio_stream_player.MaxPolyphony; } set { m_audio_stream_player.MaxPolyphony = value; } }
		public float PitchScale { get { return m_audio_stream_player.PitchScale; } set { m_audio_stream_player.PitchScale = value; } }
		public bool Playing { get { return m_audio_stream_player.Playing; } set { m_audio_stream_player.Playing = value; } }
		public AudioStream Stream { get { return m_audio_stream_player.Stream; } set { m_audio_stream_player.Stream = value; } }
		public bool StreamPaused { get { return m_audio_stream_player.StreamPaused; } set { m_audio_stream_player.StreamPaused = value; } }
		public float VolumeDb { get { return m_audio_stream_player.VolumeDb; } set { m_audio_stream_player.VolumeDb = value; } }
	}
}
