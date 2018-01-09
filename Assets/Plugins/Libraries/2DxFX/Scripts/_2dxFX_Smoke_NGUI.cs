// OctoBox, Created by Anton Torkhov 


using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("2DxFX/Standard/Smoke_NGUI")]
[System.Serializable]
public class _2dxFX_Smoke_NGUI : MonoBehaviour
{
	[HideInInspector]
	public Material ForceMaterial;
	[HideInInspector]
	public bool ActiveChange = true;
	private string shader = "2DxFX/Standard/Smoke";
	[HideInInspector] [Range(0, 1)] 
	public float _Alpha = 1f;

	[HideInInspector] 
	public Texture2D __MainTex2;
	[HideInInspector] [Range(64, 256)]
	public float _Value1 = 64;
	[HideInInspector] [Range(0, 1)]
	public float _Value2 = 1;
	[HideInInspector] [Range(0, 1)] 
	public float _Value3 = 1;
	[HideInInspector] 
	public float _Value4;
	[HideInInspector] 
	public Color _Color1 = new Color(1f, 0f, 1f, 1f);
	[HideInInspector] 
	public Color _Color2 = new Color(1f, 1f, 1f, 1f);

	[HideInInspector] 
	public bool _AutoScrollX;
	[HideInInspector] [Range(0, 10)]
	public float _AutoScrollSpeedX;
	[HideInInspector] 
	public bool _AutoScrollY;
	[HideInInspector] [Range(0, 10)] 
	public float _AutoScrollSpeedY;
	[HideInInspector]  
	private float _AutoScrollCountX;
	[HideInInspector]  
	private float _AutoScrollCountY;

	[HideInInspector] public int ShaderChange = 0;
	Material tempMaterial;
	Material defaultMaterial;
	UI2DSprite _sprite;
	
	void Awake()
	{
		_sprite = this.gameObject.GetComponent<UI2DSprite>();
	}

	void Start()
	{  
		__MainTex2 = Resources.Load("_2dxFX_SmokeTXT") as Texture2D;
		ShaderChange = 0;
		if (this.gameObject.GetComponent<UI2DSprite>() != null) {
			_sprite.material.SetTexture("_MainTex2", __MainTex2);
		}
	}

	public void CallUpdate()
	{
		Update();
	}

