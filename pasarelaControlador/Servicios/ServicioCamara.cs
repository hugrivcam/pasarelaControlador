using AForge.Video; //incompatible con linux
using AForge.Video.DirectShow;//incompatible con linux
using pasarelaControlador.Entidades;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
//using System.IO;

namespace pasarelaControlador.Servicios
{
    public class ServicioCamara
    {
        private int IdActual = 0;
        private int DiaHoy = DateTime.Now.Day; //si cambia el dia se reinicia el idactual
        private List<foto> ListaFotosTomadas = new List<foto>();  
        private string PathCamara = @"R:\CAMERAFOTOS\";
        private bool HayDispositivos = false;
        private FilterInfoCollection? MisCamaras;// carga dispositivos carga las camaras
        private List<string> ListaCamaras; //carga dispositivos llena una lista con los nombres de las camaras
        private Camara? CamaraActual = null;
        
        //private int? CamaraSeleccionadaIndice=-1;
        //private string? CamaraSeleccionadaNombre;
        //private string? CamaraMonikerString;
        private VideoCaptureDevice WebCam;

        ServicioCamara() 
        {
            CargaDispositivos();
        }

        private int CargaDispositivos() 
        {
            int c;
            MisCamaras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            c = MisCamaras.Count;
            if (MisCamaras.Count > 0)
            {
                HayDispositivos = true;
                foreach (FilterInfo? cam in MisCamaras) 
                {
                    if (cam != null)
                        ListaCamaras.Add(cam.Name.ToString());
                }
            }
            else 
            {
                HayDispositivos = false;
            }
            return c;
        }
        public int ActualizarListaCamaras()
        {
            return this.CargaDispositivos();
        }
        public List<string> GetListaCamarasByName() 
        {
            return ListaCamaras;
        }
        public void SeleccionarCamara(int ix) //si no se selecciona una camara no se puede hacer una foto
        {
            try
            {
                CerrarWebCam();
                this.CamaraActual = new();
                CamaraActual.Id = ix;
                CamaraActual.Name = this.MisCamaras[ix].Name;
                CamaraActual.MonikerName = this.MisCamaras[ix].MonikerString;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                Debug.WriteLine(msg);
                CamaraActual = null;
            }
        }
        public string HacerFoto() 
        {
            try
            {
                if (CamaraActual != null)
                {
                    CerrarWebCam();//por si acaso estuviese abierta
                    WebCam = new VideoCaptureDevice(CamaraActual.MonikerName);
                    WebCam.NewFrame += new NewFrameEventHandler(HaciendoFoto);
                    WebCam.Start();
                    return "Ok Foto";
                }
                else
                {
                    return "Fallo Foto: cámara no seleccionada";
                }
            }
            catch(Exception ex) 
            {
                return "Fallo Foto: " + ex.Message;
            }
            
        }
        public string HacerFoto(int indiceCamara) 
        {
            SeleccionarCamara(indiceCamara);
            return this.HacerFoto();
        }
        private void CerrarWebCam()
        {
            if (WebCam != null && WebCam.IsRunning)
            {
                WebCam.SignalToStop();
                WebCam = null;
            }
        }
        private void HaciendoFoto (object sender, NewFrameEventArgs eventArgs) //evento hacer foto
        {
            Bitmap img = (Bitmap)eventArgs.Frame.Clone();//genero unea nueva imgagen
            WebCam.Stop();//obtengo la imagen y paro la grabación si no estaría generando imagenes constantemente.
            foto f = new();
            string fechaHora = DateTime.Now.ToString("yyyyMMdd_hhmmss");
            string fechaActual = DateTime.Now.ToString("yyyyMMdd");
            string rutaCompleta = this.PathCamara + "\\" + fechaActual + "\\";
            ComprobarIdDia();//comprueba si el id vuelve a ser cero
            string fname = this.IdActual.ToString("0000") + "_" + this.CamaraActual.Name + "_" + fechaHora;
            f.Ruta = rutaCompleta;
            f.RutaFile = rutaCompleta + fname;
            f.Date = new();
            f.FileName = fname;
            if (!Directory.Exists(f.Ruta))
            { 
                Directory.CreateDirectory(f.Ruta);
            } 
            img.Save(f.RutaFile,ImageFormat.Jpeg); //guardo la imagen en disco
            ListaFotosTomadas.Add(f); //añado la imagen a la lista de fotos tomadas mientras el servicio está activo
        }
        //pte lista camaras
        private void ComprobarIdDia() //cada fichero tiene un id nuevo segun el dia
        {
            if (DiaHoy != DateTime.Now.Day)
            {
                IdActual = 0;
                DiaHoy = DateTime.Now.Day;
            }
            else
            {
                this.IdActual++;
            }
        }
        ~ServicioCamara() //destructor
        {
            CerrarWebCam();
        }
        public int GetTotalFotos() 
        {
            return ListaFotosTomadas.Count;
        }
        public foto? GetFoto(int ixFoto) 
        {
            if (GetTotalFotos() > 0)
            {
                return ListaFotosTomadas[ixFoto];
            }
            else return null;
        }
        public foto? GetLastFoto() 
        {
            int c = GetTotalFotos();
            if (c > 0)
            {
                return ListaFotosTomadas[c - 1];
            }
            else return null;

        }
    }


}
