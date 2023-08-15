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
	public partial class CloneBenchmark : Node
	{
		// Node to clone
		[Export]
		public PackedScene m_node_to_clone = null;
		[Export]
		public int m_clone_count = 1000;
		[Export]
		public float m_range = 1000.0f;

		// Node setup
		public override void _Ready()
		{
			for (int i = 0; i < m_clone_count; i++)
			{
				// Clone ring
				Node3D clone = (Node3D)m_node_to_clone.Instantiate();

				// Give random position
				clone.Translate(new Vector3(GD.Randf() * m_range - m_range * 0.5f, GD.Randf() * m_range, GD.Randf() * m_range - m_range * 0.5f));

				// Add clone to scene
				AddChild(clone);
			}

			// Setup base
			base._Ready();
		}
	}
}
