﻿using KerbalKonstructs.UI;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    class VariantSelector
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "VariantSelector";
        internal static string windowMessage = "Choose a Model variant for: ";
        internal static string windowTitle = "Kerbal Konstructs";

        internal static Rect windowRect = new Rect(0.5f, 0.5f, windowWidth, windowHeight);

        internal static float windowWidth = 150f;
        internal static float windowHeight = 60f;

        internal static StaticInstance staticInstance;


        internal static void CreateContent()
        {
            windowMessage = "Choose a Model variant for: " + staticInstance.model.name;


            content = new List<DialogGUIBase>();

            content.Add(new DialogGUIFlexibleSpace());
            content.Add(new DialogGUIVerticalLayout(VaiantList));

        }

        internal static DialogGUIBase[] VaiantList
        {
            get
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                list.Add(new DialogGUIFlexibleSpace());
                //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

                foreach (var variant in staticInstance.model.variants)
                {
                    list.Add(new DialogGUIButton(variant.Key, delegate { SetVariant(variant.Value.name); }, 140.0f, 30.0f, true));
                }
                list.Add(new DialogGUIFlexibleSpace());
                list.Add(new DialogGUIButton("Close", () => { }, 140.0f, 30.0f, true));
                return list.ToArray();
            }
        }


        internal static void CreateMultiOptionDialog()
        {
            optionDialog = new MultiOptionDialog(windowName, windowMessage, windowTitle, HighLogic.UISkin, windowRect, content.ToArray());
        }


        internal static void CreatePopUp()
        {
            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 0.5f), optionDialog,
                   false,
                   HighLogic.UISkin, false);
        }


        internal static Vector2 CreateRectUnderMouse()
        {

            Vector2 mousePos = (Vector2)Input.mousePosition;

            Log.Normal("Parentwindow Position:" + EditorGUI.toolRect.position);


            Vector2 pos = new Vector2();

            pos﻿.x = (mousePos.x - (windowWidth / 2) - EditorGUI.windowWidth / 2) / Screen.width;
            pos.y = (mousePos.y / Screen.height);

            return (pos);
        }

        internal static Vector2 CreateBesidesMainwindow()
        {

            Vector2 editorWindowPos = EditorGUI.toolRect.position;
            Vector2 mousePos = (Vector2)Input.mousePosition;

            //Log.Normal("Parentwindow Position:" + EditorGUI.toolRect.position);


            Vector2 pos = new Vector2();
            if (editorWindowPos.x > (Screen.width / 2))
            {
                pos﻿.x = (editorWindowPos.x - (windowWidth / 2)) / Screen.width;
            }
            else
            {
                pos﻿.x = (editorWindowPos.x + ((windowWidth / 2) + EditorGUI.windowWidth)) / Screen.width;
            }

            //pos.y = 1 - (editorWindowPos.y / Screen.height);
            pos.y = (mousePos.y / Screen.height);

            return (pos);
        }



        internal static void Open()
        {
            windowRect = new Rect(CreateBesidesMainwindow(), new Vector2(windowWidth, windowHeight));

            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();
        }


        internal static void Close()
        {
            if (dialog != null)
            {
                dialog.Dismiss();
            }
            dialog = null;
            staticInstance = null;
        }


        internal static void SetVariant(string variantName)
        {
            EditorGUI.selectedInstance.VariantName = variantName;
            EditorGUI.selectedInstance.Despawn();
            EditorGUI.selectedInstance.TrySpawn();
        }


    }
}

