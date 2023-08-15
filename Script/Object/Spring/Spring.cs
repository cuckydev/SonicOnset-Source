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
	public partial class Spring : ObjectTriggerInterest, IObject
	{
		// Spring parameters
		[Export]
		float m_power = 6.0f;
		[Export]
		int m_nocon = 60;
		[Export]
		bool m_roll = false;

		// Debounce
		Debounce m_debounce = new Debounce();

		// Nodes
		private AnimationPlayer m_animation_player;

		private Util.IAudioStreamPlayer m_bounce_sound;

		// Node setup
		public override void _Ready()
		{
			// Get nodes
			m_animation_player = GetNode<AnimationPlayer>("Model/AnimationPlayer");

			m_bounce_sound = Util.IAudioStreamPlayer.FromNode(GetNode("BounceSound"));

			// Setup base
			base._Ready();
		}

		// Trigger listener
		public void Touch(Node3D other)
		{
			// Check if player
			Player player = other as Player;
			if (player != null)
			{
				// Check debounce
				if (!m_debounce.Check())
					return;
				m_debounce.Set(10);

				// Play animation
				m_animation_player.Play("SpringBounce");

				// Play sound
				m_bounce_sound.Play();

				// Launch player
				if (player.m_state.HitObject(this))
				{
					// Align player to spring
					if (Mathf.Abs(GlobalTransform.Basis.Y.Dot(player.m_gravity)) < 0.95f)
					{
						// Align player directly up to spring
						Transform3D look_transform = GlobalTransform.LookingAt(GlobalTransform.Origin + GlobalTransform.Basis.Y, -player.m_gravity);
						player.GlobalTransform = look_transform.RotatedLocal(Vector3.Right, Mathf.Pi * -0.5f);
					}
					else
					{
						// Align player to spring
						player.OriginBasis(Util.Basis.FromTo(player.GetUp(), GlobalTransform.Basis.Y));
						if (m_nocon < 0)
							player.GlobalTransform = new Transform3D(player.GlobalTransform.Basis, GlobalTransform.Origin);
					}

					// Launch player velocity
					player.Velocity = GlobalTransform.Basis.Y * (m_power * Root.c_tick_rate);

					// Set player state
					if (m_roll)
						player.SetState(new Player.Jump(player));
					else
						player.SetState(new Player.Spring(player, m_nocon));

					player.m_ability.FlagHitBounce();

					player.m_input_stop.Set((ulong)Mathf.Abs(m_nocon));
					player.m_status.m_grounded = false;
				}
			}
		}

		// Object properties
		public bool CanHomingAttack() { return true; }
	}
}
