using Assets.Scripts;
using Assets.Scripts.Controllers;
using Assets.Scripts.Feature;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Component = Assets.Scripts.Feature.Component;

public class Breadboard : BaseItem
{
    public enum SpawnableObjectType
    {
        Wire,
        Component
    }

    private GamemodeButton.GameMode _gameMode = GamemodeButton.GameMode.PlayMode;

    // Naming abbreviations
    // H - hole
    // J - row name (from char[] H_rows)
    // 1, 2, ... , n - column (max is H_columnsMax)

    private const int H_COLUMNS_MAX = 26; // Column limit on one row
    private readonly string[] H_rows_block1 = { "J", "I", "G", "H" }; // Named rows block 1
    private readonly string[] H_rows_block2 = { "E", "D", "C", "B" }; // Named rows block 2

    private readonly string[] H_rows_NegPos_blockTop = { "NT", "PT" }; // Positive Top, Negative Top
    private readonly string[] H_rows_NegPos_blockBottom = { "NB", "PB" }; // Positive Bottom, Negative Bottom

    private const Tag H_TAG = Tag.Hole; // Hole GameObject identification
    private Transform _H_mouseOver; // Hole that player is pointing to

    // Holes being wires now
    [SerializeField] private Material _wireMaterial;

    private Transform _H_1_wiring;
    private Transform _H_2_wiring;

    private Transform _H_wiring_component;

    private Dictionary<string, Wire> _wires = new Dictionary<string, Wire>();
    private Dictionary<string, Assets.Scripts.Feature.Component> _components = new Dictionary<string, Assets.Scripts.Feature.Component>();

    private readonly HashSet<ComponentType> _spawnableComponents = new HashSet<ComponentType>();

    private float H_distanceBetweenH;

    private Tag _activeTool = Tag.WiringTool;

    [SerializeField] private Material _highlightHoleMaterial;
    [SerializeField] private Material _wiringHoleMaterial;
    [SerializeField] private Material _defaultHoleMaterial;

    private int _currentChallengeIndex = -1;

    private Stack<SpawnableObjectType> _spawnedComponentTypeStack = new Stack<SpawnableObjectType>();

    private void Start()
    {
        SubscribeEvents();

        GenerateHoles();

        _spawnableComponents.Add(ComponentType.LED);
        _spawnableComponents.Add(ComponentType.Resistor);

        //ConnectBreadboardHorizontally();
    }

    private void GenerateHoles()
    {
        // Pre-initialized holes to calc distance between,
        // Take positions and clone other related components

        // Find pre-initialized row 1, column 1 (block 1)
        GameObject H_J1 = GameObject.Find("H_J1");
        // Find pre-initialized row 1, column 2 (block 1)
        GameObject H_J2 = GameObject.Find("H_J2"); 
        GenerateHoles(H_J1, H_J2, H_rows_block1);

        // Pre-initialized holes to calc distance between,
        // Take positions and clone other related components

        // Find pre-initialized row 1, column 1 (block 2)
        GameObject H_E1 = GameObject.Find("H_E1");
        // Find pre-initialized row 1, column 2 (block 2)
        GameObject H_D1 = GameObject.Find("H_D1");
        GenerateHoles(H_E1, H_D1, H_rows_block2);

        // Pre-initialized holes to calc distance between,
        // Take positions and clone other related components

        // Find pre-initialized row 1, column 1 (negative top 1)
        GameObject H_NT1 = GameObject.Find("H_NT1");
        // Find pre-initialized row 1, column 2 (negative top 2)
        GameObject H_NT2 = GameObject.Find("H_NT2");
        GenerateHoles(H_NT1, H_NT2, H_rows_NegPos_blockTop, 5, true);

        // Pre-initialized holes to calc distance between,
        // Take positions and clone other related components

        // Find pre-initialized row 1, column 1 (negative bottom 1)
        GameObject H_NB1 = GameObject.Find("H_NB1");
        // Find pre-initialized row 1, column 2 (negative bottom 2)
        GameObject H_NB2 = GameObject.Find("H_NB2"); 
        GenerateHoles(H_NB1, H_NB2, H_rows_NegPos_blockBottom, 5, true);
    }

    private bool IsNegPoweredHole(Transform H)
    {
        return H.name.Contains(H_rows_NegPos_blockTop[0]) ||
               H.name.Contains(H_rows_NegPos_blockBottom[0]);
    }

    private bool IsPosPoweredHole(Transform H)
    {
        return H.name.Contains(H_rows_NegPos_blockTop[1]) ||
               H.name.Contains(H_rows_NegPos_blockBottom[1]);
    }

