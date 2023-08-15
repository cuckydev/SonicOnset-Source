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
	// Object trigger class
	public partial class ObjectTriggerInterest : StaticBody3D
	{
		// Exports
		[Export]
		public Node3D m_listener_node;

		[Export]
		public CollisionShape3D m_shape_node;

		// Direct space and shape query
		internal PhysicsDirectSpaceState3D m_directspace;
		internal PhysicsShapeQueryParameters3D m_query;

		// Setup
		public override void _Ready()
		{
			// Get direct space
			m_directspace = GetWorld3D().DirectSpaceState;

			// Setup shape query
			m_query = new PhysicsShapeQueryParameters3D();

			m_query.CollideWithAreas = false;
			m_query.CollideWithBodies = true;

			m_query.Shape = m_shape_node.Shape;
			m_query.ShapeRid = m_shape_node.Shape.GetRid();
			m_query.Exclude.Add(m_shape_node.Shape.GetRid());
		}

		// Query intersections
		public Godot.Collections.Array<Node3D> QueryIntersections()
		{
			// Update shape query
			m_query.CollisionMask = CollisionMask;
			m_query.Transform = m_shape_node.GlobalTransform;

			// Query intersections with shape
			Godot.Collections.Array<Godot.Collections.Dictionary> overlaps = m_directspace.IntersectShape(m_query, 256);
			Godot.Collections.Array<Node3D> nodes = new Godot.Collections.Array<Node3D>();

			foreach (var overlap in overlaps)
			{
				// Get collider node
				Node3D collider_node = (Node3D)overlap["collider"];

				// Check if collider is a trigger
				ObjectTriggerInterest trigger_interest = collider_node as ObjectTriggerInterest;
				if (trigger_interest != null)
					nodes.Add(trigger_interest.m_listener_node);
			}

			// Return nodes
			return nodes;
		}
	}

	// Listener interface
	public interface IObjectTriggerListener
	{
		// Touch event
		void Touch(Node3D other) {}
	}
}
