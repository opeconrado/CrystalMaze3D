#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CrystalMazeSceneBuilder : EditorWindow
{
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Scenes/Game.unity";

    [MenuItem("Tools/Crystal Maze 3D/Create Full Game Scenes")]
    public static void CreateFullGameScenes()
    {
        CreateRequiredFolders();
        CreatePlayerTag();
        CreateMaterials();

        CreateMainMenuScene();
        CreateGameScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Crystal Maze 3D",
            "Cenas criadas com sucesso!\n\nAbra Assets/Scenes/MainMenu.unity e aperte Play.",
            "OK"
        );
    }

    static void CreateRequiredFolders()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Materials");
    }

    static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    static void CreatePlayerTag()
    {
        foreach (string tag in InternalEditorUtility.tags)
        {
            if (tag == "Player")
            {
                return;
            }
        }

        InternalEditorUtility.AddTag("Player");
    }

    static Material GetOrCreateMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
        }

        return material;
    }

    static void CreateMaterials()
    {
        GetOrCreateMaterial("Ground_Green", new Color(0.25f, 0.55f, 0.25f));
        GetOrCreateMaterial("Wall_Gray", new Color(0.45f, 0.45f, 0.45f));
        GetOrCreateMaterial("Crystal_Cyan", new Color(0.1f, 0.9f, 1f));
        GetOrCreateMaterial("Enemy_Red", new Color(0.9f, 0.1f, 0.1f));
        GetOrCreateMaterial("Obstacle_Blue", new Color(0.15f, 0.25f, 0.7f));
    }

    static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        CreateDirectionalLight(new Vector3(50f, -30f, 0f));

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0f, 2f, -10f);
        cameraObject.tag = "MainCamera";

        GameObject musicObject = new GameObject("Menu Music");
        musicObject.AddComponent<AudioSource>();
        musicObject.AddComponent<MenuMusic>();

        GameObject controllerObject = new GameObject("Menu Controller");
        MenuController menuController = controllerObject.AddComponent<MenuController>();

        Canvas canvas = CreateCanvas("Menu Canvas");

        Text title = CreateText(canvas.transform, "Title", "CRYSTAL MAZE 3D", 54, TextAnchor.MiddleCenter, Color.white);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0f, 0f), new Vector2(900f, 100f));

        Text subtitle = CreateText(canvas.transform, "Subtitle", "Colete todos os cristais e evite o inimigo.", 24, TextAnchor.MiddleCenter, Color.white);
        SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.61f), new Vector2(0.5f, 0.61f), new Vector2(0f, 0f), new Vector2(900f, 60f));

        Button playButton = CreateButton(canvas.transform, "Play Button", "JOGAR", new Vector2(0f, -10f));
        UnityEventTools.AddPersistentListener(playButton.onClick, menuController.PlayGame);

        Button quitButton = CreateButton(canvas.transform, "Quit Button", "SAIR", new Vector2(0f, -95f));
        UnityEventTools.AddPersistentListener(quitButton.onClick, menuController.QuitGame);

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    static void CreateGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Game";

        CreateDirectionalLight(new Vector3(50f, -30f, 0f));

        CreateArena();
        GameObject player = CreatePlayer();
        CreateCrystals();
        CreateEnemy(player.transform);

        Canvas canvas = CreateCanvas("Game Canvas");
        GameManager gameManager = CreateGameManager(canvas);

        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    static void CreateDirectionalLight(Vector3 rotation)
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(rotation);
    }

    static Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }


    static Font GetBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = GetBuiltinFont();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;

        return text;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(260f, 60f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.12f, 0.25f, 0.55f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();

        Text text = CreateText(buttonObject.transform, "Text", label, 28, TextAnchor.MiddleCenter, Color.white);
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        return button;
    }

    static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    static void CreateArena()
    {
        Material groundMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Ground_Green.mat");
        Material wallMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Wall_Gray.mat");
        Material obstacleMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Obstacle_Blue.mat");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

        CreateWall("Wall North", new Vector3(0f, 1.5f, 15f), new Vector3(30f, 3f, 1f), wallMaterial);
        CreateWall("Wall South", new Vector3(0f, 1.5f, -15f), new Vector3(30f, 3f, 1f), wallMaterial);
        CreateWall("Wall East", new Vector3(15f, 1.5f, 0f), new Vector3(1f, 3f, 30f), wallMaterial);
        CreateWall("Wall West", new Vector3(-15f, 1.5f, 0f), new Vector3(1f, 3f, 30f), wallMaterial);

        CreateObstacle("Obstacle 1", new Vector3(-6f, 1f, 4f), new Vector3(2f, 2f, 5f), obstacleMaterial);
        CreateObstacle("Obstacle 2", new Vector3(5f, 1f, -3f), new Vector3(5f, 2f, 2f), obstacleMaterial);
        CreateObstacle("Obstacle 3", new Vector3(1f, 1f, 7f), new Vector3(2f, 2f, 4f), obstacleMaterial);
        CreateObstacle("Obstacle 4", new Vector3(-8f, 1f, -7f), new Vector3(4f, 2f, 2f), obstacleMaterial);
    }

    static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().sharedMaterial = material;
    }

    static void CreateObstacle(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = name;
        obstacle.transform.position = position;
        obstacle.transform.localScale = scale;
        obstacle.GetComponent<Renderer>().sharedMaterial = material;
    }

    static GameObject CreatePlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.2f, -10f);

        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = new Vector3(0f, 0f, 0f);

        PlayerController playerController = player.AddComponent<PlayerController>();

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform);
        cameraObject.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        cameraObject.transform.localRotation = Quaternion.identity;
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();

        playerController.playerCamera = cameraObject.transform;

        return player;
    }

    static void CreateCrystals()
    {
        Material crystalMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Crystal_Cyan.mat");

        Vector3[] positions =
        {
            new Vector3(-10f, 1f, -10f),
            new Vector3(10f, 1f, -10f),
            new Vector3(-11f, 1f, 8f),
            new Vector3(11f, 1f, 9f),
            new Vector3(0f, 1f, 12f),
            new Vector3(-3f, 1f, 1f),
            new Vector3(7f, 1f, 3f),
            new Vector3(-7f, 1f, -2f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crystal.name = "Crystal " + (i + 1);
            crystal.transform.position = positions[i];
            crystal.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            crystal.GetComponent<Renderer>().sharedMaterial = crystalMaterial;

            SphereCollider collider = crystal.GetComponent<SphereCollider>();
            collider.isTrigger = true;

            Rigidbody rigidbody = crystal.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            crystal.AddComponent<CrystalCollectible>();
        }
    }

    static void CreateEnemy(Transform player)
    {
        Material enemyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Enemy_Red.mat");

        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy Patrol";
        enemy.transform.position = new Vector3(-12f, 1f, 0f);
        enemy.GetComponent<Renderer>().sharedMaterial = enemyMaterial;

        GameObject pointA = new GameObject("Enemy Point A");
        pointA.transform.position = new Vector3(-12f, 1f, 0f);

        GameObject pointB = new GameObject("Enemy Point B");
        pointB.transform.position = new Vector3(12f, 1f, 0f);

        EnemyPatrol patrol = enemy.AddComponent<EnemyPatrol>();
        patrol.pointA = pointA.transform;
        patrol.pointB = pointB.transform;
        patrol.player = player;
        patrol.speed = 3f;
        patrol.catchDistance = 1.6f;
    }

    static GameManager CreateGameManager(Canvas canvas)
    {
        Text crystalText = CreateText(canvas.transform, "Crystal Text", "Cristais: 0 / 0", 24, TextAnchor.MiddleLeft, Color.white);
        SetRect(crystalText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(145f, -35f), new Vector2(280f, 50f));

        Text timerText = CreateText(canvas.transform, "Timer Text", "Tempo: 90", 24, TextAnchor.MiddleLeft, Color.white);
        SetRect(timerText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(145f, -75f), new Vector2(280f, 50f));

        GameObject panel = new GameObject("End Panel");
        panel.transform.SetParent(canvas.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Text messageText = CreateText(panel.transform, "Message Text", "", 36, TextAnchor.MiddleCenter, Color.white);
        SetRect(messageText.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(850f, 160f));

        Button restartButton = CreateButton(panel.transform, "Restart Button", "REINICIAR", new Vector2(0f, -40f));
        Button menuButton = CreateButton(panel.transform, "Menu Button", "MENU", new Vector2(0f, -125f));

        GameObject managerObject = new GameObject("Game Manager");
        GameManager gameManager = managerObject.AddComponent<GameManager>();
        gameManager.crystalText = crystalText;
        gameManager.timerText = timerText;
        gameManager.messageText = messageText;
        gameManager.endPanel = panel;
        gameManager.startTime = 90f;

        UnityEventTools.AddPersistentListener(restartButton.onClick, gameManager.RestartGame);
        UnityEventTools.AddPersistentListener(menuButton.onClick, gameManager.BackToMenu);

        panel.SetActive(false);

        return gameManager;
    }
}
#endif
