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
	public partial class NetPlayer : Node3D
	{
		// Model root node
		private Character.ModelRoot m_modelroot;
		private Transform3D m_modelroot_offset;

		// Net player state
		private Transform3D m_transform;

		// RPC methods
		private void HostRpc_Update(Transform3D transform)
		{
			// Forward the RPC to all clients
			Root.GetHostServer().RpcAll(this, "Rpc_Update", transform);
		}

		private void Rpc_Update(Transform3D transform)
		{
			// Set model root state
			m_transform = transform * m_modelroot_offset;
		}

		// Net player node
		public override void _Ready()
		{
			// Get model root
			m_modelroot = GetNode<Character.ModelRoot>("ModelRoot");
			m_modelroot_offset = GlobalTransform.Inverse() * m_modelroot.GlobalTransform;
			m_transform = GlobalTransform * m_modelroot_offset;

			// Ready base
			base._Ready();
		}

		public override void _PhysicsProcess(double delta)
		{
			// Update model root
			m_modelroot.SetTransform(m_transform);
		}
	}
}
