using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class LoadingScreenProgressBar : MonoBehaviour
	{
		[Tooltip("Progress bar slider. Value should should be 0-1.")]
		public Slider slider;

		private void Update()
		{
			if (!(slider == null))
			{
				slider.value = ((SaveSystem.currentAsyncOperation != null) ? SaveSystem.currentAsyncOperation.progress : 1f);
			}
		}
	}
}
