using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ImageMesh.ThreeMath;
using ImageMesh.MeshGen;
using ImageMesh.Samplers;
using Microsoft.Win32;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using System.Windows.Threading;

namespace ImageMesh
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected Vector3f cameraPosition = new Vector3f(0, 0, -100f);
        protected Vector3f cameraRotation = new Vector3f(0, 0, 0);

        protected MeshG mesh;
        protected Meshifier meshifier;

        protected float[] positions;
        protected float[] colors;
        protected uint[] indices;
        protected float[] normals;

        protected float scale = 1f;

        protected bool isGen = false;

        protected bool showWireFrame = false;
        protected bool culling = false;

        protected bool didImport = false;

        protected int tolerance = 1;

        protected PixelSampler sampler = new AverageSampler();
        protected bool displace = false;

        protected Meshifier.DisplacementMode displaceMode = Meshifier.DisplacementMode.Grayscale;
        protected float displacePower = 1.0f;

        protected float gaussianPower = 0.01f;
        protected int samplerRadius = 1;

        protected System.Drawing.Bitmap image;

        Point lastMousePosition;

        public MainWindow()
        {
            Toolkit.Init();
            InitializeComponent();
            InitComboBoxes();
        }

        private void InitComboBoxes()
        {
            for(int i = 0; i < 7; i++)
            {
                Meshifier.DisplacementMode md = (Meshifier.DisplacementMode)i;

                cbDisplaceModes.Items.Add(md.ToString());

                if(md == displaceMode)
                {
                    cbDisplaceModes.SelectedIndex = i;
                }
            }
        }

        private void UpdateMeshifier()
        {
            if (meshifier == null) return;

            sampler.Power = gaussianPower;
            sampler.Radius = samplerRadius;

            meshifier.ScaleImage(scale);
            meshifier.Sampler = sampler;
            meshifier.Tolerance = tolerance;
            meshifier.UseDisplacement = displace;
            meshifier.DisplaceMode = displaceMode;
            meshifier.DisplacementPower = displacePower;
        }

        private void Import(string data)
        {
            Task.Run(() =>
            {
                MeshG m = MeshReadWriter.ParseMeshString(data);
                m.FindCenter();

                if (m.Vertices.Count == 0 || m.Triangles.Count == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("No valid vertices or triangles in imesh file.");
                    });

                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    cameraPosition.x = -m.center.x;
                    cameraPosition.y = -m.center.y;
                    cameraPosition.z = -100.0f;

                    didImport = true;

                    lGenTime.Text = "0ms";

                    isGen = true;
                    mesh = null;

                    mesh = m;

                    positions = m.Vertices.ToArray();
                    indices = m.Triangles.ToArray();
                    normals = m.Normals.ToArray();
                    colors = m.Colors.ToArray();

                    lTriangleCount.Text = (indices.Length / 3).ToString("#,##");
                });

                Dispatcher.Invoke(() =>
                {
                    isGen = false;
                    Render(glControl);
                });
            });
        }

        private void GenSphere()
        {
            if (meshifier == null) return;

            didImport = false;
            btnExport.IsEnabled = false;
            UpdateMeshifier();

            lGenMessage.Visibility = Visibility.Visible;
            lGenMessage.BringIntoView();

            meshifier.Sphererize = true;

            isGen = true;
            mesh = null;

            cameraPosition.x = 0;
            cameraPosition.y = 0;
            cameraPosition.z = -10;

            cameraRotation.x = 0;
            cameraRotation.y = 0;
            cameraRotation.z = 0;

            Task.Run(async () =>
            {
                long ms = DateTime.Now.Ticks;
                MeshG m = await meshifier.Generate();

                Dispatcher.Invoke(() =>
                {
                    mesh = m;
                    positions = mesh.Vertices.ToArray();
                    colors = mesh.Colors.ToArray();
                    indices = mesh.Triangles.ToArray();
                    normals = mesh.Normals.ToArray();
                });
                long end = DateTime.Now.Ticks - ms;

                float totalMs = (float)end / 10000.0f;

                Dispatcher.Invoke(() =>
                {
                    isGen = false;
                    lGenTime.Text = totalMs + "ms";
                    lTriangleCount.Text = meshifier.TriangleCount.ToString("#,##");
                    lGenMessage.Visibility = Visibility.Hidden;
                    btnExport.IsEnabled = true;
                    Render(glControl);
                });
            });
        }

        private void GenFlat()
        {
            if (meshifier == null) return;

            didImport = false;
            btnExport.IsEnabled = false;
            UpdateMeshifier();

            meshifier.Sphererize = false;

            lGenMessage.Visibility = Visibility.Visible;
            lGenMessage.BringIntoView();

            isGen = true;
            mesh = null;

            cameraPosition.x = -(image.Width * scale) * 0.5f;
            cameraPosition.y = -(image.Height * scale) * 0.5f;
            cameraPosition.z = -100.0f;

            cameraRotation.x = 0;
            cameraRotation.y = 0;
            cameraRotation.z = 0;

            Task.Run(async () =>
            {
                long ms = DateTime.Now.Ticks;
                MeshG m = await meshifier.Generate();

                Dispatcher.Invoke(() =>
                {
                    mesh = m;
                    mesh.center.x = (image.Width * scale) * 0.5f;
                    mesh.center.y = (image.Height * scale) * 0.5f;
                    positions = mesh.Vertices.ToArray();
                    colors = mesh.Colors.ToArray();
                    indices = mesh.Triangles.ToArray();
                    normals = mesh.Normals.ToArray();
                });

                long end = DateTime.Now.Ticks - ms;

                float totalMs = (float)end / 10000.0f;

                Dispatcher.Invoke(() =>
                {
                    isGen = false;
                    lGenTime.Text = totalMs + "ms";
                    lTriangleCount.Text = meshifier.TriangleCount.ToString("#,##");
                    lGenMessage.Visibility = Visibility.Hidden;
                    btnExport.IsEnabled = true;
                    Render(glControl);
                });
            });
        }

        private void HostControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void GlControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                double dx = e.X - lastMousePosition.X;
                double dy = e.Y - lastMousePosition.Y;

                cameraPosition.x += (float)dx * 0.25f;
                cameraPosition.y += (float)dy * -0.25f;

                Render(glControl);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                double dx = e.X - lastMousePosition.X;
                double dy = e.Y - lastMousePosition.Y;
                dx *= 0.2f;
                dy *= 0.2f;

                cameraRotation.y += (float)dx;
                cameraRotation.y %= 360.0f;
                cameraRotation.x += (float)dy;
                cameraRotation.x %= 360.0f;

                Render(glControl);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                double dx = e.X - lastMousePosition.X;

                cameraPosition.z += (float)dx * 0.25f;

                Render(glControl);
            }

            lastMousePosition = new Point(e.X, e.Y);
        }

        private void chkCulling_Checked(object sender, RoutedEventArgs e)
        {
            culling = true;
            Render(glControl);
        }

        private void chkCulling_Unchecked(object sender, RoutedEventArgs e)
        {
            culling = false;
            Render(glControl);
        }

        private void chkWireframe_Unchecked(object sender, RoutedEventArgs e)
        {
            showWireFrame = false;
            Render(glControl);
        }

        private void chkWireframe_Checked(object sender, RoutedEventArgs e)
        {
            showWireFrame = true;
            Render(glControl);
        }

        private void chkDisplace_Checked(object sender, RoutedEventArgs e)
        {
            displace = true;
        }

        private void chkDisplace_Unchecked(object sender, RoutedEventArgs e)
        {
            displace = false;
        }

        private void cbDisplaceModes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            displaceMode = (Meshifier.DisplacementMode)cbDisplaceModes.SelectedIndex;
        }

        private void txtDisplace_TextChanged(object sender, TextChangedEventArgs e)
        {
            float d = 0;
            if(float.TryParse(txtDisplace.Text, out d))
            {
                displacePower = d;
            }
        }

        private void txtPixelPower_TextChanged(object sender, TextChangedEventArgs e)
        {
            float d = 0;
            if(float.TryParse(txtPixelPower.Text, out d))
            {
                gaussianPower = Math.Abs(d);
                gaussianPower = gaussianPower > 1 ? 1.0f : gaussianPower;
                gaussianPower = Math.Max(gaussianPower, 0.01f);
            }
        }

        private void txtPixelRadius_TextChanged(object sender, TextChangedEventArgs e)
        {
            int d = 0;
            if(int.TryParse(txtPixelRadius.Text, out d))
            {
                samplerRadius = Math.Abs(d);
                samplerRadius = Math.Max(samplerRadius, 1);
            }
        }

        private void txtTolerance_TextChanged(object sender, TextChangedEventArgs e)
        {
            int d = 0;
            if(int.TryParse(txtTolerance.Text, out d))
            {
                tolerance = Math.Abs(d);
            }
        }

        private void cbSampling_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(cbSampling.SelectedIndex)
            {
                case 0:
                    sampler = new PixelSampler();
                    break;
                case 1:
                    sampler = new AverageSampler();
                    break;
                case 2:
                    sampler = new MinSampler();
                    break;
                case 3:
                    sampler = new MaxSampler();
                    break;
                case 4:
                    sampler = new GaussianSampler();
                    break;
                default:
                    sampler = new AverageSampler();
                    break;
            }
        }

        private void slImageScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scale = (float)slImageScale.Value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image Files(*.BMP; *.JPG; *.GIF, *.PNG)| *.BMP; *.JPG; *.GIF; *.PNG";
            openDialog.CheckFileExists = true;

            if (openDialog.ShowDialog() == true)
            {
                image = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(openDialog.FileName);
                meshifier = new Meshifier(image);

                string name = System.IO.Path.GetFileName(openDialog.FileName);
                lImageName.Text = name;
            }
        }

        private void btnGenFlat_Click(object sender, RoutedEventArgs e)
        {
            GenFlat();
        }

        private void btnGenSphere_Click(object sender, RoutedEventArgs e)
        {
            GenSphere();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (mesh != null) {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.AddExtension = true;
                saveDialog.DefaultExt = "Image Mesh(*.imesh)|*.imesh";
                saveDialog.Filter = "Image Mesh(*.imesh)|*.imesh";
                saveDialog.CheckPathExists = true;

                if (saveDialog.ShowDialog() == true)
                {
                    string path = saveDialog.FileName;

                    Task.Run(() => {
                        MeshReadWriter.WriteMeshToFile(mesh, path);
                    });
                }
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image Mesh(*.imesh)|*.imesh";
            openDialog.CheckFileExists = true;

            if (openDialog.ShowDialog() == true)
            {
                string data = System.IO.File.ReadAllText(openDialog.FileName);

                if (string.IsNullOrEmpty(data))
                {
                    MessageBox.Show("No data in imesh file.");
                    return;
                }

                Import(data);
            }
        }

        private void Render(GLControl glControl)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UpdateCulling();

            GL.MatrixMode(MatrixMode.Modelview);

            GL.PushMatrix();

            UpdateCamera();

            DrawMesh();
            
            GL.PopMatrix();

            glControl.SwapBuffers();
        }

        private void UpdateCulling()
        {
            if (culling)
            {
                GL.Enable(EnableCap.CullFace);
            }
            else
            {
                GL.Disable(EnableCap.CullFace);
            }
        }

        private void UpdateCamera()
        {
            double tx = cameraPosition.x;
            double ty = cameraPosition.y;
            double tz = cameraPosition.z;

            double rx = cameraRotation.x;
            double ry = cameraRotation.y;

            GL.Translate(tx, ty, tz);

            if (mesh != null)
            {
                GL.Translate(mesh.center.x, mesh.center.y, 0);
            }
            GL.Rotate(rx, 1, 0, 0);
            GL.Rotate(ry, 0, 1, 0);
            if (mesh != null)
            {
                GL.Translate(-mesh.center.x, -mesh.center.y, 0);
            }
        }

        private void DrawMesh()
        {
            if (mesh != null && !isGen)
            {
                int totalIndices = indices.Length;

                GL.VertexPointer<float>(3, VertexPointerType.Float, 0, positions);
                GL.EnableClientState(ArrayCap.VertexArray);

                GL.ColorPointer<float>(3, ColorPointerType.Float, 0, colors);
                GL.EnableClientState(ArrayCap.ColorArray);

                GL.NormalPointer<float>(NormalPointerType.Float, 0, normals);
                GL.EnableClientState(ArrayCap.NormalArray);

                GL.DrawElements<uint>(PrimitiveType.Triangles, totalIndices, DrawElementsType.UnsignedInt, indices);
            }
        }

        private void GLControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            GLControl c = sender as GLControl;
            Render(c);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GL.ClearColor(Color4.Black);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            //enable depth
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.ShadeModel(ShadingModel.Smooth);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        private void GLControl_Resize(object sender, EventArgs e)
        {
            GLControl c = sender as GLControl;

            if (c.ClientSize.Height == 0)
                c.ClientSize = new System.Drawing.Size(c.ClientSize.Width, 1);

            GL.Viewport(0, 0, c.ClientSize.Width, c.ClientSize.Height);
            float ratio = (float)c.ClientSize.Width / (float)c.ClientSize.Height;
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, ratio, 1.0f, 1000.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);
            Render(c);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}
