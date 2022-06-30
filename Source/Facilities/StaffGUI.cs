﻿using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using System;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    public class StaffGUI
    {
        public static float fStaff;
        public static float fMaxStaff;
        public static float fXP;
        public static GUIStyle LabelInfo;
        public static GUIStyle BoxInfo;
        public static GUIStyle ButtonSmallText;

        public static Vector2 scrollPos;

        public static Texture tKerbal = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/billeted", false);
        public static Texture tNoKerbal = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/unbilleted", false);
        public static Texture tXPGained = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/xpgained", false);
        public static Texture tXPUngained = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/xpungained", false);

        public static Boolean bIsOpen = false;
        public static float iFundsOpen2 = 0f;

        public static bool bIsBarracks = false;

        public static float TotalBarracksPool(StaticInstance selectedFacility)
        {
            float fKerbals = 0f;

            foreach (StaticInstance instance in StaticDatabase.GetAllStatics())
            {
                //if ((string)obj.model.getSetting("DefaultFacilityType") == "None") continue;

                if (instance.FacilityType != "Barracks")
                {
                    if (instance.model.DefaultFacilityType != "Barracks") continue;
                }

                var dist = Vector3.Distance(selectedFacility.position, instance.position);
                if (dist > 5000f) continue;

                Barracks foundBarracks = instance.gameObject.GetComponent<Barracks>();

                fKerbals = fKerbals + foundBarracks.ProductionRateCurrent;


            }

            return fKerbals;
        }

        public static StaticInstance NearestBarracks(StaticInstance selectedFacility, bool bUnassigned = true)
        {
            StaticInstance soNearest = null;
            float fKerbals = 0f;

            foreach (StaticInstance instance in StaticDatabase.GetAllStatics())
            {
                //if ((string)obj.model.getSetting("DefaultFacilityType") == "None") continue;

                if (instance.FacilityType != "Barracks")
                {
                    if (instance.model.DefaultFacilityType != "Barracks") continue;
                }

                if (instance.CelestialBody.name == FlightGlobals.currentMainBody.name)
                {
                    var dist = Vector3.Distance(selectedFacility.position, instance.position);
                    if (dist > 5000f) continue;
                }
                else
                    continue;

                Barracks foundBarracks = instance.gameObject.GetComponent<Barracks>();
                if (bUnassigned)
                {
                    fKerbals = foundBarracks.ProductionRateCurrent;

                    if (fKerbals < 1) continue;
                    else
                    {
                        soNearest = instance;
                        break;
                    }
                }
                else
                {
                    if (foundBarracks.StaffCurrent == 1) continue;

                    if ((foundBarracks.StaffCurrent - 1f) == foundBarracks.ProductionRateCurrent)
                        continue;
                    else
                    {
                        soNearest = instance;
                        break;
                    }
                }
            }

            return soNearest;
        }

        public static void DrawFromBarracks(StaticInstance selectedFacility)
        {
            Barracks foundBarracks = selectedFacility.gameObject.GetComponent<Barracks>();
            foundBarracks.ProductionRateCurrent--;
        }

        public static void UnassignToBarracks(StaticInstance selectedFacility)
        {
            Barracks foundBarracks = selectedFacility.gameObject.GetComponent<Barracks>();
            foundBarracks.ProductionRateCurrent++;
        }

        public static void StaffingInterface(StaticInstance selectedFacility)
        {
            Barracks myBarracks = selectedFacility.myFacilities[0] as Barracks;
            LabelInfo = new GUIStyle(GUI.skin.label);
            LabelInfo.normal.background = null;
            LabelInfo.normal.textColor = Color.white;
            LabelInfo.fontSize = 13;
            LabelInfo.fontStyle = FontStyle.Bold;
            LabelInfo.padding.left = 3;
            LabelInfo.padding.top = 0;
            LabelInfo.padding.bottom = 0;

            BoxInfo = new GUIStyle(GUI.skin.box);
            BoxInfo.normal.textColor = Color.cyan;
            BoxInfo.fontSize = 13;
            BoxInfo.padding.top = 2;
            BoxInfo.padding.bottom = 1;
            BoxInfo.padding.left = 5;
            BoxInfo.padding.right = 5;
            BoxInfo.normal.background = null;

            ButtonSmallText = new GUIStyle(GUI.skin.button);
            ButtonSmallText.fontSize = 12;
            ButtonSmallText.fontStyle = FontStyle.Normal;


            bIsBarracks = false;

            if (selectedFacility.FacilityType == "Barracks")
                bIsBarracks = true;
            else
                if (selectedFacility.model.DefaultFacilityType == "Barracks")
                bIsBarracks = true;

            // check if we can access the staffing variables
            if (selectedFacility.FacilityType == "Barracks" || selectedFacility.FacilityType == "Business" || selectedFacility.FacilityType == "Research")
            {
                fStaff = myBarracks.StaffCurrent;
                fMaxStaff = myBarracks.StaffMax;

                if (fMaxStaff < 1)
                {
                    fMaxStaff = selectedFacility.model.DefaultStaffMax;

                    if (fMaxStaff < 1)
                    {
                        myBarracks.StaffMax = 0;
                    }
                    else
                    {
                        myBarracks.StaffMax = fMaxStaff;
                    }
                }
            }
            else
            {
                // we don't have any staffing
                fStaff = 0;
                fMaxStaff = 0;
            }


            if (fMaxStaff > 0)
            {
                float fHireFundCost = 5000;
                float fFireRefund = 2500;
                float fFireRepCost = 1;

                bIsOpen = myBarracks.isOpen;

                if (!bIsOpen)
                {
                    iFundsOpen2 = (float)selectedFacility.model.cost;
                    if (iFundsOpen2 == 0) bIsOpen = true;
                }

                GUILayout.Space(5);

                float CountCurrent = fStaff;
                float CountEmpty = fMaxStaff - fStaff;
                float funassigned = myBarracks.ProductionRateCurrent;

                scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(58));
                {
                    GUILayout.BeginHorizontal();
                    {
                        while (CountCurrent > 0)
                        {
                            GUILayout.Box(tKerbal, GUILayout.Width(23));
                            CountCurrent = CountCurrent - 1;
                        }

                        while (CountEmpty > 0)
                        {
                            GUILayout.Box(tNoKerbal, GUILayout.Width(23));
                            CountEmpty = CountEmpty - 1;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();

                GUI.enabled = bIsOpen;

                if (!bIsBarracks)
                {
                    GUILayout.Box("Assigned Staff: " + fStaff.ToString("#0") + "/" + fMaxStaff.ToString("#0"), BoxInfo);
                }
                if (bIsBarracks)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Staff: " + fStaff.ToString("#0") + "/" + fMaxStaff.ToString("#0"), LabelInfo);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Unassigned: " + funassigned.ToString("#0") + "/" + fStaff.ToString("#0"), LabelInfo);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Hire", ButtonSmallText, GUILayout.Height(20)))
                        {
                            if (fStaff == fMaxStaff)
                            {
                                MiscUtils.HUDMessage("Facility is full.", 10,
                                    3);
                            }
                            else
                            {
                                double currentfunds = Funding.Instance.Funds;

                                if (fHireFundCost > currentfunds)
                                    MiscUtils.HUDMessage("Insufficient funds to hire staff!", 10,
                                        3);
                                else
                                {
                                    myBarracks.StaffCurrent = fStaff + 1;
                                    Funding.Instance.AddFunds(-fHireFundCost, TransactionReasons.Cheating);
                                    myBarracks.ProductionRateCurrent = myBarracks.ProductionRateCurrent + 1f;
                                }
                            }

                        }
                        if (GUILayout.Button("Fire", ButtonSmallText, GUILayout.Height(20)))
                        {
                            if (fStaff < 2)
                            {
                                MiscUtils.HUDMessage("This facility must have at least one caretaker.", 10, 3);
                            }
                            else
                            {
                                if (myBarracks.ProductionRateCurrent < 1)
                                {
                                    MiscUtils.HUDMessage("All staff are assigned to duties. Staff must be unassigned in order to fire them.", 10, 3);
                                }
                                else
                                {
                                    myBarracks.StaffCurrent = fStaff - 1;
                                    myBarracks.ProductionRateCurrent = myBarracks.ProductionRateCurrent - 1f;
                                    Funding.Instance.AddFunds(fFireRefund, TransactionReasons.Cheating);
                                    Reputation.Instance.AddReputation(-fFireRepCost, TransactionReasons.Cheating);
                                }
                            }
                        }
                    }

                    GUI.enabled = true;
                    GUILayout.EndHorizontal();

                    string sHireTip = " Cost to hire next kerbal: " + fHireFundCost.ToString("#0") + " funds.";
                    string sFireTip = " Refund for firing: " + fFireRefund.ToString("#0") + " funds. Rep lost: " + fFireRepCost.ToString("#0") + ".";

                    GUILayout.Space(2);

                    if (fStaff < fMaxStaff)
                        GUILayout.Box(sHireTip, BoxInfo, GUILayout.Height(16));

                    if (fStaff > 1)
                        GUILayout.Box(sFireTip, BoxInfo, GUILayout.Height(16));
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Assign", ButtonSmallText, GUILayout.Height(20)))
                        {
                            if (fStaff == fMaxStaff)
                            {
                                MiscUtils.HUDMessage("Facility is fully staffed.", 10,
                                    3);
                            }
                            else
                            {
                                float fAvailable = TotalBarracksPool(selectedFacility);

                                if (fAvailable < 1)
                                {
                                    MiscUtils.HUDMessage("No unassigned staff available.", 10, 3);
                                }
                                else
                                {
                                    StaticInstance soNearestBarracks = NearestBarracks(selectedFacility);

                                    if (soNearestBarracks != null)
                                    {
                                        DrawFromBarracks(soNearestBarracks);

                                        myBarracks.StaffCurrent = fStaff + 1;
                                    }
                                    else
                                        MiscUtils.HUDMessage("No facility with available staff is nearby.", 10, 3);
                                }
                            }
                        }

                        if (GUILayout.Button("Unassign", ButtonSmallText, GUILayout.Height(20)))
                        {
                            if (fStaff < 2)
                            {
                                MiscUtils.HUDMessage("An open facility must have one resident caretaker.", 10, 3);
                            }
                            else
                            {
                                StaticInstance soAvailableSpace = NearestBarracks(selectedFacility, false);

                                if (soAvailableSpace != null)
                                {
                                    UnassignToBarracks(soAvailableSpace);
                                    myBarracks.StaffCurrent = fStaff - 1;
                                }
                                else
                                {
                                    MiscUtils.HUDMessage("There's no room left in a barracks or apartment for this kerbal to go to.", 10, 3);
                                }
                            }
                        }
                    }

                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(5);

            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.Box("This facility does not require staff assigned to it.", BoxInfo, GUILayout.Height(16));
            }
        }

    }
}
