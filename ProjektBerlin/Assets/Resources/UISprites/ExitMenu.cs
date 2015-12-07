using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ExitMenu : MonoBehaviour {
	
	//BEST BE FILLIN
	public Text[] defaultMenu = new Text[3];
	private Text[] selectableText;
	int selected = 0;
	bool controllerIsPressed = false;
	const float EPS = 0.1f;
	
	private Color DEF_COLOR = Color.white;
	private Color SEL_COLOR = Color.gray;
	
	private Canvas canvas;
	private LoadGame loadGame;
	private GameObject menuText;

	
	void Start(){
		canvas = GetComponent<Canvas> ();
		loadGame = GameObject.Find ("GameLogic").GetComponent<LoadGame> ();
		selectableText = defaultMenu;
		selectableText[selected].color = SEL_COLOR;
		menuText = (GameObject)Resources.Load("UISprites/MenuText");
	}
	
	// Update is called once per frame
	void Update () {
		if (canvas.isActiveAndEnabled) {
			float val = Input.GetAxis ("JoystickLV");
			if (Mathf.Abs (val) > EPS) {
				if (!controllerIsPressed) {
					selectableText [selected].color = DEF_COLOR;
					if (val > 0)//up
						selected = (selected + selectableText.Length - 1) % selectableText.Length;
					else
						selected = (selected + 1) % selectableText.Length;
					selectableText [selected].color = SEL_COLOR;
				}
				controllerIsPressed = true;
			} else
				controllerIsPressed = false;
			if (Input.GetButtonDown ("Cross")) {
				if (selected == 0) {//mainMenu
					Application.LoadLevel ("Sprint 2");
				} else if (selected == 1) {//create
					canvas.enabled = false;
				} else if (selected == 2) {//create
					Application.Quit ();
				}

			}
		}
	}
}	

