namespace pasarelaControlador.Entidades
{
    public class DatosProceso
    {
        private long _id;
        public long Id {  get; set; }  // id del proceso
        public string Nombre { get; set;} //nombre del proceso
        //public string Accion {  get; set; }//accion sobre el proceso
        public string estado { get; set; } //Procesando / parado
    }


}
