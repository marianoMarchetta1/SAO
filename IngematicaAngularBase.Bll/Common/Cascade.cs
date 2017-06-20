using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using IngematicaAngularBase.Model.Common;

namespace IngematicaAngularBase.Bll.Common
{
    public class SelectionListParameter
    {
        public string ListName;
        public string Parameter;
        public int Value;
    }

    public abstract class Cascade
    {
        public Dictionary<string, List<SelectionListSimple>> Lists;
        public Dictionary<string, SelectionListParameter> Parameters;

        public Cascade()
        {
            Lists = new Dictionary<string, List<SelectionListSimple>>();
            Parameters = new Dictionary<string, SelectionListParameter>();
        }
        private List<string> ListsToExecute;
        public List<string> GetListsToExecute()
        {
            if (ListsToExecute == null)
                FillListsToExecute();
            return ListsToExecute;
        }

        public string Action { get; set; }

        public string LastList { get; set; }

        private void FillListsToExecute()
        {
            switch (Action)
            {
                case "init":
                    ListsToExecute = UseLists();
                    break;
                case "update":
                    ListsToExecute = UseLists();
                    break;
                default:
                    List<string> list = new List<string>();
                    bool found = false;
                    foreach (var item in UseLists())
                    {
                        if (found)
                            list.Add(item);
                        if (item == Action)
                            found = true;
                         
                    }
                    ListsToExecute = list;
                    break;
            }
        }

        public void AddParameter(string listName, string parameter, int value)
        {
            if (!Parameters.Keys.Contains(string.Format("{0}_{1}", listName, parameter)))
            Parameters.Add(string.Format("{0}_{1}", listName, parameter),
            new SelectionListParameter()
            {
                ListName = listName,
                Parameter = parameter,
                Value = value
            });
        }

        public void GetLists()
        {
            List<string> listsToExecute = GetListsToExecute();

            bool returnEmptyList = false;
            foreach (var item in listsToExecute)
            {
                if (!returnEmptyList)
                    DoList(item);
                else
                    Lists.Add(item, new List<SelectionListSimple>());

                if ((Action != "update" && Lists[item].Count() != 1) || LastList == item)                    
                    returnEmptyList = true;                        
            }
        }

        public virtual void DoList(string listName)
        {
            var sb = new StringBuilder(listName);
            sb[0] = sb[0].ToString().ToUpper()[0];
            MethodInfo method = this.GetType().GetMethod(String.Format("Get{0}", sb.ToString()));
            method.Invoke(this, null);
        }

        public abstract List<string> UseLists();
    }


}
