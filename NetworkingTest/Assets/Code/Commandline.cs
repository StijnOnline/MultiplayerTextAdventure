using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using System.Reflection;
using TMPro;

public class Commandline : MonoBehaviour {
    public static Commandline Instance = null;
    [SerializeField]
    private TextMeshProUGUI output = null;
    [SerializeField]
    private TMP_InputField input = null;

    private ServerBehaviour serverBehaviour;
    private ClientBehaviour clientBehaviour;
    private bool debug = true;

    private void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

   /* private void Update() {
        if(Input.GetKeyDown(KeyCode.Return)) {
            EnterCommand(input);
        }
    }*/

    public void StartHost() {
        serverBehaviour = (new GameObject()).AddComponent<ServerBehaviour>();
    }

    public void EnterCommand(TMP_InputField inputField) {
        string line = inputField.text;

        Output($"{inputField.text}");
        inputField.text = "";

        string[] args = line.Split(' ');
        string cmd = args[0];
        //todo replace with enum
        switch(cmd) {
            case "debug":
                if(!bool.TryParse(args[1], out debug))
                    Output($"Please use 'True' or 'False'");
                break;
            case "host":
                serverBehaviour = (new GameObject()).AddComponent<ServerBehaviour>();
                break;
            case "connect":
                clientBehaviour = (new GameObject()).AddComponent<ClientBehaviour>();
                break;
            case "setname":
                string name = line.Split(new char[] { ' ' }, 2)[1];
                if(clientBehaviour == null) {
                    Output($"Client not started");
                    break;
                }
                //clientBehaviour.SetName(name);
                break;
            default:
                Output($"Unrecognised command: {cmd}");
                break;
        }

        EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
        inputField.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    public void Output(string text) {
        output.text += text + "\n";
        if(debug)
            Debug.Log(text);
    }
}