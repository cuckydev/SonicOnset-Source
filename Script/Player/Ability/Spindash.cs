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
			public partial class Spindash : Ability
			{
				// Spindash ability
				public Spindash(Player player)
				{
					// Set parent player
					m_parent = player;
				}

				internal override bool CheckSpinAbility()
				{
					// Check spin button
					if (m_parent.m_input_spin.m_pressed)
					{
						// Begin spindash
						m_parent.SetState(new Player.Spindash(m_parent));
						return true;
					}

					// Check roll button
					if (m_parent.m_input_quaternary.m_pressed && m_parent.GetAbsSpeedX() > m_parent.m_param.m_jog_speed)
					{
						// Begin rolling
						m_parent.SetState(new Player.Roll(m_parent));
						return true;
					}
					return false;
				}

				internal override bool CheckLandAbility()
				{
					// Check roll button
					if (m_parent.m_input_quaternary.m_down && m_parent.GetAbsSpeedX() > m_parent.m_param.m_jog_speed)
					{
						// Begin rolling
						m_parent.SetState(new Player.Roll(m_parent));
						return true;
					}
					return false;
				}
			}
		}
	}
}
