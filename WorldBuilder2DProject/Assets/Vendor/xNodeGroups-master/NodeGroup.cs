using dr4g0nsoul.WorldBuilder2D.Util;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.NodeGroups {
	[CreateNodeMenu("Levels/Create World")]
	public class NodeGroup : UniqueNode {
		public int width = 400;
		public int height = 400;
		public Color accentColor = new Color(1f, 1f, 1f, 1f);

		public override object GetValue(NodePort port) {
			return null;
		}

		/// <summary> Gets nodes in this group </summary>
		public List<Node> GetNodes() {
			List<Node> result = new List<Node>();
			foreach (Node node in graph.nodes) {
				if (node == this) continue;
				if (node.position.x < this.position.x) continue;
				if (node.position.y < this.position.y) continue;
				if (node.position.x > this.position.x + width) continue;
				if (node.position.y > this.position.y + height + 30) continue;
				result.Add(node);
			}
			return result;
		}


		///////////////////
		/// World stuff ///
		///////////////////

		public string worldName;
		[TextArea(4, 10)] public string worldDescription;

		//Preferred Items
		public PreferredItems preferredItems;

		//Debug
		[ReadOnly] public string assignedSceneName;
		[ReadOnly] public string assignedScenePath;

	}
}