    private class BuildCircuitArguments
    {
        public bool CircuitClosed { get; set; } = false;
        public bool ShortCircuit { get; set; } = false;
        public List<Component> ClosedCircuitComponents { get; set; }
    }

    private BuildCircuitArguments _buildCircuitArgs;
    private void Update()
    {
        if(_gameMode == GamemodeButton.GameMode.EditMode)
        {
            _buildCircuitArgs = null;
        }

        ClearAllHighlightedHoles(); // Some highlights might be stuck. Need to clear this way...
    }

    private void BuildCircuit()
    {
        BuildCircuit(out bool shortCircuit, out bool closedCircuit, out List<Component> closedCircuitComponents);

        _buildCircuitArgs = new BuildCircuitArguments()
        {
            ShortCircuit = shortCircuit,
            CircuitClosed = closedCircuit,
            ClosedCircuitComponents = closedCircuitComponents
        };

        if (closedCircuit)
        {
            if (shortCircuit)
            {
                GameEvents.current.FireEvent_HUDMessage("Trumpasis jungimas!", HUDMessageType.Warning);
            }
            else
            {
                GameEvents.current.FireEvent_HUDMessage("Grandinė uždara!", HUDMessageType.Info);
            }
        }
        else
        {
            GameEvents.current.FireEvent_HUDMessage("Grandinė atvira!", HUDMessageType.Warning);
        }
    }

    private void OnValidateChallenge(int challengeIndex)
    {
        BuildCircuit();

        switch (challengeIndex)
        {
            case 1:
            {
                ValidateChallenge1();
                return;
            }

            case 2:
            {
                ValidateChallenge2();
                return;
            }

            case 3:
            {
                ValidateChallenge3();
                return;
            }
        }
    }

    private void ValidateChallenge1()
    {
        // Shorted-out circuit
        bool valid = _buildCircuitArgs.CircuitClosed && 
                     _buildCircuitArgs.ShortCircuit;

        if (_buildCircuitArgs.ClosedCircuitComponents == null ||
            _buildCircuitArgs.ClosedCircuitComponents.Any())
        {
            GameEvents.current.FireEvent_OpenPrompt(PromptController.PromptType.Attention,
                "Reikalinga tik viena jungtis\\n " +
                "prototipavimo lentoje! Naudokite\\n" +
                "jungimo įrankį");
        }

        if (!_buildCircuitArgs.CircuitClosed)
        {
            GameEvents.current.FireEvent_OpenPrompt(PromptController.PromptType.Attention,
                "Grandinė atvira!\\n " +
                "Naudokite jungimo įrankį ir junkite\\n" +
                "mazgus prototipavimo lentoje");
        }

        GameEvents.current.FireEvent_ChallengeValidated(1, valid);
    }

    private void ValidateChallenge2()
    {
        // Closed circuit and contains single resistor
        bool valid = _buildCircuitArgs.CircuitClosed &&
                     _buildCircuitArgs.ClosedCircuitComponents
                         .Select(c => c.Type == ComponentType.Resistor).Count() == 1;

        if (!valid)
        {
            GameEvents.current.FireEvent_OpenPrompt(PromptController.PromptType.Attention,
                "Grandinėje trūksta komponento!\\n " +
                "Arba grandinė yra atvira.\\n" +
                "Atidžiai pažiūrėkite į schemą...");
        }

        GameEvents.current.FireEvent_ChallengeValidated(2, valid);
    }

    private void ValidateChallenge3()
    {
        // Closed circuit with LED ON

        bool promptFired = false;

        Component LED = null;
        bool validComponents = false;

        bool containsOneResistor = false;
        bool containsOneLED = false;
        if (_buildCircuitArgs.ClosedCircuitComponents.Any())
        {
            containsOneResistor = _buildCircuitArgs.ClosedCircuitComponents
                .Count(c => c.Type == ComponentType.Resistor) == 1;

            containsOneLED = _buildCircuitArgs.ClosedCircuitComponents
                .Count(c => c.Type == ComponentType.LED) == 1;

            validComponents = _buildCircuitArgs.CircuitClosed &&
                                   containsOneResistor &&
                                   containsOneLED;

            LED = _buildCircuitArgs.ClosedCircuitComponents.FirstOrDefault(c => c.Type == ComponentType.LED);
        }

        bool valid = false;
        if (LED != null)
        {
            bool LEDLit = LED.ComponentObject.GetComponent<LED>().Lit;
            valid = validComponents && LEDLit;

            if (!LEDLit && containsOneResistor && !promptFired)
            {
                GameEvents.current.FireEvent_OpenPrompt(PromptController.PromptType.Attention,
                    "Šviesos diodas nešviečia!\\n" +
                    "Patikrinkite rezitoriaus varžą, \\n " +
                    "naudodami Omo dėsnį...\\n" +
                    "Taip pat patikrinkite diodo poliarumą");

                promptFired = true;
            }
        }
        else if(!promptFired && LED == null)
        {
            GameEvents.current.FireEvent_OpenPrompt(PromptController.PromptType.Attention,
                "Grandinė atvira arba\\n " +
                "trūksta komponento!\\n " +
                "Atidžiai peržiūrėkite schemą...");

            promptFired = true;
        }

        if (!promptFired && !valid)
        {
            if (!_buildCircuitArgs.CircuitClosed)
            {
                GameEvents.current.FireEvent_OpenPrompt(PromptController.PromptType.Attention,
                    "Grandinė atvira...\\n " +
                    "Patikrinkite jungimo mazgus\\n");

                promptFired = true;
            }
            else
            {
                GameEvents.current.FireEvent_OpenPrompt(PromptController.PromptType.Attention,
                    "Grandinė nėra validi\\n " +
                    "Atidžiai peržiūrėkite schemą...\\n");

                promptFired = true;
            }
        }

        GameEvents.current.FireEvent_ChallengeValidated(3, valid);
    }

