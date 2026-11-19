
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[DefaultExecutionOrder(200)]
[DisallowMultipleComponent]
public sealed class AnatomyHudController : MonoBehaviour
{
    private static readonly Color PanelColor =
        new Color(0.055f, 0.075f, 0.095f, 0.96f);

    private static readonly Color ButtonColor =
        new Color(0.11f, 0.15f, 0.18f, 1f);

    private static readonly Color AccentColor =
        new Color(0.30f, 0.78f, 0.72f, 1f);

    private static readonly Color PrimaryTextColor =
        new Color(0.93f, 0.95f, 0.96f, 1f);

    private static readonly Color SecondaryTextColor =
        new Color(0.65f, 0.72f, 0.76f, 1f);

    private AnatomyViewer viewer;
    private AnatomyModelLoader modelLoader;

    private Canvas interfaceCanvas;
    private GraphicRaycaster graphicRaycaster;
    private Camera hudCamera;

    private GameObject modelPanel;
    private GameObject detailsPanel;

    private GameObject modelsButtonsContainer;
    private GameObject cardsButtonsContainer;

    private RectTransform modelsTabButton;
    private RectTransform cardsTabButton;

    private GameObject staticCardRoot;
    private GameObject menuToggleButton;
    private GameObject staticCardBackButton;

    private Text toggleLabel;

    private Text modelNameText;
    private Text speciesText;
    private Text boneText;
    private Text descriptionText;

    // Balão informativo 3D.
    private Canvas infoBubbleCanvas;
    private GameObject infoBubbleObject;

    private Text bubbleNameText;
    private Text bubbleSpeciesText;
    private Text bubbleBoneText;
    private Text bubbleDescriptionText;

    // Conteúdo do card estático.
    private Text staticNameText;
    private Text staticSpeciesText;
    private Text staticBoneText;
    private Text staticDescriptionText;

    private Sprite panelSprite;
    private Font interfaceFont;

    private bool menuVisible = true;
    private bool hasSelectedModel;
    private bool viewingStaticCard;

    // Modelo temporariamente ocultado durante a consulta.
    private Transform hiddenModelRoot;
    private bool hiddenModelWasActive;

    private void Awake()
    {
        viewer = GetComponent<AnatomyViewer>();
        modelLoader = FindObjectOfType<AnatomyModelLoader>();
    }

    private void Start()
    {
        if (viewer == null)
        {
            Debug.LogError(
                "AnatomyHudController precisa de um AnatomyViewer."
            );

            enabled = false;
            return;
        }

        if (modelLoader == null)
        {
            modelLoader = FindObjectOfType<AnatomyModelLoader>();
        }

        if (modelLoader == null)
        {
            Debug.LogError(
                "AnatomyHudController não encontrou um AnatomyModelLoader."
            );

            enabled = false;
            return;
        }

        hudCamera = viewer.ViewerCamera != null
            ? viewer.ViewerCamera
            : Camera.main;

        if (hudCamera == null)
        {
            Debug.LogError(
                "AnatomyHudController não encontrou uma câmera para a interface."
            );

            enabled = false;
            return;
        }

        DisableLegacyCanvases();

        CreateInterface(hudCamera);
        CreateInfoBubble();
        CreateStaticInfoCard();
        PopulateModelButtons();

        SetCatalogTab(false);
        SetMenuVisible(true);
    }

    private void Update()
    {
        UpdateInfoBubble();
    }

    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void DisableLegacyCanvases()
    {
        Canvas[] existingCanvases = FindObjectsOfType<Canvas>();

        foreach (Canvas existingCanvas in existingCanvases)
        {
            existingCanvas.gameObject.SetActive(false);
        }
    }

    // =========================================================
    // INTERFACE PRINCIPAL
    // =========================================================

