namespace NewFrontier.scripts.Model;

public class WormholeObject {
	private readonly GameNode _node1;
	private readonly GameNode _node2;
	public float Distance;

	public WormholeObject(GameNode node1, GameNode node2, float distance) {
		_node1 = node1;
		_node2 = node2;
		_node1.HasWormhole = true;
		_node2.HasWormhole = true;
		Distance = distance;
	}

	public bool IsConnected(GameNode node) {
		return _node1.Equals(node) || _node2.Equals(node);
	}

	public GameNode GetOtherNode(GameNode node) {
		return _node1.Equals(node) ? _node2 : _node1;
	}
}
