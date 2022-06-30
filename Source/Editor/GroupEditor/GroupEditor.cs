﻿using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    public class GroupEditor : KKWindow
    {

        private static GroupEditor _instance = null;
        public static GroupEditor instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GroupEditor();

                }
                return _instance;
            }
        }

        #region Variable Declarations
        private List<Transform> transformList = new List<Transform>();
        private CelestialBody body;
        private bool showNameField = false;

        private string newGroupName = "";

        internal Boolean foldedIn = false;
        internal Boolean doneFold = false;



        #region Texture Definitions
        // Texture definitions
        internal Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep2", false);
        internal Texture tCopyPos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/copypos", false);
        internal Texture tPastePos = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/pastepos", false);
        internal Texture tSnap = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/snapto", false);
        internal Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
        internal Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        internal Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);


        #endregion

        #region Switches


        #endregion

        #region GUI Windows
        // GUI Windows
        internal Rect toolRect = new Rect(300, 35, 330, 350);

        #endregion


        #region Holders
        // Holders

        internal static GroupCenter selectedGroup = null;
        internal GroupCenter selectedObjectPrevious = null;

        internal string refLat, refLng, headingStr;

        //internal static String facType = "None";
        //internal static String sGroup = "Ungrouped";
        private float increment = 1f;


        private VectorRenderer upVR = new VectorRenderer();
        private VectorRenderer fwdVR = new VectorRenderer();
        private VectorRenderer rightVR = new VectorRenderer();

        private VectorRenderer northVR = new VectorRenderer();
        private VectorRenderer eastVR = new VectorRenderer();


        private static Space referenceSystem = Space.Self;

        private static Vector3d position = Vector3d.zero;
        private Vector3d savedReferenceVector = Vector3d.zero;


        private static Vector3 startPosition = Vector3.zero;

        internal static float maxEditorRange = 250;

        #endregion

        #endregion

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }

            if ((selectedGroup != null))
            {
                drawEditor(selectedGroup);
            }
        }


        public override void Close()
        {

            if (KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.disable();
            }

            CloseVectors();
            EditorGizmo.CloseGizmo();
            CloseEditors();
            selectedObjectPrevious = null;
            base.Close();
        }

        #region draw Methods

        /// <summary>
        /// Wrapper to draw editors
        /// </summary>
        /// <param name="groupCenter"></param>
        internal void drawEditor(GroupCenter groupCenter)
        {
            if (groupCenter == null)
            {
                return;
            }

            if (selectedObjectPrevious != groupCenter)
            {
                selectedObjectPrevious = groupCenter;
                SetupVectors();
                UpdateStrings();
                EditorGizmo.SetupMoveGizmo(groupCenter.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
                if (!KerbalKonstructs.camControl.active)
                {
                    KerbalKonstructs.camControl.enable(groupCenter.gameObject);
                }

            }

            toolRect = GUI.Window(0xB07B1E3, toolRect, GroupEditorWindow, "", UIMain.KKWindow);

        }

        #endregion

        #region Editors

        #region Instance Editor

        /// <summary>
        /// Instance Editor window
        /// </summary>
        /// <param name="windowID"></param>
        void GroupEditorWindow(int windowID)
        {

            UpdateVectors();

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Group Editor", UIMain.DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", UIMain.DeadButtonRed, GUILayout.Height(21)))
                {
                    //KerbalKonstructs.instance.saveObjects();
                    //KerbalKonstructs.instance.deselectObject(true, true);
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(selectedGroup.Group, GUILayout.Height(23)))
            {
                showNameField = true;
                newGroupName = selectedGroup.Group;
            }
            GUILayout.EndHorizontal();

            if (showNameField)
            {

                GUILayout.Label("Enter new Name: ");

                newGroupName = GUILayout.TextField(newGroupName, 15, GUILayout.Width(150));

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("OK", GUILayout.Height(23)))
                    {
                        selectedGroup.RenameGroup(newGroupName);
                        showNameField = false;
                    }
                    if (GUILayout.Button("Cancel", GUILayout.Height(23)))
                    {
                        showNameField = false;
                    }
                }
                GUILayout.EndHorizontal();

            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (!foldedIn)
                {
                    GUILayout.Label("Increment");
                    increment = float.Parse(GUILayout.TextField(increment.ToString(), 5, GUILayout.Width(48)));

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("0.001", GUILayout.Height(18)))
                    {
                        increment = 0.001f;
                    }
                    if (GUILayout.Button("0.01", GUILayout.Height(18)))
                    {
                        increment = 0.01f;
                    }
                    if (GUILayout.Button("0.1", GUILayout.Height(18)))
                    {
                        increment = 0.1f;
                    }
                    if (GUILayout.Button("1", GUILayout.Height(18)))
                    {
                        increment = 1f;
                    }
                    if (GUILayout.Button("10", GUILayout.Height(18)))
                    {
                        increment = 10f;
                    }
                    if (GUILayout.Button("25", GUILayout.Height(16)))
                    {
                        increment = 25f;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                else
                {
                    GUILayout.Label("i");
                    increment = float.Parse(GUILayout.TextField(increment.ToString(), 3, GUILayout.Width(25)));

                    if (GUILayout.Button("0.1", GUILayout.Height(23)))
                    {
                        increment = 0.1f;
                    }
                    if (GUILayout.Button("1", GUILayout.Height(23)))
                    {
                        increment = 1f;
                    }
                    if (GUILayout.Button("10", GUILayout.Height(23)))
                    {
                        increment = 10f;
                    }
                }
            }
            GUILayout.EndHorizontal();

            //
            // Set reference butons
            //
            GUILayout.BeginHorizontal();
            GUILayout.Label("Reference System: ");
            GUILayout.FlexibleSpace();
            GUI.enabled = (referenceSystem == Space.World);

            if (GUILayout.Button(new GUIContent(UIMain.iconCubes, "Model"), GUILayout.Height(23), GUILayout.Width(23)))
            {
                referenceSystem = Space.Self;
                UpdateVectors();
            }

            GUI.enabled = (referenceSystem == Space.Self);
            if (GUILayout.Button(new GUIContent(UIMain.iconWorld, "World"), GUILayout.Height(23), GUILayout.Width(23)))
            {
                referenceSystem = Space.World;
                UpdateVectors();
            }
            GUI.enabled = true;

            GUILayout.Label(referenceSystem.ToString());

            GUILayout.EndHorizontal();
            float fTempWidth = 80f;
            //
            // Position editing
            //
            GUILayout.BeginHorizontal();

            if (referenceSystem == Space.Self)
            {
                GUILayout.Label("Back / Forward:");
                GUILayout.FlexibleSpace();

                if (foldedIn)
                    fTempWidth = 40f;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.back * increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.forward * increment);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Left / Right:");
                GUILayout.FlexibleSpace();
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.left * increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    SetTransform(Vector3.right * increment);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

            }
            else
            {
                GUILayout.Label("West / East :");
                GUILayout.FlexibleSpace();

                if (foldedIn)
                    fTempWidth = 40f;

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(0d, -increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(0d, increment);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("South / North:");
                GUILayout.FlexibleSpace();
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(-increment, 0d);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    Setlatlng(increment, 0d);
                }
            }

            GUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Lat: ");
                GUILayout.FlexibleSpace();
                refLat = GUILayout.TextField(refLat, 10, GUILayout.Width(fTempWidth));

                GUILayout.Label("  Lng: ");
                GUILayout.FlexibleSpace();
                refLng = GUILayout.TextField(refLng, 10, GUILayout.Width(fTempWidth));
            }
            GUILayout.EndHorizontal();

            // 
            // Altitude editing
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Alt.");
                GUILayout.FlexibleSpace();
                selectedGroup.RadiusOffset = float.Parse(GUILayout.TextField(selectedGroup.RadiusOffset.ToString(), 25, GUILayout.Width(fTempWidth)));
                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    selectedGroup.RadiusOffset -= increment;
                    ApplySettings();
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
                {
                    selectedGroup.RadiusOffset += increment;
                    ApplySettings();
                }
            }
            GUILayout.EndHorizontal();


            GUI.enabled = true;

            GUILayout.Space(5);



            //
            // Rotation
            //
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Rotation:");
                GUILayout.FlexibleSpace();
                headingStr = GUILayout.TextField(headingStr, 9, GUILayout.Width(fTempWidth));

                if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    SetRotation(-increment);
                }
                if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    SetRotation(-increment);
                }
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    SetRotation(increment);
                }
                if (GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
                {
                    SetRotation(increment);
                }
            }
            GUILayout.EndHorizontal();


            GUILayout.Space(1);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("SeaLevel as Reference:");
                GUILayout.FlexibleSpace();
                selectedGroup.SeaLevelAsReference = GUILayout.Toggle(selectedGroup.SeaLevelAsReference, "", GUILayout.Width(140), GUILayout.Height(23));
            }
            GUILayout.EndHorizontal();

            GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));



            GUILayout.Space(2);
            GUILayout.Space(5);



            GUI.enabled = true;



            GUI.enabled = true;
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = true;
                if (GUILayout.Button("Save&Close", GUILayout.Width(110), GUILayout.Height(23)))
                {
                    selectedGroup.Save();
                    this.Close();
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Destroy Group", GUILayout.Height(21)))
                {
                    DeleteGroupCenter();
                }

            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, UIMain.BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            if (GUI.tooltip != "")
            {
                var labelSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(GUI.tooltip));
                GUI.Box(new Rect(Event.current.mousePosition.x - (25 + (labelSize.x / 2)), Event.current.mousePosition.y - 40, labelSize.x + 10, labelSize.y + 5), GUI.tooltip);
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }


        #endregion


        /// <summary>
        /// closes the sub editor windows
        /// </summary>
        public static void CloseEditors()
        {
            FacilityEditor.instance.Close();
            LaunchSiteEditor.instance.Close();
        }


        #endregion

        #region Utility Functions


        internal void DeleteGroupCenter()
        {
            if (selectedObjectPrevious == selectedGroup)
            {
                selectedObjectPrevious = null;
            }

            InputLockManager.RemoveControlLock("KKShipLock");
            InputLockManager.RemoveControlLock("KKEVALock");
            InputLockManager.RemoveControlLock("KKCamModes");


            if (KerbalKonstructs.camControl.active)
            {
                KerbalKonstructs.camControl.disable();
            }


            if (selectedGroup == StaticsEditorGUI.GetActiveGroup())
            {
                StaticsEditorGUI.SetActiveGroup(null);
            }

            selectedGroup.DeleteGroupCenter();

            selectedGroup = null;


            StaticsEditorGUI.ResetLocalGroupList();
            this.Close();
        }



        /// <summary>
        /// the starting position of direction vectors (a bit right and up from the Objects position)
        /// </summary>
        private Vector3 vectorDrawPosition
        {
            get
            {
                return (selectedGroup.gameObject.transform.position);
            }
        }


        /// <summary>
        /// returns the heading the selected object
        /// </summary>
        /// <returns></returns>
        public float heading
        {
            get
            {
                Vector3 myForward = Vector3.ProjectOnPlane(selectedGroup.gameObject.transform.forward, upVector);
                float myHeading;

                if (Vector3.Dot(myForward, eastVector) > 0)
                {
                    myHeading = Vector3.Angle(myForward, northVector);
                }
                else
                {
                    myHeading = 360 - Vector3.Angle(myForward, northVector);
                }
                return myHeading;
            }
        }

        /// <summary>
        /// gives a vector to the east
        /// </summary>
        private Vector3 eastVector
        {
            get
            {
                return Vector3.Cross(upVector, northVector).normalized;
            }
        }

        /// <summary>
        /// vector to north
        /// </summary>
        private Vector3 northVector
        {
            get
            {
                body = FlightGlobals.ActiveVessel.mainBody;
                return Vector3.ProjectOnPlane(body.transform.up, upVector).normalized;
            }
        }

        private Vector3 upVector
        {
            get
            {
                body = FlightGlobals.ActiveVessel.mainBody;
                return (Vector3)body.GetSurfaceNVector(selectedGroup.RefLatitude, selectedGroup.RefLongitude).normalized;
            }
        }

        /// <summary>
        /// Sets the vectors active and updates thier position and directions
        /// </summary>
        private void UpdateVectors()
        {
            if (selectedGroup == null)
            {
                return;
            }

            if (referenceSystem == Space.Self)
            {
                fwdVR.SetShow(true);
                upVR.SetShow(true);
                rightVR.SetShow(true);

                northVR.SetShow(false);
                eastVR.SetShow(false);

                fwdVR.Vector = selectedGroup.gameObject.transform.forward;
                fwdVR.Start = vectorDrawPosition;
                fwdVR.Draw();

                upVR.Vector = selectedGroup.gameObject.transform.up;
                upVR.Start = vectorDrawPosition;
                upVR.Draw();

                rightVR.Vector = selectedGroup.gameObject.transform.right;
                rightVR.Start = vectorDrawPosition;
                rightVR.Draw();
            }
            if (referenceSystem == Space.World)
            {
                northVR.SetShow(true);
                eastVR.SetShow(true);

                fwdVR.SetShow(false);
                upVR.SetShow(false);
                rightVR.SetShow(false);

                northVR.Vector = northVector;
                northVR.Start = vectorDrawPosition;
                northVR.Draw();

                eastVR.Vector = eastVector;
                eastVR.Start = vectorDrawPosition;
                eastVR.Draw();
            }
        }

        /// <summary>
        /// creates the Vectors for later display
        /// </summary>
        private void SetupVectors()
        {
            // draw vectors
            fwdVR.Color = new Color(0, 0, 1);
            fwdVR.Vector = selectedGroup.gameObject.transform.forward;
            fwdVR.Scale = 30d;
            fwdVR.Start = vectorDrawPosition;
            fwdVR.SetLabel("forward");
            fwdVR.Width = 0.01d;
            fwdVR.SetLayer(5);

            upVR.Color = new Color(0, 1, 0);
            upVR.Vector = selectedGroup.gameObject.transform.up;
            upVR.Scale = 30d;
            upVR.Start = vectorDrawPosition;
            upVR.SetLabel("up");
            upVR.Width = 0.01d;

            rightVR.Color = new Color(1, 0, 0);
            rightVR.Vector = selectedGroup.gameObject.transform.right;
            rightVR.Scale = 30d;
            rightVR.Start = vectorDrawPosition;
            rightVR.SetLabel("right");
            rightVR.Width = 0.01d;

            northVR.Color = new Color(0.9f, 0.3f, 0.3f);
            northVR.Vector = northVector;
            northVR.Scale = 30d;
            northVR.Start = vectorDrawPosition;
            northVR.SetLabel("north");
            northVR.Width = 0.01d;

            eastVR.Color = new Color(0.3f, 0.3f, 0.9f);
            eastVR.Vector = eastVector;
            eastVR.Scale = 30d;
            eastVR.Start = vectorDrawPosition;
            eastVR.SetLabel("east");
            eastVR.Width = 0.01d;
        }

        /// <summary>
        /// stops the drawing of the vectors
        /// </summary>
        private void CloseVectors()
        {
            northVR.SetShow(false);
            eastVR.SetShow(false);
            fwdVR.SetShow(false);
            upVR.SetShow(false);
            rightVR.SetShow(false);
        }

        /// <summary>
        /// sets the latitude and lognitude from the deltas of north and east and creates a new reference vector
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        internal void Setlatlng(double north, double east)
        {
            body = Planetarium.fetch.CurrentMainBody;
            double latOffset = north / (body.Radius * KKMath.deg2rad);
            selectedGroup.RefLatitude += latOffset;
            double lonOffset = east / (body.Radius * KKMath.deg2rad);
            selectedGroup.RefLongitude += lonOffset * Math.Cos(Mathf.Deg2Rad * selectedGroup.RefLatitude);

            selectedGroup.RadialPosition = body.GetRelSurfaceNVector(selectedGroup.RefLatitude, selectedGroup.RefLongitude).normalized * body.Radius;
            ApplySettings();
        }





        /// <summary>
        /// changes the rotation by a defined amount
        /// </summary>
        /// <param name="increment"></param>
        internal void SetRotation(float increment)
        {
            selectedGroup.RotationAngle += (float)increment;
            selectedGroup.RotationAngle = (360f + selectedGroup.RotationAngle) % 360f;
            ApplySettings();
        }


        /// <summary>
        /// Updates the StaticObject position with a new transform
        /// </summary>
        /// <param name="direction"></param>
        internal void SetTransform(Vector3 direction)
        {
            // adjust transform for scaled models
            direction = direction / selectedGroup.ModelScale;
            direction = selectedGroup.gameObject.transform.TransformVector(direction);
            double northInc = Vector3d.Dot(northVector, direction);
            double eastInc = Vector3d.Dot(eastVector, direction);

            Setlatlng(northInc, eastInc);

        }


        internal void OnMoveCallBack(Vector3 vector)
        {
            // Log.Normal("OnMove: " + vector.ToString());
            //moveGizmo.transform.position += 3* vector;

            selectedGroup.gameObject.transform.position = EditorGizmo.moveGizmo.transform.position;
            selectedGroup.RadialPosition = selectedGroup.gameObject.transform.localPosition;

            double alt;
            selectedGroup.CelestialBody.GetLatLonAlt(EditorGizmo.moveGizmo.transform.position, out selectedGroup.RefLatitude, out selectedGroup.RefLongitude, out alt);

            selectedGroup.RadiusOffset = (float)(alt - selectedGroup.surfaceHeight);
            //float oldY = selectedInstance.gameObject.transform.localPosition.y;

        }

        internal void WhenMovedCallBack(Vector3 vector)
        {
            ApplySettings();
            //Log.Normal("WhenMoved: " + vector.ToString());
        }

        internal void UpdateMoveGizmo()
        {
            EditorGizmo.CloseGizmo();
            EditorGizmo.SetupMoveGizmo(selectedGroup.gameObject, Quaternion.identity, OnMoveCallBack, WhenMovedCallBack);
        }



        internal void ApplyInputStrings()
        {

            selectedGroup.RefLatitude = double.Parse(refLat);
            selectedGroup.RefLongitude = double.Parse(refLng);

            selectedGroup.RadialPosition = KKMath.GetRadiadFromLatLng(selectedGroup.CelestialBody, selectedGroup.RefLatitude, selectedGroup.RefLongitude);


            float oldRotation = selectedGroup.RotationAngle;
            float tgtheading = float.Parse(headingStr);
            float diffHeading = (tgtheading - selectedGroup.heading);

            selectedGroup.RotationAngle = oldRotation + diffHeading;


            ApplySettings();




            selectedGroup.RefLatitude = double.Parse(refLat);
            selectedGroup.RefLongitude = double.Parse(refLng);
        }


        internal void UpdateStrings()
        {
            refLat = Math.Round(selectedGroup.RefLatitude, 4).ToString();
            refLng = Math.Round(selectedGroup.RefLongitude, 4).ToString();

            headingStr = Math.Round(selectedGroup.heading, 3).ToString();
        }


        /// <summary>
        /// Saves the current instance settings to the object.
        /// </summary>
        internal void ApplySettings()
        {
            selectedGroup.Update();
            UpdateStrings();
            UpdateMoveGizmo();
        }


        internal void CheckEditorKeys()
        {
            if (selectedGroup != null)
            {

                if (IsOpen())
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        SetTransform(Vector3.forward * increment);
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        SetTransform(Vector3.back * increment);
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        SetTransform(Vector3.right * increment);
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        SetTransform(Vector3.left * increment);
                    }
                    if (Input.GetKey(KeyCode.PageUp))
                    {
                        SetTransform(Vector3.up * increment);
                    }
                    if (Input.GetKey(KeyCode.PageDown))
                    {
                        SetTransform(Vector3.down * increment);
                    }
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        ApplyInputStrings();
                    }
                }

            }

        }


        #endregion
    }
}
