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
	public interface IServer
	{
		// Get RPC peers
		public int GetPeerId(); // Returns your own peer ID
		public int[] GetPeerIds(); // Returns all peer IDs

		public int GetRemotePeerId(); // Returns the peer ID coming from the server

		// Send RPC functions
		public void Rpc(Node node, string name, params Variant[] args); // Send RPC to the server

		// Disconnect
		public void Disconnect(); // Disconnect from the server
	}

	public interface IHostServer : IServer
	{
		// Send RPC functions
		public void RpcAll(Node node, string name, params Variant[] args); // Send RPC to all peers
		public void RpcId(int peer_id, Node node, string name, params Variant[] args); // Send RPC to a specific peer
	}
}
