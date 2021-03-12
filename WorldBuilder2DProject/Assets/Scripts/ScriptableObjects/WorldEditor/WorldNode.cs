using dr4g0nsoul.WorldBuilder2D.Util;
using dr4g0nsoul.WorldBuilder2D.WorldEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace dr4g0nsoul.WorldBuilder2D.WorldEditor {
	[CreateNodeMenu("Levels/Create World")]
	public class WorldNode : UniqueNode {
		public int width = 400;
		public int height = 400;

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
		public Color accentColor = new Color(1f, 1f, 1f, 1f);
		[TextArea(4, 10)] public string worldDescription;
		public string[] levels;

		//Preferred Items
		public PreferredItems preferredItems;

		//Debug
		[ReadOnly] public string assignedSceneName;
		[ReadOnly] public string assignedScenePath;

	}
}