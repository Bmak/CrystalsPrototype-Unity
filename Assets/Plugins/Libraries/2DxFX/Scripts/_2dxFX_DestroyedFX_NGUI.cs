// OctoBox, Created by Anton Torkhov 

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("2DxFX/Standard/DestroyedFX_NGUI")]
[System.Serializable]
public class _2dxFX_DestroyedFX_NGUI : MonoBehaviour
{
	[HideInInspector] 
	public Material ForceMaterial;
	[HideInInspector] 
	public bool ActiveChange = true;
	private string shader = "2DxFX/Standard/DestroyedFX";
	[HideInInspector] [Range(0, 1)]
	public float _Alpha = 1f;

	[HideInInspector] [Range(0.001f, 1)]
	public float Seed = 1.0f;
	[HideInInspector] [Range(0, 1)] 
	public float Destroyed = 0.5f;

	[HideInInspector] 
	public int ShaderChange = 0;

	private Material tempMaterial = null;
	private Material defaultMaterial = null;
	private UI2DSprite _sprite = null;

	
	void Awake()
	{
		_sprite = gameObject.GetComponent<UI2DSprite>();
	}

	void Start()
	{  
		ShaderChange = 0;
	}

	public void CallUpdate()
	{
		Update();
	}

	void Update()
	{
		if (_sprite == null) {
			_sprite = gameObject.GetComponent<UI2DSprite>();
		}

		if ((ShaderChange == 0) && (ForceMaterial != null)) {
			ShaderChange = 1;
			if (tempMaterial != null) {
				DestroyImmediate(tempMaterial);
			}
			if (_sprite != null) {
				_sprite.material = ForceMaterial;
			}
			ForceMaterial.hideFlags = HideFlags.None;
			ForceMaterial.shader = Shader.Find(shader);
		}

		if ((ForceMaterial == null) && (ShaderChange == 1)) {
			if (tempMaterial != null) {
				DestroyImmediate(tempMaterial);
			}
			tempMaterial = new Material(Shader.Find(shader));
			tempMaterial.hideFlags = HideFlags.None;
			if (_sprite != null) {
				_sprite.material = tempMaterial;
			}
			ShaderChange = 0;
		}
		
		#if UNITY_EDITOR
		string dfname = "";
		if (_sprite != null && _sprite.material == null) {
			dfname = "Sprites/Default";
		}
		if (dfname == "Sprites/Default") {
			ForceMaterial.shader = Shader.Find(shader);
			ForceMaterial.hideFlags = HideFlags.None;
			if (_sprite != null && _sprite.material == null) {
				_sprite.material = ForceMaterial;
			}
		}
		#endif

		if (ActiveChange && _sprite != null) {
			_sprite.material.SetFloat("_Alpha", 1 - _Alpha);
			_sprite.material.SetFloat("_Distortion", Destroyed);
			_sprite.material.SetFloat("_Size", Seed);
			if (_sprite.panel != null) {
				_sprite.panel.RebuildAllDrawCalls();
			}
		}
	}

	void OnDestroy()
	{
		if (this.gameObject.GetComponent<UI2DSprite>() != null) {
			if (_sprite == null) {
				_sprite = this.gameObject.GetComponent<UI2DSprite>();
			}
		}
		if ((Application.isPlaying == false) && (Application.isEditor == true)) {			
			if (tempMaterial != null) {
				DestroyImmediate(tempMaterial);
			}
			
			if (gameObject.activeSelf && defaultMaterial != null) {
				if (_sprite != null) {
					_sprite.material = defaultMaterial;
					_sprite.material.hideFlags = HideFlags.None;
				}
			}	
		}
	}

	void OnDisable()
	{ 
		if (this.gameObject.GetComponent<UI2DSprite>() != null) {
			if (_sprite == null) {
				_sprite = gameObject.GetComponent<UI2DSprite>();
			}
		} 
		if (gameObject.activeSelf && defaultMaterial != null) {
			if (_sprite != null) {
				_sprite.material = defaultMaterial;
				_sprite.material.hideFlags = HideFlags.None;
			}
		}		
	}

	void OnEnable()
	{
		if (this.gameObject.GetComponent<UI2DSprite>() != null) {
			if (_sprite == null) {
				_sprite = gameObject.GetComponent<UI2DSprite>();
			}
		} 
		if (defaultMaterial == null) {
			defaultMaterial = new Material(Shader.Find("Sprites/Default"));
		}
		if (ForceMaterial == null) {
			ActiveChange = true;
			tempMaterial = new Material(Shader.Find(shader));
			tempMaterial.hideFlags = HideFlags.None;
			if (_sprite != null) {
				_sprite.material = tempMaterial;
			}
		} else {
			ForceMaterial.shader = Shader.Find(shader);
			ForceMaterial.hideFlags = HideFlags.None;
			if (_sprite != null) {
				_sprite.material = ForceMaterial;
			}
		}
		
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(_2dxFX_DestroyedFX_NGUI)),CanEditMultipleObjects]
public class _2dxFX_DestroyedFX_NGUI_Editor : Editor
{
	private SerializedObject m_object;

	public void OnEnable()
	{	
		m_object = new SerializedObject(targets);
	}

	public override void OnInspectorGUI()
	{
		m_object.Update();
		DrawDefaultInspector();
		
		_2dxFX_DestroyedFX_NGUI _2dxScript = (_2dxFX_DestroyedFX_NGUI)target;
	
		EditorGUILayout.PropertyField(m_object.FindProperty("ForceMaterial"), new GUIContent("Shared Material", "Use a unique material, reduce drastically the use of draw call"));
		
		if (_2dxScript.ForceMaterial == null) {
			_2dxScript.ActiveChange = true;
		} else {
			if (GUILayout.Button("Remove Shared Material")) {
				_2dxScript.ForceMaterial = null;
				_2dxScript.ShaderChange = 1;
				_2dxScript.ActiveChange = true;
				_2dxScript.CallUpdate();
			}
		
			EditorGUILayout.PropertyField(m_object.FindProperty("ActiveChange"), new GUIContent("Change Material Property", "Change The Material Property"));
		}

		if (_2dxScript.ActiveChange) {

			EditorGUILayout.BeginVertical("Box");

		
			Texture2D icone = Resources.Load("2dxfx-icon-value") as Texture2D;
			EditorGUILayout.PropertyField(m_object.FindProperty("Destroyed"), new GUIContent("Destroyed Value", icone, "Change the destruction value"));
			icone = Resources.Load("2dxfx-icon-seed") as Texture2D;
			EditorGUILayout.PropertyField(m_object.FindProperty("Seed"), new GUIContent("Seed", icone, "Change the random seed"));

		
			EditorGUILayout.BeginVertical("Box");

			icone = Resources.Load("2dxfx-icon-fade") as Texture2D;
			EditorGUILayout.PropertyField(m_object.FindProperty("_Alpha"), new GUIContent("Fading", icone, "Fade from nothing to showing"));

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();
		}
		
		m_object.ApplyModifiedProperties();
	}
}
#endif

