using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

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

	bool lookingAtIP = false;

	void Start(){
		canvas = GetComponent<Canvas> ();
		loadGame = GameObject.Find ("GameLogic").GetComponent<LoadGame> ();
		selectableText = defaultMenu;
		selectableText[selected].color = SEL_COLOR;
		loadGame.refreshHostList ();
		menuText = (GameObject)Resources.Load("UISprites/MenuText");
	}

	// Update is called once per frame
	void Update () {
		float val = Input.GetAxis ("JoystickLV");
		if (Mathf.Abs(val)>EPS) {
			if (!controllerIsPressed) {
				selectableText[selected].color = DEF_COLOR;
				if(val>0)//up
					selected = (selected+selectableText.Length-1)%selectableText.Length;
				else
					selected = (selected+1)%selectableText.Length;
				selectableText[selected].color = SEL_COLOR;
			}
			controllerIsPressed = true;
		} else
			controllerIsPressed = false;
		if (Input.GetButtonDown ("Cross")) {
			if(lookingAtIP){
				loadGame.connectToHost(selected);
			}else{
				if(selected==0){//create
					loadGame.makeGame();
				}else if(selected==1){//join
					toggleIP();
				}else if(selected==2){//quit
					Application.Quit();
				}
			}
		}
		if (Input.GetButtonDown ("Circle") && lookingAtIP) {
			toggleIP();
		}

	}

	void toggleIP(){
		lookingAtIP = !lookingAtIP;
		foreach (Text t in selectableText)
			t.enabled = false;

		if (lookingAtIP) {
			if(loadGame.hasHostData()){
				string[] hostInfo = loadGame.getHostInfo();
				selectableText = new Text[hostInfo.Length];
				for(int i=0;i<hostInfo.Length;i++){
					GameObject g = GameObject.Instantiate(menuText,Vector3.zero,Quaternion.identity)as GameObject;
					selectableText[i] = g.GetComponent<Text>();
					selectableText[i].font = defaultMenu[0].font;
					selectableText[i].fontSize = defaultMenu[0].fontSize; 
					selectableText[i].fontStyle = defaultMenu[0].fontStyle;
					selectableText[i].color = defaultMenu[0].color;
					g.transform.parent = transform;
					g.transform.localPosition = new Vector3(0,i*selectableText[i].fontSize,0);
					RectTransform rt = g.GetComponent<RectTransform>();
					rt.anchorMin = new Vector2(0,0);
					rt.anchorMax = new Vector2(1,1);
					selectableText[i].text = hostInfo[i];

				}
			}
			else {
				lookingAtIP=false;
				loadGame.refreshHostList();
			}
		}
		if (!lookingAtIP) {
			selectableText = defaultMenu;
		}
		foreach (Text t in selectableText) {
			t.enabled = true;
			t.color=DEF_COLOR;
		}
		selected = 0;
		selectableText[selected].color = SEL_COLOR;
	}

}
