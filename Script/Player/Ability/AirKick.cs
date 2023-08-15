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
		public partial class Ability
		{
			public partial class AirKick : Ability
			{
				// Air kick flag
				bool m_bounce_flag = false;

				// Air kick ability
				public AirKick(Player player)
				{
					// Set parent player
					m_parent = player;
				}

				internal override bool CheckJumpAbility()
				{
					// Check if bounce flag is set
					if (!m_bounce_flag)
						return false;

					// Check kick button
					if (m_parent.m_input_tertiary.m_pressed)
					{
						// Begin air kick
						m_bounce_flag = false;
						m_parent.SetState(new Player.AirKick(m_parent, m_parent.m_input_stick.m_length != 0.0f));
						return true;
					}
					return false;
				}

				internal override bool CheckFallAbility()
				{
					return CheckJumpAbility();
				}

				internal override bool CheckLandAbility()
				{
					// Clear bounce flag
					m_bounce_flag = false;
					return false;
				}

				internal override void FlagHitBounce()
				{
					// Set bounce flag
					m_bounce_flag = true;
				}

				internal override void ClearHitBounce()
				{
					// Clear bounce flag
					m_bounce_flag = false;
				}
			}
		}
	}
}
