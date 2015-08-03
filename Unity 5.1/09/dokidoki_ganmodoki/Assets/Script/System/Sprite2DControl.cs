using UnityEngine;
using System.Collections;
using MathExtension;

// 2D 스프라이트.
public class Sprite2DControl : MonoBehaviour {
	
	protected Vector2		size = Vector2.zero;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 위치 설정.
	public void	setPosition(Vector2 position)
	{
		this.transform.localPosition = new Vector3(position.x, position.y, this.transform.localPosition.z);
	}

	// 위치 획득.
	public Vector2	getPosition()
	{
		return(this.transform.localPosition.xy());
	}

	// 깊이값 설정.
	public void	setDepth(float depth)
	{
		Vector3		position = this.transform.localPosition;

		position.z = depth;

		this.transform.localPosition = position;
	}

	// 깊이값 획득.
	public float	getDepth()
	{
		return(this.transform.localPosition.z);
	}

	// [degree] 앵글(Z축 주변을 회전)을 설정한다.
	public void	setAngle(float angle)
	{
		this.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	// 스케일을 설정한다.
	public void	setScale(Vector2 scale)
	{
		this.transform.localScale = new Vector3(scale.x, scale.y, 1.0f);
	}
	
	// 사이즈를 설정한다.
	public void		setSize(Vector2 size)
	{
		Sprite2DRoot.get().setSizeToSprite(this, size);
	}
	// 사이즈를 획득한다.
	public Vector2 getSize()
	{
		return(this.size);
	}
	
	// 정점 색상 설정.
	public void		setVertexColor(Color color)
	{
		Sprite2DRoot.get().setVertexColorToSprite(this, color);
	}

	// 정점 색상의 알파값 설정.
	public void		setVertexAlpha(float alpha)
	{
		Sprite2DRoot.get().setVertexColorToSprite(this, new Color(1.0f, 1.0f, 1.0f, alpha));
	}

	// 텍스처 설정.
	public void		setTexture(Texture texture)
	{
		this.GetComponent<MeshRenderer>().material.mainTexture = texture;
	}
	// 텍스처 설정(크기도 변경).
	public void		setTextureWithSize(Texture texture)
	{
		this.GetComponent<MeshRenderer>().material.mainTexture = texture;
		Sprite2DRoot.get().setSizeToSprite(this, new Vector2(texture.width, texture.height));
	}

	// 텍스처 획득.
	public Texture	getTexture()
	{
		return(this.GetComponent<MeshRenderer>().material.mainTexture);
	}

	// 머티리얼 설정..
	public void		setMaterial(Material material)
	{
		this.GetComponent<MeshRenderer>().material = material;
	}
	// 머티리얼 획득.
	public Material		getMaterial()
	{
		return(this.GetComponent<MeshRenderer>().material);
	}

	// 좌우/상하 반전.
	public void		setFlip(bool horizontal, bool vertical)
	{
		Vector2		scale  = Vector2.one;
		Vector2		offset = Vector2.zero;

		if(horizontal) {

			scale.x  = -1.0f;
			offset.x = 1.0f;
		}
		if(vertical) {

			scale.y = -1.0f;
			offset.y = 1.0f;
		}

		this.GetComponent<MeshRenderer>().material.mainTextureScale  = scale;
		this.GetComponent<MeshRenderer>().material.mainTextureOffset = offset;
	}

	// 포인트가 스프라이트 상에 있는가?.
	public bool		isContainPoint(Vector2 point)
	{
		bool	ret = false;

		Vector2		position = this.transform.localPosition.xy();
		Vector2		scale    = this.transform.localScale.xy();

		do {

			if(point.x < position.x - this.size.x/2.0f*scale.x || position.x + this.size.x/2.0f*scale.x < point.x) {

				break;
			}
			if(point.y < position.y - this.size.y/2.0f*scale.y || position.y + this.size.y/2.0f*scale.y < point.y) {

				break;
			}

			ret = true;

		} while(false);

		return(ret);
	}

	// 표시/비표시 설정.
	public void		setVisible(bool is_visible)
	{
		this.GetComponent<MeshRenderer>().enabled = is_visible;
	}
	// 표시 중?.
	public bool		isVisible()
	{
		return(this.GetComponent<MeshRenderer>().enabled);
	}

	// 정점 위치 획득.
	public Vector3[]	getVertexPositions()
	{
		return(Sprite2DRoot.get().getVertexPositionsFromSprite(this));
	}

	// 정점 위치를 설정한다(2D).
	public void		setVertexPositions(Vector2[] positions)
	{
		Vector3[]		positions_3d = new Vector3[positions.Length];

		for(int i = 0;i < positions.Length;i++) {

			positions_3d[i] = positions[i];
		}
		Sprite2DRoot.get().setVertexPositionsToSprite(this, positions_3d);
	}

	// 정점 위치를 설정한다(3D)
	public void		setVertexPositions(Vector3[] positions)
	{
		Sprite2DRoot.get().setVertexPositionsToSprite(this, positions);
	}

	// 메시의 정점 수 획득.
	public int		getDivCount()
	{
		return(this.div_count);
	}

	// 폐기한다.
	public void		destroy()
	{
		GameObject.Destroy(this.gameObject);
	}

	public void		setParent(Sprite2DControl parent)
	{
		this.transform.parent = parent.transform;
	}

	// ================================================================ //
	// Sprite2DRoot 용.

	// 크기를 설정한다.
	public void	internalSetSize(Vector2 size)
	{
		this.size = size;
	}

	protected int	div_count = 2;

	public void	internalSetDivCount(int div_count)
	{
		this.div_count = div_count;
	}

}
