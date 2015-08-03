using UnityEngine;
using System.Collections;

public class EventDataIceAtari : MonoBehaviour {

	public Texture		texture_bikkuri = null;			// "！" 마크 말풍선.
	public Texture		texture_ice     = null;
	public Texture		texture_ice_bar = null;
	public Texture		texture_atari   = null;			// 「当たり！」 텍스처.

	public GameObject	prefab_ice_atari     = null;
	public GameObject	prefab_ice_atari_bar = null;	// 「当たり！」아이스 바.

	public Material		material_ice_sprite = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}

	void	Update()
	{
	}
}
