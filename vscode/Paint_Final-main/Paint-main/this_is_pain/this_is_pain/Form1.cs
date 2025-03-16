using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Diagnostics;

namespace this_is_pain
{
    public partial class Form1 : Form
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
        private int activeLayerIndex = 0; // The currently active layer

        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.Height = 1080;
            this.Width = 1940;
            bm = new Bitmap(pic.Width, pic.Height);
            g = Graphics.FromImage(bm);
            g.Clear(Color.White);
            pic.Image = bm;
            this.ControlBox = true;      // Fejléc és vezérlőgombok bekapcsolása
            this.MinimizeBox = true;     // 🔽 Tálcára tevés gomb engedélyezése
            this.MaximizeBox = true;
            pic.Paint += canvas_Paint;
            rácsvonalakToolStripMenuItem.Click += rácsvonalakToolStripMenuItem_Click;


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
        public void AddLayer()
{
    Bitmap newLayer = new Bitmap(pic.Width, pic.Height);
    layers.Add(newLayer);
    activeLayerIndex = layers.Count - 1;
    pic.Invalidate(); // Refresh canvas
}

public void RemoveLayer(int index)
{
    if (layers.Count > 1)
    {
        layers.RemoveAt(index);
        activeLayerIndex = Math.Max(0, activeLayerIndex - 1);
        pic.Invalidate();
    }
}

public void SetActiveLayer(int index)
{
    if (index >= 0 && index < layers.Count)
    {
        activeLayerIndex = index;
        pic.Invalidate();
    }
}


        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            using (Graphics g = Graphics.FromImage(layers[activeLayerIndex]))
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


        Bitmap bm;
        Graphics g;
        bool paint = false;
        Point px, py;
        Pen p = new Pen(Color.Black, 1);
        Pen erase = new Pen(Color.White, 10);
        int index;
        int x, y, sX, sY, cX, cY;

        ColorDialog cd = new ColorDialog();
        Color new_color;

        private List<Bitmap> history = new List<Bitmap>();
        private int historyIndex = -1;
        private bool selecting = false;  // Jelzi, hogy folyamatban van-e a kijelölés
        private Rectangle selectionRect; // A kijelölés téglalapja
        private Pen selectionPen = new Pen(Color.Black, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }; // Szaggatott vonal
        private Bitmap copiedImage = null;  // Itt tároljuk az aktuálisan másolt képrészletet
        private Point pasteLocation;        // Itt tároljuk az egér helyzetét beillesztéshez
        private bool isPasting = false;     // Jelzi, hogy folyamatban van-e a beillesztés

        private bool isModified = false;
        private PrintDocument printDocument = new PrintDocument();
        private PrintDialog printDialog = new PrintDialog();

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            paint = true;
            py = e.Location;

            cX = e.X;
            cY = e.Y;

            if (index == 6) // Ha a kijelölés aktív (6-os index, ezt a gombhoz kell majd állítani)
            {
                selecting = true;
                selectionRect = new Rectangle(e.X, e.Y, 0, 0);
            }
        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            if (paint)
            {
                if (index == 1)
                {
                    px = e.Location;
                    int dx = px.X - py.X;
                    int dy = px.Y - py.Y;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float step = p.Width / 2;

                    if (distance > 0)
                    {
                        for (float i = 0; i <= distance; i += step)
                        {
                            float t = i / distance;
                            int x = (int)(py.X + t * dx);
                            int y = (int)(py.Y + t * dy);

                            g.FillEllipse(new SolidBrush(p.Color), x - p.Width / 2, y - p.Width / 2, p.Width, p.Width);
                        }
                    }

                    py = px;
                }
                if (index == 2)
                {
                    px = e.Location;
                    g.DrawLine(erase, px, py);
                    py = px;
                }
                if (index == 4)
                {
                    pic.Refresh();
                    int startX = Math.Min(cX, e.X);
                    int startY = Math.Min(cY, e.Y);
                    int width = Math.Abs(e.X - cX);
                    int height = Math.Abs(e.Y - cY);

                    using (Graphics tempG = pic.CreateGraphics())
                    {
                        tempG.DrawRectangle(p, startX, startY, width, height);
                    }
                }
                /*if (index == 4)
                {
                    pic.Refresh();
                    int startX = Math.Min(cX, e.X);
                    int startY = Math.Min(cY, e.Y);
                    int width = Math.Abs(e.X - cX);
                    int height = Math.Abs(e.Y - cY);

                    using (Graphics tempG = Graphics.FromImage(bm))
                    {
                        g.Clear(Color.White);
                        g.DrawImage(bm, 0, 0);
                        tempG.DrawRectangle(p, startX, startY, width, height);
                    }
                    pic.Image = bm;
                }*/
            }
            pic.Refresh();
            x = e.X;
            y = e.Y;