    private void CreateInterface(Camera hudCamera)
    {
        interfaceFont =
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        panelSprite =
            Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        GameObject canvasObject = new GameObject(
            "Digital Bones Interface",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(hudCamera.transform, false);

        canvasObject.transform.localPosition =
            new Vector3(0f, 0f, 0.9f);

        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one * 0.001f;

        interfaceCanvas = canvasObject.GetComponent<Canvas>();
        interfaceCanvas.renderMode = RenderMode.WorldSpace;
        interfaceCanvas.worldCamera = hudCamera;
        interfaceCanvas.sortingOrder = 20;

        graphicRaycaster =
            canvasObject.GetComponent<GraphicRaycaster>();

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1280f, 720f);

        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect =
            canvasObject.GetComponent<RectTransform>();

        canvasRect.sizeDelta = scaler.referenceResolution;

        // Painel principal.
        modelPanel = CreatePanel(
            canvasRect,
            "Model Library",
            new Vector2(28f, -82f),
            new Vector2(390f, 450f),
            PanelColor
        ).gameObject;

        CreateText(
            modelPanel.transform,
            "DIGITAL BONES",
            13,
            FontStyle.Bold,
            AccentColor,
            new Vector2(24f, -22f),
            new Vector2(342f, 24f),
            TextAnchor.MiddleLeft
        );

        CreateText(
            modelPanel.transform,
            "Biblioteca de modelos",
            25,
            FontStyle.Bold,
            PrimaryTextColor,
            new Vector2(24f, -52f),
            new Vector2(342f, 42f),
            TextAnchor.MiddleLeft
        );

        CreateText(
            modelPanel.transform,
            "Escolha uma opção para consultar",
            14,
            FontStyle.Normal,
            SecondaryTextColor,
            new Vector2(24f, -94f),
            new Vector2(342f, 28f),
            TextAnchor.MiddleLeft
        );

        // Aba Modelos.
        modelsTabButton = CreateButton(
            modelPanel.transform,
            "Models Tab",
            new Vector2(22f, -128f),
            new Vector2(164f, 34f),
            AccentColor,
            () => SetCatalogTab(false)
        );

        CreateText(
            modelsTabButton,
            "Modelos",
            14,
            FontStyle.Bold,
            PrimaryTextColor,
            Vector2.zero,
            new Vector2(164f, 34f),
            TextAnchor.MiddleCenter
        );

        // Aba Cards.
        cardsTabButton = CreateButton(
            modelPanel.transform,
            "Cards Tab",
            new Vector2(192f, -128f),
            new Vector2(176f, 34f),
            ButtonColor,
            () => SetCatalogTab(true)
        );

        CreateText(
            cardsTabButton,
            "Cards",
            14,
            FontStyle.Bold,
            PrimaryTextColor,
            Vector2.zero,
            new Vector2(176f, 34f),
            TextAnchor.MiddleCenter
        );

        CreateText(
            modelPanel.transform,
            "ESTRUTURAS DISPONÍVEIS",
            12,
            FontStyle.Bold,
            AccentColor,
            new Vector2(24f, -170f),
            new Vector2(342f, 22f),
            TextAnchor.MiddleLeft
        );

        modelsButtonsContainer =
            CreateEmptyContainer(
                modelPanel.transform,
                "Models Buttons"
            );

        cardsButtonsContainer =
            CreateEmptyContainer(
                modelPanel.transform,
                "Cards Buttons"
            );

        // Botão normal de abrir e fechar o menu.
        RectTransform toggleRect = CreateButton(
            canvasRect,
            "Menu Toggle",
            new Vector2(28f, -28f),
            new Vector2(154f, 42f),
            ButtonColor,
            ToggleMenu
        );

        menuToggleButton = toggleRect.gameObject;

        toggleLabel = CreateText(
            canvasRect,
            "Fechar menu",
            15,
            FontStyle.Bold,
            PrimaryTextColor,
            new Vector2(44f, -28f),
            new Vector2(122f, 42f),
            TextAnchor.MiddleCenter
        );

        // Botão exclusivo para voltar dos cards.
        RectTransform backRect = CreateButton(
            canvasRect,
            "Static Card Back Button",
            new Vector2(28f, -28f),
            new Vector2(180f, 42f),
            ButtonColor,
            ToggleMenu
        );

        staticCardBackButton = backRect.gameObject;

        CreateText(
            backRect,
            "Voltar ao menu",
            14,
            FontStyle.Bold,
            PrimaryTextColor,
            Vector2.zero,
            new Vector2(180f, 42f),
            TextAnchor.MiddleCenter
        );

        staticCardBackButton.SetActive(false);

        // Painel lateral antigo, mantido oculto.
        detailsPanel = CreatePanel(
            canvasRect,
            "Selected Model Details",
            new Vector2(28f, -82f),
            new Vector2(394f, 438f),
            PanelColor
        ).gameObject;

        CreateText(
            detailsPanel.transform,
            "ESTRUTURA SELECIONADA",
            12,
            FontStyle.Bold,
            AccentColor,
            new Vector2(24f, -24f),
            new Vector2(346f, 24f),
            TextAnchor.MiddleLeft
        );

        modelNameText = CreateText(
            detailsPanel.transform,
            string.Empty,
            27,
            FontStyle.Bold,
            PrimaryTextColor,
            new Vector2(24f, -58f),
            new Vector2(346f, 64f),
            TextAnchor.MiddleLeft
        );

        CreateText(
            detailsPanel.transform,
            "ESPÉCIE",
            11,
            FontStyle.Bold,
            SecondaryTextColor,
            new Vector2(24f, -142f),
            new Vector2(150f, 20f),
            TextAnchor.MiddleLeft
        );

        speciesText = CreateText(
            detailsPanel.transform,
            string.Empty,
            16,
            FontStyle.Normal,
            PrimaryTextColor,
            new Vector2(24f, -164f),
            new Vector2(346f, 30f),
            TextAnchor.MiddleLeft
        );

        CreateText(
            detailsPanel.transform,
            "ESTRUTURA ANATÔMICA",
            11,
            FontStyle.Bold,
            SecondaryTextColor,
            new Vector2(24f, -210f),
            new Vector2(346f, 20f),
            TextAnchor.MiddleLeft
        );

        boneText = CreateText(
            detailsPanel.transform,
            string.Empty,
            16,
            FontStyle.Normal,
            PrimaryTextColor,
            new Vector2(24f, -232f),
            new Vector2(346f, 30f),
            TextAnchor.MiddleLeft
        );

        CreateText(
            detailsPanel.transform,
            "SOBRE O MODELO",
            11,
            FontStyle.Bold,
            SecondaryTextColor,
            new Vector2(24f, -278f),
            new Vector2(346f, 20f),
            TextAnchor.MiddleLeft
        );

        descriptionText = CreateText(
            detailsPanel.transform,
            string.Empty,
            15,
            FontStyle.Normal,
            SecondaryTextColor,
            new Vector2(24f, -302f),
            new Vector2(346f, 112f),
            TextAnchor.UpperLeft
        );

        CreateText(
            canvasRect,
            "R restaura  •  botão direito do mouse gira",
            12,
            FontStyle.Normal,
            SecondaryTextColor,
            new Vector2(28f, -674f),
            new Vector2(500f, 26f),
            TextAnchor.MiddleLeft
        );

        detailsPanel.SetActive(false);
    }

