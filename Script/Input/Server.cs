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
using System.Collections.Generic;
using System.Linq;

namespace SonicOnset.Input
{
	// Input server singleton
	// Due to some bugs in Godot's input mapping, we have to implement our own input server for processing the InputMap
	public partial class Server : Godot.Node
	{
		// Input state
		private Dictionary<Key, bool> m_key = new Dictionary<Key, bool>();
		private Dictionary<MouseButton, bool> m_mouse_button = new Dictionary<MouseButton, bool>();

		private float[] m_joy_axis = new float[(int)JoyAxis.Max];
		private bool[] m_joy_button = new bool[(int)JoyButton.Max];

		// Cached mapped input state
		private struct CachedAction
		{
			// Action information
			public Godot.Collections.Array<InputEvent> m_events;
			public float m_deadzone;

			// Action state
			public float m_value;

			public float m_raw_value;
			public float m_joy_value;
		}
		private Dictionary<StringName, CachedAction> m_cached_actions = new Dictionary<StringName, CachedAction>();

		// Input vectors
		private Vector2 m_move_vector = new Vector2();
		private Vector2 m_look_vector = new Vector2();

		// Singleton
		public override void _Ready()
		{
			// Cache actions
			CacheActions();

			// Register singleton
			ProcessPriority = (int)Enum.Priority.PreProcess;
			Engine.RegisterSingleton("InputServer", this);
		}

		internal static Server Singleton()
		{
			// Get singleton
			return (Server)Engine.GetSingleton("InputServer");
		}

		// Input state event
		public override void _Input(InputEvent input_event)
		{
			// Process keys
			if (input_event is InputEventKey key)
				m_key[key.PhysicalKeycode] = key.Pressed;

			// Process mouse buttons
			if (input_event is InputEventMouseButton mouse_button)
				m_mouse_button[mouse_button.ButtonIndex] = mouse_button.Pressed;

			// Process joypad axes
			if (input_event is InputEventJoypadMotion joypad_motion)
				m_joy_axis[(int)joypad_motion.Axis] = joypad_motion.AxisValue;

			// Process joypad buttons
			if (input_event is InputEventJoypadButton joypad_button)
				m_joy_button[(int)joypad_button.ButtonIndex] = joypad_button.Pressed;
		}

		// Input processing
		public override void _Process(double delta)
		{
			// Process actions
			ProcessActions();
		}

		// Internal input functions
		public void CacheActions()
		{
			// Get all action names
			Godot.Collections.Array<StringName> actions = InputMap.GetActions();

			// Reset cached actions
			m_cached_actions.Clear();

			// Setup new actions
			foreach (var action_name in actions)
			{
				// Setup cached
				CachedAction cached = new CachedAction();

				cached.m_events = InputMap.ActionGetEvents(action_name);
				cached.m_deadzone = InputMap.ActionGetDeadzone(action_name);

				cached.m_value = 0.0f;

				m_cached_actions[action_name] = cached;
			}

			// Process actions so that they are updated
			ProcessActions();
		}

		private void ProcessActions()
		{
			// Process cached actions
			foreach (var key in m_cached_actions.Keys.ToList())
			{
				// Get cached action
				CachedAction cached = m_cached_actions[key];

				// Process events
				float final_value = 0.0f;
				
				float raw_value = 0.0f;
				float joy_value = 0.0f;

				float deadzone = cached.m_deadzone;

				foreach (var input_event in cached.m_events)
				{
					// Get value
					float value = input_event switch
					{
						InputEventJoypadMotion joypad_motion => m_joy_axis[(int)joypad_motion.Axis] * joypad_motion.AxisValue,
						_ => GetEventValue(input_event)
					};
					{
						if (input_event is InputEventJoypadMotion joypad_motion)
							joy_value += m_joy_axis[(int)joypad_motion.Axis] * joypad_motion.AxisValue;
						else
							raw_value += GetEventValue(input_event);
					}

					// Apply deadzone and add
					if (value > deadzone)
						final_value += (value - deadzone) / (1.0f - deadzone);
				}

				// Set value
				cached.m_value = Mathf.Min(final_value, 1.0f);

				cached.m_raw_value = raw_value;
				cached.m_joy_value = joy_value;

				// Set cached action
				m_cached_actions[key] = cached;
			}

			// Get vectors
			m_move_vector = GetVectorValue("move_left", "move_right", "move_up", "move_down");
			m_look_vector = GetVectorValue("look_left", "look_right", "look_up", "look_down");
		}

		private float GetEventValue(Godot.InputEvent input_event)
		{
			if (input_event is InputEventKey key)
				return m_key.GetValueOrDefault(key.PhysicalKeycode) ? 1.0f : 0.0f;
			if (input_event is InputEventMouseButton mouse_button)
				return m_mouse_button.GetValueOrDefault(mouse_button.ButtonIndex) ? 1.0f : 0.0f;
			if (input_event is InputEventJoypadMotion joypad_motion)
				return m_joy_axis[(int)joypad_motion.Axis];
			if (input_event is InputEventJoypadButton joypad_button)
				return m_joy_button[(int)joypad_button.ButtonIndex] ? 1.0f : 0.0f;
			return 0.0f;
		}

		private static Vector2 DeadzoneVector(Vector2 vector, float deadzone)
		{
			float length = vector.Length();
			if (length < deadzone)
				return Vector2.Zero;
			else if (length < 1.0f)
				return vector * ((length - deadzone) / (1.0f - deadzone));
			else
				return vector.Normalized();
		}

		private Vector2 GetVectorValue(string left_name, string right_name, string up_name, string down_name)
		{
			// Get raw and joy vectors
			CachedAction cached_left = m_cached_actions[left_name];
			CachedAction cached_right = m_cached_actions[right_name];
			CachedAction cached_up = m_cached_actions[up_name];
			CachedAction cached_down = m_cached_actions[down_name];
			float deadzone = cached_left.m_deadzone;

			Vector2 raw_vector = DeadzoneVector(new Vector2(
				cached_right.m_raw_value - cached_left.m_raw_value,
				cached_up.m_raw_value - cached_down.m_raw_value
			), deadzone);
			Vector2 joy_vector = DeadzoneVector(new Vector2(
				cached_right.m_joy_value - cached_left.m_joy_value,
				cached_up.m_joy_value - cached_down.m_joy_value
			), deadzone);

			// Get final vector
			Vector2 vector = raw_vector + joy_vector;
			if (vector.LengthSquared() > 1.0f)
				vector = vector.Normalized();
			return vector;
		}

		// Input functions
		public static Vector2 GetMoveVector()
		{
			// Return move vector
			return Singleton().m_move_vector;
		}

		public static Vector2 GetLookVector()
		{
			// Return look vector
			return Singleton().m_look_vector;
		}

		public static bool GetButton(string name)
		{
			// Return mapped button
			return Singleton().m_cached_actions[name].m_value > 0.0f;
		}
	}
}
