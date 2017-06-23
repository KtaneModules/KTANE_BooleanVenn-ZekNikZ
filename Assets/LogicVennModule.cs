using UnityEngine;
using System.Collections;

using LogicVenn;



public class LogicVennModule : MonoBehaviour {

    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;

    public KMSelectable[] Buttons;
    public Material[] ColorMaterials;
    public TextMesh DisplayText;

    string challenge;
    int[] solution;
    int[] currentState = new int[8];

    public bool OP_AND(bool a, bool b) { return a && b; }
    public bool OP_NAND(bool a, bool b) { return !OP_AND(a, b); }
    public bool OP_OR(bool a, bool b) { return a || b; }
    public bool OP_NOR(bool a, bool b) { return !OP_OR(a, b); }
    public bool OP_XOR(bool a, bool b) { return a ^ b; }
    public bool OP_XNOR(bool a, bool b) { return !OP_XOR(a, b); }
    public bool OP_LIMP(bool a, bool b) { return !(a && !b); }
    public bool OP_RIMP(bool a, bool b) { return !(!a && b); }
    string[] operators = new string[] { " ∧ ", " ∨ ", " ⊻ ", "→", " | ", " ↓ ", "↔", "←"  };

    int moduleId;
    static int moduleIdCounter = 1;

    void Start () {
        moduleId = moduleIdCounter++;

        GetComponent<KMBombModule>().OnActivate += OnActivate;

        for (int i = 0; i < 8; i++) {
            var j = i;
            Buttons[i].OnInteract += delegate { HandlePress(j); return false; };
        }

        DisplayText.text = "";
    }

    void OnActivate() {
        GenerateSolution();
    }

    bool OpDec(int index, bool a, bool b) {
        switch (index) {
            case 0:
                return OP_AND(a, b);
            case 1:
                return OP_OR(a, b);
            case 2:
                return OP_XOR(a, b);
            case 3:
                return OP_LIMP(a, b);
            case 4:
                return OP_NAND(a, b);
            case 5:
                return OP_NOR(a, b);
            case 6:
                return OP_XNOR(a, b);
            case 7:
                return OP_RIMP(a, b);
            default:
                return false;
        }
    }

    void GenerateSolution() {
        int op1_i = Random.Range(0, operators.Length);
        int op2_i = Random.Range(0, operators.Length);
        int paren = Random.Range(0, 2);

        int[] sol = new int[8];

        for (int i = 0; i < 2; i++) {
            if (paren == 0) {
                for (int j = 0; j < 2; j++) {
                    bool r = OpDec(op1_i, i == 1, j == 1);
                    for (int k = 0; k < 2; k ++) {
                        sol[i * 4 + j * 2 + k] = OpDec(op2_i, r, k == 1) ? 1 : 2;
                    }
                }
            } else {
                for (int j = 0; j < 2; j++) {
                    for (int k = 0; k < 2; k++) {
                        bool r = OpDec(op2_i, j == 1, k == 1);
                        sol[i * 4 + j * 2 + k] = OpDec(op1_i, i == 1, r) ? 1 : 2;
                    }
                }
            }
        }

        solution = sol;

        string dis;
        if (paren == 0) {
            dis = "(A" + operators[op1_i] + "B)" + operators[op2_i] + "C";
        } else {
            dis = "A" + operators[op1_i] + "(B" + operators[op2_i] + "C)";
        }
        challenge = dis;
        DisplayText.text = dis;

        string solstr = (solution[0] == 1 ? "TRUE" : "FALSE");
        for (int i = 1; i < 8; i++) {
            solstr += ", " + (solution[i] == 1 ? "TRUE" : "FALSE");
        }

        Debug.LogFormat("[Boolean Venn Diagram #{0}] Expression: \"{1}\"", moduleId, challenge);
        Debug.LogFormat("[Boolean Venn Diagram #{0}] Solution: {1}", moduleId, "{" + solstr + "}");
    }

    bool CheckSolution() {
        for (int i = 0; i < 8; i++) {
            if (solution[i] == 1) {
                if (currentState[i] != 1) {
                    return false;
                }
            }
        }
        return true;
    }

    void UpdateColor(int button) {
        Buttons[button].GetComponent<Renderer>().material = ColorMaterials[currentState[button]];
    }

    void HandlePress(int button) {
        KMAudio.PlaySoundAtTransform("tick", this.transform);
        GetComponent<KMSelectable>().AddInteractionPunch(0.5f);
        if (solution[button] == 1) {
            currentState[button] = 1;
            UpdateColor(button);
            if (CheckSolution()) {
                Debug.LogFormat("[Boolean Venn Diagram #{0}] Module Solved!", moduleId);
                for (int i = 0; i < 8; i++) {
                    currentState[i] = solution[i];
                    UpdateColor(i);
                }
                BombModule.HandlePass();
            }
        } else {
            if (currentState[button] != 2) {
                Debug.LogFormat("[Boolean Venn Diagram #{0}] Button {1} pressed incorrectly!", moduleId, button);
                currentState[button] = 2;
                UpdateColor(button);
                BombModule.HandleStrike();
            }
        }
    }
}
