using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllicornsPropertyTransferTool
{
	public class PTDemoSrc : MonoBehaviour
	{
		[SerializeField]
		private GameObject gameObject1;

		[SerializeField]
		private GameObject gameObject2;

		[SerializeField]
		private List<GameObject> gameObjectList;

		public Transform transform1;

		public Transform transform2;

		public List<Transform> transformList;
	}
}