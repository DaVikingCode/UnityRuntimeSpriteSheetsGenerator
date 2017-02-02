Unity Runtime SpriteSheets Generator
====================================

Unity and plugins provide many great ways to build Sprite Sheets. However they're used directly into Unity Editor or with an external software which is perfect in many case, but none provide the ability to generate SpriteSheets at runtime.

The RectanglePacking algorithm is a port of the AS3 version made by [Ville Koskela](https://github.com/villekoskelaorg/RectanglePacking). Assets used in the demo come from [Kenney](http://kenney.nl/).

You could combine the generated Sprite Sheets.png with a [pngquant](https://pngquant.org/) compression via this script [PngQuantNativeProcess](https://github.com/DaVikingCode/PngQuantNativeProcess).

Example:
--------
Add the `AssetPacker` component to your GameObject:  
![AssetPacker](http://davikingcode.com/blog/wp-content/uploads/2017/01/AssetPacker.png)
```csharp
using DaVikingCode.AssetPacker;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AssetPackerExample : MonoBehaviour {
	
	public Image anim;

	AssetPacker assetPacker;
	
	void Start () {

		string[] files = Directory.GetFiles(Application.persistentDataPath + "/Textures", "*.png");

		assetPacker = GetComponent<AssetPacker>();

		assetPacker.OnProcessCompleted.AddListener(LaunchAnimations);

		assetPacker.AddTexturesToPack(files);
		assetPacker.Process();
	}

	void LaunchAnimations() {

		StartCoroutine(LoadAnimation());
	}

	IEnumerator LoadAnimation() {

		Sprite[] sprites = assetPacker.GetSprites("walking");

		int j = 0;
		while (j < sprites.Length) {

			anim.sprite = sprites[j++];

			yield return new WaitForSeconds(0.1f);

			if (j == sprites.Length)
				j = 0;
		}
	}
}

```
