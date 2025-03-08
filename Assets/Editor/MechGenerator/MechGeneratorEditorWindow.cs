using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.UIElements.Cursor;

namespace Tools
{
    public class MechGeneratorEditorWindow : EditorWindow
    {
        [MenuItem("Window/Mech Generator (Modular Params)")]
        public static void ShowWindow()
        {
            MechGeneratorEditorWindow wnd = GetWindow<MechGeneratorEditorWindow>();
            wnd.titleContent = new GUIContent("Generador de Mechs");
            wnd.minSize = new Vector2(1000, 600);
        }

        private VisualElement root;
        
        

        public void CreateGUI()
{
    // Contenedor raíz con disposición en fila
    VisualElement rootContainer = new VisualElement { name = "root-container" };
    rootContainer.style.flexDirection = FlexDirection.Row;
    rootContainer.style.width = Length.Percent(100);
    rootContainer.style.height = Length.Percent(100);

    // Construir cada panel
    VisualElement leftPanel = BuildLeftPanel();
    VisualElement centerPanel = BuildCenterPanel();
    VisualElement rightPanel = BuildRightPanel();

    // Establecer anchos iniciales
    leftPanel.style.width = 350;
    rightPanel.style.width = 300;
    centerPanel.style.flexGrow = 1;

    // Crear los redimensionadores utilizando nuestro método mejorado
    VisualElement leftResizer = CreateResizer();
    leftResizer.name = "left-resizer";
    
    VisualElement rightResizer = CreateResizer();
    rightResizer.name = "right-resizer";
    
    // Registrar eventos para el redimensionador izquierdo
    leftResizer.RegisterCallback<MouseDownEvent>(evt =>
    {
        leftResizer.CaptureMouse();
        evt.StopPropagation();
    });
    leftResizer.RegisterCallback<MouseMoveEvent>(evt =>
    {
        if (leftResizer.HasMouseCapture())
        {
            float newWidth = leftPanel.resolvedStyle.width + evt.mouseDelta.x;
            newWidth = Mathf.Clamp(newWidth, 200, 600);
            leftPanel.style.width = newWidth;
        }
    });
    leftResizer.RegisterCallback<MouseUpEvent>(evt =>
    {
        if (leftResizer.HasMouseCapture())
            leftResizer.ReleaseMouse();
    });
    
    // Registrar eventos para el redimensionador derecho
    rightResizer.RegisterCallback<MouseDownEvent>(evt =>
    {
        rightResizer.CaptureMouse();
        evt.StopPropagation();
    });
    rightResizer.RegisterCallback<MouseMoveEvent>(evt =>
    {
        if (rightResizer.HasMouseCapture())
        {
            // Se reduce o aumenta el ancho del rightPanel según el movimiento del mouse
            float newWidth = rightPanel.resolvedStyle.width - evt.mouseDelta.x;
            newWidth = Mathf.Clamp(newWidth, 200, 600);
            rightPanel.style.width = newWidth;
        }
    });
    rightResizer.RegisterCallback<MouseUpEvent>(evt =>
    {
        if (rightResizer.HasMouseCapture())
            rightResizer.ReleaseMouse();
    });

    // Agregar las columnas y los separadores al contenedor raíz en orden
    rootContainer.Add(leftPanel);
    rootContainer.Add(leftResizer);
    rootContainer.Add(centerPanel);
    rootContainer.Add(rightResizer);
    rootContainer.Add(rightPanel);

    // Agregar el contenedor raíz a la ventana del editor
    rootVisualElement.Add(rootContainer);

    // Cargar el USS para aplicar estilos adicionales
    StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/MechGenerator/MetchToolUSS.uss");
    if (styleSheet != null)
    {
        rootVisualElement.styleSheets.Add(styleSheet);
    }
}
        /// <summary>
        /// Crea un separador visual para redimensionar.
        /// </summary>
        private VisualElement CreateResizer()
        {
            VisualElement resizer = new VisualElement { name = "resizer" };
            resizer.style.width = 5;
            resizer.style.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1f); // Color visible para el resizer
    
            // Añadir un manipulador IMGUI para mostrar el cursor de redimensionamiento
            IMGUIContainer imgui = new IMGUIContainer(() =>
            {
                // Define el rectángulo local (en coordenadas del contenedor)
                Rect rect = new Rect(0, 0, resizer.resolvedStyle.width, resizer.resolvedStyle.height);
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            });
    
