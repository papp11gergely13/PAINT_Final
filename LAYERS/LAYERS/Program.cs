using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LayeredPaintForm());
    }
}

public class LayeredPaintForm : Form
{
    private PictureBox canvas;
    private List<Bitmap> layers = new List<Bitmap>();
    private List<string> layerDisplayNames = new List<string>();
    private int activeLayer = 0;
    private Panel rightPanel, layerPanel;
    private Button addLayerButton, removeLayerButton, toggleLayerViewButton, expandLayersButton;
    private bool showOnlyActiveLayer = true;
    private const int MaxLayers = 5;
    private bool isUpdatingLayers = false;
    private bool isLayerListExpanded = false;

    public LayeredPaintForm()
    {
        this.Text = "Layered Paint App";
        this.Size = new Size(1000, 600);

        canvas = new PictureBox()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Image = new Bitmap(800, 600)
        };
        Controls.Add(canvas);

        rightPanel = new Panel() { Dock = DockStyle.Right, Width = this.Width / 4, BackColor = Color.LightGray };
        Controls.Add(rightPanel);

        addLayerButton = new Button() { Text = "Add Layer", Dock = DockStyle.Top };
        removeLayerButton = new Button() { Text = "Remove Layer", Dock = DockStyle.Top };
        toggleLayerViewButton = new Button() { Text = "Toggle Layer View", Dock = DockStyle.Top };
        expandLayersButton = new Button() { Text = "Expand Layers", Dock = DockStyle.Top };

        addLayerButton.Click += AddLayer_Click;
        removeLayerButton.Click += RemoveLayer_Click;
        toggleLayerViewButton.Click += ToggleLayerView_Click;
        expandLayersButton.Click += ExpandLayers_Click;

        rightPanel.Controls.Add(expandLayersButton);
        rightPanel.Controls.Add(toggleLayerViewButton);
        rightPanel.Controls.Add(removeLayerButton);
        rightPanel.Controls.Add(addLayerButton);

        layerPanel = new Panel() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White, Padding = new Padding(5, 90, 5, 5) };
        rightPanel.Controls.Add(layerPanel);

        AddLayer("Layer 1");
        canvas.MouseDown += Canvas_MouseDown;
    }

    private void Canvas_MouseDown(object sender, MouseEventArgs e)
    {
        using (Graphics g = Graphics.FromImage(layers[activeLayer]))
        {
            g.FillEllipse(Brushes.Black, e.X, e.Y, 10, 10);
        }
        MergeLayers();
    }

    private void AddLayer_Click(object sender, EventArgs e)
    {
        if (layers.Count >= MaxLayers)
        {
            MessageBox.Show("Maximum layer limit reached (5 layers).", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        AddLayer($"Layer {layers.Count + 1}");
    }

    private void AddLayer(string name)
    {
        layers.Add(new Bitmap(800, 600));
        layerDisplayNames.Add(name);
        activeLayer = layers.Count - 1;
        UpdateLayerThumbnails();
        MergeLayers();
    }

    private void RemoveLayer_Click(object sender, EventArgs e)
    {
        if (layers.Count > 1)
        {
            layers.RemoveAt(activeLayer);
            layerDisplayNames.RemoveAt(activeLayer);
            activeLayer = Math.Max(0, Math.Min(activeLayer, layers.Count - 1));
            UpdateLayerThumbnails();
            MergeLayers();
        }
    }

    private void ToggleLayerView_Click(object sender, EventArgs e)
    {
        showOnlyActiveLayer = !showOnlyActiveLayer;
        toggleLayerViewButton.Text = showOnlyActiveLayer ? "Layer View Active" : "Toggle Layer View";
        MergeLayers();
    }

    private void ExpandLayers_Click(object sender, EventArgs e)
    {
        isLayerListExpanded = !isLayerListExpanded;
        expandLayersButton.Text = isLayerListExpanded ? "Collapse Layers" : "Expand Layers";
        UpdateLayerThumbnails();
    }

    private async void MergeLayers()
    {
        if (layers.Count == 0 || isUpdatingLayers) return;

        isUpdatingLayers = true;
        await Task.Delay(50);

        Bitmap merged = new Bitmap(800, 600);
        using (Graphics g = Graphics.FromImage(merged))
        {
            g.Clear(Color.White);
            if (showOnlyActiveLayer)
            {
                g.DrawImageUnscaled(layers[activeLayer], 0, 0);
            }
            else
            {
                foreach (var layer in layers)
                {
                    g.DrawImageUnscaled(layer, 0, 0);
                }
            }
        }
        canvas.Image = merged;
        canvas.Invalidate();

        isUpdatingLayers = false;
    }

    private void UpdateLayerThumbnails()
    {
        layerPanel.Controls.Clear();
        if (!isLayerListExpanded)
        {
            AddLayerControl(activeLayer);
        }
        else
        {
            for (int i = 0; i < layers.Count; i++)
            {
                AddLayerControl(i);
            }
        }
    }

    private void AddLayerControl(int index)
    {
        Panel layerContainer = new Panel() { Height = 50, Dock = DockStyle.Top, BackColor = index == activeLayer ? Color.LightBlue : Color.Gray, Padding = new Padding(5), Tag = index };

        PictureBox layerPreview = new PictureBox()
        {
            Size = new Size(40, 40),
            Dock = DockStyle.Left,
            BorderStyle = BorderStyle.FixedSingle,
            Image = layers[index],
            SizeMode = PictureBoxSizeMode.StretchImage
        };

        TextBox layerNameTextBox = new TextBox()
        {
            Text = layerDisplayNames[index],
            Dock = DockStyle.Fill,
            TextAlign = HorizontalAlignment.Center,
            Font = new Font("Arial", 10, FontStyle.Bold),
            BorderStyle = BorderStyle.None,
            ReadOnly = true
        };
        layerNameTextBox.DoubleClick += (s, e) => { layerNameTextBox.ReadOnly = false; };
        layerNameTextBox.Leave += (s, e) =>
        {
            if (index >= 0 && index < layerDisplayNames.Count)
            {
                layerDisplayNames[index] = layerNameTextBox.Text;
            }
            layerNameTextBox.ReadOnly = true;
        };

        layerContainer.Click += (s, e) =>
        {
            activeLayer = index;
            isLayerListExpanded = false;
            expandLayersButton.Text = "Expand Layers";
            UpdateLayerThumbnails();
            MergeLayers();
        };

        layerContainer.Controls.Add(layerPreview);
        layerContainer.Controls.Add(layerNameTextBox);
        layerPanel.Controls.Add(layerContainer);
    }
}