    private void ConnectBreadboardVertically()
    {

    }

    private void ConnectBreadboardHorizontally()
    {
        List<string> _wiresToAdd = new List<string>();
        for (int i = 0; i < H_COLUMNS_MAX; i++)
        {
            for (int j = 0; j < H_rows_block1.Length - 1; j++)
            {
                _wiresToAdd.Add($"W_H_{H_rows_block1[j]}{i + 1}" +
                                $"_H_{H_rows_block1[j + 1]}{i + 1}");
            }
        }

    }

    private void BuildMissingBreadboardConnections(Dictionary<string, Wire> wires)
    {
        Dictionary<char, SortedSet<int>> coordinatesByLetters = new Dictionary<char, SortedSet<int>>(); // E + List(E11, E12, E13, ...)
        HashSet<int> allRelevantCoordinates = new HashSet<int>(); // All coordinates form lists merged into one hashset

        // Sort coordinates by key
        foreach (string key in wires.Keys)
        {
            Tuple<string, string> coordinates = ExtractCoordinatesFromWireKey(key);

            string coordinate1 = coordinates.Item1;
            string coordinate2 = coordinates.Item2;

            AddCoordinate(coordinate1, allRelevantCoordinates);
            AddCoordinate(coordinate2, allRelevantCoordinates);

            //AddCoordinateByLetter(coordinatesByLetters, allRelevantCoordinates, coordinate1);
            //AddCoordinateByLetter(coordinatesByLetters, allRelevantCoordinates, coordinate2);
        }

        // Now we have lists:
        // E11 E12 E13
        // B11     B13 B14
        //     C12     C14
        //     F12 F13

        // We need to connect nodes vertically (that's how breadboard is designed)
        // Fill gaps:
        foreach (SortedSet<int> coords in coordinatesByLetters.Values)
        {
            foreach (int relevantCoord in allRelevantCoordinates)
            {
                if (!coords.Contains(relevantCoord))
                {
                    coords.Add(relevantCoord);
                }
            }
        }

        // Now we have lists:
        // E11 E12 E13 E14
        // B11 B12 B13 B14
        // C11 C12 C13 C14
        // F11 F12 F13 F14

    }

    private void AddCoordinate(string coordinate, HashSet<int> allRelevantCoords)
    {
        // Skip PT/PB/NT/NB (holes powered by battery, horizontally wired)
        if (coordinate.Contains(H_rows_NegPos_blockTop[0]) ||
            coordinate.Contains(H_rows_NegPos_blockTop[1]) ||
            coordinate.Contains(H_rows_NegPos_blockBottom[0]) ||
            coordinate.Contains(H_rows_NegPos_blockBottom[1]))
        {
            return;
        }

        allRelevantCoords.Add(int.Parse(coordinate));
    }

    private void AddCoordinateByLetter(Dictionary<char, SortedSet<int>> coordinatesByLetters, HashSet<int> allRelevantCoords, string coordinate)
    {
        // Skip PT/PB/NT/NB (holes powered by battery, horizontally wired)
        if (coordinate.Contains(H_rows_NegPos_blockTop[0]) ||
            coordinate.Contains(H_rows_NegPos_blockTop[1]) ||
            coordinate.Contains(H_rows_NegPos_blockBottom[0]) ||
            coordinate.Contains(H_rows_NegPos_blockBottom[1]))
        {
            return;
        }

        // Coordinate = E11
        char letter = coordinate[0]; // letter = E (use as key for mapping)
        if (!coordinatesByLetters.TryGetValue(letter, out SortedSet<int> list))
        {
            // First mapping attempt for this letter, create new list
            list = new SortedSet<int>();
            coordinatesByLetters.Add(letter, list);
        }

        int coord = int.Parse(coordinate.Substring(1));
        list.Add(coord); // Add coordinate int(11) to dictionary with key E
        allRelevantCoords.Add(coord);
    }

