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

namespace SonicOnset.UI.MainUI
{
	public partial class MainUI : Control
	{
		// MainUI nodes
		[Export]
		private Player m_player { get; set; }

		private Label m_score_label;
		private Label m_time_label;
		private Label m_rings_label;

		// MainUI node
		public override void _Ready()
		{
			// Get nodes
			m_score_label = GetNode<Label>("MainCounter/ScoreLabel");
			m_time_label = GetNode<Label>("TimeLabel");
			m_rings_label = GetNode<Label>("MainCounter/RingIcon/RingsLabel");

			// Process after game
			ProcessPriority = (int)Enum.Priority.PostProcess;

			// Setup base
			base._Ready();
		}

		public override void _Process(double delta)
		{
			// Update labels
			m_score_label.Text = string.Format("{0:000000000}", m_player.m_score);
			m_rings_label.Text = string.Format("{0:000}", m_player.m_rings);

			// Format time as MM:SS:FF
			ulong time = m_player.m_time;

			ulong frames = time % Root.c_tick_rate;

			ulong centi = frames * 100 / Root.c_tick_rate;

			ulong seconds = time / 60;
			ulong minutes = seconds / 60;
			seconds %= 60;

			m_time_label.Text = string.Format("{0:00}:{1:00}\"{2:00}", minutes, seconds, centi);

			// Process base
			base._Process(delta);
		}
	}
}