    private GameObject CreateEmptyContainer(
        Transform parent,
        string objectName)
    {
        GameObject container = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        container.transform.SetParent(parent, false);

        RectTransform rect =
            container.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return container;
    }

    private void SetCatalogTab(bool showCards)
    {
        if (modelsButtonsContainer == null ||
            cardsButtonsContainer == null)
        {
            return;
        }

        modelsButtonsContainer.SetActive(!showCards);
        cardsButtonsContainer.SetActive(showCards);

        if (modelsTabButton != null)
        {
            modelsTabButton.GetComponent<Image>().color =
                showCards ? ButtonColor : AccentColor;
        }

        if (cardsTabButton != null)
        {
            cardsTabButton.GetComponent<Image>().color =
                showCards ? AccentColor : ButtonColor;
        }
    }

    // =========================================================
    // BALÃO INFORMATIVO 3D
    // =========================================================

    private void CreateInfoBubble()
    {
        GameObject bubbleCanvasObject = new GameObject(
            "Anatomy Info Bubble",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler)
        );

        infoBubbleCanvas =
            bubbleCanvasObject.GetComponent<Canvas>();

        infoBubbleCanvas.renderMode = RenderMode.WorldSpace;
        infoBubbleCanvas.worldCamera = hudCamera;
        infoBubbleCanvas.sortingOrder = 30;

