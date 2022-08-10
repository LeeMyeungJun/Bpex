// Fantasy Skybox
// Version: 1.4.0
// Compatilble: Unity 2017.3.1 or higher, see more info in Readme.txt file.
//
// Developer:			Gold Experience Team (https://assetstore.unity.com/publishers/4162)
// Unity Asset Store:	https://assetstore.unity.com/packages/2d/textures-materials/sky/fantasy-skybox-18216
//
// Please direct any bugs/comments/suggestions to geteamdev@gmail.com

#region Namespaces

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

#endregion // Namespaces

namespace GE_FantasySkybox_Demo
{

	// ######################################################################
	// GE_FantasySkybox_Demo handles user key inputs.
	// This class is attached with "Canvas - UI" in "000_Start_Scene (960x600px)" scene.
	// ######################################################################
	public class GE_FantasySkybox_Demo : MonoBehaviour
	{
		// ########################################
		// Variables
		// ########################################

		#region Variables

		public Camera m_Camera = null;
		public enum CameraMoveMode
		{
			Manual,
			AutoRotateCamera
		}
		public CameraMoveMode m_CameraMoveMode = CameraMoveMode.Manual;
		private CameraMoveMode m_CameraMoveModeOld = CameraMoveMode.Manual;

		// Current scene index
		public int m_DemoSceneIndex = 0;

		// List of demo scene
		public string[] m_DemoSceneList = {
			"01A_Day_Sun",
			"01A_Day_Sunless",
			"01A_Night_Moon",
			"01A_Night_Moonless",
			"01B_Day_Sun",
			"01B_Day_Sunless",
			"01B_Night_Moon",
			"01B_Night_Moonless",
			"02A_Day_Sun",
			"02A_Day_Sunless",
			"02B_Day_Sun",
			"02B_Day_Sunless",
			"03A_Day_Sun",
			"03A_Day_Sunless",
			"03B_Day_Sun",
			"03B_Day_Sunless",
			"04A_Day_Sun",
			"04A_Day_Sunless",
			"04B_Day_Sun",
			"04B_Day_Sunless",
			"05A_Day_Sun",
			"05A_Day_Sunless",
			"05A_Night_Moon",
			"05A_Night_Moonless",
			"05B_Day_Sun",
			"05B_Day_Sunless",
			"06A_Day_Sun",
			"06A_Day_Sunless",
			"06B_Day_Sun",
			"06B_Day_Sunless",
			"07A_Night_Moon",
			"07A_Night_Moonless",
			"07B_Night_Moon",
			"07B_Night_Moonless"
			};

		#endregion // Variables

		// ########################################
		// MonoBehaviour Functions
		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.html
		// ########################################

		#region MonoBehaviour

		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html
		void Awake()
		{
			// Keep GameObjects alive
			GameObject[] GOs = FindObjectsOfType<GameObject>();
			foreach (GameObject go in GOs)
			{
				if (go.transform.parent == null)
				{
					DontDestroyOnLoad(go);
				}
			}
		}

		// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html
		void Start()
		{

			// Display first skybox
			SceneManager.LoadScene(m_DemoSceneList[0]);

			// Update UI Text elements
			UpdateDetailsText();
			UpdateHowToText();

			// Enable/disable FirstPersonController
			UpdateEnableFirstPersonController();
			m_Camera.transform.LookAt(new Vector3(0, 5, 10));
		}

		// Update is called every frame, if the MonoBehaviour is enabled.
		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html
		void Update()
		{
			// User press Spacebar key
			if (Input.GetKeyUp(KeyCode.Space))
			{
				// Toggle Auto Rotate
				if (m_CameraMoveMode == CameraMoveMode.Manual)
					m_CameraMoveMode = CameraMoveMode.AutoRotateCamera;
				else if (m_CameraMoveMode == CameraMoveMode.AutoRotateCamera)
					m_CameraMoveMode = CameraMoveMode.Manual;
			}

			if (m_CameraMoveMode != m_CameraMoveModeOld)
			{
				// Enable / disable FirstPersonController
				UpdateEnableFirstPersonController();

				m_CameraMoveModeOld = m_CameraMoveMode;
			}

			/*if (m_CameraMoveMode == CameraMoveMode.Manual)
			{

			}
			else */if (m_CameraMoveMode == CameraMoveMode.AutoRotateCamera)
			{
				// Rotate m_Camera around it self.
				m_Camera.transform.RotateAround(Vector3.zero, Vector3.up, 5 * Time.deltaTime);
			}

			// User press Left key
			if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
			{
				// Show previous skybox
				OnPreviousSkybox();
			}
			// User press Right key
			if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
			{
				// Show next skybox
				OnNextSkybox();
			}
		}

