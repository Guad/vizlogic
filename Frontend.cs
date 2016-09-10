using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VisualiazdorLogica
{
    public partial class Frontend : Form
    {
        public Frontend()
        {
            InitializeComponent();
            _parser = new DotNetParser();
            tabControl1.ItemSize = new Size(tabControl1.Size.Width / tabControl1.TabCount - 2, tabControl1.ItemSize.Height);
        }

        private INode _rootNode;

        private DotNetParser _parser;

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (richTextBox1.TextLength == 0)
            {
                richTextBox1.BackColor = Color.White;
                richTextBox1.ForeColor = Color.Black;
                toolStripStatusLabel1.Text = "";
                toolStripStatusLabel2.Text = "";
                treeView1.Nodes.Clear();
                LiteralVariables.Map.Clear();
                dataGridView1.DataSource = null;
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = null;
                return;
            }

            var cursor = richTextBox1.SelectionStart;
            richTextBox1.Text = Util.Sanitize(richTextBox1.Text);
            richTextBox1.SelectionStart = cursor;

            INode newNode = null;
            Exception exception = null;

            try
            {
                newNode = _parser.Parse(richTextBox1.Text);
            }
            catch(Exception ex)
            {
                newNode = null;
                exception = ex;
            }

            if (newNode != null)
            {
                _rootNode = newNode;
                richTextBox1.BackColor = Color.Green;
                richTextBox1.ForeColor = Color.White;

                string conectivoPrincipal = "";

                if (_rootNode is BinaryNode)
                {
                    conectivoPrincipal = string.Format("Conectivo Principal: {0} ", ((BinaryNode) _rootNode).Operator.ToString());
                }

                toolStripStatusLabel1.Text = "Formula proposicional correcta!";
                toolStripStatusLabel2.Text = string.Format("{2}Nodos: {0} Operadores Binarios: {1}",
                    recursiveCount(_rootNode, node => true), recursiveCount(_rootNode, node => node is BinaryNode), conectivoPrincipal);
                refreshTreeView();
                refreshChart();
                dataGridView1.DataSource = new Dictionary<string, bool>(LiteralVariables.Map).Select(pair => new VariableDataView()
                {
                    Nombre = pair.Key,
                    Estado = pair.Value,
                }).ToList();
            }
            else
            {
                richTextBox1.BackColor = Color.DarkRed;
                richTextBox1.ForeColor = Color.White;
                toolStripStatusLabel1.Text = "Error: " + exception?.Message;
            }
        }

        private void onDataChange()
        {
            dataGridView1.DataSource = new Dictionary<string, bool>(LiteralVariables.Map).Select(pair => new VariableDataView()
            {
                Nombre = pair.Key,
                Estado = pair.Value,
            }).ToList();
        }

        private int recursiveCount(INode node, Func<INode, bool> predicate)
        {
            int mainCounter = 0;

            if (predicate(node)) mainCounter++;

            if (node.Children != null)
            foreach (var child in node.Children)
            {
                mainCounter += recursiveCount(child, predicate);
            }

            return mainCounter;
        }

        private TreeNode addTreeNode(INode node, TreeNode parent)
        {
            var ourNode = parent.Nodes.Add(node.Prettify() +
                string.Format(" [{0}]", node.Evaluate() ? "Verdad" : "Falso"));

            if (node.Children != null)
                foreach (var child in node.Children)
                {
                    addTreeNode(child, ourNode);
                }

            return ourNode;
        }

        private void refreshTreeView()
        {
            if (_rootNode == null) return;
            treeView1.Nodes.Clear();

            var rootNode = new TreeNode(Util.Prettify(richTextBox1.Text) +
                string.Format(" [{0}]", _rootNode.Evaluate() ? "Verdad" : "Falso"));
            
            if (_rootNode.Children != null)
                foreach (var child in _rootNode.Children)
                {
                    addTreeNode(child, rootNode);
                }

            treeView1.Nodes.Add(rootNode);

            treeView1.ExpandAll();
        }

        private void refreshChart()
        {
            if (_rootNode == null)
                return;

            var rootNode = new ChartNode(_rootNode.Prettify());

            if (_rootNode.Children != null)
                foreach (var child in _rootNode.Children)
                {
                    recursiveAddChartNode(child, rootNode);
                }

            var bitmap = OrganizationChart.Generate(rootNode, pictureBox1.Width);

            pictureBox1.Image?.Dispose();
            pictureBox1.Image = bitmap;
        }

        private void recursiveAddChartNode(INode node, ChartNode parent)
        {
            var ourNode = new ChartNode(node.Prettify());

            if (node.Children != null)
            foreach (var child in node.Children)
            {
                recursiveAddChartNode(child, ourNode);
            }

            parent.Children.Add(ourNode);
        }

        private void Frontend_Resize(object sender, EventArgs e)
        {
            tabControl1.ItemSize = new Size(tabControl1.Size.Width / tabControl1.TabCount - 2, tabControl1.ItemSize.Height);
            refreshChart();
        }
        
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1) // Tree view
            {
                refreshTreeView();
            }
            else if (tabControl1.SelectedIndex == 0)
            {
                refreshChart();
            }
        }

        private void Frontend_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            string helpText = "";
            helpText += "Controles:\n";
            helpText += "~ para " + Characters.Not + "\n";
            helpText += "> para " + Characters.Conditional + "\n";
            helpText += "<> para " + Characters.Biconditional + "\n";
            helpText += "^ para " + Characters.And + "\n";
            helpText += "V para " + Characters.Or + "\n";
            helpText += "T para literal verdadero\n";
            helpText += "F para literal falso\n";

            MessageBox.Show(helpText, "Ayuda");
        }
    }

    public class VariableDataView
    {
        private bool _estado;
        public string Nombre { get; set; }
        
        public bool Estado
        {
            get { return _estado; }
            set
            {
                _estado = value; 
                LiteralVariables.Set(Nombre, value);
            }
        }
    }
}
