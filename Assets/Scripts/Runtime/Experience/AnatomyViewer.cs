
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

public sealed class AnatomyViewer : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Transform trackingOrigin;
    [SerializeField] private Camera viewerCamera;
    [SerializeField] private Text selectedPartText;
    [SerializeField] private Text detailsText;
    [SerializeField] private Text modeText;

    private AnatomicalPart[] parts = new AnatomicalPart[0];
    private AnatomicalPart selectedPart;

    private InputDevice rightController;
    private InputDevice leftController;

    private Vector3 initialScale;
    private Quaternion initialRotation;
    private Vector3 initialPosition;

    private Vector3 previousMousePosition;

    private float zoomFactor = 1f;

    private bool mouseDragging;
    private bool controllerDragging;

    private Plane dragPlane;
    private Vector3 dragOffset;

    private bool dissected;
    private bool isolated;

    private bool previousTrigger;
    private bool previousRightPrimary;
    private bool previousRightSecondary;
    private bool previousLeftPrimary;

    private AnatomyHudController hudController;

    public Camera ViewerCamera => viewerCamera;

    public bool IsManipulating =>
        mouseDragging || controllerDragging;

    private void Awake()
    {
        hudController = GetComponent<AnatomyHudController>();

        if (hudController == null)
        {
            hudController = gameObject.AddComponent<AnatomyHudController>();
        }
    }

    public void Configure(
        Transform root,
        Transform origin,
        Camera camera,
        Text selectedText,
        Text description,
        Text mode)
    {
        trackingOrigin = origin;
        viewerCamera = camera;
        selectedPartText = selectedText;
        detailsText = description;
        modeText = mode;

        SetModelRoot(root);
    }

    private void Start()
    {
        if (viewerCamera == null)
        {
            viewerCamera = Camera.main;
        }

        if (modelRoot != null)
        {
            SetModelRoot(modelRoot);
        }
        else
        {
            RefreshInformation();
            RefreshModeText();
        }
    }

    public void SetModelRoot(Transform root)
    {
        if (selectedPart != null)
        {
            selectedPart.SetSelected(false);
        }

        modelRoot = root;
        selectedPart = null;

        parts = modelRoot != null
            ? modelRoot.GetComponentsInChildren<AnatomicalPart>(true)
            : new AnatomicalPart[0];

        if (modelRoot != null)
        {
            initialScale = modelRoot.localScale;
            initialRotation = modelRoot.localRotation;
            initialPosition = modelRoot.localPosition;
        }

        zoomFactor = 1f;
        dissected = false;
        isolated = false;

        mouseDragging = false;
        controllerDragging = false;

        if (parts.Length > 0)
        {
            SelectPart(parts[0]);
        }
        else
        {
            RefreshInformation();
            RefreshModeText();
        }
    }

    private void Update()
    {
        HandleDesktopInput();
        HandleControllerInput();
    }

    // ==========================================
    // CONTROLES DO MOUSE
    // ==========================================

    private void HandleDesktopInput()
    {
        if (viewerCamera == null || modelRoot == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            bool overUI = EventSystem.current != null &&
                          EventSystem.current.IsPointerOverGameObject();

            if (!overUI)
            {
                Ray ray = viewerCamera.ScreenPointToRay(
                    Input.mousePosition
                );

                if (TryGetModelBounds(ray, out Vector3 hitPoint))
                {
                    BeginDrag(
                        ray,
                        hitPoint,
                        viewerCamera.transform.forward
                    );

                    mouseDragging = true;
                }
            }
        }

        if (mouseDragging && Input.GetMouseButton(0))
        {
            Ray ray = viewerCamera.ScreenPointToRay(
                Input.mousePosition
            );

            UpdateDrag(ray);
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseDragging = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            previousMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 mouseDelta =
                Input.mousePosition - previousMousePosition;

            modelRoot.Rotate(
                Vector3.up,
                -mouseDelta.x * 0.35f,
                Space.World
            );

            modelRoot.Rotate(
                Vector3.right,
                mouseDelta.y * 0.35f,
                Space.Self
            );

            previousMousePosition = Input.mousePosition;
        }

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            ScaleModel(scroll * 0.12f);
        }

        if (Input.GetKeyDown(KeyCode.Equals) ||
            Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            ScaleModel(0.15f);
        }

        if (Input.GetKeyDown(KeyCode.Minus) ||
            Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            ScaleModel(-0.15f);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleDissection();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleIsolation();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetView();
        }

        for (int index = 0; index < parts.Length && index < 3; index++)
        {
            if (Input.GetKeyDown(
                (KeyCode)((int)KeyCode.Alpha1 + index)))
            {
                SelectPart(parts[index]);
            }
        }
    }

    // ==========================================
    // DETECÇÃO DO MODELO
    // ==========================================

    private bool TryGetModelBounds(
        Ray ray,
        out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        if (modelRoot == null)
            return false;

        Renderer[] renderers =
            modelRoot.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
            return false;

        Bounds modelBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            modelBounds.Encapsulate(renderers[i].bounds);
        }

        if (modelBounds.IntersectRay(ray, out float distance))
        {
            hitPoint = ray.GetPoint(distance);
            return true;
        }

        return false;
    }

    // ==========================================
    // MOVIMENTAÇÃO
    // ==========================================

    private void BeginDrag(
        Ray ray,
        Vector3 hitPoint,
        Vector3 planeNormal)
    {
        if (modelRoot == null)
            return;

        dragPlane = new Plane(
            planeNormal.normalized,
            hitPoint
        );

        dragOffset = modelRoot.position - hitPoint;
    }

    private void UpdateDrag(Ray ray)
    {
        if (modelRoot == null)
            return;

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);

            modelRoot.position = targetPoint + dragOffset;
        }
    }

    // ==========================================
    // CONTROLES META QUEST 3
    // ==========================================

    private void HandleControllerInput()
    {
        if (!rightController.isValid)
        {
            rightController =
                InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }

        if (!leftController.isValid)
        {
            leftController =
                InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }

        // --------------------------------------
        // CONTROLE ESQUERDO
        // Analógico esquerdo + gatilho:
        // movimentar o modelo.
        // --------------------------------------

        bool leftTriggerPressed =
            leftController.TryGetFeatureValue(
                CommonUsages.triggerButton,
                out bool leftTrigger) && leftTrigger;

        if (modelRoot != null &&
            leftTriggerPressed &&
            !controllerDragging &&
            leftController.TryGetFeatureValue(
                CommonUsages.primary2DAxis,
                out Vector2 leftStick) &&
            leftStick.sqrMagnitude > 0.01f)
        {
            Vector3 right =
                viewerCamera != null
                    ? viewerCamera.transform.right
                    : Vector3.right;

            Vector3 up =
                viewerCamera != null
                    ? viewerCamera.transform.up
                    : Vector3.up;

            Vector3 movement =
                (right * leftStick.x + up * leftStick.y)
                * 0.6f
                * Time.deltaTime;

            modelRoot.position += movement;
        }

        // X esquerdo: isolamento.
        bool leftPrimaryPressed =
            leftController.TryGetFeatureValue(
                CommonUsages.primaryButton,
                out bool leftPrimary) && leftPrimary;

        if (leftPrimaryPressed && !previousLeftPrimary)
        {
            ToggleIsolation();
        }

        previousLeftPrimary = leftPrimaryPressed;

        // --------------------------------------
        // CONTROLE DIREITO
        // A: selecionar e interagir com a interface.
        // B: dissecação.
        // --------------------------------------

        bool triggerPressed =
            rightController.TryGetFeatureValue(
                CommonUsages.triggerButton,
                out bool triggerButton) && triggerButton;

        if (triggerPressed && !previousTrigger &&
            TryGetControllerRay(rightController, out Ray controllerRay))
        {
            if (hudController == null ||
                !hudController.TryHandleControllerRay(controllerRay))
            {
                SelectFromRay(controllerRay);
            }
        }

        previousTrigger = triggerPressed;

        // A direito: selecionar e interagir com o menu.
        bool rightPrimaryPressed =
            rightController.TryGetFeatureValue(
                CommonUsages.primaryButton,
                out bool rightPrimary) && rightPrimary;

        if (rightPrimaryPressed && !previousRightPrimary &&
            TryGetControllerRay(rightController, out Ray selectionRay))
        {
            if (hudController == null ||
                !hudController.TryHandleControllerRay(selectionRay))
            {
                SelectFromRay(selectionRay);
            }
        }

        previousRightPrimary = rightPrimaryPressed;

        // B direito: dissecação.
        bool rightSecondaryPressed =
            rightController.TryGetFeatureValue(
                CommonUsages.secondaryButton,
                out bool rightSecondary) && rightSecondary;

        if (rightSecondaryPressed && !previousRightSecondary)
        {
            ToggleDissection();
        }

        previousRightSecondary = rightSecondaryPressed;

        // --------------------------------------
        // ANALÓGICO DIREITO
        // Sem gatilho: rotacionar.
        // Com gatilho: aproximar/afastar.
        // --------------------------------------

        bool rightTriggerPressed =
            rightController.TryGetFeatureValue(
                CommonUsages.triggerButton,
                out bool rightTrigger) && rightTrigger;

        if (modelRoot != null &&
            !controllerDragging &&
            rightController.TryGetFeatureValue(
                CommonUsages.primary2DAxis,
                out Vector2 rightStick) &&
            rightStick.sqrMagnitude > 0.01f)
        {
            if (rightTriggerPressed)
            {
                // Zoom pelo eixo vertical.
                ScaleModel(
                    rightStick.y * 1.5f * Time.deltaTime
                );
            }
            else
            {
                // Rotação pelo analógico direito.
                modelRoot.Rotate(
                    Vector3.up,
                    -rightStick.x * 90f * Time.deltaTime,
                    Space.World
                );

                modelRoot.Rotate(
                    Vector3.right,
                    rightStick.y * 90f * Time.deltaTime,
                    Space.Self
                );
            }
        }
    }

    // ==========================================
    // RAY DOS CONTROLES
    // ==========================================

    private bool TryGetControllerRay(
        InputDevice controller,
        out Ray controllerRay)
    {
        if (controller.TryGetFeatureValue(
                CommonUsages.devicePosition,
                out Vector3 position) &&
            controller.TryGetFeatureValue(
                CommonUsages.deviceRotation,
                out Quaternion rotation))
        {
            Vector3 worldPosition = trackingOrigin != null
                ? trackingOrigin.TransformPoint(position)
                : position;

            Quaternion worldRotation = trackingOrigin != null
                ? trackingOrigin.rotation * rotation
                : rotation;

            controllerRay = new Ray(
                worldPosition,
                worldRotation * Vector3.forward
            );

            return true;
        }

        controllerRay = default;
        return false;
    }

    // ==========================================
    // SELEÇÃO ANATÔMICA
    // ==========================================

    private void SelectFromRay(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            AnatomicalPart part =
                hit.collider.GetComponentInParent<AnatomicalPart>();

            if (part != null)
            {
                SelectPart(part);
            }
        }
    }

    private void SelectPart(AnatomicalPart part)
    {
        if (selectedPart != null)
        {
            selectedPart.SetSelected(false);
        }

        selectedPart = part;

        if (selectedPart != null)
        {
            selectedPart.SetSelected(true);
        }

        ApplyIsolation();
        RefreshInformation();
    }

    // ==========================================
    // DISSECAÇÃO E ISOLAMENTO
    // ==========================================

    private void ToggleDissection()
    {
        dissected = !dissected;

        foreach (AnatomicalPart part in parts)
        {
            part.SetDissected(dissected);
        }

        RefreshModeText();
    }

    private void ToggleIsolation()
    {
        isolated = !isolated;

        ApplyIsolation();
        RefreshModeText();
    }

    private void ApplyIsolation()
    {
        foreach (AnatomicalPart part in parts)
        {
            part.SetVisible(!isolated || part == selectedPart);
        }
    }

    // ==========================================
    // ZOOM E RESTAURAÇÃO
    // ==========================================

    private void ScaleModel(float amount)
    {
        if (modelRoot == null)
            return;

        zoomFactor = Mathf.Clamp(
            zoomFactor + amount,
            0.25f,
            4f
        );

        modelRoot.localScale = initialScale * zoomFactor;
    }

    private void ResetView()
    {
        dissected = false;
        isolated = false;
        zoomFactor = 1f;

        mouseDragging = false;
        controllerDragging = false;

        if (modelRoot != null)
        {
            modelRoot.localScale = initialScale;
            modelRoot.localRotation = initialRotation;
            modelRoot.localPosition = initialPosition;
        }

        foreach (AnatomicalPart part in parts)
        {
            part.SetDissected(false);
            part.SetVisible(true);
        }

        RefreshInformation();
        RefreshModeText();
    }

    // ==========================================
    // INFORMAÇÕES DA INTERFACE
    // ==========================================

    private void RefreshInformation()
    {
        if (selectedPartText != null)
        {
            selectedPartText.text = selectedPart != null
                ? selectedPart.DisplayName
                : "Nenhuma estrutura selecionada";
        }

        if (detailsText != null)
        {
            detailsText.text = selectedPart != null
                ? selectedPart.Details
                : "Aponte para uma estrutura e pressione o gatilho.";
        }
    }

    private void RefreshModeText()
    {
        if (modeText != null)
        {
            string dissectionLabel = dissected
                ? "Disseccao: ativa"
                : "Disseccao: inativa";

            string isolationLabel = isolated
                ? "Isolamento: ativo"
                : "Isolamento: inativo";

            modeText.text =
                $"{dissectionLabel}  |  {isolationLabel}";
        }
    }
}