        CanvasScaler scaler =
            bubbleCanvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ConstantPixelSize;

        scaler.dynamicPixelsPerUnit = 10f;

        RectTransform canvasRect =
            bubbleCanvasObject.GetComponent<RectTransform>();

        canvasRect.sizeDelta = new Vector2(440f, 240f);

        bubbleCanvasObject.transform.localScale =
            Vector3.one * 0.003f;

        infoBubbleObject = CreatePanel(
            canvasRect,
            "Info Bubble Background",
            Vector2.zero,
            new Vector2(440f, 240f),
            PanelColor
        ).gameObject;

        CreateText(
            infoBubbleObject.transform,
            "ESTRUTURA ANATÔMICA",
            12,
            FontStyle.Bold,
            AccentColor,
            new Vector2(20f, -14f),
            new Vector2(400f, 24f),
            TextAnchor.MiddleLeft
        );

        bubbleNameText = CreateText(
            infoBubbleObject.transform,
            string.Empty,
            23,
            FontStyle.Bold,
            PrimaryTextColor,
            new Vector2(20f, -43f),
            new Vector2(400f, 42f),
            TextAnchor.MiddleLeft
        );

        bubbleSpeciesText = CreateText(
            infoBubbleObject.transform,
            string.Empty,
            14,
            FontStyle.Normal,
            SecondaryTextColor,
            new Vector2(20f, -91f),
            new Vector2(400f, 24f),
            TextAnchor.MiddleLeft
        );

        bubbleBoneText = CreateText(
            infoBubbleObject.transform,
            string.Empty,
            14,
            FontStyle.Bold,
            PrimaryTextColor,
            new Vector2(20f, -119f),
            new Vector2(400f, 26f),
            TextAnchor.MiddleLeft
        );

        bubbleDescriptionText = CreateText(
            infoBubbleObject.transform,
            string.Empty,
            13,
            FontStyle.Normal,
            SecondaryTextColor,
            new Vector2(20f, -153f),
            new Vector2(400f, 76f),
            TextAnchor.UpperLeft
        );