            sX = e.X - cX;
            sY = e.Y - cY;

            if (selecting)
            {
                int width = e.X - selectionRect.X;
                int height = e.Y - selectionRect.Y;
                selectionRect.Width = width;
                selectionRect.Height = height;
                pic.Refresh();
            }

            if (isPasting)
            {
                pasteLocation = e.Location;
                pic.Refresh();
            }
        }
        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            paint = false;

            int startX = Math.Min(cX, x);
            int startY = Math.Min(cY, y);
            int width = Math.Abs(x - cX);
            int height = Math.Abs(y - cY);

            if (index == 3)
            {
                g.DrawEllipse(p, startX, startY, width, height);
            }
            if (index == 4)
            {
                g.DrawRectangle(p, startX, startY, width, height);
            }
            if (index == 5)
            {
                g.DrawLine(p, cX, cY, x, y);
            }
            if (index == 6)
            {
                selecting = false;
                pic.Refresh();
                return;
            }

            SaveState();
        }

        private void pic_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (paint)
            {
                if (index == 3)
                {
                    g.DrawEllipse(p, cX, cY, sX, sY);
                }
                if (index == 4)
                {
                    g.DrawRectangle(p, cX, cY, sX, sY);
                }
                if (index == 5)
                {
                    g.DrawLine(p, cX, cY, x, y);
                }
            }
            if (selecting)
            {
                e.Graphics.DrawRectangle(selectionPen, selectionRect);
            }
            if (isPasting)
            {
                e.Graphics.DrawImage(copiedImage, pasteLocation); // A másolt kép megjelenítése az egér pozíciójában
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pic.Image = bm;
            index = 0;
        }

        private void btn_color_Click(object sender, EventArgs e)
        {
            cd.ShowDialog();
            new_color = cd.Color;
            pic_color.BackColor = cd.Color;
            p.Color = cd.Color;
        }

        static Point set_point(PictureBox pb, Point pt)
        {
            float pX = 1f * pb.Image.Width / pb.Width;
            float pY = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * pX), (int)(pt.Y * pY));
        }

        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = set_point(color_picker, e.Location);
            pic_color.BackColor = ((Bitmap)color_picker.Image).GetPixel(point.X, point.Y);
            new_color = pic_color.BackColor;
            p.Color = pic_color.BackColor;
        }

        private void validate(Bitmap bm, Stack<Point> sp, int x, int y, Color old_color, Color new_color)
        {
            Color cx = bm.GetPixel(x, y);
            if (cx == old_color)
            {
                sp.Push(new Point(x, y));
                bm.SetPixel(x, y, new_color);
            }
        }

        public void Fill(Bitmap bm, int x, int y, Color new_clr)
        {
            Color old_color_ = bm.GetPixel(x, y);
            Stack<Point> pixel = new Stack<Point>();
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, new_clr);
            if (old_color_ == new_clr) return;

            while (pixel.Count > 0)
            {
                Point pt = (Point)pixel.Pop();
                if (pt.X > 0 && pt.Y > 0 && pt.X < bm.Width - 1 && pt.Y < bm.Height - 1)
                {
                    validate(bm, pixel, pt.X - 1, pt.Y, old_color_, new_clr);
                    validate(bm, pixel, pt.X, pt.Y - 1, old_color_, new_clr);
                    validate(bm, pixel, pt.X + 1, pt.Y, old_color_, new_clr);
                    validate(bm, pixel, pt.X, pt.Y + 1, old_color_, new_clr);
                }
            }
        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            if (index == 7)
            {
                Point point = set_point(pic, e.Location);
                Fill(bm, point.X, point.Y, new_color);
            }
            if (isPasting)
            {
                g.DrawImage(copiedImage, pasteLocation); // Beillesztjük a másolt képet az egér pozíciójába
                isPasting = false; // Beillesztési mód kikapcsolása
                SaveState(); // A beillesztést mentjük a history listába
                pic.Refresh(); // Frissítjük a vásznat
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Image(.jpg)|*.jpg|(*.*|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Bitmap btm = bm.Clone(new Rectangle(0, 0, pic.Width, pic.Height), bm.PixelFormat);
                btm.Save(sfd.FileName, ImageFormat.Jpeg);
            }
        }

        private void btn_ellipse_Click(object sender, EventArgs e)
        {
            index = 3;
        }

        private void btn_pencil_Click(object sender, EventArgs e)
        {
            index = 1;
        }

        private void btn_eraser_Click(object sender, EventArgs e)
        {
            index = 2;
        }
        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            // Rajzolj egy keretet a panel3 köré
            e.Graphics.DrawRectangle(Pens.Black, 0, 0, panel3.Width - 1, panel3.Height - 1);
        }
        private void btn_selection_Click(object sender, EventArgs e)
        {
            index = 6; // A kijelölés aktiválása
        }

        private void btn_copy_Click(object sender, EventArgs e)
        {
            if (selectionRect.Width > 0 && selectionRect.Height > 0) // Ha van érvényes kijelölés
            {
                copiedImage = bm.Clone(selectionRect, bm.PixelFormat); // Kép kivágása
            }
        }

        private void btn_paste_Click(object sender, EventArgs e)
        {
            if (copiedImage != null) // Ha van másolt kép
            {
                selecting = false; // Kijelölést kikapcsoljuk
                pasteLocation = Point.Empty; // Alapértelmezett beillesztési pont
                isPasting = true; // Beillesztési mód aktiválása
                pic.Refresh(); // Frissítjük a vásznat
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void pic_Click(object sender, EventArgs e)
        {

        }

        private void btn_fill_Click(object sender, EventArgs e)
        {
            index = 7;
        }

        private void btn_rect_Click(object sender, EventArgs e)
        {
            index = 4;
        }

        private void btn_line_Click(object sender, EventArgs e)
        {
            index = 5;
        }


        private void másolásToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectionRect.Width > 0 && selectionRect.Height > 0) // Ha van érvényes kijelölés
            {
                copiedImage = bm.Clone(selectionRect, bm.PixelFormat); // Kép kivágása
            }
        }

        private void beillesztésToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (copiedImage != null) // Ha van másolt kép
            {
                selecting = false; // Kijelölést kikapcsoljuk
                pasteLocation = Point.Empty; // Alapértelmezett beillesztési pont
                isPasting = true; // Beillesztési mód aktiválása
                pic.Refresh(); // Frissítjük a vásznat
            }
        }

        private void kivágásToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectionRect.Width > 0 && selectionRect.Height > 0) // Ha van érvényes kijelölés
            {
                // Kép kivágása és tárolása a copiedImage változóba
                copiedImage = bm.Clone(selectionRect, bm.PixelFormat);

                // Kijelölt terület kitöltése fehér színnel (vagy átlátszóval, ha támogatott)
                using (Graphics g = Graphics.FromImage(bm))
                {
                    g.FillRectangle(Brushes.White, selectionRect);
                }

                // Frissítjük a vásznat
                pic.Refresh();

                // Kijelölés megszüntetése
                selecting = false;
                selectionRect = Rectangle.Empty;

                // Mentjük az új állapotot a visszavonáshoz
                SaveState();
            }
        }

        private void újToolStripMenuItem_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pic.Image = bm;
            index = 0;
        }

        private void mentésToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Image(.jpg)|*.jpg|(*.*|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Bitmap btm = bm.Clone(new Rectangle(0, 0, pic.Width, pic.Height), bm.PixelFormat);
                btm.Save(sfd.FileName, ImageFormat.Jpeg);
            }
        }
        private void SaveImage(ImageFormat format, string extension)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = $"{extension.ToUpper()} (*.{extension})|*.{extension}";
                sfd.Title = $"Mentés {extension.ToUpper()} formátumban";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    bm.Save(sfd.FileName, format);
                }
            }
        }

        private void bMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImage(ImageFormat.Bmp, "bmp");
        }

        private void jPEGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImage(ImageFormat.Bmp, "jpg");
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImage(ImageFormat.Bmp, "png");
        }
        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (bm != null)
            {
                e.Graphics.DrawImage(bm, e.MarginBounds);
            }
        }
        private void PrintImage()
        {
            printDialog.Document = printDocument;
            printDocument.PrintPage += new PrintPageEventHandler(printDocument_PrintPage);

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }

        private void nyomtatásToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintImage();
        }

        private void kilépésToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            if (rácsvonalakToolStripMenuItem.Checked)
            {
                DrawGrid(e.Graphics);
            }
        }
        private void DrawGrid(Graphics g)
        {
            int gridSize = 20; // Rácsvonalak közötti távolság
            Pen gridPen = new Pen(Color.LightGray, 1); // Rács színe és vastagsága

            for (int x = 0; x < pic.Width; x += gridSize)
            {
                g.DrawLine(gridPen, x, 0, x, pic.Height);
            }

            for (int y = 0; y < pic.Height; y += gridSize)
            {
                g.DrawLine(gridPen, 0, y, pic.Width, y);
            }

            gridPen.Dispose();
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void rácsvonalakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rácsvonalakToolStripMenuItem.Checked = !rácsvonalakToolStripMenuItem.Checked;
            pic.Invalidate();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            float újVastagság = trackbar1.Value;
            p = new Pen(p.Color, újVastagság);
            Invalidate(); // újrarajzolja a formot
        }
        private void pictureBoxElore_Click(object sender, EventArgs e)
        {
            if (historyIndex < history.Count - 1)
            {
                historyIndex++;
                bm = (Bitmap)history[historyIndex].Clone();
                g = Graphics.FromImage(bm);
                pic.Image = bm;
            }
        }

        private void pictureBoxVissza_Click(object sender, EventArgs e)
        {
            if (historyIndex > 0)
            {
                historyIndex--;
                bm = (Bitmap)history[historyIndex].Clone();
                g = Graphics.FromImage(bm);
                pic.Image = bm;
            }
        }

        private void SaveState()
        {
            // Ha visszaléptünk korábbi állapotokra és most új változtatás történik,
            // akkor töröljük a további állapotokat
            if (historyIndex < history.Count - 1)
            {
                history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);
            }

            // Mentjük az aktuális képet a listába
            history.Add((Bitmap)bm.Clone());
            historyIndex = history.Count - 1;
            isModified = true;
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isModified)
            {
                var result = MessageBox.Show("Szeretné menteni munkáját?", "Mentés", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var sfd = new SaveFileDialog();
                    sfd.Filter = "Image(.jpg)|*.jpg|(*.*|*.*";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap btm = bm.Clone(new Rectangle(0, 0, pic.Width, pic.Height), bm.PixelFormat);
                        btm.Save(sfd.FileName, ImageFormat.Jpeg);
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            base.OnFormClosing(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            index = 1;
            SaveState();
        }
    }
}





