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
			public partial class HomingAttack : Ability
			{
				// Homing attack ability
				public HomingAttack(Player player)
				{
					// Set parent player
					m_parent = player;
				}

				internal override bool CheckJumpAbility()
				{
					// Check if we're already homing
					if (m_parent.m_state is Player.Homing)
						return false;

					// Check jump button
					if (m_parent.m_input_jump.m_pressed)
					{
						// Switch to homing state
						m_parent.SetState(new Player.Homing(m_parent));

						// Give homing speed
						Vector3 speed = m_parent.ToSpeed(m_parent.Velocity);
						speed.X = Player.Homing.c_speed;
						m_parent.Velocity = m_parent.FromSpeed(speed);
						return true;
					}
					return false;
				}
			}
		}
	}
}
