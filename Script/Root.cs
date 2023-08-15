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

namespace SonicOnset
{
	public partial class Root : Godot.Node
	{
		// Game constants
		public const uint c_tick_rate = 60; // Tick rate for physics calculations

		// Global clock
		private ulong m_clock = 0;

		// Scene loader
		[Godot.Export]
		private Godot.PackedScene m_load_scene;
		private UI.LoadUI.LoadUI m_load_scene_node = null;

		private string m_loading_scene = null;
		private string m_next_scene = null;

		private Godot.Node m_scene = null;

		private Godot.Node m_players = null;

		// Net server
		private Godot.MultiplayerApi m_multiplayer_api = null;

		private Net.IServer m_server = null;
		private Net.NetSync m_netsync = null;

		// Root singleton
		public override void _EnterTree()
		{
			// Create multiplayer API
			m_multiplayer_api = Godot.MultiplayerApi.CreateDefaultInterface();
			GetTree().SetMultiplayer(m_multiplayer_api);

			// Connect to players joining
			m_multiplayer_api.Connect("peer_connected", new Godot.Callable(this, "Rpc_PeerConnected"));
			m_multiplayer_api.Connect("peer_disconnected", new Godot.Callable(this, "Rpc_PeerDisconnected"));

			// Register singleton
			ProcessPriority = (int)Enum.Priority.Root;
			Godot.Engine.RegisterSingleton("Root", this);
		}

		public override void _ExitTree()
		{
			// Close server
			DisconnectServer();
		}

		internal static Root Singleton()
		{
			// Get singleton
			return (Root)Godot.Engine.GetSingleton("Root");
		}

		// Load scene
		private void LoadScene(string scene_path)
		{
			// Begin loading new scene
			m_next_scene = scene_path;
		}

		private void SpawnPlayer()
		{
			// Instantiate player
			Godot.PackedScene player_scene = (Godot.PackedScene)Godot.ResourceLoader.Load("res://Prefab/Character/Sonic/Player.tscn");
			Godot.Node player = player_scene.Instantiate();
			player.Name = m_server.GetPeerId().ToString();

			// Add player to scene
			m_players.AddChild(player);
		}

		private void SpawnPeer(int peer_id)
		{
			// Instantiate player
			Godot.PackedScene player_scene = (Godot.PackedScene)Godot.ResourceLoader.Load("res://Prefab/Character/Sonic/NetPlayer.tscn");
			Godot.Node player = player_scene.Instantiate();
			player.Name = peer_id.ToString();

			// Add player to scene
			m_players.AddChild(player);
		}

		private void SpawnPeers()
		{
			// Go through all peers
			int client_id = m_server.GetPeerId();
			foreach (int peer_id in m_server.GetPeerIds())
				if (peer_id != client_id)
					SpawnPeer(peer_id);
		}

		private void DespawnPeer(int peer_id)
		{
			if (m_players != null)
			{
				// Remove player
				Godot.Node player = m_players.GetNodeOrNull(peer_id.ToString());
				if (player != null)
					player.QueueFree();
			}
		}

		private void SetupScene()
		{
			// Check if scene contains a players node
			m_players = m_scene.GetNodeOrNull("Players");

			// Spawn players
			if (m_players != null)
			{
				SpawnPlayer();
				SpawnPeers();
			}
		}

		// Root node
		public override void _Ready()
		{
			// Load scene
			LoadScene("res://Scene/Menu/Menu.tscn");
		}

		public override void _PhysicsProcess(double delta)
		{
			// Increment clock
			m_clock++;
		}