    private Tuple<string, string> ExtractCoordinatesFromWireKey(string wireKey)
    {
        // Wire key examples:
        // W_H_E11_H_E12 - wired connector (aka simple wire) (W)
        // C_H_E11_H_E12 - wired component (C)

        string[] keySplit = wireKey.Split('_');
        return new Tuple<string, string>(keySplit[2], keySplit[4]); // Tuple(E11, E12)
    }

    private void BuildCircuit(out bool shortCircuit, out bool closedCircuit, out List<Component> connectedComponents)
    {
        closedCircuit = false;
        shortCircuit = false;
        connectedComponents = new List<Component>();

        //Circuit circuit = new Circuit();
        //circuit.Add(new VoltageSource("V1", "PT1", GROUND_NODE_NAME, 0.0));

        if (!FindGroundedNodes(out List<Tuple<string, string>> groundedNodes))
        {
            // TODO: show error, exit
            return;
        }

        bool LEDPolarityOK = false;
        //int wIndex = 0;
        foreach (Tuple<string, string> groundedNode in groundedNodes)
        {
            //circuit.Add(new WireConnection($"W{wIndex++}", GROUND_NODE_NAME, groundedNode.Item1).Wire);

            bool nodeFound;
            string nodeNext = groundedNode.Item1;
            string nodeLast = groundedNode.Item2;
            do
            {
                //circuit.Add(new WireConnection($"W{wIndex++}", nodeLast, nodeNext).Wire);
                //Debug.Log(nodeNext);
                if (nodeNext.StartsWith("PT")) // Circuit is closed! We went all the way from negative (-) to positive (+) power source terminals
                {
                    closedCircuit = true;
                    if (!connectedComponents.Any())
                    {
                        shortCircuit = true;
                        return;
                    }
                    break; // Circuit is closed!
                }

                nodeFound = FindConnectedNode(nodeNext, nodeLast, out string connectedNode, out Component connectedComponent, out string lastConnectedNode);
                //Debug.Log("LAST NODE: " + nodeLast + ", " + 
                //          "NEXT NODE: " + nodeNext + ", " + 
                //          "CONN NODE: " + connectedNode + ", " +
                //          "CONN COMP: " + connectedComponent + ", " + 
                //          "LAST CNOD: " + lastConnectedNode);
                
                // First component found going from ground (-)
                if (connectedComponent != null)
                {
                    if (connectedComponent.Type == ComponentType.LED && connectedComponents.Count == 0)
                    {
                        if (connectedComponent.ComponentObject.GetComponent<LED>().CathodeCoord[1] == nodeNext[1]) // Columns match
                        {
                            LEDPolarityOK = true;
                        }
                    }

                    connectedComponents.Add(connectedComponent);
                }

                nodeLast = lastConnectedNode ?? nodeNext; // If lastConnectedNode is present, then we went vertical connection way (internal breadboard wiring)
                nodeNext = connectedNode;
            } 
            while (nodeFound);
        }

        TryLightUpLED(connectedComponents, LEDPolarityOK);
        //circuit.Validate();

        //BuildMissingBreadboardConnections(_wires);
    }

    private void TryLightUpLED(List<Component> connectedComponents, bool LEDPolarityOK)
    {
        if (!connectedComponents.Any())
        {
            return;
        }

        List<Component> LEDs = connectedComponents
            .Where(c => c.Type == ComponentType.LED).ToList();
        if (LEDs.Count != 1)
        {
            return;
        }

        List<Component> resistors = connectedComponents
            .Where(c => c.Type == ComponentType.Resistor).ToList();
        if (resistors.Count != 1)
        {
            return;
        }

        int resistance = resistors.First().ComponentObject.
            GetComponent<Resistor>().GetResistance();

        // Check if resistance is OK
        const int REQUIRED_RESISTANCE_OHM = 360;
        const int BIAS_OHM = 30; // Measurement bias
        if (resistance != REQUIRED_RESISTANCE_OHM && 
            resistance != REQUIRED_RESISTANCE_OHM + BIAS_OHM)
        {
            return;
        }

        // We are going from ground (-) to positive (+), therefore first added component must be LED, not resistor
        if (connectedComponents.First().Type != ComponentType.LED)
        {
            return;
        }

        if (LEDPolarityOK)
        {
            LEDs.First().ComponentObject.GetComponent<LED>().TurnOn();
        }
    }

