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

#endregion // Namespaces


namespace GE_FantasySkybox_Demo
{
	// ######################################################################
	// GE_FantasySkybox_UIs updates UI in the scene.
	// This class is attached with "Canvas - UI" in "000_Start_Scene (960x600px)" scene.
	// ######################################################################

	public class GE_FantasySkybox_UIs : MonoBehaviour
	{
		// ########################################
		// Variables
		// ########################################

		#region Variables

		// Canvas
		public Canvas m_Canvas = null;

		// Help
		public Button m_Help_Button = null;
		public GameObject m_Help_Window = null;

		// Details
		public GameObject m_Details = null;

		// Details Panel
		public GameObject m_PanelDetails = null;

		// HowTo
		public GameObject m_HowTo = null;

		#endregion // Variables

		// ########################################
		// MonoBehaviour Functions
		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.html
		// ########################################

		#region MonoBehaviour

		// Update is called every frame, if the MonoBehaviour is enabled.
		// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html
		void Update()
		{
		}

		#endregion // MonoBehaviour

		// ########################################
		// UI Respond functions
		// ########################################

		#region UI Respond functions

		// User press Support button
		public void Button_Help_Support()
		{
			// http://docs.unity3d.com/ScriptReference/Application.OpenURL.html
			Application.OpenURL("mailto:geteamdev@gmail.com");
		}

		// User press Products button
		public void Button_Help_Products()
		{
			// http://docs.unity3d.com/ScriptReference/Application.ExternalEval.html
			//Application.ExternalEval("window.open('https://www.facebook.com/GETeamPage/','GOLD EXPERIENCE TEAM')");

			// http://docs.unity3d.com/ScriptReference/Application.OpenURL.html
			Application.OpenURL("https://www.facebook.com/GETeamPage/");
		}

		#endregion // UI Respond Functions
	}
}