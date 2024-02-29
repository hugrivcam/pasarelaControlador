using AForge.Video; //incompatible con linux
using AForge.Video.DirectShow;//incompatible con linux
using pasarelaControlador.Entidades;
using pasarelaControlador.EntidadesJSON;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
//using System.IO;
//pendiente añadir usuarios, cada usuario tendría su propia carpeta de fotos, de forma temporal eso si, porque no he creado una base de datos

namespace pasarelaControlador.Servicios
{
    public class ServicioCamara
    {
        private Bitmap? ImagenActual;
        private int IdFotoActual = 0; 
        private int DiaHoy = DateTime.Now.Day; //si cambia el dia se reinicia el idactual
        private List<Foto> ListaFotosTomadas = new List<Foto>();  
        private string PathCamara = @"R:\CAMERAFOTOS\"; //R podría ser una unidad RAMDISK
        private bool HayDispositivos = false;
        private FilterInfoCollection? MisCamaras;// carga dispositivos carga las camaras
        private List<string>? ListaCamaras; //carga dispositivos llena una lista con los nombres de las camaras
        private List<Camara>? ListaCamarasObj;
        private Camara? CamaraActual = null; 
        
        //private int? CamaraSeleccionadaIndice=-1;
        //private string? CamaraSeleccionadaNombre;
        //private string? CamaraMonikerString;
        private VideoCaptureDevice? WebCam;

        public ServicioCamara() 
        {
            CargaDispositivos();
        }

        void mapCamara(FilterInfo cam, ref Camara? camDes, int IdActual) 
        {
            camDes = new();
            camDes.Name = cam.Name;
            camDes.MonikerName = cam.MonikerString;
            camDes.Id = IdActual;
        }
        private int CargaDispositivos() 
        {
            try
            {

                Camara? nuevaCamara = null;
                int c, id;
                id = 0;
                MisCamaras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                ListaCamaras = new();
                ListaCamarasObj = new();
                c = MisCamaras.Count;
                if (MisCamaras.Count > 0)
                {
                    HayDispositivos = true;
                    foreach (FilterInfo? cam in MisCamaras)
                    {
                        if (cam != null)
                        {

                            ListaCamaras.Add(cam.Name.ToString());
                            id++;
                            this.mapCamara(cam, ref nuevaCamara, id);
                            ListaCamarasObj.Add(nuevaCamara!);
                        }
                    }
                }
                else
                {
                    HayDispositivos = false;
                }
                return c;
            }
            catch(Exception ex) 
            {
                throw new Exception("Error cargando dispositivos." + ex.Message);
            }
        }
        private bool SeleccionarCamara(int ix) //si no se selecciona una camara no se puede hacer una foto
        {
            try
            {
                CerrarWebCam();//cerramos la camara actual antes de cambiar de camara
                this.CamaraActual = new();
                CamaraActual.Id = ix;
                CamaraActual.Name = this.MisCamaras![ix - 1].Name;
                CamaraActual.MonikerName = this.MisCamaras[ix - 1].MonikerString;
                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                Debug.WriteLine(msg);
                CamaraActual = null;
                return false;
            }
        }
        public int ActualizarListaCamaras()
        {
            return this.CargaDispositivos();
        }
        public List<string> GetListaCamarasByName() 
        {
            return ListaCamaras;
        }
        private void mapCamaraJSON(ref List<CamaraJSON> listaDestino, List<Camara> listaOrigen) 
        {
            
            listaDestino = new();
            foreach (var camara in listaOrigen) 
            {
                CamaraJSON camaraJson = new();
                camaraJson.Name = camara.Name;
                camaraJson.Id = camara.Id;
                listaDestino.Add(camaraJson);  
            }
        }
        public List<CamaraJSON> GetListaCamarasObj() 
        {
            List<CamaraJSON>? listaCamaras = null;
            mapCamaraJSON(ref listaCamaras!, ListaCamarasObj!);
            return listaCamaras;//preparamos el objeto que queremos enviar no el que usar nuestro servicio
        }
        private void GrabarCamara(object sender, NewFrameEventArgs eventArgs)
        {
            ImagenActual = (Bitmap)eventArgs.Frame.Clone();//genero unea nueva imgagen contantemente a los fps que envie la camara
        }
        public CamaraJSON EncenderCamara(int indiceCamara)  //queda preparada para hacer fotos
        {
            CamaraJSON camaraJson = new();
            try
            {
                SeleccionarCamara(indiceCamara);//la camara anterior se cierra al seleccionar una nueva
                WebCam = new VideoCaptureDevice(CamaraActual.MonikerName);
                WebCam.NewFrame += new NewFrameEventHandler(GrabarCamara);//la camara queda encendida y se pone en modo grabación, preparada para obtener cualquier imagen en cualquier momento
                WebCam.Start();
                camaraJson.Name = CamaraActual.Name;
                camaraJson.Id = CamaraActual.Id;
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                camaraJson.Name = "Camara no seleccionada";
                camaraJson.Id = -1;
            }
            return camaraJson;
        }
        public async Task<bool> ApagarCamaraActual() 
        {
            return await this.CerrarWebCam();
        } //por si la camara se apaga desde la web


