using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 확장 메소드.
namespace GameObjectExtension {

	static class _List {
		
		// 시작 요소.
		public static T 		front<T>(this List<T> list)
		{
			return(list[0]);
		}

		// 마지막 요소.
		public static T 		back<T>(this List<T> list)
		{
			return(list[list.Count - 1]);
		}
	}

	static class _MonoBehaviour {

		// 포지션을 설정한다.
		public static void 		setPosition(this MonoBehaviour mono, Vector3 position)
		{
			mono.gameObject.transform.position = position;
		}

		// 위치를 가져온다.
		public static Vector3	getPosition(this MonoBehaviour mono)
		{
			return(mono.gameObject.transform.position);
		}

		// 로컬 포지션을 설정한다.
		public static void setLocalPosition(this MonoBehaviour mono, Vector3 local_position)
		{
			mono.gameObject.transform.localPosition = local_position;
		}

		// 로컬 포지션을 설정한다.
		public static void setLocalScale(this MonoBehaviour mono, Vector3 local_scale)
		{
			mono.gameObject.transform.localScale = local_scale;
		}

		// ================================================================ //

		// 부모를 설정한다.
		public static void setParent(this MonoBehaviour mono, GameObject parent)
		{
			if(parent != null) {

				mono.gameObject.transform.parent = parent.transform;

			} else {

				mono.gameObject.transform.parent = null;
			}
		}

		// ================================================================ //
	};

	static class _GameObject {

		// 프리팹으로부터 인스턴스를 생성한다.
		public static GameObject	instantiate(this GameObject prefab)
		{
			return(GameObject.Instantiate(prefab) as GameObject);
		}

		// 자기 자신을 폐기한다.
		public static void		destroy(this GameObject go)
		{
			GameObject.Destroy(go);
		}

		// ================================================================ //

		// 표시/비표시를 설정한다.
		public static void	setVisible(this GameObject go, bool is_visible)
		{
			Renderer[]		renders = go.GetComponentsInChildren<Renderer>();
			
			foreach(var render in renders) {
			
				render.enabled = is_visible;
			}
		}

		// ================================================================ //

		// 포지션을 설정한다.
		public static void 		setPosition(this GameObject go, Vector3 position)
		{
			go.transform.position = position;
		}

		// 위치를 얻는다.
		public static Vector3	getPosition(this GameObject go)
		{
			return(go.transform.position);
		}

		// 로테이션을 설정한다.
		public static void setRotation(this GameObject go, Quaternion rotation)
		{
			go.transform.rotation = rotation;
		}

		// 로컬 포지션을 설정한다.
		public static void setLocalPosition(this GameObject go, Vector3 local_position)
		{
			go.transform.localPosition = local_position;
		}

		// 로컬 스케일을 설정한다.
		public static void setLocalScale(this GameObject go, Vector3 local_scale)
		{
			go.transform.localScale = local_scale;
		}

		// ================================================================ //

		// 부모를 설정한다.
		public static void setParent(this GameObject go, GameObject parent)
		{
			if(parent != null) {

				go.transform.parent = parent.transform;

			} else {

				go.transform.parent = null;
			}
		}

		// 자식 게임오브젝트를 찾는다.
		public static GameObject findChildGameObject(this GameObject go, string child_name)
		{
			GameObject	child_go = null;
			Transform	child    = go.transform.FindChild(child_name);

			if(child != null) {

				child_go = child.gameObject;
			}

			return(child_go);
		}

		// 자식(과 그 이하의 노드)의 게임을 찾는다.
		public static GameObject	findDescendant(this GameObject go, string name)
		{
			GameObject	descendant = null;
	
			descendant = go.findChildGameObject(name);
	
			if(descendant == null) {
	
				foreach(Transform child in go.transform) {
	
					descendant = child.gameObject.findDescendant(name);
	
					if(descendant != null) {
	
						break;
					}
				}
			}
	
			return(descendant);
		}

		// ================================================================ //

		// 머티리얼의 속성 변경(float).
		public static void	setMaterialProperty(this GameObject go, string name, float value)
		{
			SkinnedMeshRenderer[]		renders = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			
			foreach(var render in renders) {
			
				foreach(var material in render.materials) {

					material.SetFloat(name, value);
				}
			}
		}

		// 머티리얼의 속성 변경(Color).
		public static void	setMaterialProperty(this GameObject go, string name, Color color)
		{
			SkinnedMeshRenderer[]		renders = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			
			foreach(var render in renders) {
			
				foreach(var material in render.materials) {

					material.SetColor(name, color);
				}
			}
		}

	}
};