    private bool FindConnectedNode(string node, string lastNode, out string connectedNode, out Component component, 
        out string lastConnectedNode)
    {
        lastConnectedNode = null;
        component = null;
        bool found;

        // Try find directly connected Wire
        if (!(found = FindConnectedNode<Wire>(node, lastNode, out connectedNode, out Wire _, _wires)))
        {
            // Try find directly connected component
            if (!(found = FindConnectedNode<Component>(node, lastNode, out connectedNode, 
                out component, _components)))
            {
                // No direct connections found. Search in vertical way (breadboard internal connections)
                if(!(found = FindVerticalNode<Wire>(node, lastNode, out connectedNode, out Wire _, _wires,
                    out lastConnectedNode)))
                {
                    found = FindVerticalNode<Component>(node, lastNode, out connectedNode, out component, _components, 
                        out lastConnectedNode);
                }
            }
        }

        return found;
    }

    private bool FindVerticalNode<T>(string node, string lastNode, out string connectedNode, out T component, 
        Dictionary<string, T> dic, out string lastConnectedNode) where T : class
    {
        lastConnectedNode = null; // Last connected "invisible" connection (internal on breadboard)
        connectedNode = null;
        component = null;

        string[] correctNodeSearchBlock;
        string nodeIndicationRow = node[0].ToString(); // Row I, J, G, H
        string nodeIndicationColumn = node.Substring(1); // Column 1, 2, 3, 4, ..., 11

        // Find correct node block
        if (H_rows_block1.Contains(nodeIndicationRow))
        {
            correctNodeSearchBlock = H_rows_block1;
        }
        else if (H_rows_block2.Contains(nodeIndicationRow))
        {
            correctNodeSearchBlock = H_rows_block2;
        }
        else
        {
            return false; // Unable to indicate correct nodes block
        }

        foreach (string searchNodeIndicator in correctNodeSearchBlock)
        {
            if (searchNodeIndicator == nodeIndicationRow)
            {
                continue; // Skip self
            }

            // Invisible internally connected breaboard node
            string nodeToSearch = searchNodeIndicator + nodeIndicationColumn;
            if (FindConnectedNode<T>(nodeToSearch, lastNode, out connectedNode, out component, dic))
            {
                lastConnectedNode = nodeToSearch;
                return true;
            }
        }

        return false;
    }

    private bool FindConnectedNode<T>(string node, string lastNode, out string connectedNode, out T connectedValue,
        Dictionary<string, T> dic) where T : class
    {
        connectedNode = node;
        connectedValue = null;

        foreach (KeyValuePair<string, T> kvp in dic)
        {
            Tuple<string, string> nodes = ExtractCoordinatesFromWireKey(kvp.Key);

            if (nodes.Item1 != node && nodes.Item2 != node)
            {
                continue; // Connection not found here, move further
            }

            if (nodes.Item1 == lastNode || nodes.Item2 == lastNode)
            {
                continue; // Prevent moving backward
            }

            if (nodes.Item1 == node)
            {
                connectedNode = nodes.Item2;
                connectedValue = kvp.Value;
                return true;
            }

            if (nodes.Item2 == node)
            {
                connectedNode = nodes.Item1;
                connectedValue = kvp.Value;
                return true;
            }
        }

        return false;
    }

    private bool FindGroundedNodes(out List<Tuple<string, string>> groundedNodes)
    {
        groundedNodes = new List<Tuple<string, string>>();
        foreach (KeyValuePair<string, Wire> kvp in _wires)
        {
            Tuple<string, string> nodes = ExtractCoordinatesFromWireKey(kvp.Key);

            if (nodes.Item1.StartsWith("NT"))
            {
                groundedNodes.Add(new Tuple<string, string>(nodes.Item2, nodes.Item1));
            }
            else if (nodes.Item2.StartsWith("NT"))
            {
                groundedNodes.Add(new Tuple<string, string>(nodes.Item1, nodes.Item2));
            }
        }

        return groundedNodes.Any();
    }

    private void OnInventoryActiveItemChanged(Inventory.InventoryItem item)
    {
        if (item.Object.GetComponent<BaseComponent>().HasTag(Tag.WiringTool))
        {
            _activeTool = Tag.WiringTool;
        }
        else if (item.Object.GetComponent<BaseComponent>().HasTag(Tag.CuttingTool))
        {
            _activeTool = Tag.CuttingTool;
        }
        else
        {
            _activeTool = Tag.Untagged;
        }

        ResetWiring();
    }

    private void ResetWiring()
    {
        if (_H_1_wiring != null)
        {
            _H_1_wiring.GetComponent<MeshRenderer>().material = _defaultHoleMaterial;
            _H_1_wiring = null;
        }
        _H_2_wiring = null;
    }

