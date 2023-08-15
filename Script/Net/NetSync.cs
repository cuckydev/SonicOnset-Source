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

namespace SonicOnset.Net
{
	public class NetSync
	{
		// Net sync state
		private string m_scene_path;

		// Net sync functions
		public void SetScene(string scene)
		{
			// Set scene path
			m_scene_path = scene;

			// Load scene globally
			Net.IHostServer host_server = Root.GetHostServer();
			host_server.RpcAll(Root.Singleton(), "Rpc_SetScene", m_scene_path);
		}

		// Net sync join syncing
		public void SyncPeer(int peer)
		{
			// Bring peer to current scene
			Net.IHostServer host_server = Root.GetHostServer();
			host_server.RpcId(peer, Root.Singleton(), "Rpc_SetScene", m_scene_path);
		}
	}
}