        private async Task<bool> CerrarWebCam() //se llama desde apagarCamaraActual
        {
            
            VideoCaptureDevice miWebCam = this.WebCam;
            if (WebCam != null && WebCam.IsRunning)
            {
                WebCam.SignalToStop();
                WebCam = null;
                Task waitToStop = Task.Run(() =>
                {
                    long c = 0;
                    while (miWebCam!.IsRunning)
                    {
                        c++;
                    }
                });
                await waitToStop; //parecido a JS,  creamos una funcion Task lambda y la llamamos con await, así el bucle queda parado en un hilo secundario.
                return true;//indicamos que la camara se ha cerrado correctamente
            }
            return true;// Si la cámara no está en funcionamiento, considerarla como cerrada

        }

        public int HacerFoto()
        {
            try
            {
                if (CamaraActual != null)
                {
                    HaciendoFoto();
                    return this.GetLastFotoIndex(); //"Ok Foto";
                }
                else 
                {
                    return -1; //no se puede hacer  una foto
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return -1; //"Fallo Foto: " + ex.Message;
            }

        }
        private void HaciendoFoto() //evento hacer foto
        {
            //Bitmap img = (Bitmap)eventArgs.Frame.Clone();//genero unea nueva imgagen
            //WebCam.Stop();//obtengo la imagen y paro la grabación si no estaría generando imagenes constantemente.
            Bitmap img = ImagenActual;
            Foto f = new();
            string fechaHora = DateTime.Now.ToString("yyyyMMdd_hhmmss");
            string fechaActual = DateTime.Now.ToString("yyyyMMdd");
            string rutaCompleta = this.PathCamara + fechaActual + "\\";
            ComprobarIdDia();//asigna un valor válido a IdFotoActual
            string fname = this.IdFotoActual.ToString("0000") + "_" + this.CamaraActual.Name + "_" + fechaHora + ".jpg";
            f.Id = IdFotoActual;
            f.Ruta = rutaCompleta;
            f.RutaFile = rutaCompleta + fname;
            f.Date = DateTime.Now;
            f.FileName = fname;
            if (!Directory.Exists(f.Ruta))
            { 
                Directory.CreateDirectory(f.Ruta);
            } 
            img.Save(f.RutaFile,ImageFormat.Jpeg); //guardo la imagen en disco
            ListaFotosTomadas.Add(f); //añado la imagen a la lista de fotos tomadas mientras el servicio está activo
        }
        //pte lista camaras
        private void ComprobarIdDia() //cada fichero tiene un id nuevo segun el dia, cada dia el id vuelve a 1 y el fichero se genera en una carpeta nueva
        {
            if (DiaHoy != DateTime.Now.Day)
            {
                IdFotoActual = 0;
                DiaHoy = DateTime.Now.Day;
            }
            else
            {
                this.IdFotoActual++;
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
        private void MapFotoJSON(ref FotoJSON fDes, Foto foto)
        {
            fDes = new FotoJSON();
            fDes.FileName = foto.FileName;
            fDes.Id = foto.Id;
            fDes.Date = foto.Date!.Value.ToString("dd/MM/yyyy HH:mm:ss");//si foto.Date fuera nullable necesito acceder a través de Value
        }
        public FotoJSON? GetFotoJSON(int ixFoto) 
        {
            if (GetTotalFotos() > 0)
            {
                Foto f = ListaFotosTomadas[ixFoto - 1];
                FotoJSON fJSON = null;//sólo envio los campos que necesito
                MapFotoJSON(ref fJSON, f);
                return fJSON;
            }
            else return null;
        }
        public FotoJSON? GetLastFotoJSON() 
        {
            int c = GetTotalFotos();
            if (c > 0)
            {
                
                Foto f = ListaFotosTomadas[c - 1];//obtengo los datos de la ultima foto tomada
                FotoJSON fJSON = null;//sólo envio los campos que necesito
                MapFotoJSON(ref fJSON, f);
                return fJSON;
            }
            else return null;

        }
        public Foto? GetFoto(int ixFoto)
        {
            if (GetTotalFotos() > 0)
            {
                return ListaFotosTomadas[ixFoto - 1];
            }
            else return null;
        }
        public Foto? GetLastFoto()
        {
            int c = GetTotalFotos();
            if (c > 0)
            {

                return ListaFotosTomadas[c - 1];//obtengo los datos de la ultima foto tomada
            }
            else return null;

        }
        public int GetLastFotoIndex()
        {
            int c = GetTotalFotos();
            if (c > 0) 
            {
                return c - 1;
            }
            else
                return -1;     
        }
    }


}
