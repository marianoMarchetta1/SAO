using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;

namespace IngematicaAngularBase.Deployer
{
    public class DeployerTask : Task
    {
        public override bool Execute()
        {
            ReplaceText();
            return true;
        }

        private void ReplaceText()
        {
            string str = File.ReadAllText(path + "\\" + "app.js");
            str = str.Replace("@version", DateTime.Now.Ticks.ToString());
            File.WriteAllText(path + "\\" + "app.js", str);

            string strHTML = File.ReadAllText(path + "\\" + "index.html");
            strHTML = strHTML.Replace("@version", DateTime.Now.Ticks.ToString());
            File.WriteAllText(path + "\\" + "index.html", strHTML);     
        }

        private string path;
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

    }
}
