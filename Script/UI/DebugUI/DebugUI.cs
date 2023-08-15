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

namespace SonicOnset.UI.DebugUI
{
	public partial class DebugUI : CanvasLayer
	{
		// DebugUI nodes
		internal Tree m_tree;
		private TreeItem m_tree_root;

		private CheckButton m_check;

		// DebugUI node
		public override void _Ready()
		{
#if !DEBUG
			// Hide self
			this.Visible = false;
#else
			// Create tree root
			m_tree = GetNode<Tree>("DebugUIRoot/DebugUITree");
			m_tree.Visible = false;

			m_tree_root = m_tree.CreateItem(null);
			m_tree_root.SetText(0, "Debug");

			m_check = GetNode<CheckButton>("DebugUIRoot/DebugCheck");
			m_check.Connect("toggled", new Callable(this, "CheckBox"));
#endif
			// Register singleton
			ProcessPriority = (int)Enum.Priority.PostProcess;
			Engine.RegisterSingleton("DebugUI", this);
		}

		internal static DebugUI Singleton()
		{
			// Get singleton
			return (DebugUI)Engine.GetSingleton("DebugUI");
		}

		public static DebugUIContext AcquireContext(string name)
		{
			return new DebugUIContext(Singleton(), name);
		}

		// Connections
		private void CheckBox(bool toggled)
		{
			// Toggle tree visiblity
			m_tree.Visible = toggled;
		}

		// DebugUI context class
		public class DebugUIContext
		{
			// Parent DebugUI
			private DebugUI m_debug_ui;
			private TreeItem m_tree_item;

			// Debug UI context class
			public DebugUIContext(DebugUI debug_ui, string name)
			{
#if DEBUG
				// Create item in tree
				m_debug_ui = debug_ui;

				m_tree_item = m_debug_ui.m_tree.CreateItem(m_debug_ui.m_tree_root);
				m_tree_item.SetText(0, name);
#endif
			}

			~DebugUIContext()
			{
#if DEBUG
				// Remove item from tree
				m_tree_item.Free();
				m_tree_item = null;
#endif
			}

			// Context control
			public void SetItems(List<string> items)
			{
#if DEBUG
				// Update children
				int children_to_update = Mathf.Min(m_tree_item.GetChildCount(), items.Count);
				int children_to_add = items.Count - children_to_update;

				for (int i = 0; i < children_to_add; i++)
					m_debug_ui.m_tree.CreateItem(m_tree_item);
				for (int i = 0; i < items.Count; i++)
					m_tree_item.GetChild(i).SetText(0, items[i]);
				for (int j = m_tree_item.GetChildCount() - 1; j >= items.Count; j--)
					m_tree_item.GetChild(j).Free();
#endif
			}
		}
	}
}
