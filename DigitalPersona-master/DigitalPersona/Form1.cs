using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
namespace DigitalPersona
{
    delegate void Functionz();
    public partial class Form1 : Form, DPFP.Capture.EventHandler
    {
        public int PROBABILITY_ONE = 0x7FFFFFFF;
        bool registrationInProgress = false;
        int fingerCount = 0;
        public byte[] huella = null;
        System.Drawing.Graphics graphics;
        System.Drawing.Font font;
        DPFP.Capture.ReadersCollection readers;
        DPFP.Capture.ReaderDescription readerDescription;
        DPFP.Capture.Capture capturer;
        DPFP.Template template;
        DPFP.FeatureSet[] regFeatures;
        DPFP.FeatureSet verFeatures;
        DPFP.Processing.Enrollment createRegTemplate;
        DPFP.Verification.Verification verify;
        DPFP.Capture.SampleConversion converter;
        String huellabits = String.Empty;
        Funciones f = new Funciones();
        public Form1()
        {
            InitializeComponent();
            graphics = this.CreateGraphics();
            font = new Font("Times New Roman", 12, FontStyle.Bold, GraphicsUnit.Pixel);
            DPFP.Capture.ReadersCollection coll = new DPFP.Capture.ReadersCollection();
            regFeatures = new DPFP.FeatureSet[4];
            for (int i = 0; i < 4; i++)
                regFeatures[i] = new DPFP.FeatureSet();
            verFeatures = new DPFP.FeatureSet();
            createRegTemplate = new DPFP.Processing.Enrollment();
            readers = new DPFP.Capture.ReadersCollection();
            for (int i = 0; i < readers.Count; i++)
            {
                readerDescription = readers[i];
                if ((readerDescription.Vendor == "Digital Persona, Inc.") || (readerDescription.Vendor
               == "DigitalPersona, Inc."))
                {
                    try
                    {
                        capturer = new DPFP.Capture.Capture(readerDescription.SerialNumber,
                       DPFP.Capture.Priority.Normal);//CREAMOS UNA OPERACION DE CAPTURAS.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    capturer.EventHandler = this;
                    //AQUI CAPTURAMOS LOS EVENTOS.
                    converter = new DPFP.Capture.SampleConversion();
                    try
                    {
                        verify = new DPFP.Verification.Verification();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ex: " + ex.ToString());
                    }
                    break;
                }
            }    
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            registrationInProgress = true;
            fingerCount = 0;
            createRegTemplate.Clear();
            if (capturer != null)
            {
                try
                {
                    capturer.StartCapture();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public void OnComplete(object obj, string info, DPFP.Sample sample)
        {
            this.Invoke(new Functionz(delegate ()
            {
                tbInfo.Text = "Captura Completa";
            }));
            this.Invoke(new Functionz(delegate ()
            {
                Bitmap tempRef = null;
                converter.ConvertToPicture(sample, ref tempRef);
                System.Drawing.Image img = tempRef;

                 Bitmap bmp = new Bitmap(converter.ConvertToPicture(sample, ref
                tempRef), pbImage.Size);
                String pxFormat = bmp.PixelFormat.ToString();
                Point txtLoc = new Point(pbImage.Width / 2 - 20, 0);
                graphics = Graphics.FromImage(bmp);

            if (registrationInProgress)
                {
                    try
                    {

                        regFeatures[fingerCount] = ExtractFeatures(sample,DPFP.Processing.DataPurpose.Enrollment);
                        if (regFeatures[fingerCount] != null)
                        {
                            string b64 =
                           Convert.ToBase64String(regFeatures[fingerCount].Bytes);

                            regFeatures[fingerCount].DeSerialize(Convert.FromBase64String(b64));
                            if (regFeatures[fingerCount] == null)
                            {
                                txtLoc.X = pbImage.Width / 2 - 26;
                                graphics.DrawString("Bad Press", font,
                               Brushes.Cyan, txtLoc);
                                return;
                            }
                            ++fingerCount;
                            createRegTemplate.AddFeatures(regFeatures[fingerCount
                            - 1]);
                            graphics = Graphics.FromImage(bmp);
                            if (fingerCount < 4)
                                graphics.DrawString("" + fingerCount + " De 4",
                               font, Brushes.Black, txtLoc);
                            if (createRegTemplate.TemplateStatus ==
                           DPFP.Processing.Enrollment.Status.Failed)
                            {
                                capturer.StopCapture();
                                fingerCount = 0;
                                MessageBox.Show("Registration Failed, \nMake sure you use the same finger for all 4 presses.");
                            }
                            else
                            if (createRegTemplate.TemplateStatus ==
                           DPFP.Processing.Enrollment.Status.Ready)
                            {
                                string mensaje = "";
                                MemoryStream x = new MemoryStream();
                                MemoryStream mem = new MemoryStream();
                                template = createRegTemplate.Template;
                                template.Serialize(mem);
                                verFeatures = ExtractFeatures(sample,
                                DPFP.Processing.DataPurpose.Verification);
                                mensaje = "";
                                //comparar(verFeatures);
                                if (mensaje == "Ya Existe un Empleado Con LaHuella Capturada")
                            {
                                    MessageBox.Show(mensaje, "Seguridad NuevaEra", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                   
                                    capturer.StopCapture();
                                    this.Close();
                                }
else
{
                                    MessageBox.Show("Captura Completa",
                                   "Seguridad Nueva Era", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    huella = mem.GetBuffer();
                                    capturer.StopCapture();
                                }
                            }
                        }
                    }
                    catch (DPFP.Error.SDKException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    DPFP.Verification.Verification.Result rslt = new
                   DPFP.Verification.Verification.Result();
                    verFeatures = ExtractFeatures(sample,
                   DPFP.Processing.DataPurpose.Verification);
                    verify.Verify(verFeatures, template, ref rslt);
                    txtLoc.X = pbImage.Width / 2 - 38;
                    if (rslt.Verified == true)
                        graphics.DrawString("Match!!!!", font,
                       Brushes.LightGreen, txtLoc);
                    else graphics.DrawString("No Match!!!", font, Brushes.Red,
                   txtLoc);
                }
                pbImage.Image = bmp;
            }));
        }
        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            try
            {
                this.Invoke(new Functionz(delegate ()
                {
                    tbInfo.Text = "Esperando...";
                }));
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Seguridad Nueva Era",
               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            try
            {
                this.Invoke(new Functionz(delegate ()
                {
                    tbInfo.Text = "Leyendo huella";
                }));
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Seguridad Nueva Era",
               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            try
            {
                this.Invoke(new Functionz(delegate ()
                {
                    tbInfo.Text = "Lector Conectado";
                }));
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Seguridad Nueva Era",
               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            try
            {
                this.Invoke(new Functionz(delegate ()
                {
                    tbInfo.Text = "Lector Desconectado"; MessageBox.Show("readercount: " + readers.Count);
                }));
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Seguridad Nueva Era",
               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void OnSampleQuality(object Capture, string ReaderSerialNumber,
       DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            MessageBox.Show("Sample quality!!!! " + CaptureFeedback.ToString());
        }
        protected DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample,
       DPFP.Processing.DataPurpose Purpose)
        {
            DPFP.Processing.FeatureExtraction Extractor = new
           DPFP.Processing.FeatureExtraction(); // Create a feature extractor
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            try
            {
                Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref
               features);
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
            }
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommand comandodb;
                if (f.conexion.State == ConnectionState.Closed)
                    f.conexion.Open();
                comandodb = new SqlCommand("insertarHuella", f.conexion);
                comandodb.CommandType = CommandType.StoredProcedure;
                comandodb.Parameters.AddWithValue("@huella", huella);
                comandodb.Parameters.AddWithValue("@nombres", textBox1.Text);
                comandodb.Parameters.Add("@msj", SqlDbType.VarChar, 60).Direction = ParameterDirection.Output;
                comandodb.ExecuteNonQuery();
                MessageBox.Show(comandodb.Parameters["@msj"].Value + "");
                if (f.conexion.State == ConnectionState.Open)
                {
                    f.conexion.Close();
                }
                else
                {
                    MessageBox.Show("Faltan datos", "Información",
                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Información", MessageBoxButtons.OK,
               MessageBoxIcon.Exclamation);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