	void Update()
	{
		if (this.gameObject.GetComponent<UI2DSprite>() != null) {
			if (_sprite == null) {
				_sprite = this.gameObject.GetComponent<UI2DSprite>();
			}
		}		
		if ((ShaderChange == 0) && (ForceMaterial != null)) {
			ShaderChange = 1;
			if (tempMaterial != null) {
				DestroyImmediate(tempMaterial);
			}
			if (this.gameObject.GetComponent<UI2DSprite>() != null) {
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
			if (this.gameObject.GetComponent<UI2DSprite>() != null) {
				_sprite.material = tempMaterial;
			}
			ShaderChange = 0;
		}
		
		#if UNITY_EDITOR
		string dfname = "";
		if (this.gameObject.GetComponent<UI2DSprite>() != null) {
			UI2DSprite img = this.gameObject.GetComponent<UI2DSprite>();
			if (img.material == null) {
				dfname = "Sprites/Default";
			}
		}
		if (dfname == "Sprites/Default") {
			ForceMaterial.shader = Shader.Find(shader);
			ForceMaterial.hideFlags = HideFlags.None;
			if (this.gameObject.GetComponent<UI2DSprite>() != null) {
				UI2DSprite img = this.gameObject.GetComponent<UI2DSprite>();
				if (img.material == null)
					_sprite.material = ForceMaterial;
			}
			__MainTex2 = Resources.Load("_2dxFX_SmokeTXT") as Texture2D;
			if (this.gameObject.GetComponent<UI2DSprite>() != null) {
				UI2DSprite img = this.gameObject.GetComponent<UI2DSprite>();
				if (img.material == null)
					_sprite.material.SetTexture("_MainTex2", __MainTex2);
			}
		}
		#endif
		if (ActiveChange) {
			if (this.gameObject.GetComponent<UI2DSprite>() != null) {
				_sprite.material.SetFloat("_Alpha", 1 - _Alpha);
				_sprite.material.SetFloat("_Value1", _Value1);
				if (_Value2 == 1)
					_Value2 = 0.995f;
				_sprite.material.SetFloat("_Value2", _Value2);
				_sprite.material.SetFloat("_Value3", _Value3);
				_sprite.material.SetFloat("_Value4", _Value4);
				_sprite.material.SetColor("_Color1", _Color1);
				_sprite.material.SetColor("_Color2", _Color2);
				if (_sprite.panel != null) {
					_sprite.panel.RebuildAllDrawCalls();
				}
			}
		}
	}

	void OnDestroy()
	{
		if (this.gameObject.GetComponent<UI2DSprite>() != null) {
			if (_sprite == null)
				_sprite = this.gameObject.GetComponent<UI2DSprite>();
		}
		if ((Application.isPlaying == false) && (Application.isEditor == true)) {
			
			if (tempMaterial != null)
				DestroyImmediate(tempMaterial);
			
			if (gameObject.activeSelf && defaultMaterial != null) {
				if (this.gameObject.GetComponent<UI2DSprite>() != null) {
					_sprite.material = defaultMaterial;
					_sprite.material.hideFlags = HideFlags.None;
				}
			}	
		}
	}

	void OnDisable()
	{ 
		if (this.gameObject.GetComponent<UI2DSprite>() != null) {
			if (_sprite == null)
				_sprite = this.gameObject.GetComponent<UI2DSprite>();
		} 
		if (gameObject.activeSelf && defaultMaterial != null) {
			if (this.gameObject.GetComponent<UI2DSprite>() != null) {
				_sprite.material = defaultMaterial;
				_sprite.material.hideFlags = HideFlags.None;
			}
		}		
	}

	void OnEnable()
	{
		if (this.gameObject.GetComponent<UI2DSprite>() != null) {
			if (_sprite == null)
				_sprite = this.gameObject.GetComponent<UI2DSprite>();
		} 
		
		if (defaultMaterial == null) {
			defaultMaterial = new Material(Shader.Find("Sprites/Default"));			
		}
		if (ForceMaterial == null) {
			ActiveChange = true;
			tempMaterial = new Material(Shader.Find(shader));
			tempMaterial.hideFlags = HideFlags.None;
			if (this.gameObject.GetComponent<UI2DSprite>() != null) {
				_sprite.material = tempMaterial;
			}
			__MainTex2 = Resources.Load("_2dxFX_SmokeTXT") as Texture2D;
		} else {
			ForceMaterial.shader = Shader.Find(shader);
			ForceMaterial.hideFlags = HideFlags.None;
			if (this.gameObject.GetComponent<UI2DSprite>() != null) {
				_sprite.material = ForceMaterial;
			}
			__MainTex2 = Resources.Load("_2dxFX_SmokeTXT") as Texture2D;
		}
		
		if (__MainTex2) {
			__MainTex2.wrapMode = TextureWrapMode.Repeat;
			if (this.gameObject.GetComponent<UI2DSprite>() != null) {
				_sprite.material.SetTexture("_MainTex2", __MainTex2);
			}
		}
	}

}


#if UNITY_EDITOR
[CustomEditor(typeof(_2dxFX_Smoke_NGUI)),CanEditMultipleObjects]
public class _2dxFX_Smoke_NGUI_Editor : Editor
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
		
		_2dxFX_Smoke_NGUI _2dxScript = (_2dxFX_Smoke_NGUI)target;
	
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

			Texture2D icone = Resources.Load("2dxfx-icon-brightness") as Texture2D;
			EditorGUILayout.PropertyField(m_object.FindProperty("_Value2"), new GUIContent("Turn To Smoke", icone, "Turn To Smoke Value"));
		
			EditorGUILayout.PropertyField(m_object.FindProperty("_Color1"), new GUIContent("Smoke Color", icone, "Select the color of the Smoke"));
			EditorGUILayout.PropertyField(m_object.FindProperty("_Color2"), new GUIContent("Smoke Color 2", icone, "Select the color of the Smoke"));

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