            imgui.style.flexGrow = 1; // Asegurar que ocupe todo el espacio del resizer
            resizer.Add(imgui);
    
            return resizer;
        }


        /// <summary>
        /// Registra los eventos del separador para modificar el ancho del panel objetivo.
        /// </summary>
        private void RegisterResizerEvents(VisualElement resizer, VisualElement targetPanel, float minWidth, float maxWidth)
        {
            resizer.RegisterCallback<MouseDownEvent>(evt =>
            {
                resizer.CaptureMouse();
                evt.StopPropagation();
            });
            resizer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (resizer.HasMouseCapture())
                {
                    // Calcular el nuevo ancho sumando el delta horizontal
                    float newWidth = targetPanel.resolvedStyle.width + evt.mouseDelta.x;
                    newWidth = Mathf.Clamp(newWidth, minWidth, maxWidth);
                    targetPanel.style.width = newWidth;
                }
            });
            resizer.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (resizer.HasMouseCapture())
                {
                    resizer.ReleaseMouse();
                }
            });
        }



        #region Panel Building

        /// <summary>
        /// Construye el panel izquierdo (parámetros).
        /// </summary>
        private VisualElement BuildLeftPanel()
        {
            VisualElement leftPanel = new VisualElement { name = "left-panel" };
            leftPanel.AddToClassList("left-panel");
            leftPanel.style.flexDirection = FlexDirection.Column;
            leftPanel.style.width = 350;
            leftPanel.style.paddingLeft = 10;
            leftPanel.style.paddingRight = 10;
            
            // Aseguramos que el panel izquierdo ocupe toda la altura disponible
            leftPanel.style.height = Length.Percent(100);
            leftPanel.style.flexGrow = 1;
            // Título
            Label leftTitle = new Label("Generador de Mechs");
            leftTitle.AddToClassList("panel-title");
            leftPanel.Add(leftTitle);

            // Seed Container
            VisualElement seedContainer = new VisualElement { name = "seed-container" };
            seedContainer.AddToClassList("seed-container");
            VisualElement seedSlider = CreateSliderInt("Seed", 0, 999999, 12345);
            seedSlider.name = "seed-slider";
            seedContainer.Add(seedSlider);
            Toggle lockSeedToggle = CreateToggle("🔒", false);
            lockSeedToggle.name = "lock-seed-toggle";
            seedContainer.Add(lockSeedToggle);
            leftPanel.Add(seedContainer);

            // Contenedor para secciones de parámetros (vacío inicialmente)
            VisualElement parametersContainer = new VisualElement { name = "parameters-container" };
            parametersContainer.AddToClassList("parameters-container");
            parametersContainer.style.flexDirection = FlexDirection.Column;
            parametersContainer.style.flexGrow = 1;
            leftPanel.Add(parametersContainer);

            // Agregar secciones de parámetros de forma modular
            parametersContainer.Add(CreateSection("Cabeza", new VisualElement[]
            {
                CreateDropdown("Tipo de Cabeza", new List<string> { "Humanoide", "Cíclope", "Insectoide", "Tanque", "Satélite" }),
                CreateSlider("Tamaño", 0.5f, 2.0f, 1.0f),
                CreateSliderInt("Sensores", 1, 8, 2),
                CreateToggle("Antenas", false)
            }));

            parametersContainer.Add(CreateSection("Torso", new VisualElement[]
            {
                CreateDropdown("Tipo de Torso", new List<string> { "Cuadrado", "Triangular", "Cilíndrico", "Blindado", "Esquelético" }),
                CreateSlider("Ancho", 0.8f, 2.5f, 1.5f),
                CreateSlider("Alto", 0.8f, 2.5f, 1.5f),
                CreateSliderInt("Nivel de Blindaje", 1, 5, 3),
                CreateDropdown("Núcleo de Energía", new List<string> { "Reactor Nuclear", "Cristal", "Solar", "Biónico", "Vapor" })
            }));

            parametersContainer.Add(CreateSection("Brazos", new VisualElement[]
            {
                CreateSliderInt("Número de Brazos", 2, 6, 2),
                CreateDropdown("Tipo de Brazos", new List<string> { "Humanoide", "Tentáculo", "Garra", "Industrial", "Arma Integrada" }),
                CreateSlider("Tamaño de Hombros", 0.5f, 2.0f, 1.0f),
                CreateToggle("Simetría", true),
                CreateDropdown("Tipo de Articulaciones", new List<string> { "Esférica", "Pistón", "Magnética", "Engranaje", "Fluida" })
            }));

            parametersContainer.Add(CreateSection("Piernas", new VisualElement[]
            {
                CreateDropdown("Tipo de Piernas", new List<string> { "Bípedo", "Cuadrúpedo", "Tanque", "Araña", "Hover" }),
                CreateSliderInt("Número de Piernas", 0, 8, 2),
                CreateSlider("Altura", 0.5f, 3.0f, 1.5f),
                CreateDropdown("Postura", new List<string> { "Militar", "Ágil", "Pesado", "Baja", "Estable" }),
                CreateDropdown("Tipo de Pies", new List<string> { "Humanoide", "Garra", "Pata", "Oruga", "Magnético" })
            }));

            parametersContainer.Add(CreateSection("Armas", new VisualElement[]
            {
                CreateDropdown("Arma Primaria", new List<string> { "Cañón de Plasma", "Ametralladora", "Lanzacohetes", "Láser", "Espada Energética" }),
                CreateDropdown("Arma Secundaria", new List<string> { "Ninguna", "Pistola", "Granadas", "Escudo", "Lanzallamas" }),
                CreateSlider("Tamaño de Armas", 0.5f, 2.0f, 1.0f),
                CreateDropdown("Habilidad Especial", new List<string> { "Ninguna", "Camuflaje", "Jetpack", "EMP", "Regeneración" }),
                CreateToggle("Armas Montadas en Hombros", false)
            }));

            parametersContainer.Add(CreateSection("Colores y Materiales", new VisualElement[]
            {
                CreateDropdown("Esquema Preestablecido", new List<string> { "Personalizado", "Militar", "Corporativo", "Insurgente", "Expedicionario" }),
                CreateColorField("Color Primario", new Color(0.2f, 0.2f, 0.2f)),
                CreateColorField("Color Secundario", new Color(0.8f, 0.1f, 0.1f)),
                CreateColorField("Color de Acento", new Color(0.1f, 0.8f, 0.8f)),
                CreateColorField("Color Emisivo", new Color(0.0f, 0.8f, 1.0f)),
                CreateSlider("Desgaste", 0.0f, 1.0f, 0.2f)
            }));
            
            // Envolver el contenedor en un ScrollView
            ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            scrollView.Add(parametersContainer);

            // Agregar el ScrollView al panel izquierdo
            leftPanel.Add(scrollView);

            // Botones de acción fijos al final
            Button generateButton = CreateButton("GENERAR MECH", "generate-button", "generate-button");
            leftPanel.Add(generateButton);

            Button randomizeButton = CreateButton("Aleatorizar", "randomize-button", "randomize-button");
            leftPanel.Add(randomizeButton);

            Button savePresetButton = CreateButton("Guardar Preset", "save-preset-button", "save-preset-button");
            leftPanel.Add(savePresetButton);

            return leftPanel;
        }

        /// <summary>
        /// Construye el panel central (vista previa, toolbar y controles).
        /// </summary>
        private VisualElement BuildCenterPanel()
        {
            VisualElement centerPanel = new VisualElement { name = "center-panel" };
            centerPanel.AddToClassList("center-panel");

            Label centerTitle = new Label("Vista Previa");
            centerTitle.AddToClassList("panel-title");
            centerPanel.Add(centerTitle);


            #region Toolbar

            VisualElement toolbar = new VisualElement { name = "toolbar" };
            toolbar.AddToClassList("toolbar");
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.Add(CreateButton("↺", "rotate-left-button", "toolbar-button"));
            toolbar.Add(CreateButton("↻", "rotate-right-button", "toolbar-button"));
            toolbar.Add(CreateButton("+", "zoom-in-button", "toolbar-button"));
            toolbar.Add(CreateButton("-", "zoom-out-button", "toolbar-button"));
            toolbar.Add(CreateButton("⟲", "reset-view-button", "toolbar-button"));
            toolbar.Add(CreateButton("📷", "screenshot-button", "toolbar-button"));
            centerPanel.Add(toolbar);

            #endregion

            #region Preview

            VisualElement previewContainer = new VisualElement { name = "preview-container" };
            previewContainer.AddToClassList("preview-container");
            centerPanel.Add(previewContainer);

            VisualElement controlsContainer = new VisualElement { name = "controls-container" };
            controlsContainer.AddToClassList("controls-container");

            #endregion
            
            #region Footer Preview

            Label techDetails = new Label("Clic y arrastra para rotar. Rueda para zoom.");
            techDetails.AddToClassList("tech-details");
            controlsContainer.Add(techDetails);
            VisualElement animationSpeedSlider = CreateSlider("Velocidad Animación", 0, 2, 1);
            animationSpeedSlider.name = "animation-speed-slider";
            controlsContainer.Add(animationSpeedSlider);
            centerPanel.Add(controlsContainer);

            #endregion

            return centerPanel;
        }

        /// <summary>
        /// Construye el panel derecho (presets y especificaciones técnicas).
        /// </summary>
        private VisualElement BuildRightPanel()
        {
            
            VisualElement rightPanel = new VisualElement { name = "right-panel" };
            rightPanel.AddToClassList("right-panel");

            #region Presets

            Label rightTitle = new Label("Presets Guardados");
            rightTitle.AddToClassList("panel-title");
            rightPanel.Add(rightTitle);

            // Contenedor de presets envuelto en ScrollView
            VisualElement presetsContainer = new VisualElement { name = "presets-container" };
            presetsContainer.AddToClassList("presets-container");
            // Envolver el contenedor en un ScrollView
            ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            scrollView.Add(presetsContainer);

            // Agregar el ScrollView al panel izquierdo
            rightPanel.Add(scrollView);
            rightPanel.Add(presetsContainer);

            #endregion
            
            #region Info
            
            Label specsTitle = new Label("Especificaciones Técnicas");
            specsTitle.style.marginTop = 20;
            specsTitle.AddToClassList("panel-title");
            rightPanel.Add(specsTitle);

            AddSpecPair(rightPanel, "Peso:", "72.5 toneladas");
            AddSpecPair(rightPanel, "Velocidad:", "45 km/h");
            AddSpecPair(rightPanel, "Blindaje:", "Nivel 4 - Titanio reforzado");
            AddSpecPair(rightPanel, "Potencia:", "15.000 kW");
            AddSpecPair(rightPanel, "Armamento:", "Cañón de Plasma + Lanzamisiles");
            AddSpecPair(rightPanel, "Clase:", "Asalto Pesado");
            AddSpecPair(rightPanel, "Especialidad:", "Combate frontal y supresión");

            Label notesLabel = new Label("Notas:");
            notesLabel.AddToClassList("specs-label");
            rightPanel.Add(notesLabel);
            TextField notesField = new TextField { multiline = true };
            notesField.name = "notes-field";
            notesField.style.height = 80;
            notesField.AddToClassList("specs-value");
            rightPanel.Add(notesField);
            
            #endregion

            return rightPanel;
        }
        #endregion

        #region Modular Section Helper

        /// <summary>
        /// Función genérica que crea un Foldout (sección) con un título y añade los controles dados.
        /// </summary>
        private VisualElement CreateSection(string title, VisualElement[] controls)
        {
            Foldout foldout = new Foldout() { text = title, value = true };
            foldout.AddToClassList("parameter-section");
            foldout.AddToClassList("collapsible-section");
            foldout.style.flexShrink = 0;
            foldout.style.marginBottom = 8;
            foreach (var ctrl in controls)
            {
                foldout.Add(ctrl);
            }
            return foldout;
        }
        #endregion

        #region Control Creation Helpers
        private DropdownField CreateDropdown(string label, List<string> choices)
        {
            DropdownField dropdown = new DropdownField(label, choices, choices[0]);
            return dropdown;
        }

        private VisualElement CreateSlider(string label, float low, float high, float value)
        {
            // Contenedor principal en fila para el título, slider y cuadro de texto
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.height = 30; // Altura fija para todos los elementos

            // Crear el label para el título y asignarle el 20% del ancho
            Label sliderTitle = new Label(label);
            sliderTitle.style.flexBasis = Length.Percent(20);
            sliderTitle.style.flexShrink = 0;
            sliderTitle.style.unityTextAlign = TextAnchor.MiddleLeft;
            sliderTitle.style.marginRight = 5; // Espacio entre el título y el slider

            // Crear el slider sin label interno y asignarle el 60% del ancho
            Slider slider = new Slider("", low, high);
            slider.value = value;
            slider.style.flexBasis = Length.Percent(60);
            slider.style.flexShrink = 0;
            slider.style.marginRight = 5; // Espacio entre el slider y el cuadro

            // Crear el TextField para mostrar y editar el valor, asignándole el 20% del ancho
            TextField valueField = new TextField();
            valueField.value = value.ToString("F2");
            valueField.style.flexBasis = Length.Percent(20);
            valueField.style.flexShrink = 0;
            valueField.style.unityTextAlign = TextAnchor.MiddleCenter;
            valueField.style.color = new Color(1f, 1f, 1f, 1f); // Color blanco

            // Sincronizar el valor del slider con el TextField
            slider.RegisterValueChangedCallback(evt =>
            {
                valueField.value = evt.newValue.ToString("F2");
            });

            // Sincronizar el valor del TextField con el slider
            valueField.RegisterValueChangedCallback(evt =>
            {
                if (float.TryParse(evt.newValue, out float newValue))
                {
                    newValue = Mathf.Clamp(newValue, low, high);
                    slider.value = newValue;
                    valueField.value = newValue.ToString("F2");
                }
                else
                {
                    // Si el valor ingresado no es válido, revertir al valor del slider
                    valueField.value = slider.value.ToString("F2");
                }
            });

            // Agregar los tres elementos al contenedor en orden
            container.Add(sliderTitle);
            container.Add(slider);
            container.Add(valueField);

            return container;
        }

        private VisualElement CreateSliderInt(string label, int low, int high, int value)
        {
            // Contenedor principal en fila para el título, slider y cuadro de texto
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.height = 30; // Altura fija para todos los elementos

            // Crear el label para el título y asignarle el 20% del ancho
            Label sliderTitle = new Label(label);
            sliderTitle.style.flexBasis = Length.Percent(20);
            sliderTitle.style.flexShrink = 0;
            sliderTitle.style.unityTextAlign = TextAnchor.MiddleLeft;
            sliderTitle.style.marginRight = 5; // Espacio entre el título y el slider

            // Crear el slider entero sin label interno y asignarle el 60% del ancho
            SliderInt sliderInt = new SliderInt("", low, high);
            sliderInt.value = value;
            sliderInt.style.flexBasis = Length.Percent(60);
            sliderInt.style.flexShrink = 0;
            sliderInt.style.marginRight = 5; // Espacio entre el slider y el cuadro

            // Crear el TextField para mostrar y editar el valor, asignándole el 20% del ancho
            TextField valueField = new TextField();
            valueField.value = value.ToString();
            valueField.style.flexBasis = Length.Percent(20);
            valueField.style.flexShrink = 0;
            valueField.style.unityTextAlign = TextAnchor.MiddleCenter;
            valueField.style.color = new Color(1f, 1f, 1f, 1f); // Texto blanco

            // Sincronizar el valor del slider con el TextField
            sliderInt.RegisterValueChangedCallback(evt =>
            {
                valueField.value = evt.newValue.ToString();
            });

            // Sincronizar el valor del TextField con el slider
            valueField.RegisterValueChangedCallback(evt =>
            {
                if (int.TryParse(evt.newValue, out int newValue))
                {
                    newValue = Mathf.Clamp(newValue, low, high);
                    sliderInt.value = newValue;
                    valueField.value = newValue.ToString();
                }
                else
                {
                    // Si el valor ingresado no es válido, revertir al valor del slider
                    valueField.value = sliderInt.value.ToString();
                }
            });

            // Agregar los tres elementos al contenedor en orden
            container.Add(sliderTitle);
            container.Add(sliderInt);
            container.Add(valueField);

            return container;
        }


        private Toggle CreateToggle(string label, bool defaultValue = false)
        {
            Toggle toggle = new Toggle(label);
            toggle.value = defaultValue;
            return toggle;
        }

        private ColorField CreateColorField(string label, Color defaultColor)
        {
            ColorField colorField = new ColorField(label);
            colorField.value = defaultColor;
            return colorField;
        }

        private Button CreateButton(string text, string name = null, string className = null)
        {
            Button button = new Button() { text = text };
            if (!string.IsNullOrEmpty(name))
                button.name = name;
            if (!string.IsNullOrEmpty(className))
                button.AddToClassList(className);
            return button;
        }
        #endregion

        #region Specs Helper
        private void AddSpecPair(VisualElement parent, string labelText, string valueText)
        {
            Label specLabel = new Label(labelText);
            specLabel.AddToClassList("specs-label");
            parent.Add(specLabel);
            Label specValue = new Label(valueText);
            specValue.AddToClassList("specs-value");
            parent.Add(specValue);
        }
        #endregion
    }
}
