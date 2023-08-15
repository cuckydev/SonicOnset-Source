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
	public class LocalServer : IHostServer
	{
		// Local server
		public LocalServer()
		{
			
		}

		// Returns your own peer ID
		public int GetPeerId()
		{
			// 1 is the default peer ID for the server, so we'll tell the client that we're the server
			return 1;
		}

		// Returns all peer IDs
		public int[] GetPeerIds()
		{
			// Return an array with only your own peer ID
			return new int[] { GetPeerId() };
		}

		// Returns the peer ID coming from the server
		public int GetRemotePeerId()
		{
			// 1 is the default peer ID for the server, so we'll tell the client that it came from ourselves
			return 1;
		}

		// Send RPC to the server
		public void Rpc(Node node, string name, params Variant[] args)
		{
			// Send RPC to the server
			node.Call(name, args);
		}

		// Send RPC to all peers
		public void RpcAll(Node node, string name, params Variant[] args)
		{
			// Directly call node
			node.Call(name, args);
		}

		// Send RPC to a specific peer
		public void RpcId(int peer_id, Node node, string name, params Variant[] args)
		{
			// Directly call node
			node.Call(name, args);
		}

		// Disconnect from the server
		public void Disconnect() { }
	}
}