    public bool HoleIsWired(Transform H)
    {
        foreach (string key in _wires.Keys)
        {
            if (key.Contains(H.name))
            {
                return true;
            }
        }

        return false;
    }

    public bool HolesOccupied(Transform H_1, Transform H_2, out string errorText)
    {
        errorText = "Mazgų pozicijos jau užimtos!";

        bool wired = _wires.ContainsKey($"W_{H_1.name}_{H_2.name}") || _wires.ContainsKey($"W_{H_2.name}_{H_1.name}");
        bool hasComponent = _components.ContainsKey($"C_{H_1.name}_{H_2.name}") || _components.ContainsKey($"C_{H_2.name}_{H_1.name}");

        return wired || hasComponent;
    }

    public bool HolesNearbyInline(Transform H_1, Transform H_2)
    {
        // Are holes nearby each other in horizontal and vertical ways (non-diagonal)

        float horizontalDiff = (float)Math.Round(Math.Abs(Math.Abs(H_1.position.x) - Math.Abs(H_2.position.x)), 2);
        float verticalDiff = (float)Math.Round(Math.Abs(Math.Abs(H_1.position.z) - Math.Abs(H_2.position.z)), 2);

        bool inline = horizontalDiff == 0 || verticalDiff == 0;
        float distanceComparison = (float)Math.Round(Math.Abs(H_distanceBetweenH), 2);

        return (horizontalDiff == distanceComparison || verticalDiff == distanceComparison) && inline;
    }

    private void SubscribeEvents()
    {
        GameEvents.current.Event_OnMousePoint += OnMousePoint;
        GameEvents.current.Event_OnMouseLeftClick += OnMouseLeftClick;
        GameEvents.current.Event_OnComponentSpawnBreadboard += OnComponentSpawnBreadboard;
        GameEvents.current.Event_OnComponentDestroyBreadboard += OnComponentDestroyBreadboard;
        GameEvents.current.Event_OnInventoryActiveItemChanged += OnInventoryActiveItemChanged;
        GameEvents.current.Event_OnGameModeSwitch += OnGameModeSwitch;
        GameEvents.current.Event_OnMarkHoleAsWiring += OnMarkHoleAsWiring;
        GameEvents.current.Event_OnUnmarkHoleAsWiring += OnUnmarkHoleAsWiring;
        GameEvents.current.Event_OnValidateChallenge += OnValidateChallenge;
        GameEvents.current.Event_OnDestroyLastBreadboardGameobject += DestroyLastObject;
    }

    private void DestroyLastObject()
    {
        if (_spawnedComponentTypeStack.Any())
        {
            SpawnableObjectType type = _spawnedComponentTypeStack.Pop();

            switch (type)
            {
                case SpawnableObjectType.Component:
                {
                    if (_components.Any())
                    {
                        KeyValuePair<string, Component> lastComponent = _components.Last();
                        OnComponentDestroyBreadboard(lastComponent.Value.ComponentObject);
                        DestroyImmediate(lastComponent.Value.ComponentObject);
                    }

                    break;
                }
                case SpawnableObjectType.Wire:
                {

                    if (_wires.Any())
                    {
                        KeyValuePair<string, Wire> lastWire = _wires.Last();
                        CutWire(lastWire.Value.H_1);
                    }
                    break;
                }
            }
        }
    }

    private void OnMarkHoleAsWiring(Transform hole)
    {
        _H_wiring_component = hole;
        _H_wiring_component.GetComponent<MeshRenderer>().material = _wiringHoleMaterial;
    }

    private void OnUnmarkHoleAsWiring(Transform hole)
    {
        hole.GetComponent<MeshRenderer>().material = _defaultHoleMaterial;
        _H_wiring_component = null;
    }

    private void OnGameModeSwitch(GamemodeButton.GameMode gameMode)
    {
        _gameMode = gameMode;
    }

    private void OnComponentDestroyBreadboard(GameObject component)
    {
        string keyToRemove = null;
        foreach(KeyValuePair<string, Assets.Scripts.Feature.Component> kvp in _components)
        {
            if(kvp.Value.ComponentObject.Equals(component))
            {
                keyToRemove = kvp.Key;
                break;
            }
        }

        if (keyToRemove != null)
        {
            _components.Remove(keyToRemove);
            GameEvents.current.FireEvent_LogAction($"Panaikintas komponentas {keyToRemove}");
        }
    }