		public override void _Process(double delta)
		{
			while (m_next_scene != null || m_loading_scene != null)
			{
				// Check if we should start loading a scene
				if (m_next_scene != null && m_loading_scene == null)
				{
					// Unload scene
					if (m_scene != null)
					{
						m_scene.QueueFree();
						m_scene = null;
					}

					// Load UI
					m_load_scene_node = (UI.LoadUI.LoadUI)m_load_scene.Instantiate();
					AddChild(m_load_scene_node);

					// Begin loading scene
					m_loading_scene = m_next_scene;
					m_next_scene = null;
					Godot.ResourceLoader.LoadThreadedRequest(m_loading_scene);
				}

				// Check if we are loading a scene
				if (m_loading_scene != null)
				{
					// Check if scene is loaded
					Godot.Collections.Array progress = new Godot.Collections.Array();
					switch (Godot.ResourceLoader.LoadThreadedGetStatus(m_loading_scene, progress))
					{
						case Godot.ResourceLoader.ThreadLoadStatus.InProgress:
							// Update progress
							m_load_scene_node.SetProgress((float)progress[0]);
							return;
						case Godot.ResourceLoader.ThreadLoadStatus.Loaded:
							// Get scene
							Godot.PackedScene packed_scene = (Godot.PackedScene)Godot.ResourceLoader.LoadThreadedGet(m_loading_scene);
							m_loading_scene = null;

							if (m_next_scene == null)
							{
								// Unload UI
								m_load_scene_node.Free();
								m_load_scene_node = null;

								// Instantiate scene
								m_scene = packed_scene.Instantiate();
								AddChild(m_scene);

								// Setup scene
								SetupScene();
								return;
							}
							break;
						default:
							// Failed to load scene
							Godot.GD.PushError("Failed to load scene: " + m_loading_scene);
							m_loading_scene = null;
							return;
					}
				}
			}
		}

		// Get clock
		public static ulong GetClock()
		{
			return Singleton().m_clock;
		}

		// Net server classes
		public static Net.IServer GetServer()
		{
			return Singleton().m_server;
		}

		public static Net.IHostServer GetHostServer()
		{
			return Singleton().m_server as Net.IHostServer;
		}

		public static Net.NetSync GetNetSync()
		{
			return Singleton().m_netsync;
		}

		public static void Rpc(Godot.Node node, string name, params Godot.Variant[] args) => GetServer().Rpc(node, name, args);

		// Net server connection
		public void StartLocalServer()
		{
			// Start local server
			DisconnectServer();
			m_server = new Net.LocalServer();

			// Create net sync
			m_netsync = new Net.NetSync();
		}

		public void StartHostServer(int port, int max_clients)
		{
			// Start host server
			DisconnectServer();
			m_server = new Net.HostServer(m_multiplayer_api, port, max_clients);

			// Create net sync
			m_netsync = new Net.NetSync();
		}

		public void JoinServer(string ip, int port)
		{
			// Start remote server
			DisconnectServer();
			m_server = new Net.RemoteServer(m_multiplayer_api, ip, port);
		}

		public void DisconnectServer()
		{
			if (m_server != null)
			{
				// Disconnect server
				m_server.Disconnect();
				m_server = null;

				// Destroy net sync
				if (m_netsync != null)
					m_netsync = null;

				// Send us back to the main menu
				LoadScene("res://Scene/NetTest/NetTest.tscn");
			}
		}

		// RPC methods
		private void Rpc_PeerConnected(int id)
		{
			// Check if we're the host
			Net.IHostServer host_server = GetHostServer();
			if (host_server != null)
			{
				// Sync peer
				Net.NetSync net_sync = GetNetSync();
				net_sync.SyncPeer(id);
			}

			// Spawn peer if not client
			if (m_players != null)
			{
				if (id != m_server.GetPeerId())
					SpawnPeer(id);
			}
		}

		private void Rpc_PeerDisconnected(int id)
		{
			// If this peer is the host, disconnect
			if (id == 1)
				DisconnectServer();

			// Remove peer
			if (m_players != null)
				DespawnPeer(id);
		}

		[Godot.Rpc(Godot.MultiplayerApi.RpcMode.Authority)]
		private void Rpc_SetScene(string scene)
		{
			// Load scene
			LoadScene(scene);
		}

		// RPC forwarding
		// We avoid using RPC on nodes directly, as this can cause issues with RPC sync
		[Godot.Rpc(Godot.MultiplayerApi.RpcMode.AnyPeer)]
		public void Rpc_ServerForward(Godot.NodePath path, string name, Godot.Collections.Array args)
		{
			// Call method
			Godot.Node node = GetNodeOrNull(path);
			if (node != null && node.HasMethod(name))
				node.Callv(name, args);
		}

		[Godot.Rpc(Godot.MultiplayerApi.RpcMode.Authority)]
		public void Rpc_ClientForward(Godot.NodePath path, string name, Godot.Collections.Array args)
		{
			// Call method
			Godot.Node node = GetNodeOrNull(path);
			if (node != null && node.HasMethod(name))
				node.Callv(name, args);
		}
	}
}
