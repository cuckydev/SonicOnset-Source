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
	public partial class ModelRoot : Node3D
	{
		// ModelRoot state
		private Transform3D m_from, m_to;

		// ModelRoot functions
		public override void _Ready()
		{
			// Initial state
			m_from = GlobalTransform;
			m_to = GlobalTransform;

			// Setup base
			base._Ready();
		}

		public override void _Process(double delta)
		{
			float fraction = (float)Engine.GetPhysicsInterpolationFraction();
			GlobalTransform = m_from.InterpolateWith(m_to, fraction);
		}

		public void SetTransform(Transform3D transform)
		{
			m_from = m_to;
			m_to = m_to.InterpolateWith(transform, 0.4f);
			m_to.Origin = transform.Origin;
		}
	}
}
