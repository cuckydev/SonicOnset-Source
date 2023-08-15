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
	public class RemoteServer : IServer
	{
		// Multiplayer API
		private MultiplayerApi m_multiplayer_api;

		// Remote server
		public RemoteServer(MultiplayerApi multiplayer_api, string ip, int port)
		{
			// Connect multiplayer peer
			ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
			peer.CreateClient(ip, port);

			// Setup multiplayer API
			m_multiplayer_api = multiplayer_api;
			m_multiplayer_api.MultiplayerPeer = peer;
		}

		// Returns your own peer ID
		public int GetPeerId() => m_multiplayer_api.GetUniqueId();

		// Returns all peer IDs
		public int[] GetPeerIds() => m_multiplayer_api.GetPeers();

		// Returns the peer ID coming from the server
		public int GetRemotePeerId() => m_multiplayer_api.GetRemoteSenderId();

		// Send RPC to the server
		public void Rpc(Node node, string name, params Variant[] args)
		{
			// Forward RPC to the server root
			Variant[] forward = { Root.Singleton().GetPathTo(node), name, new Godot.Collections.Array(args) };
			m_multiplayer_api.Rpc(1, Root.Singleton(), "Rpc_ServerForward", new Godot.Collections.Array(forward));
		}

		// Disconnect
		public void Disconnect()
		{
			// Disconnect from the server
			m_multiplayer_api.MultiplayerPeer.Close();

			// Remove multiplayer peer
			m_multiplayer_api.MultiplayerPeer = null;
			m_multiplayer_api = null;
		}
	}
}
