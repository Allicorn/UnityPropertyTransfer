using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllicornsPropertyTransferTool
{
	public class PropertyTransfer : MonoBehaviour
	{
		[Header("Source")]
		public GameObject source;
		public string sourceType;

		[Header("Destination")]
		public GameObject destination;
		public string destinationType;

	}
}