        infoBubbleObject.SetActive(false);
    }

    private void UpdateInfoBubble()
    {
        if (infoBubbleObject == null ||
            viewer == null ||
            modelLoader == null)
        {
            return;
        }

        bool shouldShow =
            hasSelectedModel &&
            !menuVisible &&
            !viewingStaticCard &&
            !viewer.IsManipulating &&
            modelLoader.CurrentModelRoot != null;

        if (infoBubbleObject.activeSelf != shouldShow)
        {
            infoBubbleObject.SetActive(shouldShow);
        }

        if (shouldShow)
        {
            UpdateInfoBubblePosition();
        }
    }

    private void UpdateInfoBubblePosition()
    {
        Transform modelRoot = modelLoader.CurrentModelRoot;

        if (modelRoot == null || infoBubbleCanvas == null)
        {
            return;
        }

        Renderer[] renderers =
            modelRoot.GetComponentsInChildren<Renderer>();

        Bounds modelBounds = new Bounds(
            modelRoot.position,
            Vector3.zero
        );

        bool foundRenderer = false;

        foreach (Renderer modelRenderer in renderers)
        {
            if (modelRenderer == null ||
                !modelRenderer.enabled)
            {
                continue;
            }

            if (!foundRenderer)
            {
                modelBounds = modelRenderer.bounds;
                foundRenderer = true;
            }
            else
            {
                modelBounds.Encapsulate(modelRenderer.bounds);
            }
        }

        Vector3 bubblePosition;

        if (foundRenderer)
        {
            float verticalOffset =
                modelBounds.extents.y + 0.50f;

            bubblePosition =
                modelBounds.center +
                Vector3.up * verticalOffset;
        }
        else
        {
            bubblePosition =
                modelRoot.position + Vector3.up * 0.30f;
        }

        infoBubbleCanvas.transform.position = bubblePosition;

        Vector3 directionToCamera =
            bubblePosition - hudCamera.transform.position;

        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            infoBubbleCanvas.transform.rotation =
                Quaternion.LookRotation(
                    directionToCamera,
                    hudCamera.transform.up
                );
        }
    }

    private void UpdateInfoBubbleContent(
        AnatomyModelDefinition definition)
    {
        string displayName =
            string.IsNullOrWhiteSpace(definition.DisplayName)
                ? definition.ModelPrefab.name
                : definition.DisplayName;

        string species =
            string.IsNullOrWhiteSpace(definition.Species)
                ? "Não informada"
                : definition.Species;

        string bone =
            string.IsNullOrWhiteSpace(definition.Bone)
                ? "Não informada"
                : definition.Bone;

        string description =
            string.IsNullOrWhiteSpace(definition.Description)
                ? "Descrição ainda não cadastrada."
                : definition.Description;

        bubbleNameText.text = displayName;
        bubbleSpeciesText.text = "Espécie: " + species;
        bubbleBoneText.text = "Estrutura: " + bone;
        bubbleDescriptionText.text = description;
    }

    // =========================================================
    // CARD ESTÁTICO
    // =========================================================

    private void CreateStaticInfoCard()
    {
        staticCardRoot = new GameObject(
            "Static Anatomy Info Card",
            typeof(RectTransform)
        );

        staticCardRoot.transform.SetParent(
            interfaceCanvas.transform,
            false
        );

        RectTransform rootRect =
            staticCardRoot.GetComponent<RectTransform>();

        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(440f, 240f);

        // Escala reduzida para o card não ocupar a tela inteira.
        rootRect.localScale = Vector3.one * 2f;
        rootRect.localPosition = new Vector3(0f, 0f, 0.02f);

        RectTransform cardRect = CreatePanel(
            rootRect,
            "Static Info Card Background",
            Vector2.zero,
            new Vector2(440f, 240f),
            PanelColor
        );

        CreateText(
            cardRect,
            "ESTRUTURA ANATÔMICA",
            12,
            FontStyle.Bold,
            AccentColor,
            new Vector2(20f, -14f),
            new Vector2(400f, 24f),
            TextAnchor.MiddleLeft
        );

        staticNameText = CreateText(
            cardRect,
            string.Empty,
            23,
            FontStyle.Bold,
            PrimaryTextColor,
            new Vector2(20f, -43f),
            new Vector2(400f, 42f),
            TextAnchor.MiddleLeft
        );

        staticSpeciesText = CreateText(
            cardRect,
            string.Empty,
            14,
            FontStyle.Normal,
            SecondaryTextColor,
            new Vector2(20f, -91f),
            new Vector2(400f, 24f),
            TextAnchor.MiddleLeft
        );

        staticBoneText = CreateText(
            cardRect,
            string.Empty,
            14,
            FontStyle.Bold,
            PrimaryTextColor,
            new Vector2(20f, -119f),
            new Vector2(400f, 26f),
            TextAnchor.MiddleLeft
        );

        staticDescriptionText = CreateText(
            cardRect,
            string.Empty,
            13,
            FontStyle.Normal,
            SecondaryTextColor,
            new Vector2(20f, -153f),
            new Vector2(400f, 76f),
            TextAnchor.UpperLeft
        );

        staticCardRoot.SetActive(false);
    }

    private void ShowStaticCard(string modelId)
    {
        AnatomyModelDefinition definition =
            System.Array.Find(
                modelLoader.Catalog,
                item => item != null &&
                        item.ModelId == modelId
            );

        if (definition == null)
        {
            Debug.LogError(
                "Não foi possível encontrar o card do modelo: " +
                modelId
            );

            return;
        }

        UpdateStaticCardContent(definition);

        // Guarda e desativa o modelo que estava carregado.
        HideCurrentModelForStaticCard();

        menuVisible = false;
        viewingStaticCard = true;

        modelPanel.SetActive(false);
        detailsPanel.SetActive(false);

        staticCardRoot.SetActive(true);

        // Esconde o botão normal e mostra o botão de retorno.
        menuToggleButton.SetActive(false);
        staticCardBackButton.SetActive(true);

        // Garante que o botão fique na frente do card.
        staticCardBackButton.transform.SetAsLastSibling();

        if (infoBubbleObject != null)
        {
            infoBubbleObject.SetActive(false);
        }
    }

    private void HideCurrentModelForStaticCard()
    {
        hiddenModelRoot = modelLoader.CurrentModelRoot;

        if (hiddenModelRoot == null)
        {
            hiddenModelWasActive = false;

            // Garante que o Viewer não mantenha uma referência antiga.
            viewer.SetModelRoot(null);
            return;
        }

        hiddenModelWasActive =
            hiddenModelRoot.gameObject.activeSelf;

        // Limpa primeiro a referência do Viewer para impedir
        // rotação, movimentação, dissecação e isolamento.
        viewer.SetModelRoot(null);

        // Desativa o objeto inteiro, incluindo seus colliders.
        hiddenModelRoot.gameObject.SetActive(false);
    }

    private void RestoreModelAfterStaticCard()
    {
        if (hiddenModelRoot == null)
        {
            viewer.SetModelRoot(null);
            return;
        }

        hiddenModelRoot.gameObject.SetActive(hiddenModelWasActive);

        if (hiddenModelWasActive)
        {
            viewer.SetModelRoot(hiddenModelRoot);
        }
        else
        {
            viewer.SetModelRoot(null);
        }

        hiddenModelRoot = null;
        hiddenModelWasActive = false;
    }

    private void UpdateStaticCardContent(
        AnatomyModelDefinition definition)
    {
        string displayName =
            string.IsNullOrWhiteSpace(definition.DisplayName)
                ? definition.ModelPrefab.name
                : definition.DisplayName;

        string species =
            string.IsNullOrWhiteSpace(definition.Species)
                ? "Não informada"
                : definition.Species;

        string bone =
            string.IsNullOrWhiteSpace(definition.Bone)
                ? "Não informada"
                : definition.Bone;

        string description =
            string.IsNullOrWhiteSpace(definition.Description)
                ? "Descrição ainda não cadastrada."
                : definition.Description;

        staticNameText.text = displayName;
        staticSpeciesText.text = "Espécie: " + species;
        staticBoneText.text = "Estrutura: " + bone;
        staticDescriptionText.text = description;
    }

    // =========================================================
    // RAYCAST DA INTERFACE PARA OS CONTROLES VR
    // =========================================================

    public bool TryHandleControllerRay(Ray ray)
    {
        if (interfaceCanvas == null ||
            graphicRaycaster == null ||
            EventSystem.current == null)
        {
            return false;
        }

        Plane interfacePlane = new Plane(
            interfaceCanvas.transform.forward,
            interfaceCanvas.transform.position
        );

        if (!interfacePlane.Raycast(ray, out float distance) ||
            distance < 0f)
        {
            return false;
        }

        Vector3 worldPoint = ray.GetPoint(distance);

        Vector2 screenPoint =
            RectTransformUtility.WorldToScreenPoint(
                hudCamera,
                worldPoint
            );

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current)
            {
                position = screenPoint
            };

        List<RaycastResult> raycastResults =
            new List<RaycastResult>();

        graphicRaycaster.Raycast(
            pointerData,
            raycastResults
        );

        foreach (RaycastResult result in raycastResults)
        {
            Button button =
                result.gameObject.GetComponentInParent<Button>();

            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // CATÁLOGO E BOTÕES
    // =========================================================

    private void PopulateModelButtons()
    {
        AnatomyModelDefinition[] catalog = modelLoader.Catalog;

        if (catalog == null || catalog.Length == 0)
        {
            Debug.LogError(
                "O catálogo de modelos está vazio; não foi possível criar o menu."
            );

            return;
        }

        int buttonIndex = 0;

        foreach (AnatomyModelDefinition definition in catalog)
        {
            if (definition == null ||
                definition.ModelPrefab == null)
            {
                continue;
            }

            string modelId = definition.ModelId;

            string displayName =
                string.IsNullOrWhiteSpace(definition.DisplayName)
                    ? definition.ModelPrefab.name
                    : definition.DisplayName;

            string subtitle =
                string.IsNullOrWhiteSpace(definition.Bone)
                    ? "Modelo anatômico"
                    : definition.Bone;

            Vector2 buttonPosition = new Vector2(
                22f,
                -190f - buttonIndex * 84f
            );

            // Botão da aba Modelos.
            CreateModelButton(
                modelsButtonsContainer.transform,
                displayName,
                subtitle + "  •  selecionar",
                buttonPosition,
                () => SelectModel(modelId)
            );

            // Botão da aba Cards.
            CreateModelButton(
                cardsButtonsContainer.transform,
                displayName,
                subtitle + "  •  consultar informações",
                buttonPosition,
                () => ShowStaticCard(modelId)
            );

            buttonIndex++;
        }

        if (buttonIndex == 0)
        {
            Debug.LogError(
                "O catálogo não contém modelos com prefabs associados."
            );
        }
    }

    // =========================================================
    // SELEÇÃO DE MODELO
    // =========================================================

    private void SelectModel(string modelId)
    {
        if (!modelLoader.TryLoadModel(modelId))
        {
            return;
        }

        AnatomyModelDefinition definition =
            System.Array.Find(
                modelLoader.Catalog,
                item => item != null &&
                        item.ModelId == modelId
            );

        if (definition == null)
        {
            Debug.LogError(
                "O modelo carregado não foi encontrado no catálogo: " +
                modelId
            );

            return;
        }

        modelNameText.text =
            string.IsNullOrWhiteSpace(definition.DisplayName)
                ? definition.ModelPrefab.name
                : definition.DisplayName;

        speciesText.text =
            string.IsNullOrWhiteSpace(definition.Species)
                ? "Não informada no catálogo"
                : definition.Species;

        boneText.text =
            string.IsNullOrWhiteSpace(definition.Bone)
                ? "Não informada no catálogo"
                : definition.Bone;

        descriptionText.text =
            string.IsNullOrWhiteSpace(definition.Description)
                ? "A descrição detalhada deste modelo ainda não foi cadastrada."
                : definition.Description;

        UpdateInfoBubbleContent(definition);

        hasSelectedModel = true;
        viewingStaticCard = false;

        if (staticCardRoot != null)
        {
            staticCardRoot.SetActive(false);
        }

        if (staticCardBackButton != null)
        {
            staticCardBackButton.SetActive(false);
        }

        if (menuToggleButton != null)
        {
            menuToggleButton.SetActive(true);
        }

        SetMenuVisible(false);

        UpdateInfoBubblePosition();
    }

    // =========================================================
    // CONTROLE DO MENU
    // =========================================================

    private void ToggleMenu()
    {
        if (viewingStaticCard)
        {
            // Fecha o card estático.
            viewingStaticCard = false;

            staticCardRoot.SetActive(false);
            staticCardBackButton.SetActive(false);
            menuToggleButton.SetActive(true);

            // Mantém o último osso oculto.
            // Não restaura o modelo que estava selecionado antes do card.
            viewer.SetModelRoot(null);

            // Descarta a referência temporária ao modelo ocultado.
            hiddenModelRoot = null;
            hiddenModelWasActive = false;

            // Impede que o balão do último osso reapareça.
            hasSelectedModel = false;

            // Retorna ao menu principal.
            SetMenuVisible(true);
            return;
        }

        SetMenuVisible(!menuVisible);
    }

    private void SetMenuVisible(bool visible)
    {
        menuVisible = visible;

        modelPanel.SetActive(visible);

        // O painel lateral antigo permanece oculto.
        detailsPanel.SetActive(false);

        if (staticCardRoot != null)
        {
            staticCardRoot.SetActive(false);
        }

        if (staticCardBackButton != null)
        {
            staticCardBackButton.SetActive(false);
        }

        if (menuToggleButton != null)
        {
            menuToggleButton.SetActive(true);
        }

        toggleLabel.text =
            visible ? "Fechar menu" : "Abrir modelos";

        if (infoBubbleObject != null)
        {
            bool shouldShowBubble =
                !visible &&
                !viewingStaticCard &&
                hasSelectedModel &&
                modelLoader.CurrentModelRoot != null &&
                !viewer.IsManipulating;

            infoBubbleObject.SetActive(shouldShowBubble);
        }
    }

    // =========================================================
    // CRIAÇÃO DOS ELEMENTOS VISUAIS
    // =========================================================

    private RectTransform CreatePanel(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject panelObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        panelObject.transform.SetParent(parent, false);

        RectTransform rect =
            panelObject.GetComponent<RectTransform>();

        SetTopLeftRect(rect, position, size);

        Image image = panelObject.GetComponent<Image>();

        image.color = color;
        image.sprite = panelSprite;

        image.type =
            panelSprite != null
                ? Image.Type.Sliced
                : Image.Type.Simple;

        return rect;
    }

    private void CreateModelButton(
        Transform parent,
        string title,
        string subtitle,
        Vector2 position,
        UnityEngine.Events.UnityAction onClick)
    {
        RectTransform buttonRect = CreateButton(
            parent,
            "Model " + title,
            position,
            new Vector2(346f, 72f),
            ButtonColor,
            onClick
        );

        GameObject accent = new GameObject(
            "Accent",
            typeof(RectTransform),
            typeof(Image)
        );

        accent.transform.SetParent(buttonRect, false);

        RectTransform accentRect =
            accent.GetComponent<RectTransform>();

        SetTopLeftRect(
            accentRect,
            new Vector2(0f, -12f),
            new Vector2(4f, 48f)
        );

        accent.GetComponent<Image>().color = AccentColor;

        CreateText(
            buttonRect,
            title,
            17,
            FontStyle.Bold,
            PrimaryTextColor,
            new Vector2(18f, -11f),
            new Vector2(310f, 27f),
            TextAnchor.MiddleLeft
        );

        CreateText(
            buttonRect,
            subtitle,
            12,
            FontStyle.Normal,
            SecondaryTextColor,
            new Vector2(18f, -39f),
            new Vector2(310f, 22f),
            TextAnchor.MiddleLeft
        );
    }

    private RectTransform CreateButton(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        buttonObject.transform.SetParent(parent, false);

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        SetTopLeftRect(rect, position, size);

        Image image = buttonObject.GetComponent<Image>();

        image.color = color;
        image.sprite = panelSprite;

        image.type =
            panelSprite != null
                ? Image.Type.Sliced
                : Image.Type.Simple;

        Button button =
            buttonObject.GetComponent<Button>();

        button.targetGraphic = image;

        button.colors = new ColorBlock
        {
            normalColor = color,
            highlightedColor =
                new Color(0.16f, 0.23f, 0.25f, 1f),
            pressedColor =
                new Color(0.20f, 0.36f, 0.36f, 1f),
            selectedColor = color,
            disabledColor =
                new Color(color.r, color.g, color.b, 0.5f),
            colorMultiplier = 1f,
            fadeDuration = 0.12f
        };

        button.onClick.AddListener(onClick);

        return rect;
    }

    private Text CreateText(
        Transform parent,
        string initialText,
        int fontSize,
        FontStyle fontStyle,
        Color color,
        Vector2 position,
        Vector2 size,
        TextAnchor alignment)
    {
        GameObject textObject = new GameObject(
            string.IsNullOrEmpty(initialText)
                ? "Text"
                : initialText,
            typeof(RectTransform),
            typeof(Text)
        );

        textObject.transform.SetParent(parent, false);

        RectTransform rect =
            textObject.GetComponent<RectTransform>();

        SetTopLeftRect(rect, position, size);

        Text text = textObject.GetComponent<Text>();

        text.font = interfaceFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.text = initialText;

        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;

        return text;
    }

    private static void SetTopLeftRect(
        RectTransform rect,
        Vector2 position,
        Vector2 size)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}