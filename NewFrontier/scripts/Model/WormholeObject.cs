using NewFrontier.scripts.helpers;

namespace NewFrontier.scripts.Model;

public class WormholeObject {
	private readonly GameNode _node1;
	private readonly GameNode _node2;
	public SectorJumpGateStatus SectorJumpGateStatus;
	public bool Highlighted = false;

	public WormholeObject(GameNode node1, GameNode node2) {
		_node1 = node1;
		_node2 = node2;
	}

	public byte GetNode1Sector => MapHelpers.GetSectorIndexFromOffset(_node1.Position);
	public byte GetNode2Sector => MapHelpers.GetSectorIndexFromOffset(_node2.Position);

	public bool IsConnected(GameNode gameNode) {
		return gameNode == _node1 || gameNode == _node2;
	}

	public GameNode GetOtherNode(GameNode gameNode) {
		return gameNode == _node1 ? _node2 : _node1;
	}
}
