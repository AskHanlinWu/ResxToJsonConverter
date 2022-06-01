using Nancy.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            label2.Text = "We only care about en-us for now. JSON file will be exported to the same folder as selected resx file";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = this.openFileDialog1.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    int fileConverted = 0;
                    label1.Text = string.Format("{0} selected", openFileDialog1.FileNames.Length);
                    // Read the files
                    foreach (String fileName in openFileDialog1.FileNames)
                    {
                        ConvertResourceFile(fileName);
                        fileConverted++;
                    }

                    MessageBox.Show(string.Format("Success! {0} files converted.", fileConverted));
                    label1.Text = string.Format("Success! {0} files converted.", fileConverted);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error!");
                label1.Text = ex.ToString();
            }
        }

        private void ConvertResourceFile(string sourceFileName)
        {

            // source: https://stackoverflow.com/questions/47631098/how-to-convert-resx-xml-file-to-json-file-in-c-sharp
            //string sourceFileName = @"C:\Users\Han\Desktop\ViewMySafety.ascx.resx";
            var xml = File.ReadAllText(sourceFileName);
            var obj = new
            {
                Texts = XElement.Parse(xml)
                    .Elements("data")
                    .Select(el => new
                    {
                        id = el.Attribute("name").Value,
                        text = el.Element("value").Value.Trim()
                    })
                    .ToList()
            };
            string json = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);


            // code above doesn't generate exactly what we want, but it's close enough. Therefore, I added the following code to generate exactly what we want.
            var dictionary = new Dictionary<string, string>();
            for (int i = 0; i < obj.Texts.Count; i++)
            {
                var key = ReplaceLast(".Text", string.Empty, obj.Texts[i].id);
                var value = obj.Texts[i].text;


                bool keyExists = dictionary.ContainsKey(key);
                if (keyExists)
                {
                    MessageBox.Show(string.Format("Key {0} already exists. Clean the original resource file and convert again", key));
                }
                else
                {
                    dictionary.Add(key, value);
                }
            }

            string finalJsonString = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            string finalFileName = sourceFileName.Replace(".ascx", "").Replace(".resx", ".en-us.json"); // WE ONLY CARE about en-us now. That's why we hard-coded it here

            File.WriteAllText(finalFileName, finalJsonString);
        }

        /// <summary>
        /// Get rid of ".Text" from resource file's "Name"
        /// Source: https://bytenota.com/csharp-replace-last-occurrence-of-a-string/
        /// </summary>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private string ReplaceLast(string find, string replace, string str)
        {
            int lastIndex = str.LastIndexOf(find);

            if (lastIndex == -1)
            {
                return str;
            }

            string beginString = str.Substring(0, lastIndex);
            string endString = str.Substring(lastIndex + find.Length);

            return beginString + replace + endString;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeOpenFileDialog();
        }

        private void InitializeOpenFileDialog()
        {
            this.openFileDialog1.Filter = "resource files (*.resx)|*.resx|All files (*.*)|*.*";
            //  Allow the user to select multiple filess.
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.Title = "Select resource files";
        }
    }
}
