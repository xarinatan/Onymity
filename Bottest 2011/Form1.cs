using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bottest_2011
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox1.KeyDown += new KeyEventHandler(textBox1_KeyDown);
            foreach (string sentence in new string[] {
                "The cat is a limbo dancer.",
                "The dog hates cats.", 
                "Little Mary had a flute.", 
                "Cheese is always a good answer.", 
                "Benji is a faggot.", 
                "Faggots are cigarretes.", 
                "Y U NO CHEESE" ,
                //"hm. I've lost a machine.. literally _lost_. it responds to ping, it works completely, I just can't figure out where in my apartment it is.",
                //"I'm going to become rich and famous after i invent a device that allows you to stab people in the face over the internet",
                //"A woman has a close male friend. This means that he is probably interested in her, which is why he hangs around so much. She sees him strictly as a friend. This always starts out with, you're a great guy, but I don't like you in that way. This is roughly the equivalent for the guy of going to a job interview and the company saying, You have a great resume, you have all the qualifications we are looking for, but we're not going to hire you. We will, however, use your resume as the basis for comparison for all other applicants. But, we're going to hire somebody who is far less qualified and is probably an alcoholic. And if he doesn't work out, we'll hire somebody else, but still not you. In fact, we will never hire you. But we will call you from time to time to complain about the person that we hired."
                //"Now, I’m sure many of you have encountered little shits in supermarkets. Little kids running about and knocking things over, being rude, walking all over their parents, you know the kind. But the worst are the biters. Yes, those little cunts that feel it is okay to bite you whenever they feel like it."
            
            })
            {
                nodestruct.parsestring(sentence);
            }
        }

        void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                nodestruct.parsestring(textBox1.Text);
                richTextBox1.AppendText(">" + textBox1.Text + Environment.NewLine);
                textBox1.Text = "";
                richTextBox1.AppendText("Bot: " + nodestruct.makeresponse() + Environment.NewLine);
                richTextBox1.ScrollToCaret();
            }
        }

        
    }

    public static class nodestruct
    {
        public static Dictionary<string, node> translatenodes = new Dictionary<string, node>();

        public static string makeresponse()
        {
            StringBuilder sbuilder = new StringBuilder();
            Random r = new Random();
            int length = r.Next(5, translatenodes.Count);
            string prevword = translatenodes.Keys.ToArray()[r.Next(0,translatenodes.Count)];
            sbuilder.Append(prevword + " ");
            int loopsdone = 0;
            while (sbuilder.ToString().Split(' ').Length < length && loopsdone < (length*5000))
            {
                if (translatenodes[prevword].childnodes.Count > 0)
                {
                    prevword = translatenodes[prevword].childnodes[r.Next(0, translatenodes[prevword].childnodes.Count)].text;
                    sbuilder.Append(prevword + " ");
                }
                loopsdone++;
            }


            return sbuilder.ToString();
        }
        

        public static void parsestring(string stringtoparse)
        {
            string[] splittedstring = stringtoparse.Split(' ');
            int parsetimes = 10;
            for (int i = 0; i < parsetimes; i++)
            {
                runparse(splittedstring);
            }

        }
        static void runparse(string[] splittedstring)
        {
            for (int i = 0; i < splittedstring.Length; i++)
            {
                if (!translatenodes.Keys.Contains(splittedstring[i]))
                {
                    node newnode = new node(splittedstring[i], 0);
                    translatenodes.Add(splittedstring[i], newnode);
                }

                if (i < splittedstring.Length - 1)
                    if (!translatenodes[splittedstring[i]].childnodes.Exists(x => x.text == splittedstring[i + 1]))
                    {
                        if (translatenodes.ContainsKey(splittedstring[i + 1]))
                        {
                            translatenodes[splittedstring[i]].childnodes.Add(translatenodes[splittedstring[i + 1]]);
                        }
                    }
            }
        }
    }
    

    public class node
    {
        public node(string _text, int _weight)
        {
            text = _text;
            weight = _weight;
        }
        public string text = "";
        public int weight = 0;
        public List<node> childnodes = new List<node>();
    }
}