    private void OnComponentSpawnBreadboard(Transform H_1, Transform H_2, ComponentType type, GameObject component)
    {
        if(!_spawnableComponents.Contains(type))
        {
            return;
        }

        _components.Add($"C_{H_1.name}_{H_2.name}", new Assets.Scripts.Feature.Component() 
        { 
            ComponentObject = component,
            Type = type
        });

        OnUnmarkHoleAsWiring(H_1);
        OnUnmarkHoleAsWiring(H_2);

        _spawnedComponentTypeStack.Push(SpawnableObjectType.Component);

        string componentName;
        switch (type)
        {
            case ComponentType.LED:
            {
                componentName = "šviesos diodas";
                break;
            }

            case ComponentType.Resistor:
            {
                componentName = "rezistorius";
                break;
            }

            default:
            {
                componentName = "nežinomas komponentas";
                break;
            }
        }
        GameEvents.current.FireEvent_LogAction($"Sujungtas {componentName} su {H_1.name} ir {H_2.name}");
    }

    private void GenerateHoles(GameObject H_1, GameObject H_2, string[] rows, 
        int? skipEveryNColumns = null, bool skipFirstColumn = false)
    {
        // Calc distance between holes
        H_distanceBetweenH = Math.Abs(H_1.transform.position.x) - 
                             Math.Abs(H_2.transform.position.x);

        // Start from column 3, because 1 and 2 are pre-initialized on row 1
        int H_columnIndex = 2; 
        int H_rowIndex = 0; // Start from first row

        // Generate pre-initialized _H_J1 clones with adjusted positions
        float H_cloneY = H_2.transform.position.y;
        float H_cloneZ = H_2.transform.position.z;

        float H_lastCloneX = H_2.transform.position.x;

        while (H_rowIndex < rows.Length) // Rows
        {
            while (H_columnIndex < H_COLUMNS_MAX) // Columns
            {
                float H_cloneX = H_lastCloneX + H_distanceBetweenH;
                H_lastCloneX = H_cloneX;

                // Skipping not defined || first column         
                if (skipEveryNColumns == null || H_columnIndex == 0 
                                              || H_columnIndex 
                                              % skipEveryNColumns > 0)
                {
                    if (!skipFirstColumn || H_columnIndex != 0)
                    {
                        Transform parent = this.transform;
                        GameObject H_clone = GameObject.Instantiate(H_2, 
                            new Vector3(H_cloneX, H_cloneY, H_cloneZ),
                            H_1.transform.rotation, parent);

                        // H_J1, H_J2, etc.
                        H_clone.name = 
                            $"H_{rows[H_rowIndex]}{H_columnIndex + 1}";
                    }
                }
         
                H_columnIndex++; // Next column
            }

            // First column position minus distance (negative result).
            // This will let us start from the very first column 
            H_lastCloneX = H_1.transform.position.x - H_distanceBetweenH; 

            H_rowIndex++; // Next row index
            H_cloneZ -= H_distanceBetweenH; // Move to next row
            H_columnIndex = 0; // Reset column index
        }

        if (skipFirstColumn)
        {
            Destroy(H_1); // Destroy pre-initialized hole on column 1
        }
    }

    private void TryCutWire(BaseComponent baseComponent, Transform point)
    {
        if (_activeTool == Tag.CuttingTool && baseComponent != null && baseComponent.HasTag(Tag.Hole))
        {
            CutWire(point);
        }
    }

    private void CutWire(Transform point)
    {
        foreach (KeyValuePair<string, Wire> kvp in _wires)
        {
            if (kvp.Value.WiredTo(point))
            {
                _wires.Remove(kvp.Key);
                DestroyImmediate(transform.Find(kvp.Key).gameObject);

                ResetHoleMaterial(kvp.Value.H_1);
                ResetHoleMaterial(kvp.Value.H_2);

                GameEvents.current.FireEvent_LogAction($"Panaikintas mazgas {kvp.Value.H_1.name} į {kvp.Value.H_2.name}");
                break;
            }
        }
    }

    private void ResetHoleMaterial(Transform H)
    {
        H.GetComponent<MeshRenderer>().material = _defaultHoleMaterial;
    }

