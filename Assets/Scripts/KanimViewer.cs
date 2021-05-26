using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Globalization;
using UnityEngine;

public class KanimViewer : MonoBehaviour, System.IDisposable {
	[Range(0,60)]
	public int frameRate = 30;
	public bool paused = false;
	public Material material;
	public Transform spriteParent;

	List<BuildDef> buildDefs = new List<BuildDef>();
	List<AnimDef> animDefs = new List<AnimDef>();

	List<SpriteRenderer> renderers = new List<SpriteRenderer>();
	List<MaterialPropertyBlock> propertyBlocks = new List<MaterialPropertyBlock>();

	private float frameProgress = -1;
	private int frameIdx;
	private AnimDef.Kanim currentKanim;
	public static bool updateFrame = false;

	// Update is called once per frame
	void Update() {
		foreach (var build in buildDefs) {
			build.OnUpdate();
		}
		foreach (var anim in animDefs) {
			anim.OnUpdate();
		}

		if (currentKanim != null) {
			if (!paused) {
				frameProgress += Time.deltaTime * frameRate;
				if (frameProgress >= 1) {
					frameProgress--;
					frameIdx++;
					updateFrame = true;
					if (currentKanim.frames.Length <= frameIdx) {
						frameIdx = 0;
					}
				}
			}

			if (updateFrame) {
				updateFrame = false;

				var currentFrame = currentKanim.frames[frameIdx];
				for (var k = renderers.Count; k < currentFrame.elements.Length; k++) {
					var newObj = new GameObject();
					newObj.transform.parent = spriteParent;
					newObj.transform.localPosition = new Vector3(0, 0, 10);
					var render = newObj.AddComponent<SpriteRenderer>();
					render.sortingOrder = -k;
					render.material = material; ;
					renderers.Add(render);
					propertyBlocks.Add(new MaterialPropertyBlock());
				}

				for (var i = 0; i < renderers.Count; i++) {
					var render = renderers[i];
					if (i < currentFrame.elements.Length) {
						var propertyBlock = propertyBlocks[i];
						var element = currentFrame.elements[i];
						var frame = FindSymbolFrame(element.name, element.frame);
						render.gameObject.name = element.name + ' ' + element.frame;

						if (frame != null) {
							propertyBlock.SetFloat("_a", element.a);
							propertyBlock.SetFloat("_b", -element.b);
							propertyBlock.SetFloat("_c", -element.c);
							propertyBlock.SetFloat("_d", element.d);
							var offX = frame.x - frame.w * 0.5f;
							var offY = frame.y - frame.h * 0.5f;
							propertyBlock.SetFloat("_tx", element.tx + element.a * offX + element.c * offY);
							propertyBlock.SetFloat("_ty", -element.ty - element.b * offX - element.d * offY);
							propertyBlock.SetColor("_CM", element.colorMult);
							propertyBlock.SetColor("_CA", element.colorAdd);
							propertyBlock.SetTexture("_MainTex", frame.sprite.texture);

							render.sprite = frame.sprite;
							render.SetPropertyBlock(propertyBlock);
							render.gameObject.SetActive(true);
						} else {
							render.gameObject.SetActive(false);
						}
					} else {
						render.gameObject.SetActive(false);
					}
				}
			}
		}
	}
	private BuildDef.Symbol.Frame FindSymbolFrame(string symbol, int frameIdx) {
		symbol = symbol.ToLower();
		foreach (var build in buildDefs) {
			if (build.symbols.ContainsKey(symbol) && build.symbols[symbol].frames.Length > frameIdx) {
				return build.symbols[symbol].frames[frameIdx];
			}
		}

		return null;
	}

	public BuildDef LoadBuild(string directory) {
		var buildDef = BuildDef.LoadBuildDef(directory);
		if (buildDef != null) {
			updateFrame = true;
			buildDefs.Add(buildDef);
		}
		return buildDef;
	}

	public AnimDef LoadAnimDef(string directory) {
		var animDef = AnimDef.LoadAnimDef(directory);
		if (animDef != null && animDef.kanims.Length > 0) {
			animDefs.Add(animDef);
			currentKanim = animDef.kanims[0];
			frameIdx = 0;
			frameProgress = -1;
			updateFrame = true;
		}
		return animDef;
	}

	void OnApplicationQuit() {
		Dispose();
	}
	private void OnDestroy() {
		Dispose();
	}

	public void SetFrameRate(int rate) {
		frameRate = rate;
	}

	public void SetFrameIdx(int idx) {
		if (idx < 0) {
			idx = 0;
		} else if (idx >= GetAnimLength()) {
			idx = GetAnimLength() - 1;
		}

		if (idx != frameIdx) {
			updateFrame = true;
			frameIdx = idx;
		}
	}

	public int GetFrameIdx() {
		return frameIdx;
	}

	public int GetAnimLength() {
		if (currentKanim == null)
			return 0;
		return currentKanim.frames.Length;
	}

	public void RemoveBuildDef(BuildDef build) {
		updateFrame = true;
		buildDefs.Remove(build);
		build.Dispose();
	}

	public void RemoveAnimDef(AnimDef anim) {
		updateFrame = true;
		animDefs.Remove(anim);
		anim.Dispose();
	}

	public void SetCurrentKanim(AnimDef.Kanim kanim) {
		updateFrame = true;
		currentKanim = kanim;
		frameIdx = 0;
		frameProgress = -1;
	}

	public AnimDef.Kanim GetCurrentKanim() {
		return currentKanim;
	}

	public void Dispose() {
		foreach (var build in buildDefs) {
			build.Dispose();
		}
		foreach (var anim in animDefs) {
			anim.Dispose();
		}
	}
}
