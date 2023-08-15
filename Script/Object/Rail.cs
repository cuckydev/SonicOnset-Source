using Godot;

namespace SonicOnset
{
	public partial class Rail : Path3D
	{
		Player testplayer = null;

		public override void _Ready()
		{
			base._Ready();
		}

		public override void _Process(double delta)
		{
			base._Process(delta);

			// Get player instance
			if (testplayer == null)
			{
				Node players = GetParent().GetParent().FindChild("Players", true);
				if (players != null)
					testplayer = players.GetNode<Player>("1");
				return;
			}

			// Check if player is touching path
			Vector3 closest = Curve.GetClosestPoint(ToLocal(testplayer.GlobalTransform.Origin));
			Vector3 diff = testplayer.GlobalTransform.Origin - ToGlobal(closest);

			if (diff.Length() < 6.0f)
			{

			}
		}
	}
}
