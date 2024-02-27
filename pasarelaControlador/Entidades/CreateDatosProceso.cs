using System.ComponentModel.DataAnnotations;

namespace pasarelaControlador.Entidades
{
    public class CreateDatosProceso
    {
        [Required(ErrorMessage = "Hay que especificar el tipo de acción")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "Hay que especificar una acción, Start o Stop")]
        //public string Accion { get; set; } //start / stop
        public string estado {  get; set; } //Working / Stopped
    }
}