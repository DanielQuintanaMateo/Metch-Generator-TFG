using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tools
{
    /// <summary>
    /// Clase encargada de renderizar un objeto 3D en el editor y manejar la interacción del usuario.
    /// </summary>
    public class PreviewRenderer
    {
        // Referencia al contenedor de la vista previa
        private VisualElement previewContainer;
        
        // Objeto que se renderizará (inicialmente un cubo)
        private GameObject previewObject;
        
        // Cámara para renderizar la vista previa
        private Camera previewCamera;
        
        // Textura de renderizado
        private RenderTexture renderTexture;
        
        // Imagen UI para mostrar la textura renderizada
        private Image previewImage;
        
        // Variables para controlar la rotación y el zoom
        private Vector2 lastMousePosition;
        private float rotationSpeed = 0.3f; // Reducida para mayor precisión
        private float zoomSpeed = 0.1f;
        private float currentZoom = 5f;
        private Vector3 rotation = new Vector3(30f, 45f, 0f);
        
        // Variables para rotación orbital más intuitiva
        private Quaternion targetRotation;
        private Quaternion currentRotation;
        private float rotationDamping = 0.1f; // Suavizado de rotación
        private bool isDragging = false;
        
        // Variables para persistencia de estado durante recompilación
        [System.NonSerialized] private static bool isInitialized = false;
        [System.NonSerialized] private static Vector3 savedRotation = new Vector3(30f, 45f, 0f);
        [System.NonSerialized] private static float savedZoom = 5f;
        
        // Tamaño de la textura de renderizado
        private int textureWidth = 512;
        private int textureHeight = 512;
        
        /// <summary>
        /// Constructor que inicializa el renderizador con el contenedor de vista previa.
        /// </summary>
        /// <param name="container">Contenedor donde se mostrará la vista previa</param>
        public PreviewRenderer(VisualElement container)
        {
            previewContainer = container;
            InitializePreview();
        }
        
        /// <summary>
        /// Inicializa todos los componentes necesarios para la vista previa.
        /// </summary>
        private void InitializePreview()
        {
            // Restaurar valores guardados si ya se había inicializado antes
            if (isInitialized)
            {
                rotation = savedRotation;
                currentZoom = savedZoom;
            }
            else
            {
                isInitialized = true;
            }
            // Limpiar cualquier placeholder existente primero
            CleanupAllPlaceholders();
            
            // Crear la textura de renderizado
            renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
            renderTexture.antiAliasing = 4;
            renderTexture.filterMode = FilterMode.Bilinear;
            
            // Crear la cámara para la vista previa
            GameObject cameraObject = new GameObject("Preview Camera");
            cameraObject.hideFlags = HideFlags.HideAndDontSave; // Ocultar en la jerarquía
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Fondo gris oscuro
            previewCamera.targetTexture = renderTexture;
            previewCamera.transform.position = new Vector3(0, 0, -currentZoom);
            previewCamera.transform.LookAt(Vector3.zero);
            
            // Crear el objeto de vista previa (cubo por defecto)
            // Asegurarse de que no haya objetos previos antes de crear uno nuevo
            CleanupAllPlaceholders();
            previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewObject.hideFlags = HideFlags.HideAndDontSave; // Ocultar en la jerarquía
            
            // Crear la imagen UI para mostrar la textura
            previewImage = new Image();
            previewImage.image = renderTexture;
            previewImage.scaleMode = ScaleMode.ScaleToFit;
            previewImage.style.flexGrow = 1;
            previewContainer.Add(previewImage);
            
            // Registrar eventos de interacción
            RegisterInteractionEvents();
            
            // Iniciar el renderizado
            EditorApplication.update += UpdatePreview;
        }
        
        /// <summary>
        /// Registra los eventos de interacción para rotación y zoom.
        /// </summary>
        private void RegisterInteractionEvents()
        {
            // Inicializar rotaciones
            currentRotation = Quaternion.Euler(rotation);
            targetRotation = currentRotation;
            
            // Evento para iniciar rotación (arrastrar con el mouse)
            previewImage.RegisterCallback<MouseDownEvent>(OnMouseDown);
            
            // Evento para finalizar rotación
            previewImage.RegisterCallback<MouseUpEvent>(OnMouseUp);
            
            previewImage.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            
            previewImage.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            
            // Evento para zoom (rueda del mouse)
            previewImage.RegisterCallback<WheelEvent>(OnWheel);
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            lastMousePosition = evt.mousePosition;
            isDragging = true;
            if (previewContainer != null)
            {
                previewContainer.AddToClassList("rotating"); // Clase visual para feedback
            }
            evt.StopPropagation();
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            isDragging = false;
            if (previewContainer != null)
            {
                previewContainer.RemoveFromClassList("rotating");
            }
            evt.StopPropagation();
        }
        
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            if (isDragging && previewContainer != null)
            {
                isDragging = false;
                previewContainer.RemoveFromClassList("rotating");
            }
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (isDragging) // Botón izquierdo presionado
            {
                Vector2 delta = evt.mousePosition - lastMousePosition;
                
                // Calcular rotación relativa a la cámara
                // El movimiento horizontal del mouse rota alrededor del eje Y global
                // El movimiento vertical del mouse rota alrededor del eje X local
                rotation.y += delta.x * rotationSpeed;
                rotation.x -= delta.y * rotationSpeed;
                
                // Limitar la rotación en X para evitar giros extraños
                rotation.x = Mathf.Clamp(rotation.x, -89f, 89f);
                
                // Actualizar la rotación objetivo usando Euler angles
                targetRotation = Quaternion.Euler(rotation);
                
                // Guardar la rotación actual para persistencia
                savedRotation = rotation;
                
                lastMousePosition = evt.mousePosition;
                evt.StopPropagation();
            }
        }
        
        private void OnWheel(WheelEvent evt)
        {
            currentZoom += evt.delta.y * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, 2f, 10f); // Limitar el zoom
            
            // Guardar el zoom actual para persistencia
            savedZoom = currentZoom;
            
            evt.StopPropagation();
        }
        
        
        /// <summary>
        /// Actualiza la vista previa en cada frame.
        /// </summary>
        private void UpdatePreview()
        {
            // Verificar que todos los objetos necesarios existan
            if (previewObject == null || previewCamera == null || previewContainer == null || renderTexture == null) 
            {
                // Si faltan objetos esenciales, intentar reinicializar
                if (previewContainer != null)
                {
                    try
                    {
                        InitializePreview();
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Error al reinicializar la vista previa: " + ex.Message);
                        return;
                    }
                }
                return;
            }
            
            try
            {
                // Suavizar la rotación para que sea más natural
                currentRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationDamping);
                
                // Actualizar la rotación del objeto con interpolación suave
                if (previewObject != null && previewObject.transform != null)
                {
                    // Aplicar la rotación al objeto directamente
                    previewObject.transform.rotation = currentRotation;
                }
                
                // Actualizar la posición de la cámara para el zoom
                if (previewCamera != null && previewCamera.transform != null)
                {
                    previewCamera.transform.position = new Vector3(0, 0, -currentZoom);
                    previewCamera.transform.LookAt(Vector3.zero);
                    
                    // Forzar el renderizado
                    previewCamera.Render();
                }
                
                // Solicitar repintado del contenedor
                if (previewContainer != null)
                {
                    previewContainer.MarkDirtyRepaint();
                }
            }
            catch (System.Exception e)
            {
                // Si ocurre algún error, detener el bucle de actualización
                Debug.LogError("Error en UpdatePreview: " + e.Message);
                EditorApplication.update -= UpdatePreview;
            }
        }
        
        /// <summary>
        /// Reemplaza el objeto de vista previa actual por uno nuevo.
        /// </summary>
        /// <param name="newObject">Nuevo objeto para mostrar</param>
        public void SetPreviewObject(GameObject newObject)
        {
            // Eliminar todos los placeholders anteriores
            CleanupAllPlaceholders();
            
            // Asegurarse de que el nuevo objeto no sea nulo
            if (newObject != null)
            {
                // Crear una copia del objeto para evitar problemas de referencia
                previewObject = Object.Instantiate(newObject);
                previewObject.hideFlags = HideFlags.HideAndDontSave;
                
                // Asegurarse de que el objeto esté en la posición correcta
                previewObject.transform.position = Vector3.zero;
            }
            
            // Resetear la rotación y el zoom
            rotation = new Vector3(30f, 45f, 0f);
            targetRotation = Quaternion.Euler(rotation);
            currentRotation = targetRotation;
            currentZoom = 5f;
            
            // Guardar los valores para persistencia
            savedRotation = rotation;
            savedZoom = currentZoom;
        }
        
        /// <summary>
        /// Elimina todos los objetos placeholder que pudieran haber quedado en la escena.
        /// </summary>
        private void CleanupAllPlaceholders()
        {
            // Destruir explícitamente el objeto de vista previa actual si existe
            if (previewObject != null)
            {
                Object.DestroyImmediate(previewObject);
                previewObject = null;
            }
            
            // Buscar todos los GameObjects en la escena, incluyendo los inactivos
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            
            // Eliminar todos los objetos que sean placeholders (con HideFlags.HideAndDontSave)
            foreach (GameObject obj in allObjects)
            {
                // Verificar si el objeto no es la cámara de vista previa y tiene el flag HideAndDontSave
                if (obj != null && 
                    (previewCamera == null || obj != previewCamera.gameObject) && 
                    (obj.hideFlags & HideFlags.HideAndDontSave) == HideFlags.HideAndDontSave)
                {
                    try
                    {
                        // Verificar si el objeto es un objeto interno de Unity que no debe ser destruido
                        string objName = obj.name;
                        if (!objName.Contains("InternalIdentityTransform") && 
                            !objName.StartsWith("Unity") && 
                            !objName.StartsWith("Editor"))
                        {
                            // Asegurarse de que el objeto sea destruido inmediatamente
                            Object.DestroyImmediate(obj, true);
                        }
                    }
                    catch (System.Exception e)
                    {
                        // Registrar el error pero continuar con la limpieza
                        Debug.LogWarning("No se pudo destruir el objeto: " + e.Message);
                    }
                }
            }
            
            // Forzar la recolección de basura para liberar memoria
            System.GC.Collect();
        }
        
        /// <summary>
        /// Limpia los recursos cuando se cierra la ventana.
        /// </summary>
        public void Cleanup()
        {
            // Detener el bucle de actualización
            EditorApplication.update -= UpdatePreview;
            
            // Desregistrar eventos de interacción si la imagen existe
            if (previewImage != null)
            {
                previewImage.UnregisterCallback<MouseDownEvent>(OnMouseDown);
                previewImage.UnregisterCallback<MouseUpEvent>(OnMouseUp);
                previewImage.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
                previewImage.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
                previewImage.UnregisterCallback<WheelEvent>(OnWheel);
            }
            
            // Limpiar todos los placeholders
            CleanupAllPlaceholders();
            
            // Destruir la cámara de vista previa
            if (previewCamera != null)
            {
                Object.DestroyImmediate(previewCamera.gameObject, true);
                previewCamera = null;
            }
            
            // Liberar la textura de renderizado
            if (renderTexture != null)
            {
                renderTexture.Release();
                Object.DestroyImmediate(renderTexture);
                renderTexture = null;
            }
            
            // Eliminar la imagen UI
            if (previewImage != null && previewContainer != null)
            {
                previewContainer.Remove(previewImage);
                previewImage = null;
            }
            
            // Forzar la recolección de basura
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
        
        /// <summary>
        /// Rota el objeto a la izquierda (90 grados en Y).
        /// </summary>
        public void RotateLeft()
        {
            rotation.y -= 90f;
            targetRotation = Quaternion.Euler(rotation);
            savedRotation = rotation;
        }
        
        /// <summary>
        /// Rota el objeto a la derecha (90 grados en Y).
        /// </summary>
        public void RotateRight()
        {
            rotation.y += 90f;
            targetRotation = Quaternion.Euler(rotation);
            savedRotation = rotation;
        }
        
        /// <summary>
        /// Acerca la cámara al objeto.
        /// </summary>
        public void ZoomIn()
        {
            currentZoom = Mathf.Max(2f, currentZoom - 1f);
            savedZoom = currentZoom;
        }
        
        /// <summary>
        /// Aleja la cámara del objeto.
        /// </summary>
        public void ZoomOut()
        {
            currentZoom = Mathf.Min(10f, currentZoom + 1f);
            savedZoom = currentZoom;
        }

        /// <summary>
        /// Restablece la vista a los valores predeterminados.
        /// </summary>
        public void ResetView()
        {
            rotation = new Vector3(30f, 45f, 0f);
            targetRotation = Quaternion.Euler(rotation);
            currentRotation = targetRotation;
            currentZoom = 5f;

            // Guardar los valores para persistencia
            savedRotation = rotation;
            savedZoom = currentZoom;

        }

        /// <summary>
        /// Captura una imagen de la vista actual.
        /// </summary>
        public void TakeScreenshot()
        {
            string path = EditorUtility.SaveFilePanel(
                "Guardar Captura",
                "",
                "MechPreview.png",
                "png");
                
            if (string.IsNullOrEmpty(path)) return;
            
            RenderTexture.active = renderTexture;
            Texture2D screenshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;
            
            byte[] bytes = screenshot.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            
            Object.DestroyImmediate(screenshot);
            
            EditorUtility.RevealInFinder(path);
        }
    }
}