    private void TryWire(BaseComponent baseComponent, Transform point)
    {
        if (_activeTool == Tag.WiringTool && _H_mouseOver != null && baseComponent != null && baseComponent.HasTag(Tag.Hole))
        {
            // First wiring endpoint clicked
            if (_H_1_wiring == null && _H_2_wiring == null)
            {
                _H_1_wiring = point;
                _H_1_wiring.GetComponent<MeshRenderer>().material = _wiringHoleMaterial;
                return;
            }

            // Second wiring endpoint clicked
            if (_H_1_wiring != null && _H_2_wiring == null)
            {
                _H_2_wiring = point;
                bool selfWire = (_H_1_wiring == _H_2_wiring);

                if (!selfWire)
                {
                    //if (!DuplicateWire())
                    if (!HolesOccupied(_H_1_wiring, _H_2_wiring, out string errorText))
                    {
                        CreateWire();

                        //string HUDMessageText = $"Wire {_H_1_wiring.name.Split('_')[1]} <> {_H_2_wiring.name.Split('_')[1]} created!";
                        //GameEvents.current.FireEvent_HUDMessage(HUDMessageText, Assets.Scripts.Controllers.HUDMessageType.Info);
                    }
                    else
                    {
                        //string HUDMessageText = $"Wire {_H_1_wiring.name.Split('_')[1]} <> {_H_2_wiring.name.Split('_')[1]} already exists!";
                        GameEvents.current.FireEvent_HUDMessage(errorText, Assets.Scripts.Controllers.HUDMessageType.Error);
                    }

                    _H_2_wiring.GetComponent<MeshRenderer>().material = _wiringHoleMaterial;

                    // Reset wiring process
                    _H_1_wiring = null;
                    _H_2_wiring = null;
                }
                else
                {
                    _H_2_wiring = null;

                    string HUDMessageText = $"Du sykius paspausta ant to paties mazgo!";
                    GameEvents.current.FireEvent_HUDMessage(HUDMessageText, Assets.Scripts.Controllers.HUDMessageType.Error);
                }

                return;
            }
        }
    }

    private void OnMouseLeftClick(Transform point)
    {
        if (_gameMode == GamemodeButton.GameMode.PlayMode) { return; }
        BaseComponent baseComponent = point.GetComponent<BaseComponent>();

        TryCutWire(baseComponent, point);
        TryWire(baseComponent, point);
    }

    private void CreateWire()
    {
        string wireName = $"W_{_H_1_wiring.name}_{_H_2_wiring.name}";

        GameObject wire = new GameObject(wireName);
        wire.transform.parent = transform;
        LineRenderer line = wire.AddComponent<LineRenderer>();
        wire.AddComponent<BaseComponent>().Tags.Add(Tag.Wire);
        wire.AddComponent<BoxCollider2D>();

        line.material = _wireMaterial;

        float width = .05f;
        line.startWidth = width;
        line.endWidth = width;

        Color darkishRed = new Color(178 / 255f, 34 / 255f, 34 / 255f);

        line.startColor = darkishRed;
        line.endColor = darkishRed;
        line.positionCount = 2;

        Vector3 linePos0Vector = _H_1_wiring.position;
        Vector3 linePos1Vector = _H_2_wiring.position;

        float yLift = .02f; // Prevent line sinking into breadboard
        linePos0Vector.y += yLift;
        linePos1Vector.y += yLift;

        line.SetPosition(0, linePos0Vector);   
        line.SetPosition(1, linePos1Vector);

        _wires.Add(wireName, new Wire(_H_1_wiring, _H_2_wiring));

        _spawnedComponentTypeStack.Push(SpawnableObjectType.Wire);

        GameEvents.current.FireEvent_LogAction($"Sujungtas mazgas {_H_1_wiring.name} su {_H_2_wiring.name}");
        //Debug.Log($"Wired {_H_1_wiring.name} to {_H_2_wiring.name}!");
    }

    private void OnMousePoint(Transform point)
    {
        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        if (baseComponent == null)
        {
            ResetWiringHole_1Material();
            return;
        }

        bool isHole = baseComponent.HasTag(H_TAG);
        if (isHole)
        {
            _H_mouseOver = point.transform;

            if (_gameMode == GamemodeButton.GameMode.EditMode && 
                _H_mouseOver != _H_1_wiring &&
                _H_mouseOver != _H_wiring_component &&
                !HoleIsWired(_H_mouseOver))
            {
                
                _H_mouseOver.GetComponent<MeshRenderer>().material = _highlightHoleMaterial;
            }
        }
        else
        {
            ResetWiringHole_1Material();
        }
    }

    private void ResetWiringHole_1Material()
    {
        if (_H_mouseOver != null &&
            _H_mouseOver != _H_1_wiring &&
            _H_mouseOver != _H_wiring_component &&
            !HoleIsWired(_H_mouseOver))
        {
            _H_mouseOver.GetComponent<MeshRenderer>().material = _defaultHoleMaterial;
            _H_mouseOver = null;
        }
    }

    private void ClearAllHighlightedHoles()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (!child.name.StartsWith("H_"))
            {
                continue;
            }

            if (child != _H_mouseOver &&
                child.GetComponent<MeshRenderer>().material.name.Replace("(Instance)", "").Trim() == 
                _highlightHoleMaterial.name)
            {
                child.GetComponent<MeshRenderer>().material = _defaultHoleMaterial;
            }
        }
    }
}
