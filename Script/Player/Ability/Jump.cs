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
			public partial class Jump : Ability
			{
				// Jump ability
				public Jump(Player player)
				{
					// Set parent player
					m_parent = player;
				}

				internal override bool CheckJump()
				{
					// Check jump button
					if (m_parent.m_input_jump.m_pressed)
					{
						// Switch to jump state
						m_parent.SetState(new Player.Jump(m_parent));
						m_parent.m_status.m_grounded = false;

						// Play jump sound
						m_parent.PlaySound("Jump");

						// Jump off floor
						m_parent.Velocity = Util.Vector3.PlaneProject(m_parent.Velocity, m_parent.GetUp());
						m_parent.Velocity += m_parent.FromSpeed(Vector3.Up * m_parent.m_param.m_jump_speed);
						return true;
					}
					return false;
				}
			}
		}
	}
}