		// Enable/disable FirstPersonController
		void UpdateEnableFirstPersonController()
		{
			if (m_CameraMoveMode == CameraMoveMode.Manual)
			{
				FirstPersonController pFirstPersonController = FindObjectOfType<FirstPersonController>();
				if (pFirstPersonController.isActiveAndEnabled == false)
					pFirstPersonController.enabled = true;
			}
			else
			{
				FirstPersonController pFirstPersonController = FindObjectOfType<FirstPersonController>();
				if (pFirstPersonController.isActiveAndEnabled == true)
					pFirstPersonController.enabled = false;
			}
		}

		// OnTriggerExit is called when the Collider other has stopped touching the trigger.
		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.html
		void OnTriggerExit(Collider other)
		{
			Debug.Log("OnTriggerExit=" + other.name);

			// Reset player position when user move it away from terrain
			this.transform.localPosition = new Vector3(0, 1, 0);
		}

		#endregion // MonoBehaviour

		// ########################################
		// Switch skybox functions
		// ########################################

		#region Switch skybox functions

		// Switch to previous skybox
		public void OnPreviousSkybox()
		{
			SwitchSkyBox(-1);
			UpdateDetailsText();
		}

		// Switch to next skybox
		public void OnNextSkybox()
		{
			SwitchSkyBox(+1);
			UpdateDetailsText();
		}

		#endregion // MonoBehaviour

		// ########################################
		// Show skybox functions
		// ########################################

		#region Show skybox functions

		// Switch to a skybox by direction
		// DirNumber less than 0 means previous skybox, larger than 0 means next skybox
		void SwitchSkyBox(int DirNumber)
		{
			// Update add m_DemoSceneIndex with DirNumber
			m_DemoSceneIndex += DirNumber;

			// Clamp m_DemoSceneIndex between 0 and m_LightAndSkyList.Length
			if (m_DemoSceneIndex < 0)
			{
				m_DemoSceneIndex = m_DemoSceneList.Length - 1;
			}
			if (m_DemoSceneIndex > m_DemoSceneList.Length - 1)
			{
				m_DemoSceneIndex = 0;
			}

			// Load Demo scene
			SceneManager.LoadScene(m_DemoSceneList[m_DemoSceneIndex]);
		}

		#endregion // Show skybox functions

		// ########################################
		// Update UI text functions
		// ########################################

		#region Update UI text functions

		// Update details UI Text
		void UpdateDetailsText()
		{
			// Update ItemNum text
			GameObject Text_ItemNum = GameObject.Find("Text_ItemNum");
			if (Text_ItemNum != null)
			{
				Text pText = Text_ItemNum.GetComponent<Text>();
				pText.text = string.Format("{0:00} of {1:00}", m_DemoSceneIndex + 1, m_DemoSceneList.Length);
			}

			// Update Details text
			GameObject Text_Details = GameObject.Find("Text_Details");
			if (Text_Details != null)
			{
				Text pText = Text_Details.GetComponent<Text>();
				pText.text = string.Format(m_DemoSceneList[m_DemoSceneIndex]);
			}
		}

		// Update how to UI Text
		void UpdateHowToText()
		{
			// Find Text_HowTo in the scene
			GameObject Text_HowTo = GameObject.Find("Text_HowTo");
			if (Text_HowTo != null)
			{
				// Update text according to target platform
				if (Application.platform == RuntimePlatform.IPhonePlayer ||
					Application.platform == RuntimePlatform.Android) // || Application.platform == RuntimePlatform.BlackBerryPlayer || Application.platform == RuntimePlatform.WSAPlayer)
				{
					Text pText = Text_HowTo.GetComponent<Text>();
					pText.text = "Move: Left-side Joystick | Look: Right-side Joystick | Change Skybox: Tap On Screen";
				}
				else
				{
					Text pText = Text_HowTo.GetComponent<Text>();
					pText.text = "Skybox: Left and Right Arrow Keys | Turn: Mouse Drag | Release Mouse: ESC Key | Auto Rotate: Spacebar";
				}
			}
		}

		#endregion // Update UI text functions

	}
}