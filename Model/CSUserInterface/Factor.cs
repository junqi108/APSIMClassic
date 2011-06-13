﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using CSGeneral;
using System.Collections.Specialized;

namespace CSUserInterface
{
    public partial class Factor : Controllers.BaseView
    {
        DataTable Table = null;
        
        public Factor()
        {
            InitializeComponent();
            MyHelpLabel.Visible = false;
        }
        protected override void OnLoad()
        {
            FactorTargets.OnLoad(Controller, NodePath, Data.OuterXml);
        }
        public override void OnRefresh()
        {
            FactorTargets.OnRefresh();
            //ApsimFile.Component comp = Controller.ApsimData.Find(NodePath);
            if (Table != null)
            {
                Table.Clear();
                Table.Columns.Clear();
            }
            LoadManagerVariables();
        }
        public override void OnSave()
        {
            FactorTargets.OnSave();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(FactorTargets.GetData());
            Data.InnerXml = doc.DocumentElement.InnerXml;

            if (Table != null)
            {
                XmlNode variablesNode = Data.SelectSingleNode("//vars");
                if (variablesNode == null)
                    variablesNode = Data.AppendChild(Data.OwnerDocument.CreateElement("vars"));
                variablesNode.RemoveAll();
                foreach (DataRow row in Table.Rows)
                {
                    if ((bool)row[0])
                    {
                        XmlNode varNode = variablesNode.AppendChild(Data.OwnerDocument.CreateElement(row[1].ToString()));
                        varNode.InnerText = row[3].ToString();
                    }
                }
            }
        }
        
        public override bool OnDropData(StringCollection SourcePaths, string FullXML)
        {
           //if there are no children, then add the droppednodes as targets
           //don't add to targets if there is existing children
           //don't add to targets if the source is from the Factorial Tree
           ApsimFile.Component thisComp = Controller.ApsimData.Find(NodePath);
           if (thisComp.ChildNodes.Count == 0)
           {
              FactorTargets.AddTargets(SourcePaths);
           }
           thisComp.Add(FullXML);
           OnRefresh();
           return true;
        }
         public void LoadVariableTypes(XmlNode metNode, List<string> variableTypes)
         {
            if(metNode != null && variableTypes != null)
            {
               foreach (XmlNode varNode in XmlHelper.ChildNodes(metNode, "facvar"))
               {
                  variableTypes.Add(varNode.InnerText);
               }
            }
         }

        public void LoadManagerVariables()
        {
            //if there is a single manager component as a child, then allow parameters to be defined
            //parameters will be stored in a separate node "variables"
            //we don't allow combination similar to complexfactor... yet.
            ApsimFile.Component thisComp = Controller.ApsimData.Find(NodePath);
            if (thisComp.ChildNodes.Count == 1)
            {
               ApsimFile.Component childComp = thisComp.ChildNodes[0];

               List<string> variableTypes = new List<string>();
               LoadVariableTypes(Types.Instance.MetaDataNode("Factor", "FactorVariables"), variableTypes);
               if(variableTypes.Contains(childComp.Type))
               //if (childComp.Type == "manager" || childComp.Type == "rule" || childComp.Type == "cropui" || childComp.Type == "sorghumConstants" || childComp.Type == "sorghumGenotype")
               {
                  Table = CreateTable(childComp);
               }
            }
            //gridManager.DataSourceTable = Table;
            gridManager.DataSource = Table;

            //Size the grid columns sensibly
            gridManager.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            foreach (DataGridViewColumn col in gridManager.Columns)
            {
                col.Width = col.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            if (gridManager.Columns.Count > 3)
            {
                //gridManager.Columns[1].Visible = false;
                gridManager.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

        }
        public DataTable CreateTable(ApsimFile.Component childComp)
        {
            DataTable Table = new DataTable();
            Table.Columns.Add(" ", typeof(System.Boolean));
            Table.Columns.Add("names", typeof(string));
            Table.Columns.Add("Manager Variables", typeof(string));
            Table.Columns.Add("Parameters", typeof(string));

            XmlNodeList uiNodes = childComp.ContentsAsXML.SelectNodes("//ui/* | //CustomUI/*");
            if (childComp.Type == "sorghumConstants" || childComp.Type == "sorghumGenotype")
            {
               uiNodes = childComp.ContentsAsXML.ChildNodes;
            }
            foreach (XmlNode ui in uiNodes)
            {
                DataRow newRow = Table.NewRow();
                newRow[0] = false;

                string text = "";
                if (ui.Attributes["description"] != null)
                    text = ui.Attributes["description"].Value;
                else
                    text = ui.Name;

                newRow[1] = ui.Name;
                newRow[2] = text;
                newRow[3] = ui.InnerText;

                //look for corresponding node in the variables node
                XmlNode varNode = Data.SelectSingleNode("//vars/" + ui.Name);
                if (varNode != null)
                {
                    newRow[0] = true;
                    newRow[3] = varNode.InnerText;
                }

                Table.Rows.Add(newRow);
            }
            return Table;
        }
    }
}
